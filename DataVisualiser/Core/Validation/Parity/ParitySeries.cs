namespace DataVisualiser.Core.Validation.Parity;

public sealed class ParitySeries
{
    public string SeriesKey { get; init; } = string.Empty;
    public IReadOnlyList<ParityPoint> Points { get; init; } = Array.Empty<ParityPoint>();
}