using System.Windows;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class DistributionChartControllerAdapterTests
{
    [Fact]
    public void UpdateSubtypeOptions_ShouldPopulateCombo_AndPersistSelectionDuringInitialization()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    IsDistributionVisible = true,
                    SelectedDistributionMode = DistributionMode.Weekly
            };

            var metricState = new MetricState();
            metricState.SetSeriesSelections(
            [
                new MetricSeriesSelection("weight", "avg"),
                new MetricSeriesSelection("steps", "(All)")
            ]);
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new DistributionChartController();
            var distributionService = new StubDistributionService(CreateRangeResult());
            var renderingContract = new DistributionRenderingContract(
                    () => null,
                    distributionService,
                    distributionService,
                    new DistributionPolarRenderingService());
            var adapter = new DistributionChartControllerAdapter(
                    controller,
                    viewModel,
                    () => true,
                    () => NoOpScope.Instance,
                    metricService,
                    renderingContract,
                    () => null);

            adapter.UpdateSubtypeOptions();

            Assert.Equal(2, controller.SubtypeCombo.Items.Count);
            Assert.Equal(metricState.SelectedSeries[0], chartState.SelectedDistributionSeries);
            Assert.True(controller.SubtypeCombo.IsEnabled);
        });
    }

    [Fact]
    public void Clear_ResetZoom_AndHasSeries_ShouldDelegateToRenderingContract_WithResolvedRoute()
    {
        StaTestHelper.Run(() =>
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
            var renderingContract = new FakeDistributionRenderingContract
            {
                    HasRenderableContentResult = true
            };
            var adapter = new DistributionChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    renderingContract,
                    () => null);

            adapter.Clear(chartState);
            adapter.ResetZoom();
            var hasSeries = adapter.HasSeries(chartState);

            Assert.True(hasSeries);
            Assert.Same(controller.Chart, renderingContract.LastClearHost!.CartesianChart);
            Assert.Same(controller.PolarChart, renderingContract.LastClearHost.PolarChart);
            Assert.Equal(DistributionRenderingRoute.PolarFallback, renderingContract.LastResetRoute);
            Assert.Equal(DistributionRenderingRoute.PolarFallback, renderingContract.LastHasRenderableRoute);
        });
    }

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
            var renderingContract = new DistributionRenderingContract(
                    () => null,
                    distributionService,
                    distributionService,
                    new DistributionPolarRenderingService());
            var adapter = new DistributionChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    renderingContract,
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

    [Fact]
    public async Task ResetZoom_InPolarMode_ReappliesPolarProjectionBounds()
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
            var controller = new DistributionChartController
            {
                    Width = 800,
                    Height = 400
            };
            controller.Chart.Width = 800;
            controller.Chart.Height = 400;
            controller.Chart.Measure(new Size(800, 400));
            controller.Chart.Arrange(new Rect(0, 0, 800, 400));
            controller.Chart.UpdateLayout();

            var distributionService = new StubDistributionService(CreateRangeResult());
            var renderingContract = new DistributionRenderingContract(
                    () => null,
                    distributionService,
                    distributionService,
                    new DistributionPolarRenderingService());
            var adapter = new DistributionChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    renderingContract,
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

            controller.Chart.AxisX[0].MinValue = double.NaN;
            controller.Chart.AxisX[0].MaxValue = double.NaN;
            controller.Chart.AxisY[0].MinValue = double.NaN;
            controller.Chart.AxisY[0].MaxValue = double.NaN;

            adapter.ResetZoom();

            Assert.False(double.IsNaN(controller.Chart.AxisX[0].MinValue));
            Assert.False(double.IsNaN(controller.Chart.AxisX[0].MaxValue));
            Assert.False(double.IsNaN(controller.Chart.AxisY[0].MinValue));
            Assert.False(double.IsNaN(controller.Chart.AxisY[0].MaxValue));
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

    private sealed class FakeDistributionRenderingContract : IDistributionRenderingContract
    {
        public DistributionChartRenderHost? LastClearHost { get; private set; }
        public DistributionRenderingRoute? LastResetRoute { get; private set; }
        public DistributionRenderingRoute? LastHasRenderableRoute { get; private set; }
        public bool HasRenderableContentResult { get; set; }

        public IReadOnlyList<DistributionBackendQualification> GetBackendQualificationMatrix()
        {
            return [];
        }

        public DistributionRenderingCapabilities GetCapabilities(DistributionRenderingRoute route)
        {
            return new DistributionRenderingCapabilities("test", DistributionRenderingQualification.Qualified, true, true, true, true, true, true);
        }

        public Task RenderAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host)
        {
            return Task.CompletedTask;
        }

        public void Clear(DistributionChartRenderHost host)
        {
            LastClearHost = host;
        }

        public void ResetView(DistributionRenderingRoute route, DistributionChartRenderHost host)
        {
            LastResetRoute = route;
        }

        public bool HasRenderableContent(DistributionRenderingRoute route, DistributionChartRenderHost host)
        {
            LastHasRenderableRoute = route;
            return HasRenderableContentResult;
        }
    }
}
