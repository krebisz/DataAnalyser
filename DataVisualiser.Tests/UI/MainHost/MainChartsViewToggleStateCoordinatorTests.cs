using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewToggleStateCoordinatorTests
{
    [Fact]
    public void UpdatePrimaryChartToggles_ShouldSetPrimaryToggleStatesAndStackAvailability()
    {
        var coordinator = new MainChartsViewToggleStateCoordinator();
        var controllers = CreateControllers();

        coordinator.UpdatePrimaryChartToggles(
            CreateContext(includeSecondary: true),
            selectedSubtypeCount: 2,
            CreateActions(controllers));

        Assert.True(controllers[ChartControllerKeys.Main].ToggleEnabled);
        Assert.True(((FakeMainChartController)controllers[ChartControllerKeys.Main]).StackedAvailable);
        Assert.True(controllers[ChartControllerKeys.WeeklyTrend].ToggleEnabled);
        Assert.True(controllers[ChartControllerKeys.Distribution].ToggleEnabled);
        Assert.True(controllers[ChartControllerKeys.Transform].ToggleEnabled);
        Assert.True(controllers[ChartControllerKeys.BarPie].ToggleEnabled);
    }

    [Fact]
    public void UpdateSecondaryChartToggles_ShouldDisableAndHideSecondaryChartsWhenLoadedSecondaryMissing()
    {
        var coordinator = new MainChartsViewToggleStateCoordinator();
        var controllers = CreateControllers();
        var calls = new List<string>();

        coordinator.UpdateSecondaryChartToggles(
            CreateContext(includeSecondary: false),
            new MainChartsViewToggleStateCoordinator.Actions(
                key => controllers[key],
                key => calls.Add($"clear:{key}"),
                () => calls.Add("hide:normalized"),
                () => calls.Add("hide:diffratio")));

        Assert.False(controllers[ChartControllerKeys.Normalized].ToggleEnabled);
        Assert.False(controllers[ChartControllerKeys.DiffRatio].ToggleEnabled);
        Assert.Equal(
            [$"clear:{ChartControllerKeys.Normalized}", "hide:normalized", $"clear:{ChartControllerKeys.DiffRatio}", "hide:diffratio"],
            calls);
    }

    [Fact]
    public void UpdateSecondaryChartToggles_ShouldKeepVisibleChartsWhenLoadedSecondaryExists()
    {
        var coordinator = new MainChartsViewToggleStateCoordinator();
        var controllers = CreateControllers();
        var calls = new List<string>();

        coordinator.UpdateSecondaryChartToggles(
            CreateContext(includeSecondary: true),
            new MainChartsViewToggleStateCoordinator.Actions(
                key => controllers[key],
                key => calls.Add($"clear:{key}"),
                () => calls.Add("hide:normalized"),
                () => calls.Add("hide:diffratio")));

        Assert.True(controllers[ChartControllerKeys.Normalized].ToggleEnabled);
        Assert.True(controllers[ChartControllerKeys.DiffRatio].ToggleEnabled);
        Assert.Empty(calls);
    }

    private static MainChartsViewToggleStateCoordinator.Actions CreateActions(Dictionary<string, FakeChartController> controllers)
    {
        return new MainChartsViewToggleStateCoordinator.Actions(
            key => controllers[key],
            _ => { },
            () => { },
            () => { });
    }

    private static Dictionary<string, FakeChartController> CreateControllers()
    {
        return new Dictionary<string, FakeChartController>
        {
            [ChartControllerKeys.Main] = new FakeMainChartController(),
            [ChartControllerKeys.WeeklyTrend] = new FakeChartController(),
            [ChartControllerKeys.Distribution] = new FakeChartController(),
            [ChartControllerKeys.Transform] = new FakeChartController(),
            [ChartControllerKeys.BarPie] = new FakeChartController(),
            [ChartControllerKeys.Normalized] = new FakeChartController(),
            [ChartControllerKeys.DiffRatio] = new FakeChartController()
        };
    }

    private static ChartDataContext CreateContext(bool includeSecondary)
    {
        return new ChartDataContext
        {
            Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
            Data2 = includeSecondary ? [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }] : null
        };
    }

    private class FakeChartController : IChartController
    {
        public bool ToggleEnabled { get; private set; }
        public string Key { get; init; } = string.Empty;
        public bool RequiresPrimaryData => false;
        public bool RequiresSecondaryData => false;
        public void Initialize() { }
        public Task RenderAsync(ChartDataContext context) => Task.CompletedTask;
        public void Clear(ChartState state) { }
        public void ResetZoom() { }
        public bool HasSeries(ChartState state) => false;
        public void UpdateSubtypeOptions() { }
        public void ClearCache() { }
        public void SetVisible(bool isVisible) { }
        public void SetTitle(string? title) { }
        public void SetToggleEnabled(bool enabled) => ToggleEnabled = enabled;
    }

    private sealed class FakeMainChartController : FakeChartController, IMainChartControllerExtras
    {
        public bool StackedAvailable { get; private set; }
        public void SetStackedAvailability(bool isAvailable) => StackedAvailable = isAvailable;
        public void SyncDisplayModeSelection() { }
    }
}
