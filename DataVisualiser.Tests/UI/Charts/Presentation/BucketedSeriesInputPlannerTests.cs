using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class BucketedSeriesInputPlannerTests
{
    [Fact]
    public void GetDistinctSelectedSeries_DeduplicatesByDisplayKeyIgnoringCase()
    {
        var viewModel = CreateViewModel();
        viewModel.MetricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "Fat Mass"),
            new MetricSeriesSelection("weight", "fat mass"),
            new MetricSeriesSelection("Weight", "Fat Free Mass")
        ]);

        var selections = BucketedSeriesInputPlanner.GetDistinctSelectedSeries(viewModel);

        Assert.Equal(2, selections.Count);
        Assert.Equal("Weight:Fat Mass", selections[0].DisplayKey);
        Assert.Equal("Weight:Fat Free Mass", selections[1].DisplayKey);
    }

    [Fact]
    public void TryResolveDateRange_PrefersExplicitMetricDateRangeOverLastContext()
    {
        var viewModel = CreateViewModel();
        viewModel.MetricState.FromDate = new DateTime(2026, 2, 1);
        viewModel.MetricState.ToDate = new DateTime(2026, 2, 28);
        viewModel.ChartState.LastContext = new ChartDataContext
        {
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 31)
        };

        var resolved = BucketedSeriesInputPlanner.TryResolveDateRange(viewModel, out var from, out var to);

        Assert.True(resolved);
        Assert.Equal(new DateTime(2026, 2, 1), from);
        Assert.Equal(new DateTime(2026, 2, 28), to);
    }

    [Fact]
    public void TryResolveDateRange_UsesLastContextWhenMetricDateRangeIsMissing()
    {
        var viewModel = CreateViewModel();
        viewModel.ChartState.LastContext = new ChartDataContext
        {
            From = new DateTime(2026, 3, 1),
            To = new DateTime(2026, 3, 31)
        };

        var resolved = BucketedSeriesInputPlanner.TryResolveDateRange(viewModel, out var from, out var to);

        Assert.True(resolved);
        Assert.Equal(new DateTime(2026, 3, 1), from);
        Assert.Equal(new DateTime(2026, 3, 31), to);
    }

    private static MainWindowViewModel CreateViewModel()
    {
        var chartState = new ChartState();
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        return new MainWindowViewModel(chartState, metricState, uiState, metricService);
    }
}
