namespace DataVisualiser.VNext.Contracts;

public sealed record ConfidenceAnnotation(
    ConfidenceAnnotationKind Kind,
    ConfidenceSeverity Severity,
    string SeriesId,
    int? PointIndex,
    string Message,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    public IReadOnlyDictionary<string, string> ResolvedMetadata { get; } = Metadata == null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(Metadata);

    public string Signature =>
        $"{Kind}:{Severity}:{SeriesId}:{PointIndex?.ToString() ?? "<series>"}:{string.Join("|", ResolvedMetadata.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"))}";
}

public sealed record ConfidenceAnnotationSet(
    string SourceSignature,
    IReadOnlyList<ConfidenceAnnotation> Annotations)
{
    public bool HasAnnotations => Annotations.Count > 0;
    public int CriticalCount => Annotations.Count(annotation => annotation.Severity == ConfidenceSeverity.Critical);
    public int WarningCount => Annotations.Count(annotation => annotation.Severity == ConfidenceSeverity.Warning);

    public string Signature =>
        $"{SourceSignature}::{string.Join("||", Annotations.Select(annotation => annotation.Signature))}";

    public static ConfidenceAnnotationSet Empty(string sourceSignature)
    {
        if (string.IsNullOrWhiteSpace(sourceSignature))
            sourceSignature = "<unknown>";

        return new ConfidenceAnnotationSet(sourceSignature, []);
    }
}
