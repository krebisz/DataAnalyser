namespace DataVisualiser.VNext.Contracts;

public sealed record OperationChainRequest
{
    public OperationChainRequest(
        MetricSelectionRequest selection,
        IReadOnlyList<OperationChainStep> steps,
        ConsumerDeliveryContract? delivery = null,
        string title = "Operation Chain",
        IReadOnlyList<ConsumerDeliveryContract>? additionalDeliveries = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(steps);
        if (selection.Series.Count == 0)
            throw new ArgumentException("Operation chain requires at least one input series.", nameof(selection));
        if (steps.Count == 0)
            throw new ArgumentException("Operation chain requires at least one operation step.", nameof(steps));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Operation chain title cannot be null or empty.", nameof(title));

        Selection = selection;
        Steps = steps.ToArray();
        Delivery = delivery ?? ConsumerDeliveryContract.Export(ChartProgramKind.Transform, "OperationChainWorkbench");
        Deliveries = BuildDeliveries(Delivery, additionalDeliveries);
        Title = title;
        Planning = OperationChainPlanningRules.Assess(Selection, Steps);
    }

    public MetricSelectionRequest Selection { get; }
    public IReadOnlyList<OperationChainStep> Steps { get; }
    public ConsumerDeliveryContract Delivery { get; }
    public IReadOnlyList<ConsumerDeliveryContract> Deliveries { get; }
    public string Title { get; }
    public OperationChainPlanningAssessment Planning { get; }

    public string Signature =>
        string.Join("::", new[]
        {
            Selection.Signature,
            string.Join("|", Deliveries.Select(delivery => delivery.Signature)),
            string.Join("|", Steps.Select(step => step.Signature))
        });

