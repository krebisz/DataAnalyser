using DataFileReader.Canonical;
using DataVisualiser.Models;

namespace DataVisualiser.Services.Abstractions;

/// <summary>
///     Unified data preparation, filtering, and conversion service.
///     Eliminates duplication across strategies and standardizes CMS handling.
/// </summary>
public interface IDataPreparationService
{
    /// <summary>
    ///     Filters and orders legacy HealthMetricData by date range.
    /// </summary>
    IReadOnlyList<HealthMetricData> PrepareLegacyData(IEnumerable<HealthMetricData>? source, DateTime from, DateTime to);

    /// <summary>
    ///     Filters and orders CMS data samples (returns CMS, not converted).
    /// </summary>
    IReadOnlyList<MetricSample> PrepareCmsData(ICanonicalMetricSeries? cms, DateTime from, DateTime to);

    /// <summary>
    ///     Converts CMS to legacy format (when needed for compatibility).
    ///     Should be avoided when possible - prefer using CMS directly.
    /// </summary>
    IReadOnlyList<HealthMetricData> ConvertCmsToLegacy(ICanonicalMetricSeries cms, DateTime from, DateTime to);

    /// <summary>
    ///     Validates legacy data.
    /// </summary>
    bool ValidateLegacyData(IReadOnlyList<HealthMetricData> data);

    /// <summary>
    ///     Validates CMS data.
    /// </summary>
    bool ValidateCmsData(ICanonicalMetricSeries cms);
}