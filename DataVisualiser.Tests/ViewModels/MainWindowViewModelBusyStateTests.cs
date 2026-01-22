using DataVisualiser.Core.Services;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public class MainWindowViewModelBusyStateTests
{
    [Fact]
    public void IsBusy_TracksUiStateFlags()
    {
        var uiState = new UiState();
        var viewModel = CreateViewModel(uiState);

        Assert.False(viewModel.IsBusy);

        uiState.IsLoadingMetricTypes = true;
        Assert.True(viewModel.IsBusy);

        uiState.IsLoadingMetricTypes = false;
        Assert.False(viewModel.IsBusy);

        uiState.IsUiBusy = true;
        Assert.True(viewModel.IsBusy);
    }

    [Fact]
    public void IsBusy_RaisesPropertyChanged_WhenUiStateChanges()
    {
        var uiState = new UiState();
        var viewModel = CreateViewModel(uiState);
        var observed = new List<string?>();
        viewModel.PropertyChanged += (_, e) => observed.Add(e.PropertyName);

        uiState.IsLoadingSubtypes = true;

        Assert.Contains(nameof(MainWindowViewModel.IsBusy), observed);
    }

    private static MainWindowViewModel CreateViewModel(UiState uiState)
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var metricService = new MetricSelectionService("TestConnection");

        return new MainWindowViewModel(chartState, metricState, uiState, metricService);
    }
}