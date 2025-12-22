using DataVisualiser.Charts.Computation;

namespace DataVisualiser.Charts.Parity
{
    /// <summary>
    /// Adapter for converting ChartComputationResult to Parity execution results.
    /// Centralizes conversion logic to avoid duplication.
    /// </summary>
    public static class ParityResultAdapter
    {
        public static LegacyExecutionResult ToLegacyExecutionResult(ChartComputationResult? result)
        {
            if (result == null)
                return new LegacyExecutionResult { Series = Array.Empty<ParitySeries>() };

            var series = new List<ParitySeries>();

            // Primary series
            if (result.PrimaryRawValues != null && result.PrimaryRawValues.Count > 0)
            {
                series.Add(new ParitySeries
                {
                    SeriesKey = "Primary",
                    Points = result.Timestamps
                        .Zip(result.PrimaryRawValues, (t, v) => new ParityPoint { Time = t, Value = v })
                        .ToList()
                });
            }

            // Secondary series
            if (result.SecondaryRawValues != null && result.SecondaryRawValues.Count > 0)
            {
                series.Add(new ParitySeries
                {
                    SeriesKey = "Secondary",
                    Points = result.Timestamps
                        .Zip(result.SecondaryRawValues, (t, v) => new ParityPoint { Time = t, Value = v })
                        .ToList()
                });
            }

            // Multi-series support
            if (result.Series != null && result.Series.Count > 0)
            {
                foreach (var s in result.Series)
                {
                    series.Add(new ParitySeries
                    {
                        SeriesKey = s.SeriesId,
                        Points = s.Timestamps
                            .Zip(s.RawValues, (t, v) => new ParityPoint { Time = t, Value = v })
                            .ToList()
                    });
                }
            }

            return new LegacyExecutionResult { Series = series };
        }

        public static CmsExecutionResult ToCmsExecutionResult(ChartComputationResult? result)
        {
            // CMS and Legacy produce same ChartComputationResult structure
            return new CmsExecutionResult
            {
                Series = ToLegacyExecutionResult(result).Series
            };
        }

        public static IReadOnlyList<ParitySeries> AdaptSeriesResultsToParitySeries(List<SeriesResult>? series)
        {
            if (series == null || series.Count == 0)
                return Array.Empty<ParitySeries>();

            return series.Select(s => new ParitySeries
            {
                SeriesKey = s.SeriesId,
                Points = s.Timestamps.Zip(
                    s.RawValues,
                    (t, v) => new ParityPoint
                    {
                        Time = t,
                        Value = v
                    }).ToList()
            }).ToList();
        }
    }
}
