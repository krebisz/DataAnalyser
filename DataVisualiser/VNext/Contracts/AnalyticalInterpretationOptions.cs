namespace DataVisualiser.VNext.Contracts;

public sealed record AnalyticalInterpretationOptions(
    bool IncludeAverageLines = false,
    bool IncludeMedianLines = false,
    bool ExcludeCriticalConfidenceSeriesFromOverlays = false)
{
    public static AnalyticalInterpretationOptions None { get; } = new();
}
