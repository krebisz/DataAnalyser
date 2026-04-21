using System.Collections.Generic;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Syncfusion;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.Controls;

public sealed class SyncfusionSunburstChartControllerAdapterTests
{
    [Fact]
    public void RenderAsync_WhenNoSelections_ReturnsEmptyItems()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new StubSyncfusionController();
            var (viewModel, metricService) = CreateViewModel();
            var adapter = new SyncfusionSunburstChartControllerAdapter(controller, viewModel, metricService);

            adapter.RenderAsync(new ChartDataContext()).GetAwaiter().GetResult();

            var items = Assert.IsAssignableFrom<IEnumerable<SunburstItem>>(controller.ItemsSource);
            Assert.Empty(items);
            Assert.True(viewModel.ChartState.RenderPlanDiagnostics.ContainsKey(ChartProgramKind.SyncfusionSunburst));
            Assert.Equal(
                "SyncfusionSunburst",
                viewModel.ChartState.RenderPlanDiagnostics[ChartProgramKind.SyncfusionSunburst].Metadata["ProgramKind"]);
            Assert.Contains(viewModel.ChartState.RenderPlanHistory, entry =>
                entry.ProgramKind == ChartProgramKind.SyncfusionSunburst.ToString() &&
                entry.Metadata["Route"] == SyncfusionSunburstRenderingRoute.Hierarchy.ToString());
        });
    }

    [Fact]
    public void LifecycleMethods_ShouldDelegateToRenderingContract()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new StubSyncfusionController
            {
                ItemsSource = new List<SunburstItem>
                {
                    new("Vitals", "Temp", 10)
                }
            };
            var (viewModel, metricService) = CreateViewModel();
            var renderingContract = new FakeSyncfusionSunburstRenderingContract();
            var adapter = new SyncfusionSunburstChartControllerAdapter(controller, viewModel, metricService, renderingContract);

            adapter.Clear(viewModel.ChartState);
            adapter.ResetZoom();
            adapter.HasSeries(viewModel.ChartState);

            Assert.NotNull(renderingContract.LastClearHost);
            Assert.NotNull(renderingContract.LastResetHost);
            Assert.NotNull(renderingContract.LastHasRenderableHost);
            Assert.Equal(SyncfusionSunburstRenderingRoute.Hierarchy, renderingContract.LastResetRoute);
            Assert.Equal(SyncfusionSunburstRenderingRoute.Hierarchy, renderingContract.LastHasRenderableRoute);
        });
    }

    [Fact]
    public void Clear_ResetsItemsSource()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new StubSyncfusionController
            {
                ItemsSource = new List<SunburstItem>
                {
                    new("Vitals", "Temp", 10)
                }
            };
            var (viewModel, metricService) = CreateViewModel();
            var adapter = new SyncfusionSunburstChartControllerAdapter(controller, viewModel, metricService);

            adapter.Clear(new ChartState());

            var items = Assert.IsAssignableFrom<IEnumerable<SunburstItem>>(controller.ItemsSource);
            Assert.Empty(items);
        });
    }

    private static (MainWindowViewModel ViewModel, Core.Services.MetricSelectionService MetricService) CreateViewModel()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new Core.Services.MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        return (viewModel, metricService);
    }

    private sealed class StubSyncfusionController : ISyncfusionSunburstChartController
    {
        public object? ItemsSource { get; set; }
        public ChartPanelController Panel { get; } = new();
        public Button ToggleButton { get; } = new();
    }

    private sealed class FakeSyncfusionSunburstRenderingContract : ISyncfusionSunburstRenderingContract
    {
        public SyncfusionSunburstChartRenderHost? LastClearHost { get; private set; }
        public SyncfusionSunburstChartRenderHost? LastResetHost { get; private set; }
        public SyncfusionSunburstChartRenderHost? LastHasRenderableHost { get; private set; }
        public SyncfusionSunburstRenderingRoute? LastResetRoute { get; private set; }
        public SyncfusionSunburstRenderingRoute? LastHasRenderableRoute { get; private set; }

        public Task RenderAsync(SyncfusionSunburstChartRenderRequest request, SyncfusionSunburstChartRenderHost host)
        {
            host.Controller.ItemsSource = request.Items;
            return Task.CompletedTask;
        }

        public void Clear(SyncfusionSunburstChartRenderHost host)
        {
            LastClearHost = host;
        }

        public void ResetView(SyncfusionSunburstRenderingRoute route, SyncfusionSunburstChartRenderHost host)
        {
            LastResetRoute = route;
            LastResetHost = host;
        }

        public bool HasRenderableContent(SyncfusionSunburstRenderingRoute route, SyncfusionSunburstChartRenderHost host)
        {
            LastHasRenderableRoute = route;
            LastHasRenderableHost = host;
            return true;
        }
    }
}
