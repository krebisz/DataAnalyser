using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public sealed class ChartLoadPolicyTests
{
    [Fact]
    public void ShouldUseMainFamilyPath_ShouldAllowMainOnly()
    {
        var state = new ChartState { IsMainVisible = true };

        Assert.True(ChartLoadPolicy.ShouldUseMainFamilyPath(state));
        Assert.True(ChartLoadPolicy.SupportsOnlyMainChart(state));
    }

    [Theory]
    [InlineData(nameof(ChartState.IsNormalizedVisible))]
    [InlineData(nameof(ChartState.IsDiffRatioVisible))]
    public void ShouldUseMainFamilyPath_ShouldAllowSecondaryComputedChartsButNotMarkMainOnly(string visibleProperty)
    {
        var state = new ChartState { IsMainVisible = true };
        typeof(ChartState).GetProperty(visibleProperty)!.SetValue(state, true);

        Assert.True(ChartLoadPolicy.ShouldUseMainFamilyPath(state));
        Assert.False(ChartLoadPolicy.SupportsOnlyMainChart(state));
    }

    [Theory]
    [InlineData(nameof(ChartState.IsDistributionVisible))]
    [InlineData(nameof(ChartState.IsWeeklyTrendVisible))]
    [InlineData(nameof(ChartState.IsBarPieVisible))]
    public void ShouldUseMainFamilyPath_ShouldAllowIdentityStyleChartFamilies(string visibleProperty)
    {
        var state = new ChartState { IsMainVisible = true };
        typeof(ChartState).GetProperty(visibleProperty)!.SetValue(state, true);

        Assert.True(ChartLoadPolicy.ShouldUseMainFamilyPath(state));
    }

    [Fact]
    public void ShouldUseMainFamilyPath_ShouldRejectTransformUntilOperationsCanBePassed()
    {
        var state = new ChartState
        {
            IsMainVisible = true,
            IsTransformPanelVisible = true
        };

        Assert.False(ChartLoadPolicy.ShouldUseMainFamilyPath(state));
    }
}
