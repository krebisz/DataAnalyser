using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration;

internal static class ChartYAxisDataBuilder
{
    internal static List<MetricData> BuildSyntheticRawData(ChartRenderModel model)
    {
        var metrics = ShouldUseStackedTotals(model)
            ? BuildStackedRawData(model)
            : EnumerateRawPoints(model).Select(p => CreateMetric(p.Timestamp, p.Value, model.Unit)).ToList();

        AppendOverlayRawData(metrics, model);
        return metrics;
    }

    internal static List<double> CollectSmoothedValues(ChartRenderModel model)
    {
        if (ShouldUseStackedTotals(model))
            return BuildStackedSmoothedValues(model);

        var smoothedList = new List<double>();

        if (model.Series != null && model.Series.Count > 0)
        {
            foreach (var series in model.Series)
                if (series.Smoothed != null)
                    smoothedList.AddRange(series.Smoothed);
        }
        else
        {
            if (model.PrimarySmoothed != null)
                smoothedList.AddRange(model.PrimarySmoothed);
            if (model.SecondarySmoothed != null)
                smoothedList.AddRange(model.SecondarySmoothed);
        }

        AppendOverlaySmoothedValues(smoothedList, model);
        return smoothedList;
    }

    internal static void EnsureOverlayExtremes(List<MetricData> rawData, IReadOnlyList<SeriesResult>? overlaySeries, string? unit)
    {
        if (overlaySeries == null || overlaySeries.Count == 0)
            return;

        double? min = null;
        double? max = null;

        foreach (var series in overlaySeries)
        {
            AccumulateMinMax(series.RawValues, ref min, ref max);
            AccumulateMinMax(series.Smoothed, ref min, ref max);
        }

        if (!min.HasValue || !max.HasValue)
            return;

        var timestamp = rawData.FirstOrDefault()?.NormalizedTimestamp ?? DateTime.UtcNow;
        rawData.Add(new MetricData { NormalizedTimestamp = timestamp, Value = (decimal)min.Value, Unit = unit });
        rawData.Add(new MetricData { NormalizedTimestamp = timestamp, Value = (decimal)max.Value, Unit = unit });
    }

    internal static void EnsureStackedBaseline(List<MetricData> rawData, List<double> smoothedValues, string? unit)
    {
        var min = GetMinimumValue(rawData, smoothedValues);
        if (double.IsNaN(min) || min <= 0)
            return;

        var timestamp = rawData.FirstOrDefault()?.NormalizedTimestamp ?? DateTime.UtcNow;
        rawData.Add(new MetricData { NormalizedTimestamp = timestamp, Value = 0m, Unit = unit });
    }

    internal static (double? Min, double? Max) GetOverlayMinMax(IReadOnlyList<SeriesResult> overlaySeries)
    {
        double? min = null;
        double? max = null;

        foreach (var series in overlaySeries)
        {
            AccumulateMinMax(series.RawValues, ref min, ref max);
            AccumulateMinMax(series.Smoothed, ref min, ref max);
        }

        return (min, max);
    }

    private static bool ShouldUseStackedTotals(ChartRenderModel model)
    {
        if (!model.IsStacked)
            return false;

        if (model.Series != null && model.Series.Count > 1)
            return true;

        return model.SecondaryRaw != null || model.SecondarySmoothed != null;
    }

    private static List<MetricData> BuildStackedRawData(ChartRenderModel model)
    {
        var timestamps = GetStackTimeline(model);
        var totals = new double[timestamps.Count];
        var hasValue = new bool[timestamps.Count];

        if (model.Series != null && model.Series.Count > 0)
        {
            foreach (var series in model.Series)
            {
                var aligned = SeriesAlignmentHelper.AlignSeriesToTimeline(series.Timestamps, series.RawValues, timestamps);
                for (var i = 0; i < aligned.Count; i++)
                {
                    var value = aligned[i];
                    if (double.IsNaN(value) || double.IsInfinity(value))
                        continue;

                    totals[i] += value;
                    hasValue[i] = true;
                }
            }
        }
        else
        {
            var primaryRaw = model.PrimaryRaw ?? new List<double>();
            var secondaryRaw = model.SecondaryRaw;
            var count = timestamps.Count;

            for (var i = 0; i < count; i++)
            {
                var sum = 0.0;
                var found = false;

                if (i < primaryRaw.Count && !double.IsNaN(primaryRaw[i]) && !double.IsInfinity(primaryRaw[i]))
                {
                    sum += primaryRaw[i];
                    found = true;
                }

                if (secondaryRaw != null && i < secondaryRaw.Count && !double.IsNaN(secondaryRaw[i]) && !double.IsInfinity(secondaryRaw[i]))
                {
                    sum += secondaryRaw[i];
                    found = true;
                }

                if (found)
                {
                    totals[i] = sum;
                    hasValue[i] = true;
                }
            }
        }

        var metrics = new List<MetricData>(timestamps.Count);
        for (var i = 0; i < timestamps.Count; i++)
        {
            var value = hasValue[i] ? (decimal)totals[i] : (decimal?)null;
            metrics.Add(new MetricData { NormalizedTimestamp = timestamps[i], Value = value, Unit = model.Unit });
        }

        return metrics;
    }

    private static MetricData CreateMetric(DateTime timestamp, double value, string? unit)
    {
        return new MetricData
        {
            NormalizedTimestamp = timestamp,
            Value = double.IsNaN(value) ? null : (decimal)value,
            Unit = unit
        };
    }

