using System.Windows.Controls;
using DataVisualiser.Core.Services;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Controls;
using DataVisualiser.UI.Rendering;
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
            var metricService = new MetricSelectionService("Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True");
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
            var metricService = new MetricSelectionService("Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True");
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
}
