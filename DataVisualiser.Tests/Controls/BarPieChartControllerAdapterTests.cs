using System.Windows.Controls;
using DataVisualiser.Core.Services;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.Controls;

public sealed class BarPieChartControllerAdapterTests
{
    [Fact]
    public void InitializeControls_PopulatesBucketCounts_AndSelectsCurrentValue()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    BarPieBucketCount = 5
            };
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new BarPieChartController();
            var rendererResolver = new ChartRendererResolver();
            var surfaceFactory = new ChartSurfaceFactory(rendererResolver);
            var adapter = new BarPieChartControllerAdapter(controller, viewModel, () => false, metricService, rendererResolver, surfaceFactory);

            adapter.InitializeControls();

            Assert.Equal(20, controller.BucketCountCombo.Items.Count);
            Assert.Equal(5, ((ComboBoxItem)controller.BucketCountCombo.SelectedItem!).Tag);
        });
    }

    [Fact]
    public void ToggleRequested_UpdatesVisibilityAndEmitsUpdateArgs()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    IsBarPieVisible = false
            };
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new BarPieChartController();
            var rendererResolver = new ChartRendererResolver();
            var surfaceFactory = new ChartSurfaceFactory(rendererResolver);
            var adapter = new BarPieChartControllerAdapter(controller, viewModel, () => false, metricService, rendererResolver, surfaceFactory);

            bool? showBarPie = null;
            viewModel.ChartUpdateRequested += (_, args) =>
            {
                if (args.ToggledChartName == ChartControllerKeys.BarPie)
                    showBarPie = args.ShowBarPie;
            };

            viewModel.CompleteInitialization();
            adapter.OnToggleRequested(this, EventArgs.Empty);

            Assert.True(chartState.IsBarPieVisible);
            Assert.True(showBarPie);
        });
    }

    [Fact]
    public void HasSeries_UsesTrackedRenderedContentState()
    {
        StaTestHelper.Run(() =>
        {
            var surface = new StubTrackedChartSurface
            {
                HasRenderedContentValue = true
            };
            var adapter = CreateAdapter(surface);
            var state = new ChartState();

            Assert.True(adapter.HasSeries(state));

            surface.SetHasRenderedContent(false);

            Assert.False(adapter.HasSeries(state));
        });
    }

    private static BarPieChartControllerAdapter CreateAdapter(StubTrackedChartSurface surface)
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        var controller = new BarPieChartController();
        var rendererResolver = new StubChartRendererResolver();
        var surfaceFactory = new StubChartSurfaceFactory(surface);
        return new BarPieChartControllerAdapter(controller, viewModel, () => false, metricService, rendererResolver, surfaceFactory);
    }

    private sealed class StubTrackedChartSurface : IChartSurface, ITrackedChartContentSurface
    {
        public bool HasRenderedContentValue { get; set; }

        public bool HasRenderedContent => HasRenderedContentValue;

        public void SetHasRenderedContent(bool hasRenderedContent)
        {
            HasRenderedContentValue = hasRenderedContent;
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

    private sealed class StubChartRendererResolver : IChartRendererResolver
    {
        public ChartRendererKind ResolveKind(string chartKey)
        {
            return ChartRendererKind.LiveCharts;
        }

        public IChartRenderer ResolveRenderer(string chartKey)
        {
            return new DataVisualiser.UI.Charts.Rendering.LiveCharts.LiveChartsChartRenderer();
        }
    }
}
