namespace DataVisualiser.VNext.Contracts;

public sealed record ChartProgramRequest
{
    public ChartProgramRequest(
        ChartProgramKind kind,
        ChartDisplayMode displayMode = ChartDisplayMode.Regular,
        string? titleOverride = null,
        IReadOnlyList<SeriesOperationRequest>? seriesOperations = null)
    {
        Kind = kind;
        DisplayMode = displayMode;
        TitleOverride = titleOverride;
        SeriesOperations = seriesOperations?.ToArray() ?? [];
    }

    public ChartProgramKind Kind { get; }
    public ChartDisplayMode DisplayMode { get; }
    public string? TitleOverride { get; }
    public IReadOnlyList<SeriesOperationRequest> SeriesOperations { get; }

    public static ChartProgramRequest MainProgram(ChartDisplayMode displayMode = ChartDisplayMode.Regular) => new(ChartProgramKind.Main, displayMode);
    public static ChartProgramRequest Normalized() => new(ChartProgramKind.Normalized);
    public static ChartProgramRequest Difference() => new(ChartProgramKind.Difference);
    public static ChartProgramRequest Ratio() => new(ChartProgramKind.Ratio);
    public static ChartProgramRequest Transform(string title, IReadOnlyList<SeriesOperationRequest> seriesOperations, ChartDisplayMode displayMode = ChartDisplayMode.Regular) =>
        new(ChartProgramKind.Transform, displayMode, title, seriesOperations);
}
