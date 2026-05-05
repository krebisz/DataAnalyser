namespace DataVisualiser.VNext.Contracts;

public static class DerivedDatasetConfidenceRules
{
    private const string Authoritative = "False";

    public static ConfidenceAnnotationSet Evaluate(
        string datasetId,
        string operationSignature,
        IReadOnlyList<double> rawValues,
        IReadOnlyList<double> smoothedValues,
        IReadOnlyList<string> sourceSeriesSignatures)
    {
        if (string.IsNullOrWhiteSpace(datasetId))
            throw new ArgumentException("Dataset id cannot be null or empty.", nameof(datasetId));
        if (string.IsNullOrWhiteSpace(operationSignature))
            throw new ArgumentException("Operation signature cannot be null or empty.", nameof(operationSignature));
        ArgumentNullException.ThrowIfNull(rawValues);
        ArgumentNullException.ThrowIfNull(smoothedValues);
        ArgumentNullException.ThrowIfNull(sourceSeriesSignatures);

        var annotations = new List<ConfidenceAnnotation>();
        AddNonFiniteAnnotations(
            annotations,
            datasetId,
            operationSignature,
            sourceSeriesSignatures,
            rawValues,
            ConfidenceAnnotationKind.NonFiniteRawValue,
            "Derived raw value is not finite.");
        AddNonFiniteAnnotations(
            annotations,
            datasetId,
            operationSignature,
            sourceSeriesSignatures,
            smoothedValues,
            ConfidenceAnnotationKind.NonFiniteSmoothedValue,
            "Derived smoothed value is not finite.");

        return annotations.Count == 0
            ? ConfidenceAnnotationSet.Empty(operationSignature)
            : new ConfidenceAnnotationSet(operationSignature, annotations);
    }

    private static void AddNonFiniteAnnotations(
        ICollection<ConfidenceAnnotation> annotations,
        string datasetId,
        string operationSignature,
        IReadOnlyList<string> sourceSeriesSignatures,
        IReadOnlyList<double> values,
        ConfidenceAnnotationKind kind,
        string message)
    {
        for (var index = 0; index < values.Count; index++)
        {
            if (double.IsFinite(values[index]))
                continue;

            annotations.Add(new ConfidenceAnnotation(
                kind,
                ConfidenceSeverity.Critical,
                datasetId,
                index,
                message,
                new Dictionary<string, string>
                {
                    [ConstructionMetadataKeys.OperationSignature] = operationSignature,
                    [ConstructionMetadataKeys.SourceSeriesSignatures] = string.Join("|", sourceSeriesSignatures),
                    ["Authoritative"] = Authoritative
                }));
        }
    }
}
