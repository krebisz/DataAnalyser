using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.VNext;

public sealed class ChartRenderPlanProjectorTests
{
    [Fact]
    public void ProjectCartesian_ShouldPreserveProgramIdentityAndSeriesShape()
    {
        var program = CreateProgram(ChartProgramKind.Main);
        var projector = new ChartRenderPlanProjector();

        var plan = projector.ProjectCartesian(program);

        Assert.Equal(ChartRenderPlanKind.Cartesian, plan.PlanKind);
        Assert.Equal(ChartProgramKind.Main, plan.ProgramKind);
        Assert.Equal(ChartDisplayMode.Regular, plan.DisplayMode);
        Assert.Equal("sig-1", plan.SourceSignature);
        Assert.Equal("Main:sig-1", plan.Id);
        Assert.Equal(2, plan.Series.Count);
        Assert.Empty(plan.HierarchyRoots);
        Assert.Equal(ChartRenderDensityMode.FullFidelity, plan.Density.Mode);
        Assert.Equal(4, plan.Density.SourcePointCount);
        Assert.Equal(4, plan.Density.RenderedPointCount);
        Assert.True(plan.Interaction.SupportsZoom);
        Assert.Equal("Morning", plan.Series[0].Label);
        Assert.Equal([1d, 2d], plan.Series[0].RawValues);
        Assert.Equal([1.1d, 1.9d], plan.Series[0].SmoothedValues);
        Assert.Equal(2, plan.Series[0].SourcePointCount);
        Assert.Equal(2, plan.Series[0].RenderedPointCount);
    }

    [Fact]
    public void ProjectCartesian_ShouldAllowDensityPolicyToBeProvidedBeforeBackendBinding()
    {
        var program = CreateProgram(ChartProgramKind.Main);
        var density = new RenderDensityPlan(
            ChartRenderDensityMode.AggregatedOverview,
            SourcePointCount: 100_000,
            RenderedPointCount: 2_000,
            BucketCount: 2_000,
            Viewport: new ChartViewport(program.From, program.To));
        var projector = new ChartRenderPlanProjector();

        var plan = projector.ProjectCartesian(program, density);

        Assert.Equal(ChartRenderDensityMode.AggregatedOverview, plan.Density.Mode);
        Assert.Equal(4, plan.Density.RenderedPointCount);
        Assert.Equal(2_000, plan.Density.BucketCount);
        Assert.Equal(program.From, plan.Density.Viewport?.From);
    }

    [Fact]
    public void ProjectCartesian_WithAggregatedDensity_ShouldEmitNeutralRenderBuffers()
    {
        var program = CreateLargeProgram(ChartProgramKind.Transform, pointCount: 10);
        var density = new RenderDensityPlan(
            ChartRenderDensityMode.AggregatedOverview,
            SourcePointCount: 10,
            RenderedPointCount: 3,
            BucketCount: 3);
        var projector = new ChartRenderPlanProjector();

        var plan = projector.ProjectCartesian(program, density);

        Assert.Single(plan.Series);
        Assert.Equal(ChartRenderDensityMode.AggregatedOverview, plan.Density.Mode);
        Assert.Equal(3, plan.Series[0].RenderBuffer.Points.Count);
        Assert.Equal(10, plan.Series[0].RenderBuffer.SourcePointCount);
        Assert.Equal(3, plan.Series[0].RenderBuffer.RenderedPointCount);
        Assert.Equal(2d, plan.Series[0].RenderBuffer.Points[0].Y);
        Assert.Equal(1d, plan.Series[0].RenderBuffer.Points[0].Minimum);
        Assert.Equal(3d, plan.Series[0].RenderBuffer.Points[0].Maximum);
    }

    [Fact]
    public void RenderDensityPolicy_ShouldKeepSmallProgramsFullFidelity()
    {
        var program = CreateLargeProgram(ChartProgramKind.Main, pointCount: 4);
        var policy = new RenderDensityPolicy(new RenderDensityPolicyOptions(
            FullFidelityPointThreshold: 5,
            OverviewTargetPointCount: 2));

        var density = policy.Resolve(program);

        Assert.Equal(ChartRenderDensityMode.FullFidelity, density.Mode);
        Assert.Equal(4, density.SourcePointCount);
        Assert.Equal(4, density.RenderedPointCount);
    }

    [Fact]
    public void RenderDensityPolicy_ShouldAggregateLargeProgramsBeforeBackendBinding()
    {
        var program = CreateLargeProgram(ChartProgramKind.Normalized, pointCount: 10);
        var policy = new RenderDensityPolicy(new RenderDensityPolicyOptions(
            FullFidelityPointThreshold: 5,
            OverviewTargetPointCount: 3));

        var density = policy.Resolve(program);

        Assert.Equal(ChartRenderDensityMode.AggregatedOverview, density.Mode);
        Assert.Equal(10, density.SourcePointCount);
        Assert.Equal(3, density.RenderedPointCount);
        Assert.Equal(3, density.BucketCount);
    }

    [Fact]
    public void RenderDensityPolicy_ShouldEmitViewportRefinedIntentWhenBackendSupportsIt()
    {
        var program = CreateLargeProgram(ChartProgramKind.Main, pointCount: 10);
        var policy = new RenderDensityPolicy(new RenderDensityPolicyOptions(
            FullFidelityPointThreshold: 5,
            OverviewTargetPointCount: 3));
        var backend = ChartBackendCapabilities.LiveChartsWpf with { SupportsViewportRefinement = true };
        var viewport = new ChartViewport(program.From.AddDays(2), program.From.AddDays(5));

        var density = policy.Resolve(program, viewport, backend);

        Assert.Equal(ChartRenderDensityMode.ViewportRefined, density.Mode);
        Assert.Equal(viewport, density.Viewport);
    }

