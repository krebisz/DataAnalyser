using DataVisualiser.Core.Computation.Results;

namespace DataVisualiser.Core.Rendering.Helpers;

public sealed class ChartStackingTooltipState
{
    public ChartStackingTooltipState(bool includeTotal, bool isCumulative, IReadOnlyList<SeriesResult>? originalSeries = null)
    {
        IncludeTotal = includeTotal;
        IsCumulative = isCumulative;
        OriginalSeries = originalSeries;
    }

    public bool IncludeTotal { get; }
    public bool IsCumulative { get; }
    public IReadOnlyList<SeriesResult>? OriginalSeries { get; }
}