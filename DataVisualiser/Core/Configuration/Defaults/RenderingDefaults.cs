using System.Collections.Generic;

namespace DataVisualiser.Core.Configuration.Defaults;

/// <summary>
///     Centralized rendering defaults to reduce scattered magic numbers.
/// </summary>
public static class RenderingDefaults
{
    public const double MaxColumnWidth = 40.0;

    public const double HoverPopupOffsetPx = 10;

    public const double ChartTickSpacingPx = 30.0;
    public const double ChartPaddingPx = 100.0;
    public const double ChartMaxHeightPx = 2000.0;

    public const int TooltipHoverCheckIntervalMs = 100;
    public const int TooltipHoverTimeoutMs = 300;
    public const double TooltipMaxHeightPx = 400.0;
    public const double TooltipHeaderFontSize = 14.0;
    public const double TooltipSubHeaderFontSize = 12.0;
    public const double TooltipRowFontSize = 11.0;

    public const string NormalizedChartName = "ChartNormControl";
    public const string TransformChartName = "ChartTransformResultControl";
    public const string WeekdayTrendChartName = "ChartWeekdayTrend";
    public const string WeekdayTrendPolarChartName = "ChartWeekdayTrendPolar";

    public static readonly HashSet<string> DeltaTooltipChartNames = new(StringComparer.OrdinalIgnoreCase)
    {
            NormalizedChartName,
            TransformChartName,
            WeekdayTrendChartName,
            WeekdayTrendPolarChartName
    };
}
