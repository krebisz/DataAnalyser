namespace DataVisualiser.VNext.Contracts;

public sealed record ProvenanceDescriptor
{
    public ProvenanceDescriptor(
        string sourceSignature,
        string authority = "VNext",
        string trustClass = "Derived",
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(sourceSignature))
            throw new ArgumentException("Source signature cannot be null or empty.", nameof(sourceSignature));
        if (string.IsNullOrWhiteSpace(authority))
            throw new ArgumentException("Authority cannot be null or empty.", nameof(authority));
        if (string.IsNullOrWhiteSpace(trustClass))
            throw new ArgumentException("Trust class cannot be null or empty.", nameof(trustClass));

        SourceSignature = sourceSignature;
        Authority = authority;
        TrustClass = trustClass;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public ProvenanceDescriptor(
        string sourceSignature,
        AnalyticalAuthority authority,
        ProvenanceTrustClass trustClass,
        IReadOnlyDictionary<string, string>? metadata = null)
        : this(sourceSignature, authority.ToString(), trustClass.ToString(), metadata)
    {
    }

    public string SourceSignature { get; }
    public string Authority { get; }
    public string TrustClass { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public string Signature => $"{Authority}:{TrustClass}:{SourceSignature}";

    public static ProvenanceDescriptor FromSelection(MetricSelectionRequest selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new ProvenanceDescriptor(selection.Signature, AnalyticalAuthority.VNext, ProvenanceTrustClass.Requested);
    }

    public static ProvenanceDescriptor Raw(string sourceSignature, AnalyticalAuthority authority = AnalyticalAuthority.Legacy) =>
        new(sourceSignature, authority, ProvenanceTrustClass.Raw);

    public static ProvenanceDescriptor Derived(string sourceSignature, AnalyticalAuthority authority = AnalyticalAuthority.VNext) =>
        new(sourceSignature, authority, ProvenanceTrustClass.Derived);

    public static ProvenanceDescriptor Projected(string sourceSignature, AnalyticalAuthority authority = AnalyticalAuthority.VNext) =>
        new(sourceSignature, authority, ProvenanceTrustClass.Projected);

    public static ProvenanceDescriptor Delivered(string sourceSignature, AnalyticalAuthority authority = AnalyticalAuthority.VNext) =>
        new(sourceSignature, authority, ProvenanceTrustClass.Delivered);
}
