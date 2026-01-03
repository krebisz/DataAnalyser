namespace DataFileReader.Normalization.Canonical;

public sealed class MetricIdentity
{
    // ---------------------------------
    // Canonical Metric Identities
    // ---------------------------------

    /// <summary>
    ///     Canonical identity for body weight.
    ///     Dimension: Mass
    /// </summary>
    public static readonly MetricIdentity BodyWeight = new("metric.body_weight");

    /// <summary>
    ///     Canonical identity for sleep.
    ///     Dimension: Duration
    /// </summary>
    public static readonly MetricIdentity Sleep = new("metric.sleep");

    public MetricIdentity(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Metric identity must be non-empty.", nameof(id));

        Id = id;
    }

    public string Id { get; }

    public override string ToString()
    {
        return Id;
    }
}