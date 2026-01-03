namespace DataVisualiser.Charts.Computation;

public sealed class SeriesResult
{
    public string SeriesId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;

    public List<DateTime> Timestamps { get; init; } = new();
    public List<double> RawValues { get; init; } = new();
    public List<double>? Smoothed { get; init; }
}