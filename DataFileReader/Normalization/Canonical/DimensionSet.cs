namespace DataFileReader.Normalization.Canonical;

/// <summary>
///     Immutable set of dimensions qualifying a canonical metric.
///     Dimensions are fixed across the entire series.
/// </summary>
public sealed class DimensionSet
{
    public DimensionSet(IReadOnlyDictionary<string, string> dimensions)
    {
        Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
    }

    public IReadOnlyDictionary<string, string> Dimensions { get; }
}