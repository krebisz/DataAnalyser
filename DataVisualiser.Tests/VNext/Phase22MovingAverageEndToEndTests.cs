using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;
using DataVisualiser.VNext.Rendering;
using DataVisualiser.VNext.Rendering.MovingAverage;

namespace DataVisualiser.Tests.VNext;

public sealed class Phase22MovingAverageEndToEndTests
{
    // ── Chart consumer (TabularSummary backend) ─────────────────────────────

    [Fact]
    public async Task MovingAverage_ChartConsumer_ShouldProduceCartesianRenderPlan_WithTabularSummaryBackend()
    {
        var pipeline = CreatePipeline();
        var intent = CreateMovingAverageIntent(
            ConsumerDeliveryContract.Chart(ChartProgramKind.MovingAverage, "MovingAverageChart"),
            windowSize: 2);

        var result = await pipeline.BuildCartesianAsync(
            intent,
            new ChartBackendCandidateSet([ChartBackendCapabilities.TabularSummary]));

        Assert.Equal(ChartProgramKind.MovingAverage, result.Execution.Program.Kind);
        Assert.Equal(ChartRenderPlanKind.Cartesian, result.RenderPlan.PlanKind);
        Assert.Equal("MovingAverageChart", result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal(AnalyticalCapabilityKind.Smoothing.ToString(), result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        Assert.Equal(CompositionKind.DerivedSeries.ToString(), result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
        Assert.Equal(ConsumerProviderContracts.TabularSummaryChart.ProviderKey, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.ProviderKey]);
        Assert.Equal(ChartBackendCapabilities.TabularSummary.BackendKey, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.BackendKey]);
        Assert.Equal("Tabular Summary", result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.BackendDisplayName]);
        Assert.True(result.RenderPlan.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.IntentSignature));
        Assert.True(result.RenderPlan.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.ProvenanceSignature));
    }

    [Fact]
    public async Task MovingAverage_ChartConsumer_ShouldResolveTabularSummaryProviderWithoutExplicitBackend()
    {
        var pipeline = CreatePipeline();
        var intent = CreateMovingAverageIntent(
            ConsumerDeliveryContract.Chart(ChartProgramKind.MovingAverage, "MovingAverageChart"),
            windowSize: 3);

        var result = await pipeline.BuildCartesianAsync(intent);

        Assert.Equal(ChartRenderPlanKind.Cartesian, result.RenderPlan.PlanKind);
        Assert.Equal(ConsumerProviderContracts.TabularSummaryChart.ProviderKey, result.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.ProviderKey]);
        Assert.False(result.RenderPlan.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.BackendKey));
    }

    // ── Non-chart (API) consumer ─────────────────────────────────────────────

    [Fact]
    public async Task MovingAverage_ApiConsumer_ShouldExecuteWithoutRenderPlan()
    {
        var pipeline = CreatePipeline();
        var intent = CreateMovingAverageIntent(
            ConsumerDeliveryContract.Api(ChartProgramKind.MovingAverage),
            windowSize: 3);

        var result = await pipeline.ExecuteForConsumerAsync(intent);

        Assert.Same(intent, result.Execution.Intent);
        Assert.Equal(ChartProgramKind.MovingAverage, result.Execution.Program.Kind);
        Assert.False(result.Evidence.RequiresRenderPlan);
        Assert.Equal(ConsumerKind.Api, result.Evidence.ConsumerKind);
        Assert.Equal(ConsumerProviderContracts.ApiResponse.ProviderKey, result.Evidence.ProviderKey);
        Assert.Equal(AnalyticalCapabilityKind.Smoothing, result.Evidence.CapabilityKind);
        Assert.Equal(CompositionKind.DerivedSeries, result.Evidence.CompositionKind);
        Assert.Equal(intent.Signature, result.Evidence.IntentSignature);
        Assert.Equal(result.Execution.Signature, result.Evidence.ExecutionSignature);
    }

    // ── Backend replaceability proof ─────────────────────────────────────────

    [Fact]
    public async Task MovingAverage_SameIntent_ProducesDifferentBackendMetadata_ProvingReplaceability()
    {
        var pipeline = CreatePipeline();
        var intent = CreateMovingAverageIntent(
            ConsumerDeliveryContract.Chart(ChartProgramKind.MovingAverage, "MovingAverageChart"),
            windowSize: 5);

        var withTabular = await pipeline.BuildCartesianAsync(
            intent,
            new ChartBackendCandidateSet([ChartBackendCapabilities.TabularSummary]));

        var withoutBackend = await pipeline.BuildCartesianAsync(intent);

        Assert.Equal("TabularSummaryChart", withTabular.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.BackendKey]);
        Assert.False(withoutBackend.RenderPlan.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.BackendKey));
        Assert.Equal(withTabular.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature],
                     withoutBackend.RenderPlan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature]);
    }

    // ── MovingAverage computation correctness ────────────────────────────────

    [Fact]
    public void MovingAverageOperation_ShouldComputeRollingMean_WithWindowSize3()
    {
        var kernel = new OperationKernel();
        var bundle = CreateBundle([1d, 2d, 3d, 4d, 5d]);
        var operation = SeriesOperationRequest.MovingAverage(0, 3, "ma::3", "MA(3)");

        var result = kernel.BuildSeries(bundle, operation);

        Assert.Equal(5, result.RawValues.Count);
        Assert.Equal(1d, result.RawValues[0], 6);
        Assert.Equal(1.5d, result.RawValues[1], 6);
        Assert.Equal(2d, result.RawValues[2], 6);
        Assert.Equal(3d, result.RawValues[3], 6);
        Assert.Equal(4d, result.RawValues[4], 6);
    }

    [Fact]
    public void MovingAverageOperation_ShouldSkipNaN_AndReturnNaN_WhenWindowIsAllNaN()
    {
        var kernel = new OperationKernel();
        var bundle = CreateBundle([double.NaN, double.NaN, 3d]);
        var operation = SeriesOperationRequest.MovingAverage(0, 2, "ma::2", "MA(2)");

        var result = kernel.BuildSeries(bundle, operation);

        Assert.True(double.IsNaN(result.RawValues[0]));
        Assert.True(double.IsNaN(result.RawValues[1]));
        Assert.Equal(3d, result.RawValues[2], 6);
    }

    // ── Capability contract guard ────────────────────────────────────────────

    [Fact]
    public void MovingAverageCapabilityContract_ShouldRejectWrongProgramKind()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new MovingAverageCapabilityContract(
                ChartProgramRequest.MainProgram(),
                new CapabilityRequest(AnalyticalCapabilityKind.Smoothing, CompositionKind.SingleSeries),
                ConsumerDeliveryContract.Chart(ChartProgramKind.Main)));

        Assert.Contains("MovingAverage", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MovingAverageCapabilityContract_ShouldRejectMismatchedDeliveryKind()
    {
        var programRequest = ChartProgramRequest.MovingAverage(
            "Test",
            [SeriesOperationRequest.MovingAverage(0, 7, "ma", "MA")]);

        var ex = Assert.Throws<ArgumentException>(() =>
            new MovingAverageCapabilityContract(
                programRequest,
                CapabilityRequest.FromProgramRequest(programRequest),
                ConsumerDeliveryContract.Chart(ChartProgramKind.Main)));

        Assert.Contains("delivery", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MovingAverageCapabilityContract_Create_ShouldBuildValidContract()
    {
        var operations = new[]
        {
            SeriesOperationRequest.MovingAverage(0, 7, "ma::7", "7-day MA")
        };

        var contract = MovingAverageCapabilityContract.Create("7-day Moving Average", operations);

        Assert.Equal(ChartProgramKind.MovingAverage, contract.ProgramRequest.Kind);
        Assert.Equal(AnalyticalCapabilityKind.Smoothing, contract.Capability.CapabilityKind);
        Assert.Equal(CompositionKind.DerivedSeries, contract.Capability.CompositionKind);
        Assert.Equal(ChartProgramKind.MovingAverage, contract.Delivery.ProgramKind);
        Assert.Equal("MovingAverageChart", contract.Delivery.DeliveryTarget);
        Assert.Equal(ConsumerKind.Chart, contract.Delivery.ConsumerKind);
    }

    // ── No old hub required proof ────────────────────────────────────────────

    [Fact]
    public async Task MovingAverage_CanBeBuiltWithoutChartUpdateCoordinator_PureVNextSpine()
    {
        // Builds the full pipeline using only VNext types — no ChartUpdateCoordinator,
        // no ChartDataContext, no LegacyChartProgramProjector involved.
        var loader = new StubMetricSeriesLoader();
        var gateway = new MetricLoadSnapshotGateway(loader);
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var engine = new ReasoningEngine(gateway, planner);
        var pipeline = new AnalyticalRenderPlanPipeline(engine);

        var selection = CreateSelection();
        var programRequest = ChartProgramRequest.MovingAverage(
            "Weekly MA",
            [SeriesOperationRequest.MovingAverage(0, 7, "ma::7", "7-day MA")]);
        var intent = AnalyticalIntent.FromRequests(
            selection,
            programRequest,
            ConsumerDeliveryContract.Chart(ChartProgramKind.MovingAverage, "MovingAverageChart"));

        var result = await pipeline.BuildCartesianAsync(intent);

        Assert.Equal(ChartProgramKind.MovingAverage, result.Execution.Program.Kind);
        Assert.Single(result.Execution.Program.Series);
        Assert.NotEmpty(result.RenderPlan.Series);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AnalyticalRenderPlanPipeline CreatePipeline()
    {
        var engine = new ReasoningEngine(
            new MetricLoadSnapshotGateway(new StubMetricSeriesLoader()),
            new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel()));
        return new AnalyticalRenderPlanPipeline(engine);
    }

    private static AnalyticalIntent CreateMovingAverageIntent(
        ConsumerDeliveryContract delivery,
        int windowSize = 7)
    {
        var selection = CreateSelection();
        var operations = new[]
        {
            SeriesOperationRequest.MovingAverage(0, windowSize, $"ma::{windowSize}", $"{windowSize}-day MA")
        };
        var programRequest = ChartProgramRequest.MovingAverage($"{windowSize}-day Moving Average", operations);
        return AnalyticalIntent.FromRequests(selection, programRequest, delivery);
    }

    private static MetricSelectionRequest CreateSelection()
    {
        return new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "body_weight")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 7),
            "HealthMetrics");
    }

    private static AlignedSeriesBundle CreateBundle(IReadOnlyList<double> values)
    {
        var timestamps = Enumerable.Range(0, values.Count)
            .Select(i => new DateTime(2026, 1, 1).AddDays(i))
            .ToArray();
        var series = new[]
        {
            new AlignedMetricSeries(
                new MetricSeriesRequest("Weight", "body_weight"),
                values,
                values)
        };
        return new AlignedSeriesBundle(timestamps, series);
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
            return Task.FromResult(new LoadedMetricSeries(
                [
                    new MetricData { NormalizedTimestamp = from, Value = 1m },
                    new MetricData { NormalizedTimestamp = from.AddDays(1), Value = 2m },
                    new MetricData { NormalizedTimestamp = from.AddDays(2), Value = 3m },
                    new MetricData { NormalizedTimestamp = from.AddDays(3), Value = 4m },
                    new MetricData { NormalizedTimestamp = to, Value = 5m }
                ],
                null));
        }
    }
}
