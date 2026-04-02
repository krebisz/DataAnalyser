using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewChartUpdateCoordinatorTests
{
    [Fact]
    public async Task TryHandleVisibilityOnlyToggleAsync_ShouldRefreshVisibleChart()
    {
        var rendered = new List<string>();
        var coordinator = new MainChartsViewChartUpdateCoordinator();
        var chartState = new ChartState { IsBarPieVisible = true };
        var args = new ChartUpdateRequestedEventArgs
        {
            IsVisibilityOnlyToggle = true,
            ToggledChartName = ChartControllerKeys.BarPie,
            ShowBarPie = true
        };

        var handled = await coordinator.TryHandleVisibilityOnlyToggleAsync(
            args,
            chartState,
            CreateContext(),
            CreateActions(renderChartAsync: (key, _) =>
            {
                rendered.Add(key);
                return Task.CompletedTask;
            }));

        Assert.True(handled);
        Assert.Contains(ChartControllerKeys.BarPie, rendered);
    }

    [Fact]
    public async Task TryHandleVisibilityOnlyToggleAsync_ShouldHandleTransformWithoutRender()
    {
        var transformHandled = false;
        var rendered = new List<string>();
        var coordinator = new MainChartsViewChartUpdateCoordinator();
        var args = new ChartUpdateRequestedEventArgs
        {
            IsVisibilityOnlyToggle = true,
            ToggledChartName = ChartControllerKeys.Transform,
            ShowTransformPanel = true
        };

        var handled = await coordinator.TryHandleVisibilityOnlyToggleAsync(
            args,
            new ChartState { IsTransformPanelVisible = true },
            CreateContext(),
            CreateActions(
                handleTransformVisibilityOnlyToggle: _ => transformHandled = true,
                renderChartAsync: (key, _) =>
                {
                    rendered.Add(key);
                    return Task.CompletedTask;
                }));

        Assert.True(handled);
        Assert.True(transformHandled);
        Assert.Empty(rendered);
    }

    [Fact]
    public async Task RenderVisibleChartsAsync_ShouldRenderVisibleChartsAndClearSecondaryOnlyChartsWithoutSecondaryData()
    {
        var rendered = new List<string>();
        var cleared = new List<string>();
        var coordinator = new MainChartsViewChartUpdateCoordinator();
        var chartState = new ChartState
        {
            IsMainVisible = true,
            IsNormalizedVisible = true,
            IsDiffRatioVisible = true,
            IsDistributionVisible = true,
            IsWeeklyTrendVisible = true,
            IsTransformPanelVisible = true,
            IsBarPieVisible = true
        };

        await coordinator.RenderVisibleChartsAsync(
            chartState,
            CreateContext(includeSecondary: false),
            CreateActions(
                renderChartAsync: (key, _) =>
                {
                    rendered.Add(key);
                    return Task.CompletedTask;
                },
                clearChart: cleared.Add));

        Assert.Equal(
            [ChartControllerKeys.Main, ChartControllerKeys.Distribution, ChartControllerKeys.WeeklyTrend, ChartControllerKeys.Transform, ChartControllerKeys.BarPie],
            rendered);
        Assert.Contains(ChartControllerKeys.Normalized, cleared);
        Assert.Contains(ChartControllerKeys.DiffRatio, cleared);
    }

    [Fact]
    public void ApplyAllChartVisibilities_ShouldApplyVisibilityAndSpecializedViewUpdates()
    {
        var updated = new List<string>();
        var coordinator = new MainChartsViewChartUpdateCoordinator();
        var args = new ChartUpdateRequestedEventArgs
        {
            ShowMain = true,
            ShowNormalized = true,
            ShowDiffRatio = false,
            ShowDistribution = true,
            ShowWeeklyTrend = true,
            ShowBarPie = false
        };

        coordinator.ApplyAllChartVisibilities(
            args,
            new ChartState { IsTransformPanelVisible = true },
            CreateActions(
                setChartVisibility: (key, isVisible) => updated.Add($"{key}:{isVisible}"),
                updateDistributionChartTypeVisibility: () => updated.Add("distribution-type"),
                updateWeekdayTrendChartTypeVisibility: () => updated.Add("weekday-type")));

        Assert.Contains($"{ChartControllerKeys.Main}:True", updated);
        Assert.Contains($"{ChartControllerKeys.Normalized}:True", updated);
        Assert.Contains($"{ChartControllerKeys.DiffRatio}:False", updated);
        Assert.Contains($"{ChartControllerKeys.Transform}:True", updated);
        Assert.Contains("distribution-type", updated);
        Assert.Contains("weekday-type", updated);
    }

    private static ChartDataContext CreateContext(bool includeSecondary = true)
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

    private static MainChartsViewChartUpdateActions CreateActions(
        Action<string, bool>? setChartVisibility = null,
        Action? updateDistributionChartTypeVisibility = null,
        Action? updateWeekdayTrendChartTypeVisibility = null,
        Action<ChartDataContext?>? handleTransformVisibilityOnlyToggle = null,
        Func<string, ChartDataContext, Task>? renderChartAsync = null,
        Action<string>? clearChart = null)
    {
        return new MainChartsViewChartUpdateActions(
            setChartVisibility ?? ((_, _) => { }),
            updateDistributionChartTypeVisibility ?? (() => { }),
            updateWeekdayTrendChartTypeVisibility ?? (() => { }),
            handleTransformVisibilityOnlyToggle ?? (_ => { }),
            renderChartAsync ?? ((_, _) => Task.CompletedTask),
            clearChart ?? (_ => { }));
    }
}
