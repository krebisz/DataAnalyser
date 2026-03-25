using System.Windows;
using System.Windows.Threading;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.DistributionCharts;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;
using Moq;

namespace DataVisualiser.Tests.Orchestration;

public sealed class DistributionChartOrchestrationPipelineTests
{
    [Fact]
    public async Task RenderAsync_ShouldNotifyAndClear_WhenRenderInvocationThrows()
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
                    new ColumnSeries
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

                var preparationStage = new Mock<IDistributionChartPreparationStage>(MockBehavior.Strict);
                preparationStage
                    .Setup(stage => stage.Prepare(It.IsAny<DistributionChartOrchestrationRequest>()))
                    .Returns(new DistributionChartPreparedData(
                        Mock.Of<IDistributionService>(),
                        TestDataBuilders.HealthMetricData().BuildSeries(2, TimeSpan.FromDays(1)),
                        "Primary",
                        new DateTime(2024, 1, 1),
                        new DateTime(2024, 1, 2),
                        chartState.GetDistributionSettings(DistributionMode.Weekly),
                        null));

                var renderStage = new Mock<IDistributionChartRenderInvocationStage>(MockBehavior.Strict);
                renderStage
                    .Setup(stage => stage.RenderAsync(It.IsAny<DistributionChartPreparedData>(), chart))
                    .ThrowsAsync(new InvalidOperationException("Render failed."));

                var pipeline = new DistributionChartOrchestrationPipeline(preparationStage.Object, renderStage.Object, notifications);
                var request = new DistributionChartOrchestrationRequest(
                    new ChartDataContext
                    {
                        Data1 = TestDataBuilders.HealthMetricData().BuildSeries(2, TimeSpan.FromDays(1)),
                        DisplayName1 = "Primary",
                        From = new DateTime(2024, 1, 1),
                        To = new DateTime(2024, 1, 2)
                    },
                    chartState,
                    DistributionMode.Weekly);

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
