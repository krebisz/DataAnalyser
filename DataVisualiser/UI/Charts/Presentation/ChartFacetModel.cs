namespace DataVisualiser.UI.Charts.Presentation;

public sealed class ChartFacetModel
{
    public string? Title { get; init; }
    public IReadOnlyList<ChartSeriesModel> Series { get; init; } = Array.Empty<ChartSeriesModel>();
}
