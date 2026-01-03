namespace DataVisualiser.Core.Rendering.Models;

/// <summary>
///     Controls whether charts render raw, smoothed, or both series.
/// </summary>
public enum ChartSeriesMode
{
    SmoothedOnly   = 0,
    RawOnly        = 1,
    RawAndSmoothed = 2
}