    [Fact]
    public void ProjectHierarchy_ShouldRepresentSunburstStyleDataWithoutSyncfusionTypes()
    {
        var program = CreateProgram(ChartProgramKind.BarPie);
        var roots = new[]
        {
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
                        new Dictionary<string, string> { ["Subtype"] = "morning" })
                ],
                new Dictionary<string, string> { ["Metric"] = "Weight" })
        };
        var projector = new ChartRenderPlanProjector();

        var plan = projector.ProjectHierarchy(program, roots);

        Assert.Equal(ChartRenderPlanKind.Hierarchy, plan.PlanKind);
        Assert.Empty(plan.Series);
        Assert.Single(plan.HierarchyRoots);
        Assert.Equal("All", plan.HierarchyRoots[0].Label);
        Assert.Equal("Morning", plan.HierarchyRoots[0].Children[0].Label);
        Assert.Equal(2, plan.Density.RenderedPointCount);
        Assert.False(plan.Interaction.SupportsZoom);
        Assert.True(plan.Interaction.SupportsTooltips);
    }

    [Fact]
    public void BackendCapabilities_ShouldDistinguishLiveChartsAndSyncfusionPlanSupport()
    {
        var liveCharts = ChartBackendCapabilities.LiveChartsWpf;
        var syncfusion = ChartBackendCapabilities.SyncfusionSunburst;

        Assert.True(liveCharts.Supports(ChartRenderPlanKind.Cartesian));
        Assert.True(liveCharts.Supports(ChartRenderPlanKind.Faceted));
        Assert.False(liveCharts.Supports(ChartRenderPlanKind.Hierarchy));
        Assert.True(syncfusion.Supports(ChartRenderPlanKind.Hierarchy));
        Assert.False(syncfusion.Supports(ChartRenderPlanKind.Cartesian));
    }

    [Fact]
    public void BackendSelector_ShouldChooseFirstBackendThatSupportsPlanKind()
    {
        var plan = new ChartRenderPlanProjector().ProjectHierarchy(
            CreateProgram(ChartProgramKind.BarPie),
            [
                new ChartHierarchyNodePlan(
                    "root",
                    "All",
                    10,
                    Array.Empty<ChartHierarchyNodePlan>(),
                    new Dictionary<string, string>())
            ]);
        var selector = new ChartBackendSelector();

        var selected = selector.Select(
            plan,
            [
                ChartBackendCapabilities.LiveChartsWpf,
                ChartBackendCapabilities.SyncfusionSunburst
            ]);

        Assert.Equal("SyncfusionSunburst", selected.BackendKey);
    }

    [Fact]
    public void BackendSelector_ShouldFailWhenNoBackendSupportsPlanKind()
    {
        var plan = new ChartRenderPlanProjector().ProjectHierarchy(
            CreateProgram(ChartProgramKind.BarPie),
            [
                new ChartHierarchyNodePlan(
                    "root",
                    "All",
                    10,
                    Array.Empty<ChartHierarchyNodePlan>(),
                    new Dictionary<string, string>())
            ]);
        var selector = new ChartBackendSelector();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            selector.Select(plan, [ChartBackendCapabilities.LiveChartsWpf]));

        Assert.Contains("Hierarchy", ex.Message);
    }

    [Theory]
    [InlineData(ChartProgramKind.Main)]
    [InlineData(ChartProgramKind.Normalized)]
    [InlineData(ChartProgramKind.Difference)]
    [InlineData(ChartProgramKind.Ratio)]
    [InlineData(ChartProgramKind.Transform)]
    [InlineData(ChartProgramKind.Distribution)]
    [InlineData(ChartProgramKind.WeekdayTrend)]
    [InlineData(ChartProgramKind.BarPie)]
    public void ProjectCartesian_ShouldAcceptEveryCurrentChartFamilyWithoutUiBackendTypes(ChartProgramKind kind)
    {
        var program = CreateProgram(kind);
        var projector = new ChartRenderPlanProjector();

        var plan = projector.ProjectCartesian(program);

        Assert.Equal(kind, plan.ProgramKind);
        Assert.Equal(ChartRenderPlanKind.Cartesian, plan.PlanKind);
        Assert.All(plan.Series, series => Assert.NotNull(series.RenderBuffer));
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
                new ChartSeriesProgram("morning", "Morning", [1d, 2d], [1.1d, 1.9d]),
                new ChartSeriesProgram("evening", "Evening", [3d, 4d], [3.1d, 3.9d])
            ],
            "sig-1");
    }

    private static ChartProgram CreateLargeProgram(ChartProgramKind kind, int pointCount)
    {
        var from = new DateTime(2026, 1, 1);
        var timeline = Enumerable.Range(0, pointCount)
            .Select(offset => from.AddDays(offset))
            .ToArray();
        var values = Enumerable.Range(1, pointCount)
            .Select(value => (double)value)
            .ToArray();

        return new ChartProgram(
            kind,
            ChartDisplayMode.Regular,
            "Weight",
            from,
            from.AddDays(pointCount - 1),
            timeline,
            [new ChartSeriesProgram("series", "Series", values, values)],
            "sig-large");
    }
}
