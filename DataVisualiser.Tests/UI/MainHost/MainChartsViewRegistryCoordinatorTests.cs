using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewRegistryCoordinatorTests
{
    [Fact]
    public void ResolveControllers_ShouldPreferRegisteredControllers()
    {
        var coordinator = new MainChartsViewRegistryCoordinator();
        var registered = new List<IChartController>
        {
            new FakeChartController(),
            new FakeChartController()
        };

        var result = coordinator.ResolveControllers(
            new MainChartsViewRegistryCoordinator.Actions(
                () => registered,
                _ => throw new InvalidOperationException("Fallback resolution should not be used.")));

        Assert.Same(registered, result);
    }

    [Fact]
    public void ResolveControllers_ShouldFallbackToAllKnownKeysWhenRegistryMissing()
    {
        var coordinator = new MainChartsViewRegistryCoordinator();
        var calls = new List<string>();

        var result = coordinator.ResolveControllers(
            new MainChartsViewRegistryCoordinator.Actions(
                () => null,
                key =>
                {
                    calls.Add(key);
                    return new FakeChartController();
                }));

        Assert.Equal(ChartControllerKeys.All, calls);
        Assert.Equal(ChartControllerKeys.All.Length, result.Count);
    }

    [Fact]
    public void ClearRegisteredCharts_ShouldClearEveryResolvedController()
    {
        var coordinator = new MainChartsViewRegistryCoordinator();
        var chartState = new ChartState();
        var first = new FakeChartController();
        var second = new FakeChartController();

        coordinator.ClearRegisteredCharts(
            chartState,
            new MainChartsViewRegistryCoordinator.Actions(
                () => [first, second],
                _ => throw new InvalidOperationException("Fallback resolution should not be used.")));

        Assert.Same(chartState, first.LastClearedState);
        Assert.Same(chartState, second.LastClearedState);
    }

    private sealed class FakeChartController : IChartController
    {
        public ChartState? LastClearedState { get; private set; }

        public string Key { get; init; } = string.Empty;
        public bool RequiresPrimaryData => false;
        public bool RequiresSecondaryData => false;
        public void Initialize() { }
        public Task RenderAsync(ChartDataContext context) => Task.CompletedTask;
        public void Clear(ChartState state) => LastClearedState = state;
        public void ResetZoom() { }
        public bool HasSeries(ChartState state) => false;
        public void UpdateSubtypeOptions() { }
        public void ClearCache() { }
        public void SetVisible(bool isVisible) { }
        public void SetTitle(string? title) { }
        public void SetToggleEnabled(bool enabled) { }
    }
}
