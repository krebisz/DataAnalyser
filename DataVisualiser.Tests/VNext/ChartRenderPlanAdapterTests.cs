using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.VNext;

public sealed class ChartRenderPlanAdapterTests
{
    [Fact]
    public async Task Dispatcher_ShouldRouteCartesianPlanToCartesianAdapter()
    {
        var surface = new FakeRenderSurface();
        var plan = CreateCartesianPlan();
        var dispatcher = new ChartRenderPlanAdapterDispatcher<FakeRenderSurface>(
            [
                new FakeHierarchyAdapter(),
                new FakeCartesianAdapter()
            ]);

        var result = await dispatcher.ApplyAsync(surface, plan);

        Assert.Equal("LiveChartsWpf", result.BackendKey);
        Assert.Equal(ChartRenderPlanKind.Cartesian, result.PlanKind);
        Assert.Equal(2, result.RenderedSeriesCount);
        Assert.Equal(4, result.RenderedPointCount);
        Assert.Equal("LiveChartsWpf:Main:sig-1", surface.LastAppliedPlan);
    }

    [Fact]
    public async Task Dispatcher_ShouldRouteHierarchyPlanToHierarchyAdapter()
    {
        var surface = new FakeRenderSurface();
        var plan = CreateHierarchyPlan();
        var dispatcher = new ChartRenderPlanAdapterDispatcher<FakeRenderSurface>(
            [
                new FakeCartesianAdapter(),
                new FakeHierarchyAdapter()
            ]);

        var result = await dispatcher.ApplyAsync(surface, plan);

        Assert.Equal("SyncfusionSunburst", result.BackendKey);
        Assert.Equal(ChartRenderPlanKind.Hierarchy, result.PlanKind);
        Assert.Equal(2, result.RenderedHierarchyNodeCount);
        Assert.Equal("SyncfusionSunburst:BarPie:sig-1", surface.LastAppliedPlan);
    }

    [Fact]
    public async Task Dispatcher_ShouldFailWhenNoAdapterCanRenderPlan()
    {
        var surface = new FakeRenderSurface();
        var plan = CreateHierarchyPlan();
        var dispatcher = new ChartRenderPlanAdapterDispatcher<FakeRenderSurface>(
            [new FakeCartesianAdapter()]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await dispatcher.ApplyAsync(surface, plan));

        Assert.Contains("Hierarchy", ex.Message);
    }

    [Fact]
    public void AdapterCanRender_ShouldUseBackendCapabilities()
    {
        var cartesian = new FakeCartesianAdapter();
        var hierarchy = new FakeHierarchyAdapter();

        Assert.True(cartesian.CanRender(CreateCartesianPlan()));
        Assert.False(cartesian.CanRender(CreateHierarchyPlan()));
        Assert.True(hierarchy.CanRender(CreateHierarchyPlan()));
        Assert.False(hierarchy.CanRender(CreateCartesianPlan()));
    }

    private static ChartRenderPlan CreateCartesianPlan()
    {
        var program = CreateProgram(ChartProgramKind.Main);
        return new ChartRenderPlanProjector().ProjectCartesian(program);
    }

    private static ChartRenderPlan CreateHierarchyPlan()
    {
        var program = CreateProgram(ChartProgramKind.BarPie);
        return new ChartRenderPlanProjector().ProjectHierarchy(
            program,
            [
                new ChartHierarchyNodePlan(
                    "root",
                    "All",
                    10,
                    [
                        new ChartHierarchyNodePlan(
                            "child",
                            "Morning",
                            6,
                            Array.Empty<ChartHierarchyNodePlan>(),
                            new Dictionary<string, string>())
                    ],
                    new Dictionary<string, string>())
            ]);
    }

    private static ChartProgram CreateProgram(ChartProgramKind kind)
    {
        return new ChartProgram(
            kind,
            ChartDisplayMode.Regular,
            "Weight",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [
                new ChartSeriesProgram("morning", "Morning", [1d, 2d], [1d, 2d]),
                new ChartSeriesProgram("evening", "Evening", [3d, 4d], [3d, 4d])
            ],
            "sig-1");
    }

    private sealed class FakeRenderSurface
    {
        public string? LastAppliedPlan { get; set; }
    }

    private sealed class FakeCartesianAdapter : IChartRenderPlanAdapter<FakeRenderSurface>
    {
        public ChartBackendCapabilities Capabilities => ChartBackendCapabilities.LiveChartsWpf;

        public bool CanRender(ChartRenderPlan plan) => Capabilities.Supports(plan.PlanKind);

        public ValueTask<ChartRenderAdapterResult> ApplyAsync(
            FakeRenderSurface surface,
            ChartRenderPlan plan,
            CancellationToken cancellationToken = default)
        {
            surface.LastAppliedPlan = $"{Capabilities.BackendKey}:{plan.Id}";
            return ValueTask.FromResult(new ChartRenderAdapterResult(
                Capabilities.BackendKey,
                plan.Id,
                plan.PlanKind,
                plan.Density.Mode,
                plan.Series.Count,
                0,
                plan.Series.Sum(series => series.RenderBuffer.RenderedPointCount),
                new Dictionary<string, string>()));
        }
    }

    private sealed class FakeHierarchyAdapter : IChartRenderPlanAdapter<FakeRenderSurface>
    {
        public ChartBackendCapabilities Capabilities => ChartBackendCapabilities.SyncfusionSunburst;

        public bool CanRender(ChartRenderPlan plan) => Capabilities.Supports(plan.PlanKind);

        public ValueTask<ChartRenderAdapterResult> ApplyAsync(
            FakeRenderSurface surface,
            ChartRenderPlan plan,
            CancellationToken cancellationToken = default)
        {
            surface.LastAppliedPlan = $"{Capabilities.BackendKey}:{plan.Id}";
            return ValueTask.FromResult(new ChartRenderAdapterResult(
                Capabilities.BackendKey,
                plan.Id,
                plan.PlanKind,
                plan.Density.Mode,
                0,
                CountNodes(plan.HierarchyRoots),
                plan.Density.RenderedPointCount,
                new Dictionary<string, string>()));
        }

        private static int CountNodes(IReadOnlyList<ChartHierarchyNodePlan> nodes)
        {
            var count = 0;
            foreach (var node in nodes)
            {
                count++;
                count += CountNodes(node.Children);
            }

            return count;
        }
    }
}
