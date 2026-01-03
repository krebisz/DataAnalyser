namespace DataVisualiser.Validation.Parity;

public sealed class LegacyExecutionResult
{
    public IReadOnlyList<ParitySeries> Series { get; init; } = Array.Empty<ParitySeries>();
}
