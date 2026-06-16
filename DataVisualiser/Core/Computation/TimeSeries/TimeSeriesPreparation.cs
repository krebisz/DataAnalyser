using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Computation.TimeSeries;

public static class TimeSeriesPreparation
{
    public static PreparedTimeSeriesPair Prepare(
        IReadOnlyList<MetricData> primarySeries,
        IReadOnlyList<MetricData> secondarySeries,
        int smoothingWindow)
    {
        var timestamps = TimeSeriesAlignment.BuildUnifiedDailyTimeline(primarySeries, secondarySeries);
        var rawValues1 = TimeSeriesAlignment.AlignDailyForwardFilledValues(primarySeries, timestamps, ComputationDefaults.ForwardFillSeedValue);
        var rawValues2 = TimeSeriesAlignment.AlignDailyForwardFilledValues(secondarySeries, timestamps, ComputationDefaults.ForwardFillSeedValue);

        return new PreparedTimeSeriesPair(
            timestamps,
            rawValues1,
            rawValues2,
            SeriesMath.Smooth(rawValues1, smoothingWindow),
            SeriesMath.Smooth(rawValues2, smoothingWindow),
            SeriesMath.Difference(rawValues1, rawValues2),
            SeriesMath.Ratio(rawValues1, rawValues2),
            SeriesMath.NormalizeToMax(rawValues1),
            SeriesMath.NormalizeToMax(rawValues2));
    }
}

public sealed record PreparedTimeSeriesPair(
    IReadOnlyList<DateTime> Timestamps,
    IReadOnlyList<double> RawValues1,
    IReadOnlyList<double> RawValues2,
    IReadOnlyList<double> SmoothedValues1,
    IReadOnlyList<double> SmoothedValues2,
    IReadOnlyList<double> DifferenceValues,
    IReadOnlyList<double> RatioValues,
    IReadOnlyList<double> NormalizedValues1,
    IReadOnlyList<double> NormalizedValues2);
