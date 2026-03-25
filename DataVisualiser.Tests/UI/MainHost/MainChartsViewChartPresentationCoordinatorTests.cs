using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewChartPresentationCoordinatorTests
{
    [Fact]
    public void ApplyDefaultTitles_ShouldAssignSharedDefaultTitles()
    {
        var titles = new Dictionary<string, string>();
        var coordinator = new MainChartsViewChartPresentationCoordinator();

        coordinator.ApplyDefaultTitles(CreateActions(
            setMainChartTitle: title => titles["main"] = title,
            setNormalizedChartTitle: title => titles["normalized"] = title,
            setDiffRatioChartTitle: title => titles["diff"] = title));

        Assert.Equal("Metrics: Total", titles["main"]);
        Assert.Equal("Metrics: Normalized", titles["normalized"]);
        Assert.Equal("Difference / Ratio", titles["diff"]);
    }

    [Fact]
    public void UpdateTitlesFromSelections_ShouldUpdateTitlesStateAndLabels()
    {
        var titles = new Dictionary<string, string>();
        var labels = new Dictionary<string, string>();
        var stateTitles = (left: string.Empty, right: string.Empty);
        var coordinator = new MainChartsViewChartPresentationCoordinator();
        var selections = new[]
        {
            new MetricSeriesSelection("Weight", "fat", "Weight", "Fat Mass"),
            new MetricSeriesSelection("Weight", "water", "Weight", "Total Body Water")
        };

        coordinator.UpdateTitlesFromSelections(
            selections,
            isDiffRatioDifferenceMode: false,
            CreateActions(
                setChartStateTitles: (left, right) => stateTitles = (left, right),
                setMainChartTitle: title => titles["main"] = title,
                setNormalizedChartTitle: title => titles["normalized"] = title,
                setDiffRatioChartTitle: title => titles["diff"] = title,
                updateMainChartLabel: label => labels["main"] = label,
                updateDiffRatioChartLabel: label => labels["diff"] = label));

        Assert.Equal(("Weight - Fat Mass", "Weight - Total Body Water"), stateTitles);
        Assert.Equal("Weight - Fat Mass vs. Weight - Total Body Water", titles["main"]);
        Assert.Equal("Weight - Fat Mass ~ Weight - Total Body Water", titles["normalized"]);
        Assert.Equal("Weight - Fat Mass / Weight - Total Body Water", titles["diff"]);
        Assert.Equal("Weight - Fat Mass vs Weight - Total Body Water", labels["main"]);
        Assert.Equal("Weight - Fat Mass / Weight - Total Body Water", labels["diff"]);
    }

    [Fact]
    public void ClearHiddenCharts_ShouldClearOnlyHiddenKeys()
    {
        var cleared = new List<string>();
        var coordinator = new MainChartsViewChartPresentationCoordinator();
        var state = new ChartState
        {
            IsMainVisible = true,
            IsNormalizedVisible = false,
            IsDiffRatioVisible = false,
            IsDistributionVisible = true,
            IsWeeklyTrendVisible = false,
            IsTransformPanelVisible = true,
            IsBarPieVisible = false,
            IsSyncfusionSunburstVisible = false
        };

        coordinator.ClearHiddenCharts(state, CreateActions(clearChart: cleared.Add));

        Assert.Contains(ChartControllerKeys.Normalized, cleared);
        Assert.Contains(ChartControllerKeys.DiffRatio, cleared);
        Assert.Contains(ChartControllerKeys.WeeklyTrend, cleared);
        Assert.Contains(ChartControllerKeys.BarPie, cleared);
        Assert.Contains(ChartControllerKeys.SyncfusionSunburst, cleared);
        Assert.DoesNotContain(ChartControllerKeys.Main, cleared);
        Assert.DoesNotContain(ChartControllerKeys.Distribution, cleared);
        Assert.DoesNotContain(ChartControllerKeys.Transform, cleared);
    }

    private static MainChartsViewChartPresentationActions CreateActions(
        Action<string, string>? setChartStateTitles = null,
        Action<string>? setMainChartTitle = null,
        Action<string>? setNormalizedChartTitle = null,
        Action<string>? setDiffRatioChartTitle = null,
        Action<string>? updateMainChartLabel = null,
        Action<string>? updateDiffRatioChartLabel = null,
        Action<string>? clearChart = null)
    {
        return new MainChartsViewChartPresentationActions(
            setChartStateTitles ?? ((_, _) => { }),
            setMainChartTitle ?? (_ => { }),
            setNormalizedChartTitle ?? (_ => { }),
            setDiffRatioChartTitle ?? (_ => { }),
            updateMainChartLabel ?? (_ => { }),
            updateDiffRatioChartLabel ?? (_ => { }),
            clearChart ?? (_ => { }));
    }
}
