using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.Controls;

public sealed class ChartVisibilityHelperTests
{
    [Fact]
    public void GetHiddenChartKeys_ReturnsNoneWhenAllVisible()
    {
        var state = new ChartState
        {
                IsMainVisible = true,
                IsNormalizedVisible = true,
                IsDiffRatioVisible = true,
                IsDistributionVisible = true,
                IsWeeklyTrendVisible = true,
                IsTransformPanelVisible = true,
                IsBarPieVisible = true,
                IsSyncfusionSunburstVisible = true
        };

        var hidden = ChartVisibilityHelper.GetHiddenChartKeys(state);

        Assert.Empty(hidden);
    }

    [Fact]
    public void GetHiddenChartKeys_TracksHiddenCharts()
    {
        var state = new ChartState
        {
                IsMainVisible = false,
                IsNormalizedVisible = true,
                IsDiffRatioVisible = false,
                IsDistributionVisible = true,
                IsWeeklyTrendVisible = false,
                IsTransformPanelVisible = false,
                IsBarPieVisible = false,
                IsSyncfusionSunburstVisible = false
        };

        var hidden = ChartVisibilityHelper.GetHiddenChartKeys(state);

        Assert.Contains(ChartControllerKeys.Main, hidden);
        Assert.DoesNotContain(ChartControllerKeys.Normalized, hidden);
        Assert.Contains(ChartControllerKeys.DiffRatio, hidden);
        Assert.DoesNotContain(ChartControllerKeys.Distribution, hidden);
        Assert.Contains(ChartControllerKeys.WeeklyTrend, hidden);
        Assert.Contains(ChartControllerKeys.Transform, hidden);
        Assert.Contains(ChartControllerKeys.BarPie, hidden);
        Assert.Contains(ChartControllerKeys.SyncfusionSunburst, hidden);
    }
}
