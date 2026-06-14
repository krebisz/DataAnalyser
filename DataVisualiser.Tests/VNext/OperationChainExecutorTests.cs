using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class OperationChainExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldChainMultipleOperationsAcrossWorkingDatasets()
    {
        var executor = CreateExecutor();
        var request = CreateRequest(
            OperationChainStep.Lossless(SeriesOperationRequest.Sum([0, 1], "Total")),
            OperationChainStep.Lossless(SeriesOperationRequest.Ratio(2, 1, "Total / Evening")),
            OperationChainStep.Lossy(SeriesOperationRequest.MovingAverage(3, 2, "ma-ratio", "MA Ratio"), "WindowedSmoothing"));

        var result = await executor.ExecuteAsync(request);

        Assert.Equal(3, result.DerivedDatasets.Count);
        Assert.Equal("sum", result.DerivedDatasets[0].Id);
        Assert.Equal([3d, 6d, 9d], result.DerivedDatasets[0].RawValues);
        Assert.Equal("ratio", result.DerivedDatasets[1].Id);
        Assert.Equal([1.5d, 1.5d, 1.5d], result.DerivedDatasets[1].RawValues);
        Assert.Equal("ma-ratio", result.DerivedDatasets[2].Id);
        Assert.Equal([1.5d, 1.5d, 1.5d], result.DerivedDatasets[2].RawValues);
        Assert.Equal([SeriesOperationKind.Sum, SeriesOperationKind.Ratio, SeriesOperationKind.MovingAverage],
            result.Trace.Entries.Select(entry => entry.OperationKind).ToArray());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreserveProvenanceTraceEvidenceAndConsumptionContractMetadata()
    {
        var executor = CreateExecutor();
        var request = CreateRequest(
            OperationChainStep.Lossless(
                SeriesOperationRequest.Difference(0, 1, "Morning - Evening"),
                reversible: true),
            OperationChainStep.Lossy(
                SeriesOperationRequest.Normalize(2, "normalized-diff", "Normalized Difference"),
                "ScaleNormalized"));

        var result = await executor.ExecuteAsync(request);

        Assert.Equal(request.Selection.Signature, result.Plan.SourceSignature);
        Assert.Equal(["Weight:morning", "Weight:evening"], result.Plan.SourceSeriesSignatures);
        Assert.Equal(result.Trace.Signature, result.Evidence.TraceSignature);
        Assert.Equal(result.ConsumptionContract.Signature, result.Evidence.ContractSignature);
        Assert.Equal(ConsumerKind.Export, result.ConsumptionContract.Delivery.ConsumerKind);
        Assert.Same(result.ConsumptionContract, result.ConsumptionContracts[0]);
        Assert.Equal("OperationChainWorkbench", result.ConsumptionContract.Delivery.DeliveryTarget);
        Assert.Equal(ConsumerSurfaceModelKind.DerivedDataset, result.ConsumptionContract.SurfaceModel.Kind);
        Assert.Equal("2", result.ConsumptionContract.Metadata[ConstructionMetadataKeys.OperationChainOutputCount]);
        Assert.Equal(result.Plan.Signature, result.ConsumptionContract.Metadata[ConstructionMetadataKeys.OperationChainPlanSignature]);
        Assert.Equal(result.Trace.Signature, result.ConsumptionContract.Metadata[ConstructionMetadataKeys.OperationChainTraceSignature]);
        Assert.Equal(OperationChainPlanningStatus.WithinBudget.ToString(), result.ConsumptionContract.Metadata[ConstructionMetadataKeys.OperationChainPlanningStatus]);
        Assert.Equal(request.Planning.ReplaySignature, result.ConsumptionContract.Metadata[ConstructionMetadataKeys.OperationChainPlanningReplaySignature]);
        Assert.Equal("1", result.ConsumptionContract.Metadata[ConstructionMetadataKeys.OperationChainConsumerContractCount]);
        Assert.Equal("2", result.ConsumptionContract.Metadata[$"Surface.{ConstructionMetadataKeys.DatasetCount}"]);
        Assert.Equal("difference|normalized-diff", result.ConsumptionContract.Metadata[$"Surface.{ConstructionMetadataKeys.DatasetIds}"]);
        Assert.Contains("Difference", result.ConsumptionContract.Metadata[$"Surface.{ConstructionMetadataKeys.OperationSignatures}"]);
        Assert.Null(result.ConsumptionContract.SurfaceModel.RenderPlanKind);
        Assert.Equal("True", result.DerivedDatasets[0].Metadata[ConstructionMetadataKeys.Reversible]);
        Assert.Equal("ScaleNormalized", result.DerivedDatasets[1].Metadata[ConstructionMetadataKeys.Lossiness]);
        Assert.Contains("Weight:morning", result.DerivedDatasets[0].SourceSeriesSignatures);
        Assert.Equal("DerivedDataset", result.Evidence.Metadata["SurfaceKind"]);
        Assert.Equal(ConstructionEvidenceStatus.Retained, result.Evidence.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldServeOneDerivedOutputThroughMultipleConsumerContracts()
    {
        var executor = CreateExecutor();
        var request = new OperationChainRequest(
            CreateSelection(),
            [OperationChainStep.Lossless(SeriesOperationRequest.Sum([0, 1], "Total"))],
            ConsumerDeliveryContract.Export(ChartProgramKind.Transform, "OperationChainWorkbench"),
            "Derived Workbench",
            [ConsumerDeliveryContract.Api(ChartProgramKind.Transform, "OperationChainApi")]);

        var result = await executor.ExecuteAsync(request);

        Assert.Equal(2, result.ConsumptionContracts.Count);
        Assert.Equal([ConsumerKind.Export, ConsumerKind.Api], result.ConsumptionContracts.Select(contract => contract.Delivery.ConsumerKind).ToArray());
        Assert.All(result.ConsumptionContracts, contract =>
        {
            Assert.Same(result.ConsumptionContract.SurfaceModel, contract.SurfaceModel);
            Assert.Equal(ConsumerSurfaceModelKind.DerivedDataset, contract.SurfaceModel.Kind);
            Assert.Equal("2", contract.Metadata[ConstructionMetadataKeys.OperationChainConsumerContractCount]);
            Assert.Equal("Export|Api", contract.Metadata[ConstructionMetadataKeys.OperationChainConsumerKinds]);
        });
        Assert.Equal("2", result.Evidence.Metadata[ConstructionMetadataKeys.OperationChainConsumerContractCount]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectLoadedSnapshotsWithoutAnyInputSeries()
    {
        var selection = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 3),
            "HealthMetrics");
        var request = new OperationChainRequest(
            selection,
            [OperationChainStep.Lossless(SeriesOperationRequest.SquareRoot(0, "sqrt", "Sqrt"))]);
        var executor = new OperationChainExecutor(new StubReasoningEngine(new MetricLoadSnapshot(
            selection,
            [],
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => executor.ExecuteAsync(request));

        Assert.Contains("at least one loaded input series", ex.Message, StringComparison.Ordinal);
    }

    private static OperationChainExecutor CreateExecutor()
    {
        var selection = CreateSelection();
        return new OperationChainExecutor(new StubReasoningEngine(CreateSnapshot(selection)));
    }

    private static OperationChainRequest CreateRequest(params OperationChainStep[] steps) =>
        new(CreateSelection(), steps, title: "Derived Workbench");

    private static MetricSelectionRequest CreateSelection() =>
        new(
            "Weight",
            [
                new MetricSeriesRequest("Weight", "morning"),
                new MetricSeriesRequest("Weight", "evening")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 3),
            "HealthMetrics");

    private static MetricLoadSnapshot CreateSnapshot(MetricSelectionRequest selection)
    {
        var series = selection.Series
            .Select((request, index) => new MetricSeriesSnapshot(
                request,
                CreateData(index == 0 ? [1m, 2m, 3m] : [2m, 4m, 6m]),
                null))
            .ToArray();

        return new MetricLoadSnapshot(selection, series, new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc));
    }

    private static IReadOnlyList<MetricData> CreateData(IReadOnlyList<decimal> values) =>
        values
            .Select((value, index) => new MetricData
            {
                NormalizedTimestamp = new DateTime(2026, 1, 1).AddDays(index),
                Value = value
            })
            .ToArray();

    private sealed class StubReasoningEngine(MetricLoadSnapshot snapshot) : IReasoningEngine
    {
        public Task<MetricLoadSnapshot> LoadAsync(MetricSelectionRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(snapshot);

        public Task<AnalyticalExecutionResult> ExecuteAsync(AnalyticalIntent intent, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AnalyticalResultSet> ExecuteAsync(AnalyticalIntentSet intentSet, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request) =>
            throw new NotSupportedException();

        public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, AnalyticalIntent intent) =>
            throw new NotSupportedException();

        public ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular) =>
            throw new NotSupportedException();

        public ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot) =>
            throw new NotSupportedException();

        public ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot) =>
            throw new NotSupportedException();

        public ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot) =>
            throw new NotSupportedException();
    }
}
