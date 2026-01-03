using System.Windows.Media;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Models;

namespace DataVisualiser.Charts.Rendering;

public sealed class ChartRenderModel
{
    public string PrimarySeriesName { get; init; } = "Primary";
    public string SecondarySeriesName { get; init; } = "Secondary";

    public IList<double> PrimaryRaw { get; init; } = Array.Empty<double>();
    public IList<double>? SecondaryRaw { get; init; }

    public IList<double> PrimarySmoothed { get; init; } = Array.Empty<double>();
    public IList<double>? SecondarySmoothed { get; init; }

    public Color PrimaryColor { get; init; } = Colors.DarkGray;
    public Color SecondaryColor { get; init; } = Colors.Red;

    public string? Unit { get; init; }

    public List<DateTime> Timestamps { get; init; } = new();
    public List<int> IntervalIndices { get; init; } = new();
    public List<DateTime> NormalizedIntervals { get; init; } = new();
    public TickInterval TickInterval { get; init; }

    /// <summary>
    ///     Controls which series types are drawn (raw, smoothed, both).
    ///     Default keeps current behaviour (raw + smoothed).
    /// </summary>
    public ChartSeriesMode SeriesMode { get; init; } = ChartSeriesMode.RawAndSmoothed;

    // New properties for proper label formatting
    public string? MetricType { get; init; }
    public string? PrimarySubtype { get; init; }
    public string? SecondarySubtype { get; init; }
    public string? OperationType { get; init; } // "-", "/", "~", or null for independent charts
    public bool IsOperationChart { get; init; } = false; // true for difference, ratio, normalization charts

    // NEW: Multi-series support for Main Chart (when Series is present, it takes precedence over Primary/Secondary)
    public List<SeriesResult>? Series { get; init; }
}