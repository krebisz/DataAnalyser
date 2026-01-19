using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DataVisualiser.UI.Rendering;

public sealed class ChartRenderModel
{
    public string? Title { get; init; }
    public bool IsVisible { get; init; }

    public IReadOnlyList<ChartSeriesModel> Series { get; init; } = Array.Empty<ChartSeriesModel>();
    public IReadOnlyList<ChartAxisModel> AxesX { get; init; } = Array.Empty<ChartAxisModel>();
    public IReadOnlyList<ChartAxisModel> AxesY { get; init; } = Array.Empty<ChartAxisModel>();
    public ChartLegendModel? Legend { get; init; }
    public ChartInteractionModel? Interactions { get; init; }
    public IReadOnlyList<ChartOverlayModel> Overlays { get; init; } = Array.Empty<ChartOverlayModel>();
    public IReadOnlyList<ChartFacetModel> Facets { get; init; } = Array.Empty<ChartFacetModel>();
}

public enum ChartSeriesType
{
    Unknown,
    Line,
    Column,
    Area,
    Pie,
    Scatter
}

public sealed class ChartSeriesModel
{
    public string? Name { get; init; }
    public ChartSeriesType SeriesType { get; init; } = ChartSeriesType.Unknown;
    public IReadOnlyList<double?> Values { get; init; } = Array.Empty<double?>();
    public IReadOnlyList<string>? Labels { get; init; }
    public Color? Color { get; init; }
}

public sealed class ChartAxisModel
{
    public string? Title { get; init; }
    public IReadOnlyList<string>? Labels { get; init; }
    public double? Min { get; init; }
    public double? Max { get; init; }
}

public sealed class ChartLegendModel
{
    public bool IsVisible { get; init; } = true;
    public ChartLegendPlacement Placement { get; init; } = ChartLegendPlacement.Right;
    public IReadOnlyList<ChartLegendItemModel> Items { get; init; } = Array.Empty<ChartLegendItemModel>();
}

public enum ChartLegendPlacement
{
    Left,
    Right,
    Top,
    Bottom
}

public sealed class ChartLegendItemModel
{
    public string? Name { get; init; }
    public bool IsVisible { get; init; } = true;
}

public sealed class ChartInteractionModel
{
    public bool EnableZoomX { get; init; }
    public bool EnableZoomY { get; init; }
    public bool EnablePanX { get; init; }
    public bool EnablePanY { get; init; }
    public bool Hoverable { get; init; }
}

public sealed class ChartOverlayModel
{
    public string? Kind { get; init; }
    public string? Label { get; init; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

public sealed class ChartFacetModel
{
    public string? Title { get; init; }
    public IReadOnlyList<ChartSeriesModel> Series { get; init; } = Array.Empty<ChartSeriesModel>();
}
