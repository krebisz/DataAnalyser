namespace DataVisualiser.UI.Charts.Rendering;

public sealed class UiChartRenderModel
{
    public string? ChartName { get; init; }
    public string? Title { get; init; }
    public bool IsVisible { get; init; }

    public IReadOnlyList<ChartSeriesModel> Series { get; init; } = Array.Empty<ChartSeriesModel>();
    public IReadOnlyList<ChartAxisModel> AxesX { get; init; } = Array.Empty<ChartAxisModel>();
    public IReadOnlyList<ChartAxisModel> AxesY { get; init; } = Array.Empty<ChartAxisModel>();
    public ChartLegendModel? Legend { get; init; }
    public ChartInteractionModel? Interactions { get; init; }
    public IReadOnlyList<ChartOverlayModel> Overlays { get; init; } = Array.Empty<ChartOverlayModel>();
    public IReadOnlyList<ChartFacetModel> Facets { get; init; } = Array.Empty<ChartFacetModel>();

    public bool HasRenderableContent => Series.Count > 0 || Facets.Any(facet => facet.Series.Count > 0);
}
