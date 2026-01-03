using DataVisualiser.Charts.Computation;

namespace DataVisualiser.Charts.Parity;

/// <summary>
///     Adapter for converting ChartComputationResult to Parity execution results.
///     Centralizes conversion logic to avoid duplication.
/// </summary>
public static class ParityResultAdapter
{
    public static LegacyExecutionResult ToLegacyExecutionResult(ChartComputationResult? result)
    {
        if (result == null)
            return new LegacyExecutionResult
            {
                Series = new List<ParitySeries>()
            };

        var series = new List<ParitySeries>();

        // Create primary series if available
        if (result.PrimaryRawValues?.Count > 0)
            series.Add(CreateSeries("Primary", result.Timestamps, result.PrimaryRawValues));

        // Create secondary series if available
        if (result.SecondaryRawValues?.Count > 0)
            series.Add(CreateSeries("Secondary", result.Timestamps, result.SecondaryRawValues));

        // Create multi-series if available
        if (result.Series?.Count > 0)
            foreach (var seriesItem in result.Series)
                series.Add(CreateSeries(seriesItem.SeriesId, seriesItem.Timestamps, seriesItem.RawValues));

        return new LegacyExecutionResult
        {
            Series = series
        };
    }

    private static ParitySeries CreateSeries(string seriesKey, IReadOnlyList<DateTime> timestamps, IReadOnlyList<double> rawValues)
    {
        return new ParitySeries
        {
            SeriesKey = seriesKey,
            Points = timestamps.Zip(rawValues, (t, v) => new ParityPoint
                {
                    Time = t,
                    Value = v
                }).
                ToList()
        };
    }

    public static CmsExecutionResult ToCmsExecutionResult(ChartComputationResult? result)
    {
        // CMS and Legacy produce same ChartComputationResult structure
        return new CmsExecutionResult
        {
            Series = ToLegacyExecutionResult(result).
                Series
        };
    }

    public static IReadOnlyList<ParitySeries> AdaptSeriesResultsToParitySeries(List<SeriesResult>? series)
    {
        if (series == null || series.Count == 0)
            return Array.Empty<ParitySeries>();

        return series.Select(s => new ParitySeries
            {
                SeriesKey = s.SeriesId,
                Points = s.Timestamps.Zip(s.RawValues, (t, v) => new ParityPoint
                    {
                        Time = t,
                        Value = v
                    }).
                    ToList()
            }).
            ToList();
    }
}