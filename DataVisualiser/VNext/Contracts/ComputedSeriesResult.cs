namespace DataVisualiser.VNext.Contracts;

public sealed record ComputedSeriesResult
{
    public ComputedSeriesResult(
        string id,
        string label,
        IReadOnlyList<DateTime> timeline,
        IReadOnlyList<double> rawValues,
        IReadOnlyList<double> smoothedValues,
        IReadOnlyList<string>? sourceSeriesSignatures = null,
        string? operationSignature = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Computed series id cannot be null or empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Computed series label cannot be null or empty.", nameof(label));
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(rawValues);
        ArgumentNullException.ThrowIfNull(smoothedValues);
        if (timeline.Count != rawValues.Count || timeline.Count != smoothedValues.Count)
            throw new ArgumentException("Computed series timeline, raw values, and smoothed values must have matching cardinality.");

        Id = id;
        Label = label;
        Timeline = timeline.ToArray();
        RawValues = rawValues.ToArray();
        SmoothedValues = smoothedValues.ToArray();
        SourceSeriesSignatures = sourceSeriesSignatures?.ToArray() ?? Array.Empty<string>();
        OperationSignature = operationSignature ?? string.Empty;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public string Id { get; }
    public string Label { get; }
    public IReadOnlyList<DateTime> Timeline { get; }
    public IReadOnlyList<double> RawValues { get; }
    public IReadOnlyList<double> SmoothedValues { get; }
    public IReadOnlyList<string> SourceSeriesSignatures { get; }
    public string OperationSignature { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public static ComputedSeriesResult FromDerivedDataset(DerivedDataset dataset)
    {
        ArgumentNullException.ThrowIfNull(dataset);

        return new ComputedSeriesResult(
            dataset.Id,
            dataset.Label,
            dataset.Timeline,
            dataset.RawValues,
            dataset.SmoothedValues,
            dataset.SourceSeriesSignatures,
            dataset.OperationSignature,
            dataset.Metadata);
    }

    public static ComputedSeriesResult FromChartSeriesProgram(
        ChartSeriesProgram program,
        IReadOnlyList<DateTime> timeline,
        IReadOnlyList<string>? sourceSeriesSignatures = null,
        string? operationSignature = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(program);

        return new ComputedSeriesResult(
            program.Id,
            program.Label,
            timeline,
            program.RawValues,
            program.SmoothedValues,
            sourceSeriesSignatures,
            operationSignature,
            metadata);
    }
}
