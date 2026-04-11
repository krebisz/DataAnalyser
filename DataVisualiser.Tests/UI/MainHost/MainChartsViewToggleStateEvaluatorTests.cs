using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewToggleStateEvaluatorTests
{
    [Fact]
    public void CanTogglePrimaryCharts_ShouldRemainTrueWhenLoadedContextExists()
    {
        var context = CreateContext(includeSecondary: false);

        Assert.True(MainChartsViewToggleStateEvaluator.CanTogglePrimaryCharts(context));
    }

    [Fact]
    public void CanTogglePrimaryCharts_ShouldBeFalseWithoutLoadedContext()
    {
        Assert.False(MainChartsViewToggleStateEvaluator.CanTogglePrimaryCharts(null));
    }

    [Fact]
    public void CanToggleSecondaryCharts_ShouldDependOnLoadedSecondaryData()
    {
        var singleSeriesContext = CreateContext(includeSecondary: false);
        var twoSeriesContext = CreateContext(includeSecondary: true);

        Assert.False(MainChartsViewToggleStateEvaluator.CanToggleSecondaryCharts(singleSeriesContext));
        Assert.True(MainChartsViewToggleStateEvaluator.CanToggleSecondaryCharts(twoSeriesContext));
    }

    [Fact]
    public void CanUseStackedDisplay_ShouldPreferLoadedContextOverPendingSelectionCount()
    {
        var singleSeriesContext = CreateContext(includeSecondary: false);

        Assert.False(MainChartsViewToggleStateEvaluator.CanUseStackedDisplay(singleSeriesContext, selectedSubtypeCount: 3));
        Assert.True(MainChartsViewToggleStateEvaluator.CanUseStackedDisplay(null, selectedSubtypeCount: 2));
    }

    private static ChartDataContext CreateContext(bool includeSecondary)
    {
        return new ChartDataContext
        {
            Data1 =
            [
                new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }
            ],
            Data2 = includeSecondary
                ? [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }]
                : null
        };
    }
}
