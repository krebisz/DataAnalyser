namespace DataVisualiser.UI.Rendering;

public sealed class ChartLegendModel
{
    public bool IsVisible { get; init; } = true;
    public ChartLegendPlacement Placement { get; init; } = ChartLegendPlacement.Right;
    public IReadOnlyList<ChartLegendItemModel> Items { get; init; } = Array.Empty<ChartLegendItemModel>();
}