using System.Windows;
using System.Windows.Threading;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.SecondaryCharts;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;
using Moq;

namespace DataVisualiser.Tests.Orchestration;

public sealed class SecondaryMetricChartOrchestrationPipelineTests
{
    [Fact]
    public async Task RenderAsync_ShouldNotifyAndClear_WhenStageThrows()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chart = new CartesianChart
            {
                Width = 800,
                Height = 400,
                Visibility = Visibility.Visible,
                Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Existing",
                        Values = new ChartValues<double> { 1d, 2d }
                    }
                }
            };
            var window = new Window
            {
                Width = 900,
                Height = 600,
                Content = chart
            };

            window.Show();
            window.UpdateLayout();
            chart.Measure(new Size(800, 400));
            chart.Arrange(new Rect(0, 0, 800, 400));
            chart.UpdateLayout();
            await chart.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            try
            {
                var chartState = new ChartState();
                chartState.ChartTimestamps[chart] = [new DateTime(2024, 1, 1)];
                var notifications = new CapturingNotificationService();

                var selectionStage = new Mock<ISecondaryMetricChartStrategySelectionStage>(MockBehavior.Strict);
                selectionStage
                    .Setup(stage => stage.Select(It.IsAny<SecondaryMetricChartRenderRequest>()))
                    .Throws(new InvalidOperationException("Selection failed."));

                var renderStage = new Mock<ISecondaryMetricChartRenderInvocationStage>(MockBehavior.Strict);
                var pipeline = new SecondaryMetricChartOrchestrationPipeline(selectionStage.Object, renderStage.Object, notifications);
                var request = new SecondaryMetricChartRenderRequest(
                    new ChartDataContext
                    {
                        Data1 = TestDataBuilders.HealthMetricData().BuildSeries(2, TimeSpan.FromDays(1)),
                        Data2 = TestDataBuilders.HealthMetricData().BuildSeries(2, TimeSpan.FromDays(1)),
                        DisplayName1 = "Primary",
                        DisplayName2 = "Secondary",
                        From = new DateTime(2024, 1, 1),
                        To = new DateTime(2024, 1, 2)
                    },
                    chartState,
                    SecondaryMetricChartRoute.Normalized);

                await pipeline.RenderAsync(request, chart);

                Assert.Single(notifications.Errors);
                Assert.Empty(chart.Series);
                Assert.False(chartState.ChartTimestamps.ContainsKey(chart));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private sealed class CapturingNotificationService : IUserNotificationService
    {
        public List<(string Title, string Message)> Errors { get; } = [];

        public void ShowError(string title, string message)
        {
            Errors.Add((title, message));
        }
    }
}
