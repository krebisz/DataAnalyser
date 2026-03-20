using System.Windows;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class DistributionChartControllerAdapterTests
{
    [Fact]
    public async Task RenderAsync_InPolarMode_RendersOnCartesianHost_AndKeepsPolarHostCollapsed()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartState = new ChartState
            {
                    IsDistributionVisible = true,
                    IsDistributionPolarMode = true,
                    SelectedDistributionMode = DistributionMode.Weekly
            };

            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new DistributionChartController();
            var distributionService = new StubDistributionService(CreateRangeResult());
            var adapter = new DistributionChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    () => null,
                    distributionService,
                    distributionService,
                    new DistributionPolarRenderingService(),
                    () => null);

            adapter.InitializeControls();
            adapter.UpdateChartTypeVisibility();

            var context = new ChartDataContext
            {
                    Data1 =
                    [
                            new MetricData
                            {
                                    NormalizedTimestamp = new DateTime(2026, 1, 1),
                                    Value = 1m,
                                    Unit = "kg"
                            }
                    ],
                    DisplayName1 = "Weight",
                    MetricType = "weight",
                    PrimaryMetricType = "weight",
                    From = new DateTime(2026, 1, 1),
                    To = new DateTime(2026, 1, 7)
            };

            await adapter.RenderAsync(context);

            Assert.Equal(Visibility.Visible, controller.Chart.Visibility);
            Assert.Equal(Visibility.Collapsed, controller.PolarChart.Visibility);
            Assert.Equal("Cartesian", controller.ChartTypeToggleButton.Content);
            Assert.Contains(controller.Chart.Series, series => string.Equals(series.Title, "Min", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(controller.Chart.Series, series => string.Equals(series.Title, "Max", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(controller.Chart.Series, series => string.Equals(series.Title, "Avg", StringComparison.OrdinalIgnoreCase));
            Assert.IsType<DistributionPolarProjectionTooltip>(controller.Chart.Tag);
            Assert.True(adapter.HasSeries(chartState));
        });
    }

    private static DistributionRangeResult CreateRangeResult()
    {
        return new DistributionRangeResult(
                [1, 2, 3, 4, 5, 6, 7],
                [2, 3, 4, 5, 6, 7, 8],
                [1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5],
                1,
                8,
                "kg");
    }

    private sealed class StubDistributionService : IDistributionService
    {
        private readonly DistributionRangeResult _rangeResult;

        public StubDistributionService(DistributionRangeResult rangeResult)
        {
            _rangeResult = rangeResult;
        }

        public Task UpdateDistributionChartAsync(CartesianChart targetChart, IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, double minHeight = 400, bool useFrequencyShading = true, int intervalCount = 10, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            return Task.CompletedTask;
        }

        public Task<DistributionRangeResult?> ComputeSimpleRangeAsync(IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            return Task.FromResult<DistributionRangeResult?>(_rangeResult);
        }

        public void SetShadingStrategy(IIntervalShadingStrategy strategy)
        {
        }
    }

    private sealed class NoOpScope : IDisposable
    {
        public static NoOpScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
