namespace DataFileReader.Normalization;

/// <summary>
///     Shared, immutable context for normalization execution.
///     Carries configuration and diagnostic state.
/// </summary>
public sealed class NormalizationContext
{
    public NormalizationContext(IReadOnlyDictionary<string, string> parameters)
    {
        Parameters = parameters;
    }

    public IReadOnlyDictionary<string, string> Parameters { get; }
}