    private static IReadOnlyList<ConsumerDeliveryContract> BuildDeliveries(
        ConsumerDeliveryContract primary,
        IReadOnlyList<ConsumerDeliveryContract>? additionalDeliveries)
    {
        var deliveries = new List<ConsumerDeliveryContract> { primary };
        if (additionalDeliveries != null)
            deliveries.AddRange(additionalDeliveries);

        return deliveries
            .GroupBy(delivery => delivery.Signature, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
    }
}

public sealed record OperationChainStep
{
    public OperationChainStep(
        SeriesOperationRequest operation,
        bool reversible,
        string lossiness,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (string.IsNullOrWhiteSpace(lossiness))
            throw new ArgumentException("Lossiness cannot be null or empty.", nameof(lossiness));

        Operation = operation;
        Reversible = reversible;
        Lossiness = lossiness;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public SeriesOperationRequest Operation { get; }
    public bool Reversible { get; }
    public string Lossiness { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public string Signature =>
        $"{Operation.Kind}:{Operation.Id}:{string.Join(",", Operation.InputIndexes)}:{Operation.WindowSize}:{Operation.NormalizationMode}:{string.Join(",", Operation.NormalizationReferenceIndexes)}:{Reversible}:{Lossiness}";

    public static OperationChainStep Lossless(SeriesOperationRequest operation, bool reversible = false) =>
        new(operation, reversible, SeriesOperationRules.Lossless);

    public static OperationChainStep Lossy(SeriesOperationRequest operation, string lossiness = SeriesOperationRules.WindowedSmoothing) =>
        new(operation, reversible: false, lossiness);
}

public sealed record OperationChainProgram(
    string Id,
    string Title,
    MetricSelectionRequest Selection,
    IReadOnlyList<OperationChainStep> Steps,
    ConsumerDeliveryContract Delivery);

public sealed record OperationChainExecutionPlan(
    OperationChainProgram Program,
    string SourceSignature,
    IReadOnlyList<string> SourceSeriesSignatures,
    IReadOnlyList<SeriesOperationRequest> Operations)
{
    public string Signature =>
        $"{Program.Id}::{SourceSignature}::{string.Join("|", Operations.Select(operation => operation.Id))}";
}

public sealed record DerivedDataset
{
    public DerivedDataset(
        string id,
        string label,
        IReadOnlyList<DateTime> timeline,
        IReadOnlyList<double> rawValues,
        IReadOnlyList<double> smoothedValues,
        IReadOnlyList<string> sourceSeriesSignatures,
        string operationSignature,
        IReadOnlyDictionary<string, string>? metadata = null,
        IReadOnlyList<ConstructionRelation>? relations = null)
    {
        DerivedDatasetRules.Validate(
            id,
            label,
            timeline,
            rawValues,
            smoothedValues,
            sourceSeriesSignatures,
            operationSignature);

        Id = id;
        Label = label;
        Timeline = timeline.ToArray();
        RawValues = rawValues.ToArray();
        SmoothedValues = smoothedValues.ToArray();
        SourceSeriesSignatures = sourceSeriesSignatures.ToArray();
        OperationSignature = operationSignature;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
        Relations = relations?.ToArray() ?? BuildDefaultRelations(id, sourceSeriesSignatures, operationSignature);
        Confidence = DerivedDatasetConfidenceRules.Evaluate(
            id,
            operationSignature,
            RawValues,
            SmoothedValues,
            SourceSeriesSignatures);
        Fitness = DerivedDatasetFitnessRules.Evaluate(
            id,
            operationSignature,
            RawValues,
            SmoothedValues,
            Confidence);
    }

    public string Id { get; }
    public string Label { get; }
    public IReadOnlyList<DateTime> Timeline { get; }
    public IReadOnlyList<double> RawValues { get; }
    public IReadOnlyList<double> SmoothedValues { get; }
    public IReadOnlyList<string> SourceSeriesSignatures { get; }
    public string OperationSignature { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
    public IReadOnlyList<ConstructionRelation> Relations { get; }
    public ConfidenceAnnotationSet Confidence { get; }
    public AnalyticalFitnessAssessment Fitness { get; }

    private static IReadOnlyList<ConstructionRelation> BuildDefaultRelations(
        string datasetId,
        IReadOnlyList<string> sourceSeriesSignatures,
        string operationSignature)
    {
        var relations = sourceSeriesSignatures
            .Select(source => new ConstructionRelation(
                ConstructionRelationKind.SourceSeriesDerivation,
                source,
                datasetId))
            .ToList();
        relations.Add(new ConstructionRelation(
            ConstructionRelationKind.OperationDerivation,
            operationSignature,
            datasetId));

        return relations;
    }
}

public sealed record OperationChainTraceEntry(
    int StepIndex,
    SeriesOperationKind OperationKind,
    IReadOnlyList<int> InputIndexes,
    string OutputDatasetId,
    bool Reversible,
    string Lossiness,
    IReadOnlyDictionary<string, string> Metadata)
{
    public string Signature =>
        $"{StepIndex}:{OperationKind}:{string.Join(",", InputIndexes)}:{OutputDatasetId}:{Reversible}:{Lossiness}";
}

public sealed record OperationChainTrace(IReadOnlyList<OperationChainTraceEntry> Entries)
{
    public string Signature => string.Join("|", Entries.Select(entry => entry.Signature));
}

public sealed record OperationChainEvidence(
    string SourceSignature,
    string PlanSignature,
    string TraceSignature,
    string ContractSignature,
    IReadOnlyList<string> SourceSeriesSignatures,
    IReadOnlyList<string> DerivedDatasetIds,
    IReadOnlyDictionary<string, string> Metadata,
    ConstructionEvidenceStatus Status = ConstructionEvidenceStatus.Retained);

public sealed record OperationChainResult(
    OperationChainRequest Request,
    OperationChainExecutionPlan Plan,
    IReadOnlyList<DerivedDataset> DerivedDatasets,
    OperationChainTrace Trace,
    OperationChainEvidence Evidence,
    VNextUiConsumptionContract ConsumptionContract)
{
    public IReadOnlyList<VNextUiConsumptionContract> ConsumptionContracts { get; init; } = [ConsumptionContract];
}
