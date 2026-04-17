using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public sealed class VNextChartRoutePolicyTests
{
    [Fact]
    public void ShouldUseMainFamilyPath_ShouldAllowMainOnly()
    {
        var state = new ChartState { IsMainVisible = true };

        Assert.True(VNextChartRoutePolicy.ShouldUseMainFamilyPath(state));
        Assert.True(VNextChartRoutePolicy.SupportsOnlyMainChart(state));
    }

    [Theory]
    [InlineData(nameof(ChartState.IsNormalizedVisible))]
    [InlineData(nameof(ChartState.IsDiffRatioVisible))]
    public void ShouldUseMainFamilyPath_ShouldAllowSecondaryComputedChartsButNotMarkMainOnly(string visibleProperty)
    {
        var state = new ChartState { IsMainVisible = true };
        typeof(ChartState).GetProperty(visibleProperty)!.SetValue(state, true);

        Assert.True(VNextChartRoutePolicy.ShouldUseMainFamilyPath(state));
        Assert.False(VNextChartRoutePolicy.SupportsOnlyMainChart(state));
    }

    [Theory]
    [InlineData(nameof(ChartState.IsDistributionVisible))]
    [InlineData(nameof(ChartState.IsWeeklyTrendVisible))]
    [InlineData(nameof(ChartState.IsTransformPanelVisible))]
    [InlineData(nameof(ChartState.IsBarPieVisible))]
    public void ShouldUseMainFamilyPath_ShouldRejectNonSlicedChartFamilies(string visibleProperty)
    {
        var state = new ChartState { IsMainVisible = true };
        typeof(ChartState).GetProperty(visibleProperty)!.SetValue(state, true);

        Assert.False(VNextChartRoutePolicy.ShouldUseMainFamilyPath(state));
    }
}
