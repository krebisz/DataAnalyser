using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Core.Computation.Results;

public static class ComputedSeriesResultAdapters
{
    public static SeriesResult ToSeriesResult(ComputedSeriesResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new SeriesResult
        {
            SeriesId = result.Id,
            DisplayName = result.Label,
            Timestamps = result.Timeline.ToList(),
            RawValues = result.RawValues.ToList(),
            Smoothed = result.SmoothedValues.ToList()
        };
    }

    public static ChartComputationResult ToChartComputationResult(ComputedSeriesResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ChartComputationResult
        {
            Timestamps = result.Timeline.ToList(),
            NormalizedIntervals = result.Timeline.ToList(),
            IntervalIndices = Enumerable.Range(0, result.Timeline.Count).ToList(),
            PrimaryRawValues = result.RawValues.ToList(),
            PrimarySmoothed = result.SmoothedValues.ToList(),
            Series = [ToSeriesResult(result)]
        };
    }
}
