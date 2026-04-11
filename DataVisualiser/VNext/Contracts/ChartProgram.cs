namespace DataVisualiser.VNext.Contracts;

public sealed record ChartSeriesProgram(
    string Id,
    string Label,
    IReadOnlyList<double> RawValues,
    IReadOnlyList<double> SmoothedValues);

public sealed record ChartProgram(
    ChartProgramKind Kind,
    ChartDisplayMode DisplayMode,
    string Title,
    DateTime From,
    DateTime To,
    IReadOnlyList<DateTime> Timeline,
    IReadOnlyList<ChartSeriesProgram> Series,
    string SourceSignature);
