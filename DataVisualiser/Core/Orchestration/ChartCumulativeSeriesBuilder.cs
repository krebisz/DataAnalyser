using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Helpers;
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
            return BuildFromMulti(result.Series);

        return BuildFromLegacy(result, strategy, primaryLabel, secondaryLabel);
    }

    private static (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildFromMulti(IReadOnlyList<SeriesResult> seriesResults)
    {
        var mainTimeline = seriesResults.SelectMany(s => s.Timestamps).Distinct().OrderBy(t => t).ToList();
        if (mainTimeline.Count == 0)
            return (null, null);

        var cumulativeRaw = new double[mainTimeline.Count];
        var cumulativeSmoothed = new double[mainTimeline.Count];
        var cumulativeSeries = new List<SeriesResult>();
        var originalSeries = new List<SeriesResult>();

        foreach (var series in seriesResults)
        {
            var alignedRaw = SeriesAlignmentHelper.AlignSeriesToTimeline(series.Timestamps, series.RawValues, mainTimeline);
            var alignedSmooth = SeriesAlignmentHelper.AlignSeriesToTimeline(series.Timestamps, series.Smoothed ?? series.RawValues, mainTimeline);

            originalSeries.Add(new SeriesResult
            {
                SeriesId = series.SeriesId,
                DisplayName = series.DisplayName,
                Timestamps = mainTimeline.ToList(),
                RawValues = alignedRaw.ToList(),
                Smoothed = alignedSmooth.ToList()
            });

            AddIntoCumulative(cumulativeRaw, alignedRaw);
            AddIntoCumulative(cumulativeSmoothed, alignedSmooth);

            cumulativeSeries.Add(new SeriesResult
            {
                SeriesId = series.SeriesId,
                DisplayName = series.DisplayName,
                Timestamps = mainTimeline.ToList(),
                RawValues = cumulativeRaw.ToList(),
                Smoothed = cumulativeSmoothed.ToList()
            });
        }

        return (cumulativeSeries, originalSeries);
    }

    private static (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildFromLegacy(
        ChartComputationResult result,
        IChartComputationStrategy strategy,
        string primaryLabel,
        string? secondaryLabel)
    {
        var timeline = result.Timestamps ?? new List<DateTime>();
        if (timeline.Count == 0)
            return (null, null);

        var cumulativeRaw = new double[timeline.Count];
        var cumulativeSmoothed = new double[timeline.Count];
        var cumulativeSeries = new List<SeriesResult>();
        var originalSeries = new List<SeriesResult>();

        AddIntoCumulative(cumulativeRaw, result.PrimaryRawValues);
        AddIntoCumulative(cumulativeSmoothed, result.PrimarySmoothed.Count > 0 ? result.PrimarySmoothed : result.PrimaryRawValues);

        originalSeries.Add(new SeriesResult
        {
            SeriesId = "primary-original",
            DisplayName = strategy.PrimaryLabel ?? primaryLabel,
            Timestamps = timeline.ToList(),
            RawValues = result.PrimaryRawValues.ToList(),
            Smoothed = result.PrimarySmoothed.Count > 0 ? result.PrimarySmoothed.ToList() : result.PrimaryRawValues.ToList()
        });

        cumulativeSeries.Add(new SeriesResult
        {
            SeriesId = "primary",
            DisplayName = strategy.PrimaryLabel ?? primaryLabel,
            Timestamps = timeline.ToList(),
            RawValues = cumulativeRaw.ToList(),
            Smoothed = cumulativeSmoothed.ToList()
        });

        if (result.SecondaryRawValues != null && result.SecondaryRawValues.Count > 0)
        {
            AddIntoCumulative(cumulativeRaw, result.SecondaryRawValues);
            var secondarySmooth = result.SecondarySmoothed != null && result.SecondarySmoothed.Count > 0 ? result.SecondarySmoothed : result.SecondaryRawValues;
            AddIntoCumulative(cumulativeSmoothed, secondarySmooth);

            originalSeries.Add(new SeriesResult
            {
                SeriesId = "secondary-original",
                DisplayName = strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty,
                Timestamps = timeline.ToList(),
                RawValues = result.SecondaryRawValues.ToList(),
                Smoothed = secondarySmooth.ToList()
            });

            cumulativeSeries.Add(new SeriesResult
            {
                SeriesId = "secondary",
                DisplayName = strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty,
                Timestamps = timeline.ToList(),
                RawValues = cumulativeRaw.ToList(),
                Smoothed = cumulativeSmoothed.ToList()
            });
        }

        return (cumulativeSeries, originalSeries);
    }

    private static void AddIntoCumulative(double[] cumulative, IList<double> values)
    {
        var count = Math.Min(cumulative.Length, values.Count);
        for (var i = 0; i < count; i++)
        {
            var value = values[i];
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            cumulative[i] += value;
        }
    }
}
