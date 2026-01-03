using DataFileReader.Canonical;
using DataVisualiser.Models;

namespace DataVisualiser.Helper;

/// <summary>
///     Helper for converting Canonical Metric Series to HealthMetricData.
///     Provides a single source of truth for CMS-to-HealthMetricData conversion,
///     eliminating duplication across strategies and services.
/// </summary>
public static class CmsConversionHelper
{
    /// <summary>
    ///     Converts CMS samples to HealthMetricData with optional date range filtering.
    /// </summary>
    /// <param name="cms">The canonical metric series to convert</param>
    /// <param name="from">Optional start date filter (inclusive)</param>
    /// <param name="to">Optional end date filter (inclusive)</param>
    /// <returns>Ordered collection of HealthMetricData</returns>
    public static IEnumerable<HealthMetricData> ConvertSamplesToHealthMetricData(ICanonicalMetricSeries cms, DateTime? from = null, DateTime? to = null)
    {
        if (cms == null)
            throw new ArgumentNullException(nameof(cms));

        return cms.Samples.Where(s => s.Value.HasValue && (!from.HasValue || s.Timestamp.DateTime >= from.Value) && (!to.HasValue || s.Timestamp.DateTime <= to.Value)).
            Select(s => new HealthMetricData
            {
                NormalizedTimestamp = s.Timestamp.DateTime,
                Value = s.Value,
                Unit = cms.Unit.Symbol,
                Provider = cms.Provenance.SourceProvider
            }).
            OrderBy(d => d.NormalizedTimestamp);
    }

    /// <summary>
    ///     Converts multiple CMS instances to HealthMetricData.
    ///     Useful when aggregating data from multiple canonical series.
    /// </summary>
    /// <param name="cmsList">Collection of canonical metric series</param>
    /// <param name="from">Optional start date filter (inclusive)</param>
    /// <param name="to">Optional end date filter (inclusive)</param>
    /// <returns>Ordered collection of HealthMetricData from all CMS instances</returns>
    public static IEnumerable<HealthMetricData> ConvertMultipleCmsToHealthMetricData(IEnumerable<ICanonicalMetricSeries> cmsList, DateTime? from = null, DateTime? to = null)
    {
        if (cmsList == null)
            throw new ArgumentNullException(nameof(cmsList));

        var result = new List<HealthMetricData>();

        foreach (var cms in cmsList)
        {
            if (cms == null)
                continue;

            result.AddRange(ConvertSamplesToHealthMetricData(cms, from, to));
        }

        return result.OrderBy(d => d.NormalizedTimestamp);
    }
}