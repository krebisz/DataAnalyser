using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public sealed class MainWindowViewModelSelectionBatchTests
{
    [Fact]
    public void BatchedSelectionUpdates_ShouldRaiseSelectionStateChangedOnce()
    {
        var viewModel = CreateViewModel();
        var eventCount = 0;
        viewModel.SelectionStateChanged += (_, _) => eventCount++;

        using (viewModel.BeginSelectionStateBatch())
        {
            viewModel.SetSelectedMetricType("Weight");
            viewModel.SetSelectedSeries(
            [
                new MetricSeriesSelection("Weight", "Morning", "Weight", "Morning")
            ]);
            viewModel.SetDateRange(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
        }

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void NestedSelectionBatches_ShouldRaiseSelectionStateChangedOnceAfterOuterScopeDisposes()
    {
        var viewModel = CreateViewModel();
        var eventCount = 0;
        viewModel.SelectionStateChanged += (_, _) => eventCount++;

        using (viewModel.BeginSelectionStateBatch())
        {
            viewModel.SetSelectedMetricType("Weight");

            using (viewModel.BeginSelectionStateBatch())
                viewModel.SetResolutionTableName("All");

            Assert.Equal(0, eventCount);
        }

        Assert.Equal(1, eventCount);
    }

    private static MainWindowViewModel CreateViewModel()
    {
        return new MainWindowViewModel(
            new ChartState(),
            new MetricState(),
            new UiState(),
            new MetricSelectionService("TestConnection"));
    }
}
