using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.VNext.Application;

public sealed class OperationChainExecutor
{
    private readonly IReasoningEngine _engine;
    private readonly TimeSeriesAlignmentKernel _alignmentKernel;
    private readonly OperationKernel _operationKernel;
    private readonly ConsumerProviderRegistry _providerRegistry;

    public OperationChainExecutor(
        IReasoningEngine engine,
        TimeSeriesAlignmentKernel? alignmentKernel = null,
        OperationKernel? operationKernel = null,
        ConsumerProviderRegistry? providerRegistry = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _alignmentKernel = alignmentKernel ?? new TimeSeriesAlignmentKernel();
        _operationKernel = operationKernel ?? new OperationKernel();
        _providerRegistry = providerRegistry ?? ConsumerProviderRegistry.BuiltIn;
    }

    public async Task<OperationChainResult> ExecuteAsync(
        OperationChainRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var snapshot = await _engine.LoadAsync(request.Selection, cancellationToken);
        var aligned = _alignmentKernel.Align(snapshot);
        if (aligned.Series.Count < 2)
            throw new InvalidOperationException("Operation chain requires at least two loaded input series.");

        var program = new OperationChainProgram(
            request.Signature,
            request.Title,
            request.Selection,
            request.Steps,
            request.Delivery);
        var plan = new OperationChainExecutionPlan(
            program,
            snapshot.Signature,
            aligned.Series.Select(series => series.Request.SignatureToken).ToArray(),
            request.Steps.Select(step => step.Operation).ToArray());

        var workingSeries = aligned.Series.ToList();
        var datasets = new List<DerivedDataset>();
        var traceEntries = new List<OperationChainTraceEntry>();

        for (var index = 0; index < request.Steps.Count; index++)
        {
            var step = request.Steps[index];
            var bundle = new AlignedSeriesBundle(aligned.Timeline, workingSeries);
            var output = _operationKernel.BuildSeries(bundle, step.Operation);
            var sourceSignatures = step.Operation.InputIndexes
                .Select(inputIndex => ResolveSeriesSignature(workingSeries, inputIndex))
                .ToArray();
            var operationSignature = BuildOperationSignature(step.Operation);
            var metadata = BuildDatasetMetadata(step, index, sourceSignatures);
            var dataset = new DerivedDataset(
                output.Id,
                output.Label,
                aligned.Timeline,
                output.RawValues,
                output.SmoothedValues,
                sourceSignatures,
                operationSignature,
                metadata);

            datasets.Add(dataset);
            traceEntries.Add(new OperationChainTraceEntry(
                index,
                step.Operation.Kind,
                step.Operation.InputIndexes.ToArray(),
                dataset.Id,
                step.Reversible,
                step.Lossiness,
                metadata));

            workingSeries.Add(new AlignedMetricSeries(
                new MetricSeriesRequest(
                    request.Selection.MetricType,
                    dataset.Id,
                    dataset.Label,
                    dataset.Id),
                dataset.RawValues,
                dataset.SmoothedValues));
        }

        var trace = new OperationChainTrace(traceEntries);
        var surfaceModel = ConsumerSurfaceModel.FromDerivedDatasets(datasets);
        var contracts = request.Deliveries
            .Select(delivery => BuildConsumptionContract(
                request,
                snapshot.Signature,
                plan.Signature,
                trace.Signature,
                datasets.Count,
                surfaceModel,
                delivery))
            .ToArray();
        var contract = contracts[0];
        var evidence = new OperationChainEvidence(
            snapshot.Signature,
            plan.Signature,
            trace.Signature,
            contract.Signature,
            plan.SourceSeriesSignatures,
            datasets.Select(dataset => dataset.Id).ToArray(),
            new Dictionary<string, string>
            {
                ["ConsumerKind"] = contract.Delivery.ConsumerKind.ToString(),
                ["DeliveryTarget"] = contract.Delivery.DeliveryTarget,
                ["ProviderKey"] = contract.Provider.ProviderKey,
                ["SurfaceKind"] = contract.SurfaceModel.Kind.ToString(),
                [ConstructionMetadataKeys.OperationChainPlanningStatus] = request.Planning.Status.ToString(),
                [ConstructionMetadataKeys.OperationChainConsumerContractCount] = contracts.Length.ToString(),
                [ConstructionMetadataKeys.OperationChainConsumerKinds] = string.Join("|", contracts.Select(item => item.Delivery.ConsumerKind))
            });

        return new OperationChainResult(
            request,
            plan,
            datasets,
            trace,
            evidence,
            contract)
        {
            ConsumptionContracts = contracts
        };
    }

