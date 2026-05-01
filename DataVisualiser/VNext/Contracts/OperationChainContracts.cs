namespace DataVisualiser.VNext.Contracts;

public sealed record OperationChainRequest
{
    public OperationChainRequest(
        MetricSelectionRequest selection,
        IReadOnlyList<OperationChainStep> steps,
        ConsumerDeliveryContract? delivery = null,
        string title = "Operation Chain")
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(steps);
        if (selection.Series.Count < 2)
            throw new ArgumentException("Operation chain requires at least two input series.", nameof(selection));
        if (steps.Count == 0)
            throw new ArgumentException("Operation chain requires at least one operation step.", nameof(steps));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Operation chain title cannot be null or empty.", nameof(title));

        Selection = selection;
        Steps = steps.ToArray();
        Delivery = delivery ?? ConsumerDeliveryContract.Export(ChartProgramKind.Transform, "OperationChainWorkbench");
        Title = title;
    }

    public MetricSelectionRequest Selection { get; }
    public IReadOnlyList<OperationChainStep> Steps { get; }
    public ConsumerDeliveryContract Delivery { get; }
    public string Title { get; }

    public string Signature =>
        string.Join("::", new[]
        {
            Selection.Signature,
            Delivery.Signature,
            string.Join("|", Steps.Select(step => step.Signature))
        });
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
        $"{Operation.Kind}:{Operation.Id}:{string.Join(",", Operation.InputIndexes)}:{Operation.WindowSize}:{Reversible}:{Lossiness}";

    public static OperationChainStep Lossless(SeriesOperationRequest operation, bool reversible = false) =>
        new(operation, reversible, "Lossless");

    public static OperationChainStep Lossy(SeriesOperationRequest operation, string lossiness = "Lossy") =>
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

public sealed record DerivedDataset(
    string Id,
    string Label,
    IReadOnlyList<DateTime> Timeline,
    IReadOnlyList<double> RawValues,
    IReadOnlyList<double> SmoothedValues,
    IReadOnlyList<string> SourceSeriesSignatures,
    string OperationSignature,
    IReadOnlyDictionary<string, string> Metadata);

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
    IReadOnlyDictionary<string, string> Metadata);

public sealed record OperationChainResult(
    OperationChainRequest Request,
    OperationChainExecutionPlan Plan,
    IReadOnlyList<DerivedDataset> DerivedDatasets,
    OperationChainTrace Trace,
    OperationChainEvidence Evidence,
    VNextUiConsumptionContract ConsumptionContract);
