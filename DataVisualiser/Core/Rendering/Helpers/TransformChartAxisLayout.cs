namespace DataVisualiser.Core.Rendering.Helpers;

public sealed class TransformChartAxisLayout
{
    public double MinValue { get; init; }

    public double MaxValue { get; init; }

    public double? Step { get; init; }

    public bool ShowLabels { get; init; }
}
