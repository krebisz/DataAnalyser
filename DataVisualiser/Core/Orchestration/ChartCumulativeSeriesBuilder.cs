using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Computation.TimeSeries;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Orchestration;

internal static class ChartCumulativeSeriesBuilder
{
    internal static (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) Build(
        ChartComputationResult result,
        IChartComputationStrategy strategy,
        string primaryLabel,
        string? secondaryLabel)
    {
        if (result.Series != null && result.Series.Count > 0)
            return CumulativeSeriesCalculator.BuildFromSeries(result.Series);

        return CumulativeSeriesCalculator.BuildFromLegacy(
            result,
            strategy.PrimaryLabel ?? primaryLabel,
            strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty);
    }
}
