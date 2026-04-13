using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewSelectionCoordinatorTests
{
    [Fact]
    public void UpdateSelectedSubtypes_ShouldRefreshPeerChartsAndToggleStates()
    {
        var coordinator = new MainChartsViewSelectionCoordinator();
        IReadOnlyList<MetricSeriesSelection>? appliedSelections = null;
        var subtypeOptions = new List<string>();
        var primaryCounts = new List<int>();
        var secondaryCounts = new List<int>();
        var transformUpdated = 0;

        coordinator.UpdateSelectedSubtypes(
            new[]
            {
                new MetricSeriesSelection("Weight", "body_mass", "Weight:body_mass"),
                new MetricSeriesSelection("Weight", "fat_free_mass", "Weight:fat_free_mass")
            },
            new MainChartsViewSelectionCoordinator.Actions(
                selections => appliedSelections = selections,
                key => subtypeOptions.Add(key),
                () => transformUpdated++,
                count => primaryCounts.Add(count),
                count => secondaryCounts.Add(count),
                () => { },
                () => Task.CompletedTask,
                () => Task.CompletedTask));

        Assert.NotNull(appliedSelections);
        Assert.Equal(
            [ChartControllerKeys.Normalized, ChartControllerKeys.DiffRatio, ChartControllerKeys.Distribution, ChartControllerKeys.WeeklyTrend, ChartControllerKeys.Main],
            subtypeOptions);
        Assert.Equal([2], primaryCounts);
        Assert.Equal([2], secondaryCounts);
        Assert.Equal(1, transformUpdated);
    }

    [Fact]
    public async Task HandleSubtypeSelectionChangedAsync_ShouldRenderLoadedChartsWhenSelectionMatchesContext()
    {
        var coordinator = new MainChartsViewSelectionCoordinator();
        var calls = new List<string>();

        await coordinator.HandleSubtypeSelectionChangedAsync(
            hasLoadedData: true,
            shouldRefreshDateRangeForCurrentSelection: true,
            new MainChartsViewSelectionCoordinator.Actions(
                _ => { },
                _ => { },
                () => { },
                _ => { },
                _ => { },
                () => calls.Add("titles"),
                () =>
                {
                    calls.Add("render");
                    return Task.CompletedTask;
                },
                () =>
                {
                    calls.Add("dateRange");
                    return Task.CompletedTask;
                }));

        Assert.Equal(["titles", "render"], calls);
    }

    [Fact]
    public async Task HandleSubtypeSelectionChangedAsync_ShouldRefreshDateRangeWhenLoadedContextIsNotReusable()
    {
        var coordinator = new MainChartsViewSelectionCoordinator();
        var calls = new List<string>();

        await coordinator.HandleSubtypeSelectionChangedAsync(
            hasLoadedData: false,
            shouldRefreshDateRangeForCurrentSelection: true,
            new MainChartsViewSelectionCoordinator.Actions(
                _ => { },
                _ => { },
                () => { },
                _ => { },
                _ => { },
                () => calls.Add("titles"),
                () =>
                {
                    calls.Add("render");
                    return Task.CompletedTask;
                },
                () =>
                {
                    calls.Add("dateRange");
                    return Task.CompletedTask;
                }));

        Assert.Equal(["dateRange"], calls);
    }
}