    private static IEnumerable<(DateTime Timestamp, double Value)> EnumerateRawPoints(ChartRenderModel model)
    {
        if (model.Series != null && model.Series.Count > 0)
        {
            foreach (var series in model.Series)
            {
                var count = Math.Min(series.Timestamps.Count, series.RawValues.Count);
                for (var i = 0; i < count; i++)
                    yield return (series.Timestamps[i], series.RawValues[i]);
            }

            yield break;
        }

        var timestamps = model.Timestamps;
        var primaryRaw = model.PrimaryRaw ?? new List<double>();
        var secondaryRaw = model.SecondaryRaw;
        var primaryCount = Math.Min(timestamps.Count, primaryRaw.Count);

        for (var i = 0; i < primaryCount; i++)
        {
            yield return (timestamps[i], primaryRaw[i]);

            if (secondaryRaw != null && i < secondaryRaw.Count)
                yield return (timestamps[i], secondaryRaw[i]);
        }
    }

    private static void AppendOverlayRawData(List<MetricData> metrics, ChartRenderModel model)
    {
        if (model.OverlaySeries == null || model.OverlaySeries.Count == 0)
            return;

        foreach (var point in EnumerateOverlayRawPoints(model))
            metrics.Add(CreateMetric(point.Timestamp, point.Value, model.Unit));
    }

    private static IEnumerable<(DateTime Timestamp, double Value)> EnumerateOverlayRawPoints(ChartRenderModel model)
    {
        if (model.OverlaySeries == null)
            yield break;

        foreach (var series in model.OverlaySeries)
        {
            var count = Math.Min(series.Timestamps.Count, series.RawValues.Count);
            for (var i = 0; i < count; i++)
                yield return (series.Timestamps[i], series.RawValues[i]);
        }
    }

    private static void AppendOverlaySmoothedValues(List<double> values, ChartRenderModel model)
    {
        if (model.OverlaySeries == null || model.OverlaySeries.Count == 0)
            return;

        foreach (var series in model.OverlaySeries)
        {
            if (series.Smoothed != null && series.Smoothed.Count > 0)
            {
                values.AddRange(series.Smoothed);
                continue;
            }

            if (series.RawValues != null && series.RawValues.Count > 0)
                values.AddRange(series.RawValues);
        }
    }

    private static List<double> BuildStackedSmoothedValues(ChartRenderModel model)
    {
        var timestamps = GetStackTimeline(model);
        var totals = new double[timestamps.Count];
        var hasValue = new bool[timestamps.Count];

        if (model.Series != null && model.Series.Count > 0)
        {
            foreach (var series in model.Series)
            {
                if (series.Smoothed == null)
                    continue;

                var aligned = SeriesAlignmentHelper.AlignSeriesToTimeline(series.Timestamps, series.Smoothed, timestamps);
                for (var i = 0; i < aligned.Count; i++)
                {
                    var value = aligned[i];
                    if (double.IsNaN(value) || double.IsInfinity(value))
                        continue;

                    totals[i] += value;
                    hasValue[i] = true;
                }
            }
        }
        else
        {
            var primarySmoothed = model.PrimarySmoothed ?? new List<double>();
            var secondarySmoothed = model.SecondarySmoothed;
            var count = timestamps.Count;

            for (var i = 0; i < count; i++)
            {
                var sum = 0.0;
                var found = false;

                if (i < primarySmoothed.Count && !double.IsNaN(primarySmoothed[i]) && !double.IsInfinity(primarySmoothed[i]))
                {
                    sum += primarySmoothed[i];
                    found = true;
                }

                if (secondarySmoothed != null && i < secondarySmoothed.Count && !double.IsNaN(secondarySmoothed[i]) && !double.IsInfinity(secondarySmoothed[i]))
                {
                    sum += secondarySmoothed[i];
                    found = true;
                }

                if (found)
                {
                    totals[i] = sum;
                    hasValue[i] = true;
                }
            }
        }

        var values = new List<double>(timestamps.Count);
        for (var i = 0; i < timestamps.Count; i++)
            values.Add(hasValue[i] ? totals[i] : double.NaN);

        return values;
    }

    private static List<DateTime> GetStackTimeline(ChartRenderModel model)
    {
        var timeline = model.Timestamps;
        if ((timeline == null || timeline.Count == 0) && model.Series != null)
            timeline = model.Series.SelectMany(s => s.Timestamps).Distinct().OrderBy(t => t).ToList();

        return timeline ?? new List<DateTime>();
    }

    private static double GetMinimumValue(List<MetricData> rawData, List<double> smoothedValues)
    {
        double? min = null;

        foreach (var point in rawData)
            if (point.Value.HasValue)
            {
                var value = (double)point.Value.Value;
                min = min.HasValue ? Math.Min(min.Value, value) : value;
            }

        foreach (var value in smoothedValues)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            min = min.HasValue ? Math.Min(min.Value, value) : value;
        }

        return min ?? double.NaN;
    }

    private static void AccumulateMinMax(IReadOnlyList<double>? values, ref double? min, ref double? max)
    {
        if (values == null)
            return;

        foreach (var value in values)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            min = min.HasValue ? Math.Min(min.Value, value) : value;
            max = max.HasValue ? Math.Max(max.Value, value) : value;
        }
    }
}
