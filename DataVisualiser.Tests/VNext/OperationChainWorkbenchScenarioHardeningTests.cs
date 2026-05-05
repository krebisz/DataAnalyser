using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class OperationChainWorkbenchScenarioHardeningTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldHardenOperationChainWorkbenchScenarioAcrossConstructionAndConsumers()
    {
        var selection = CreateSelection();
        var executor = new OperationChainExecutor(new StubReasoningEngine(CreateSnapshot(selection)));
        var request = new OperationChainRequest(
            selection,
            [OperationChainStep.Lossless(SeriesOperationRequest.Ratio(0, 1, "Morning / Evening"))],
            ConsumerDeliveryContract.Export(ChartProgramKind.Transform, "OperationChainWorkbench"),
            "Operation Chain Workbench",
            [ConsumerDeliveryContract.Api(ChartProgramKind.Transform, "OperationChainApi")]);

        var result = await executor.ExecuteAsync(request);

        var dataset = Assert.Single(result.DerivedDatasets);
        Assert.Equal("ratio", dataset.Id);
        Assert.Equal([2d, double.NaN, 2d], dataset.RawValues);
        Assert.Contains(dataset.Relations, relation => relation.Kind == ConstructionRelationKind.SourceSeriesDerivation);
        Assert.Contains(dataset.Relations, relation => relation.Kind == ConstructionRelationKind.OperationDerivation);
        Assert.True(dataset.Confidence.HasAnnotations);
        Assert.Equal(2, dataset.Confidence.CriticalCount);
        Assert.Equal(AnalyticalFitnessStatus.Caution, dataset.Fitness.Status);

        Assert.Equal(OperationChainPlanningStatus.WithinBudget, request.Planning.Status);
        Assert.Equal(ConstructionEvidenceStatus.Retained, result.Evidence.Status);
        Assert.Equal(result.Trace.Signature, result.Evidence.TraceSignature);
        Assert.Equal("2", result.Evidence.Metadata[ConstructionMetadataKeys.OperationChainConsumerContractCount]);

        Assert.Equal(2, result.ConsumptionContracts.Count);
        Assert.Equal([ConsumerKind.Export, ConsumerKind.Api], result.ConsumptionContracts.Select(contract => contract.Delivery.ConsumerKind).ToArray());
        Assert.All(result.ConsumptionContracts, contract =>
        {
            Assert.Same(result.ConsumptionContract.SurfaceModel, contract.SurfaceModel);
            Assert.Equal(ConsumerSurfaceModelKind.DerivedDataset, contract.SurfaceModel.Kind);
            Assert.False(contract.SurfaceModel.RequiresRenderPlan);
            Assert.Equal(request.Planning.ReplaySignature, contract.Metadata[ConstructionMetadataKeys.OperationChainPlanningReplaySignature]);
        });
    }

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
        var series = new[]
        {
            new MetricSeriesSnapshot(selection.Series[0], CreateData([2m, 4m, 6m]), null),
            new MetricSeriesSnapshot(selection.Series[1], CreateData([1m, 0m, 3m]), null)
        };

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
