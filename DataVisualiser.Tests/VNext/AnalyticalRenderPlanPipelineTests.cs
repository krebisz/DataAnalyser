using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.VNext;

public sealed class AnalyticalRenderPlanPipelineTests
{
    [Fact]
    public async Task BuildCartesianAsync_ShouldExecuteIntentAndProjectRenderPlan()
    {
        var pipeline = CreatePipeline();
        var selection = CreateSelection(seriesCount: 2);
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.Difference(),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Difference, "DiffRatioChart"));

        var result = await pipeline.BuildCartesianAsync(intent);

        Assert.Same(intent, result.Execution.Intent);
        Assert.Equal(ChartProgramKind.Difference, result.Execution.Program.Kind);
        Assert.Equal(ChartRenderPlanKind.Cartesian, result.RenderPlan.PlanKind);
        Assert.Equal(intent.Signature, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature]);
        Assert.Equal("DiffRatioChart", result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal(AnalyticalCapabilityKind.Comparison.ToString(), result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
    }

    [Fact]
    public async Task BuildCartesianAsync_ShouldApplyDensityPolicyBeforeProjection()
    {
        var pipeline = CreatePipeline(new RenderDensityPolicy(new RenderDensityPolicyOptions(
            FullFidelityPointThreshold: 1,
            OverviewTargetPointCount: 1)));
        var selection = CreateSelection(seriesCount: 1);
        var intent = AnalyticalIntent.FromRequests(selection, ChartProgramRequest.MainProgram());

        var result = await pipeline.BuildCartesianAsync(intent);

        Assert.Equal(ChartRenderDensityMode.AggregatedOverview, result.RenderPlan.Density.Mode);
        Assert.Equal(1, result.RenderPlan.Density.BucketCount);
        Assert.Single(result.RenderPlan.Series[0].RenderBuffer.Points);
    }

    [Fact]
    public async Task BuildHierarchyAsync_ShouldProjectHierarchyWithoutBackendTypes()
    {
        var pipeline = CreatePipeline();
        var selection = CreateSelection(seriesCount: 1);
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.SyncfusionSunburst(),
            ConsumerDeliveryContract.HierarchyChart(ChartProgramKind.SyncfusionSunburst, "Sunburst"));

        var result = await pipeline.BuildHierarchyAsync(
            intent,
            [new ChartHierarchyNodePlan("root", "All", 10, Array.Empty<ChartHierarchyNodePlan>(), new Dictionary<string, string>())]);

        Assert.Equal(ChartRenderPlanKind.Hierarchy, result.RenderPlan.PlanKind);
        Assert.Equal(ConsumerKind.HierarchyChart.ToString(), result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.ConsumerKind]);
        Assert.Equal("Sunburst", result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
    }

    private static AnalyticalRenderPlanPipeline CreatePipeline(RenderDensityPolicy? densityPolicy = null)
    {
        var engine = new ReasoningEngine(
            new LegacyMetricViewGateway(new StubMetricSeriesLoader()),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));

        return new AnalyticalRenderPlanPipeline(engine, densityPolicy);
    }

    private static MetricSelectionRequest CreateSelection(int seriesCount)
    {
        var series = Enumerable.Range(0, seriesCount)
            .Select(index => new MetricSeriesRequest("Weight", $"series-{index + 1}"))
            .ToArray();

        return new MetricSelectionRequest(
            "Weight",
            series,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 3),
            "HealthMetrics");
    }

    private sealed class StubMetricSeriesLoader : IMetricSeriesLoader
    {
        public Task<LoadedMetricSeries> LoadAsync(
            MetricSeriesRequest request,
            DateTime from,
            DateTime to,
            string resolutionTableName,
            CancellationToken cancellationToken = default)
        {
            var offset = request.QuerySubtype?.EndsWith("2", StringComparison.OrdinalIgnoreCase) == true ? 10 : 0;
            return Task.FromResult(new LoadedMetricSeries(
                [
                    new MetricData { NormalizedTimestamp = from, Value = 1m + offset },
                    new MetricData { NormalizedTimestamp = from.AddDays(1), Value = 2m + offset },
                    new MetricData { NormalizedTimestamp = to, Value = 3m + offset }
                ],
                null));
        }
    }
}
