using System.Collections.Generic;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Syncfusion;

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
        var metricService = new Core.Services.MetricSelectionService("Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        return (viewModel, metricService);
    }

    private sealed class StubSyncfusionController : ISyncfusionSunburstChartController
    {
        public object? ItemsSource { get; set; }
        public ChartPanelController Panel { get; } = new();
        public Button ToggleButton { get; } = new();
    }
}
