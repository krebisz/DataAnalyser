namespace DataVisualiser.VNext.Contracts;

public static class DerivedDatasetRules
{
    public static void Validate(
        string id,
        string label,
        IReadOnlyList<DateTime> timeline,
        IReadOnlyList<double> rawValues,
        IReadOnlyList<double> smoothedValues,
        IReadOnlyList<string> sourceSeriesSignatures,
        string operationSignature)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Derived dataset id cannot be null or empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Derived dataset label cannot be null or empty.", nameof(label));
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(rawValues);
        ArgumentNullException.ThrowIfNull(smoothedValues);
        ArgumentNullException.ThrowIfNull(sourceSeriesSignatures);
        if (string.IsNullOrWhiteSpace(operationSignature))
            throw new ArgumentException("Derived dataset operation signature cannot be null or empty.", nameof(operationSignature));
        if (timeline.Count != rawValues.Count)
            throw new ArgumentException("Derived dataset raw values must match the timeline cardinality.", nameof(rawValues));
        if (timeline.Count != smoothedValues.Count)
            throw new ArgumentException("Derived dataset smoothed values must match the timeline cardinality.", nameof(smoothedValues));
        if (sourceSeriesSignatures.Count == 0 || sourceSeriesSignatures.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Derived dataset source series signatures must be present.", nameof(sourceSeriesSignatures));
    }
}
