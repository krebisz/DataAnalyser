using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.Controls;

public sealed class CartesianMetricControllerAdapterLifecycleTests
{
    [Fact]
    public void MainLifecycleMethods_ShouldDelegateToRenderingContract_WithMainRoute()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    IsMainVisible = true
            };
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new MainChartController();
            var renderingContract = new FakeCartesianMetricRenderingContract
            {
                    HasRenderableContentResult = true
            };
            var adapter = new MainChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    metricService,
                    renderingContract);

            adapter.Clear(chartState);
            adapter.ResetZoom();
            var hasSeries = adapter.HasSeries(chartState);

            Assert.True(hasSeries);
            Assert.Equal(CartesianMetricChartRoute.Main, renderingContract.LastClearRoute);
            Assert.Equal(CartesianMetricChartRoute.Main, renderingContract.LastResetRoute);
            Assert.Equal(CartesianMetricChartRoute.Main, renderingContract.LastHasRenderableRoute);
            Assert.Same(controller.Chart, renderingContract.LastClearHost!.Chart);
            Assert.Same(chartState, renderingContract.LastClearHost.ChartState);
        });
    }

    [Fact]
    public void DiffRatioLifecycleMethods_ShouldDelegateToRenderingContract_WithDiffRatioRoute()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    IsDiffRatioVisible = true
            };
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new DiffRatioChartController();
            var renderingContract = new FakeCartesianMetricRenderingContract
            {
                    HasRenderableContentResult = true
            };
            var adapter = new DiffRatioChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    () => null,
                    renderingContract);

            adapter.Clear(chartState);
            adapter.ResetZoom();
            var hasSeries = adapter.HasSeries(chartState);

            Assert.True(hasSeries);
            Assert.Equal(CartesianMetricChartRoute.DiffRatio, renderingContract.LastClearRoute);
            Assert.Equal(CartesianMetricChartRoute.DiffRatio, renderingContract.LastResetRoute);
            Assert.Equal(CartesianMetricChartRoute.DiffRatio, renderingContract.LastHasRenderableRoute);
            Assert.Same(controller.Chart, renderingContract.LastClearHost!.Chart);
            Assert.Same(chartState, renderingContract.LastClearHost.ChartState);
        });
    }

    [Fact]
    public void NormalizedLifecycleMethods_ShouldDelegateToRenderingContract_WithNormalizedRoute()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    IsNormalizedVisible = true
            };
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new NormalizedChartController();
            var renderingContract = new FakeCartesianMetricRenderingContract
            {
                    HasRenderableContentResult = true
            };
            var adapter = new NormalizedChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    renderingContract);

            adapter.Clear(chartState);
            adapter.ResetZoom();
            var hasSeries = adapter.HasSeries(chartState);

            Assert.True(hasSeries);
            Assert.Equal(CartesianMetricChartRoute.Normalized, renderingContract.LastClearRoute);
            Assert.Equal(CartesianMetricChartRoute.Normalized, renderingContract.LastResetRoute);
            Assert.Equal(CartesianMetricChartRoute.Normalized, renderingContract.LastHasRenderableRoute);
            Assert.Same(controller.Chart, renderingContract.LastClearHost!.Chart);
            Assert.Same(chartState, renderingContract.LastClearHost.ChartState);
        });
    }

    private sealed class FakeCartesianMetricRenderingContract : ICartesianMetricChartRenderingContract
    {
        public CartesianMetricChartRoute? LastClearRoute { get; private set; }
        public CartesianMetricChartRenderHost? LastClearHost { get; private set; }
        public CartesianMetricChartRoute? LastResetRoute { get; private set; }
        public CartesianMetricChartRenderHost? LastResetHost { get; private set; }
        public CartesianMetricChartRoute? LastHasRenderableRoute { get; private set; }
        public CartesianMetricChartRenderHost? LastHasRenderableHost { get; private set; }
        public bool HasRenderableContentResult { get; set; }

        public IReadOnlyList<CartesianMetricBackendQualification> GetBackendQualificationMatrix()
        {
            return [];
        }

        public CartesianMetricRenderingCapabilities GetCapabilities(CartesianMetricChartRoute route)
        {
            return new CartesianMetricRenderingCapabilities("test", CartesianMetricRenderingQualification.Qualified, true, true, true, true, true, true);
        }

        public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
        {
            return Task.CompletedTask;
        }

        public void Clear(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
            LastClearRoute = route;
            LastClearHost = host;
        }

        public void ResetView(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
            LastResetRoute = route;
            LastResetHost = host;
        }

        public bool HasRenderableContent(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
            LastHasRenderableRoute = route;
            LastHasRenderableHost = host;
            return HasRenderableContentResult;
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
