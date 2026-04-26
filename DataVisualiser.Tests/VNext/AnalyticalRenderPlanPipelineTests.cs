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
        Assert.Equal(ConsumerProviderContracts.LiveChartsWpf.ProviderKey, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.ProviderKey]);
        Assert.Equal(ConsumerProviderContracts.LiveChartsWpf.DisplayName, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.ProviderDisplayName]);
        Assert.Equal(ConsumerProviderContracts.LiveChartsWpf.Signature, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.ProviderSignature]);
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
        Assert.Equal(ConsumerProviderContracts.SyncfusionSunburst.ProviderKey, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.ProviderKey]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSupportExportConsumerWithoutRenderPlan()
    {
        var pipeline = CreatePipeline();
        var selection = CreateSelection(seriesCount: 1);
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.MainProgram(),
            ConsumerDeliveryContract.Export(ChartProgramKind.Main));

        var result = await pipeline.ExecuteAsync(intent);

        Assert.Equal(ConsumerKind.Export, result.Intent.Delivery.ConsumerKind);
        Assert.False(result.Intent.Delivery.RequiresRenderPlan);
        Assert.Equal(ChartProgramKind.Main, result.Program.Kind);
        Assert.Equal(selection.Signature, result.Snapshot.Signature);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectNonRenderingConsumerWithoutProvider()
    {
        var registry = new ConsumerProviderRegistry([ConsumerProviderContracts.LiveChartsWpf]);
        var pipeline = CreatePipeline(providerRegistry: registry);
        var selection = CreateSelection(seriesCount: 1);
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.MainProgram(),
            ConsumerDeliveryContract.Export(ChartProgramKind.Main));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.ExecuteAsync(intent));

        Assert.Contains("No consumer provider supports", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Export", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BuildCartesianAsync_ShouldRejectConsumersThatDoNotRequireRenderPlans()
    {
        var pipeline = CreatePipeline();
        var selection = CreateSelection(seriesCount: 1);
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.MainProgram(),
            ConsumerDeliveryContract.Api(ChartProgramKind.Main));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.BuildCartesianAsync(intent));

        Assert.Contains("does not require a render plan", ex.Message, StringComparison.Ordinal);
        Assert.Contains("ExecuteAsync", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BuildCartesianAsync_ShouldRejectDeliveryWithoutProvider()
    {
        var registry = new ConsumerProviderRegistry([ConsumerProviderContracts.SyncfusionSunburst]);
        var pipeline = CreatePipeline(providerRegistry: registry);
        var selection = CreateSelection(seriesCount: 1);
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.MainProgram(),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pipeline.BuildCartesianAsync(intent));

        Assert.Contains("No consumer provider supports", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Main", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Cartesian", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BuildCartesianSetAsync_ShouldExecuteAndProjectMultipleProgramsForOneSelection()
    {
        var pipeline = CreatePipeline();
        var selection = CreateSelection(seriesCount: 2);
        var intents = new[]
        {
            AnalyticalIntent.FromRequests(
                selection,
                ChartProgramRequest.MainProgram(),
                ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart")),
            AnalyticalIntent.FromRequests(
                selection,
                ChartProgramRequest.Normalized(),
                ConsumerDeliveryContract.Chart(ChartProgramKind.Normalized, "NormalizedChart"))
        };

        var result = await pipeline.BuildCartesianSetAsync(AnalyticalIntentSet.FromIntents(intents));

        Assert.Equal(selection.Signature, result.ExecutionSet.Selection.Signature);
        Assert.Equal([ChartProgramKind.Main, ChartProgramKind.Normalized], result.ExecutionSet.ProgramKinds);
        Assert.Equal(2, result.RenderPlans.Count);
        Assert.Equal("MainChart", result.RenderPlans[0].Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal("NormalizedChart", result.RenderPlans[1].Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.All(result.RenderPlans, plan =>
            Assert.Equal(ConsumerProviderContracts.LiveChartsWpf.ProviderKey, plan.Metadata[ChartRenderPlanMetadataKeys.ProviderKey]));
    }

    [Fact]
    public void BuildCartesianSetAsync_ShouldRejectMixedSelections()
    {
        var firstSelection = CreateSelection(seriesCount: 1);
        var secondSelection = new MetricSelectionRequest(
            "Sleep",
            [new MetricSeriesRequest("Sleep", "total")],
            firstSelection.From,
            firstSelection.To,
            firstSelection.ResolutionTableName);
        var intents = new[]
        {
            AnalyticalIntent.FromRequests(firstSelection, ChartProgramRequest.MainProgram()),
            AnalyticalIntent.FromRequests(secondSelection, ChartProgramRequest.MainProgram())
        };

        var ex = Assert.Throws<ArgumentException>(() => AnalyticalIntentSet.FromIntents(intents));

        Assert.Contains("share the intent-set selection", ex.Message, StringComparison.Ordinal);
    }

    private static AnalyticalRenderPlanPipeline CreatePipeline(
        RenderDensityPolicy? densityPolicy = null,
        ConsumerProviderRegistry? providerRegistry = null)
    {
        var engine = new ReasoningEngine(
            new LegacyMetricViewGateway(new StubMetricSeriesLoader()),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));

        return new AnalyticalRenderPlanPipeline(engine, densityPolicy, providerRegistry: providerRegistry);
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
