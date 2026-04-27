namespace DataVisualiser.UI.Charts.Presentation;

public sealed class ChartAxisModel
{
    public string? Title { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public double? Min { get; init; }
    public double? Max { get; init; }
}
