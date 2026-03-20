using System.Windows.Controls;
using DataVisualiser.Core.Services;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class BarPieChartControllerAdapterResetZoomTests
{
    [Fact]
    public void ResetZoom_WhenTrackedCartesianChartPresent_ClearsAxisBounds()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart();
            chart.AxisX.Add(new Axis
            {
                MinValue = 0,
                MaxValue = 10
            });

            var adapter = CreateAdapter(new StubTrackedCartesianChartSurface(chart), new StubChartRenderer());

            adapter.ResetZoom();

            var axis = chart.AxisX[0];
            Assert.True(double.IsNaN(axis.MinValue));
            Assert.True(double.IsNaN(axis.MaxValue));
        });
    }

    [Fact]
    public void ResetZoom_WhenNoTrackedCartesianChart_DoesNotThrow()
    {
        StaTestHelper.Run(() =>
        {
            var adapter = CreateAdapter(new StubTrackedCartesianChartSurface(null), new StubChartRenderer());

            adapter.ResetZoom();
        });
    }

    [Fact]
    public async Task ResetZoom_AfterRender_UsesTrackedRenderedChart()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chart = new CartesianChart();
            chart.AxisX.Add(new Axis
            {
                MinValue = 2,
                MaxValue = 8
            });

            var surface = new StubTrackedCartesianChartSurface(null);
            var adapter = CreateAdapter(surface, new StubChartRenderer(chart));

            await adapter.RenderAsync(new DataVisualiser.Core.Orchestration.ChartDataContext());

            adapter.ResetZoom();

            var axis = chart.AxisX[0];
            Assert.True(double.IsNaN(axis.MinValue));
            Assert.True(double.IsNaN(axis.MaxValue));
        });
    }

    private static BarPieChartControllerAdapter CreateAdapter(IChartSurface surface, IChartRenderer renderer)
    {
        var chartState = new ChartState
        {
            IsBarPieVisible = true
        };
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        var controller = new BarPieChartController();
        var rendererResolver = new StubChartRendererResolver(renderer);
        var surfaceFactory = new StubChartSurfaceFactory(surface);

        return new BarPieChartControllerAdapter(controller, viewModel, () => false, metricService, rendererResolver, surfaceFactory);
    }

    private sealed class StubTrackedCartesianChartSurface : IChartSurface, ITrackedCartesianChartSurface
    {
        public StubTrackedCartesianChartSurface(CartesianChart? chart)
        {
            RenderedCartesianChart = chart;
        }

        public CartesianChart? RenderedCartesianChart { get; private set; }

        public void SetRenderedCartesianChart(CartesianChart? chart)
        {
            RenderedCartesianChart = chart;
        }

        public void SetTitle(string? title)
        {
        }

        public void SetIsVisible(bool isVisible)
        {
        }

        public void SetHeader(System.Windows.UIElement? header)
        {
        }

        public void SetBehavioralControls(System.Windows.UIElement? controls)
        {
        }

        public void SetChartContent(System.Windows.UIElement? content)
        {
        }
    }

    private sealed class StubChartRenderer : IChartRenderer
    {
        private readonly CartesianChart? _chartToTrack;

        public StubChartRenderer(CartesianChart? chartToTrack = null)
        {
            _chartToTrack = chartToTrack;
        }

        public Task ApplyAsync(IChartSurface surface, UiChartRenderModel model, CancellationToken cancellationToken = default)
        {
            if (surface is ITrackedCartesianChartSurface trackedSurface)
                trackedSurface.SetRenderedCartesianChart(_chartToTrack);

            return Task.CompletedTask;
        }
    }

    private sealed class StubChartRendererResolver : IChartRendererResolver
    {
        private readonly IChartRenderer _renderer;

        public StubChartRendererResolver(IChartRenderer renderer)
        {
            _renderer = renderer;
        }

        public ChartRendererKind ResolveKind(string chartKey)
        {
            return ChartRendererKind.LiveCharts;
        }

        public IChartRenderer ResolveRenderer(string chartKey)
        {
            return _renderer;
        }
    }

    private sealed class StubChartSurfaceFactory : IChartSurfaceFactory
    {
        private readonly IChartSurface _surface;

        public StubChartSurfaceFactory(IChartSurface surface)
        {
            _surface = surface;
        }

        public IChartSurface Create(string chartKey, IChartPanelHost panelHost)
        {
            return _surface;
        }
    }
}
