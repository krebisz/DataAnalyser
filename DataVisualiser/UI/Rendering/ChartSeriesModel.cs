using System.Windows.Media;

namespace DataVisualiser.UI.Rendering;

public sealed class ChartSeriesModel
{
    public string? Name { get; init; }
    public ChartSeriesType SeriesType { get; init; } = ChartSeriesType.Unknown;
    public IReadOnlyList<double?> Values { get; init; } = Array.Empty<double?>();
    public IReadOnlyList<string>? Labels { get; init; }
    public Color? Color { get; init; }
}