using System.Windows;
using System.Windows.Threading;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.MainHost;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewChartPipelineFactoryTests
{
    [Fact]
    public async Task Create_ShouldReturnConfiguredPipelineComponents()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chart = new CartesianChart();
            var window = new Window
            {
                Content = chart
            };
            window.Show();
            await chart.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var factory = new MainChartsViewChartPipelineFactory();
                var result = factory.Create(new MainChartsViewChartPipelineFactory.Context(
                    new Dictionary<CartesianChart, List<DateTime>>(),
                    tooltipManager,
                    "Server=(local);Database=Health;Integrated Security=True;TrustServerCertificate=True"));

                Assert.NotNull(result.ChartUpdateCoordinator);
                Assert.NotNull(result.WeeklyDistributionService);
                Assert.NotNull(result.HourlyDistributionService);
                Assert.NotNull(result.DistributionPolarRenderingService);
                Assert.NotNull(result.StrategyCutOverService);
                Assert.NotNull(result.ChartRenderingOrchestrator);
                Assert.NotNull(result.WeekdayTrendChartUpdateCoordinator);
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }
}
