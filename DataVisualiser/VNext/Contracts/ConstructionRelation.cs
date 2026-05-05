namespace DataVisualiser.VNext.Contracts;

public enum ConstructionRelationKind
{
    SourceSeriesDerivation = 0,
    OperationDerivation = 1,
    Projection = 2
}

public sealed record ConstructionRelation
{
    public ConstructionRelation(
        ConstructionRelationKind kind,
        string sourceId,
        string targetId,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("Construction relation source id cannot be null or empty.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(targetId))
            throw new ArgumentException("Construction relation target id cannot be null or empty.", nameof(targetId));

        Kind = kind;
        SourceId = sourceId;
        TargetId = targetId;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public ConstructionRelationKind Kind { get; }
    public string SourceId { get; }
    public string TargetId { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