    private VNextUiConsumptionContract BuildConsumptionContract(
        OperationChainRequest request,
        string snapshotSignature,
        string planSignature,
        string traceSignature,
        int outputCount,
        ConsumerSurfaceModel surfaceModel,
        ConsumerDeliveryContract delivery)
    {
        var intent = AnalyticalIntent.FromRequests(
            request.Selection,
            ChartProgramRequest.Transform(
                request.Title,
                request.Steps.Select(step => step.Operation).ToArray()),
            delivery,
            ProvenanceDescriptor.Derived(snapshotSignature),
            new CapabilityRequest(
                AnalyticalCapabilityKind.Transform,
                CompositionKind.DerivedSeries,
                request.Steps.Select(step => step.Operation).ToArray()));
        var provider = _providerRegistry.Resolve(delivery);
        return VNextUiConsumptionContract.FromIntent(
            intent,
            provider,
            surfaceModel,
            new Dictionary<string, string>
            {
                [ConstructionMetadataKeys.OperationChainPlanSignature] = planSignature,
                [ConstructionMetadataKeys.OperationChainTraceSignature] = traceSignature,
                [ConstructionMetadataKeys.OperationChainOutputCount] = outputCount.ToString(),
                [ConstructionMetadataKeys.OperationChainPlanningStatus] = request.Planning.Status.ToString(),
                [ConstructionMetadataKeys.OperationChainPlanningReplaySignature] = request.Planning.ReplaySignature,
                [ConstructionMetadataKeys.OperationChainPlanningStepCount] = request.Planning.StepCount.ToString(),
                [ConstructionMetadataKeys.OperationChainPlanningInputReferenceCount] = request.Planning.InputReferenceCount.ToString(),
                [ConstructionMetadataKeys.OperationChainPlanningWorkingSetSize] = request.Planning.WorkingSetSize.ToString(),
                [ConstructionMetadataKeys.OperationChainConsumerContractCount] = request.Deliveries.Count.ToString(),
                [ConstructionMetadataKeys.OperationChainConsumerKinds] = string.Join("|", request.Deliveries.Select(item => item.ConsumerKind))
            });
    }

    private static string ResolveSeriesSignature(IReadOnlyList<AlignedMetricSeries> series, int inputIndex)
    {
        if (inputIndex < 0 || inputIndex >= series.Count)
            throw new ArgumentOutOfRangeException(nameof(inputIndex), $"Series index {inputIndex} is outside the operation-chain working set.");

        return series[inputIndex].Request.SignatureToken;
    }

    private static string BuildOperationSignature(SeriesOperationRequest operation)
    {
        return $"{operation.Kind}:{operation.Id}:{string.Join(",", operation.InputIndexes)}:{operation.WindowSize}";
    }

    private static IReadOnlyDictionary<string, string> BuildDatasetMetadata(
        OperationChainStep step,
        int stepIndex,
        IReadOnlyList<string> sourceSignatures)
    {
        var metadata = new Dictionary<string, string>
        {
            [ConstructionMetadataKeys.StepIndex] = stepIndex.ToString(),
            [ConstructionMetadataKeys.OperationKind] = step.Operation.Kind.ToString(),
            [ConstructionMetadataKeys.OperationId] = step.Operation.Id,
            [ConstructionMetadataKeys.OperationLabel] = step.Operation.Label,
            [ConstructionMetadataKeys.InputIndexes] = string.Join(",", step.Operation.InputIndexes),
            [ConstructionMetadataKeys.SourceSeriesSignatures] = string.Join("|", sourceSignatures),
            [ConstructionMetadataKeys.Reversible] = step.Reversible.ToString(),
            [ConstructionMetadataKeys.Lossiness] = step.Lossiness
        };

        foreach (var pair in step.Metadata)
            metadata[$"Step.{pair.Key}"] = pair.Value;

        return metadata;
    }
}
