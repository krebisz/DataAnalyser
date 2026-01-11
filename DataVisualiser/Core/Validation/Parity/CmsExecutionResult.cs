namespace DataVisualiser.Core.Validation.Parity;

public sealed class CmsExecutionResult
{
    public IReadOnlyList<ParitySeries> Series { get; init; } = Array.Empty<ParitySeries>();
}