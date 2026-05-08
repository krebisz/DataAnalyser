using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewToggleStateEvaluatorTests
{
    [Fact]
    public void CanTogglePrimaryCharts_ShouldRemainTrueWhenLoadedContextExists()
    {
        var snapshot = CreateSnapshot(includeSecondary: false);

        Assert.True(MainChartsViewToggleStateEvaluator.CanTogglePrimaryCharts(snapshot));
    }

    [Fact]
    public void CanTogglePrimaryCharts_ShouldBeFalseWithoutLoadedContext()
    {
        Assert.False(MainChartsViewToggleStateEvaluator.CanTogglePrimaryCharts(LoadedChartDataSnapshot.Empty));
    }

    [Fact]
    public void CanToggleSecondaryCharts_ShouldDependOnLoadedSecondaryData()
    {
        var singleSeriesSnapshot = CreateSnapshot(includeSecondary: false);
        var twoSeriesSnapshot = CreateSnapshot(includeSecondary: true);

        Assert.False(MainChartsViewToggleStateEvaluator.CanToggleSecondaryCharts(singleSeriesSnapshot));
        Assert.True(MainChartsViewToggleStateEvaluator.CanToggleSecondaryCharts(twoSeriesSnapshot));
    }

    [Fact]
    public void CanUseStackedDisplay_ShouldPreferLoadedContextOverPendingSelectionCount()
    {
        var singleSeriesSnapshot = CreateSnapshot(includeSecondary: false);

        Assert.False(MainChartsViewToggleStateEvaluator.CanUseStackedDisplay(singleSeriesSnapshot, selectedSubtypeCount: 3));
        Assert.True(MainChartsViewToggleStateEvaluator.CanUseStackedDisplay(LoadedChartDataSnapshot.Empty, selectedSubtypeCount: 2));
    }

    private static LoadedChartDataSnapshot CreateSnapshot(bool includeSecondary)
    {
        return LoadedChartDataSnapshot.FromContext(new ChartDataContext
        {
            Data1 =
            [
                new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }
            ],
            Data2 = includeSecondary
                ? [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }]
                : null
        });
    }
}
