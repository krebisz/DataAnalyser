using DataVisualiser.Core.Computation.Results;

namespace DataVisualiser.Core.Computation.TimeSeries;

public static class CumulativeSeriesCalculator
{
    public static (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildFromSeries(IReadOnlyList<SeriesResult> seriesResults)
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
            var alignedRaw = TimeSeriesAlignment.AlignSeriesToTimeline(series.Timestamps, series.RawValues, mainTimeline);
            var alignedSmooth = TimeSeriesAlignment.AlignSeriesToTimeline(series.Timestamps, series.Smoothed ?? series.RawValues, mainTimeline);

            originalSeries.Add(new SeriesResult
            {
                SeriesId = series.SeriesId,
                DisplayName = series.DisplayName,
                Timestamps = mainTimeline.ToList(),
                RawValues = alignedRaw.ToList(),
                Smoothed = alignedSmooth.ToList()
            });

            SeriesMath.AddInto(cumulativeRaw, alignedRaw);
            SeriesMath.AddInto(cumulativeSmoothed, alignedSmooth);

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

    public static (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildFromLegacy(
        ChartComputationResult result,
        string primaryLabel,
        string secondaryLabel)
    {
        var timeline = result.Timestamps ?? new List<DateTime>();
        if (timeline.Count == 0)
            return (null, null);

        var cumulativeRaw = new double[timeline.Count];
        var cumulativeSmoothed = new double[timeline.Count];
        var cumulativeSeries = new List<SeriesResult>();
        var originalSeries = new List<SeriesResult>();

        SeriesMath.AddInto(cumulativeRaw, result.PrimaryRawValues);
        SeriesMath.AddInto(cumulativeSmoothed, result.PrimarySmoothed.Count > 0 ? result.PrimarySmoothed : result.PrimaryRawValues);

        originalSeries.Add(new SeriesResult
        {
            SeriesId = "primary-original",
            DisplayName = primaryLabel,
            Timestamps = timeline.ToList(),
            RawValues = result.PrimaryRawValues.ToList(),
            Smoothed = result.PrimarySmoothed.Count > 0 ? result.PrimarySmoothed.ToList() : result.PrimaryRawValues.ToList()
        });

        cumulativeSeries.Add(new SeriesResult
        {
            SeriesId = "primary",
            DisplayName = primaryLabel,
            Timestamps = timeline.ToList(),
            RawValues = cumulativeRaw.ToList(),
            Smoothed = cumulativeSmoothed.ToList()
        });

        if (result.SecondaryRawValues != null && result.SecondaryRawValues.Count > 0)
        {
            SeriesMath.AddInto(cumulativeRaw, result.SecondaryRawValues);
            var secondarySmooth = result.SecondarySmoothed != null && result.SecondarySmoothed.Count > 0 ? result.SecondarySmoothed : result.SecondaryRawValues;
            SeriesMath.AddInto(cumulativeSmoothed, secondarySmooth);

            originalSeries.Add(new SeriesResult
            {
                SeriesId = "secondary-original",
                DisplayName = secondaryLabel,
                Timestamps = timeline.ToList(),
                RawValues = result.SecondaryRawValues.ToList(),
                Smoothed = secondarySmooth.ToList()
            });

            cumulativeSeries.Add(new SeriesResult
            {
                SeriesId = "secondary",
                DisplayName = secondaryLabel,
                Timestamps = timeline.ToList(),
                RawValues = cumulativeRaw.ToList(),
                Smoothed = cumulativeSmoothed.ToList()
            });
        }

        return (cumulativeSeries, originalSeries);
    }
}
