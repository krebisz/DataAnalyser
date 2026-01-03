using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Unified data preparation, filtering, and conversion service.
///     Eliminates duplication across strategies and standardizes CMS handling.
/// </summary>
public interface IDataPreparationService
{
    /// <summary>
    ///     Filters and orders legacy MetricData by date range.
    /// </summary>
    IReadOnlyList<MetricData> PrepareLegacyData(IEnumerable<MetricData>? source, DateTime from, DateTime to);

    /// <summary>
    ///     Filters and orders CMS data samples (returns CMS, not converted).
    /// </summary>
    IReadOnlyList<MetricSample> PrepareCmsData(ICanonicalMetricSeries? cms, DateTime from, DateTime to);

    /// <summary>
    ///     Converts CMS to legacy format (when needed for compatibility).
    ///     Should be avoided when possible - prefer using CMS directly.
    /// </summary>
    IReadOnlyList<MetricData> ConvertCmsToLegacy(ICanonicalMetricSeries cms, DateTime from, DateTime to);
}