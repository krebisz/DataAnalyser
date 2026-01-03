namespace DataVisualiser.Charts.Parity;

public sealed class CmsExecutionResult
{
    public IReadOnlyList<ParitySeries> Series { get; init; } = Array.Empty<ParitySeries>();
}