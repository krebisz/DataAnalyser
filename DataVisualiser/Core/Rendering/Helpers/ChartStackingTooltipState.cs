using System;
using System.Collections.Generic;
using System.Linq;
using DataVisualiser.Core.Computation.Results;

namespace DataVisualiser.Core.Rendering.Helpers;

public sealed class ChartStackingTooltipState
{
    public ChartStackingTooltipState(bool includeTotal, bool isCumulative, IReadOnlyList<SeriesResult>? originalSeries = null, IReadOnlyCollection<string>? overlaySeriesNames = null)
    {
        IncludeTotal = includeTotal;
        IsCumulative = isCumulative;
        OriginalSeries = originalSeries;
        OverlaySeriesNames = overlaySeriesNames != null && overlaySeriesNames.Count > 0 ? new HashSet<string>(overlaySeriesNames.Select(NormalizeOverlayName), StringComparer.OrdinalIgnoreCase) : null;
    }

    public bool IncludeTotal { get; }
    public bool IsCumulative { get; }
    public IReadOnlyList<SeriesResult>? OriginalSeries { get; }
    public IReadOnlyCollection<string>? OverlaySeriesNames { get; }

    public static string NormalizeOverlayName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return TrimSuffix(name.Trim());
    }

    private static string TrimSuffix(string name)
    {
        if (name.EndsWith(" (smooth)", StringComparison.OrdinalIgnoreCase))
            return name[..^9];

        if (name.EndsWith(" (raw)", StringComparison.OrdinalIgnoreCase))
            return name[..^6];

        return name;
    }
}
