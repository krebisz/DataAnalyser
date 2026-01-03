using DataFileReader.Canonical;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using MetricSample = DataFileReader.Canonical.MetricSample;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Implementation of IDataPreparationService.
///     Provides unified data preparation, filtering, and conversion.
/// </summary>
public sealed class DataPreparationService : IDataPreparationService
{
    public IReadOnlyList<HealthMetricData> PrepareLegacyData(IEnumerable<HealthMetricData>? source, DateTime from, DateTime to)
    {
        if (source == null)
            return Array.Empty<HealthMetricData>();

        return source.Where(d => d != null && d.Value.HasValue && d.NormalizedTimestamp >= from && d.NormalizedTimestamp <= to).
            OrderBy(d => d.NormalizedTimestamp).
            ToList();
    }

    public IReadOnlyList<MetricSample> PrepareCmsData(ICanonicalMetricSeries? cms, DateTime from, DateTime to)
    {
        if (cms == null || cms.Samples.Count == 0)
            return Array.Empty<MetricSample>();

        return cms.Samples.Where(s => s.Value.HasValue && s.Timestamp.DateTime >= from && s.Timestamp.DateTime <= to).
            OrderBy(s => s.Timestamp).
            ToList();
    }

    public IReadOnlyList<HealthMetricData> ConvertCmsToLegacy(ICanonicalMetricSeries cms, DateTime from, DateTime to)
    {
        if (cms == null)
            throw new ArgumentNullException(nameof(cms));

        // Use existing helper for consistency
        return CmsConversionHelper.ConvertSamplesToHealthMetricData(cms, from, to).
            ToList();
    }

    public bool ValidateLegacyData(IReadOnlyList<HealthMetricData> data)
    {
        if (data == null || data.Count == 0)
            return false;

        // Check for null values, invalid timestamps, etc.
        return data.All(d => d != null && d.Value.HasValue && d.NormalizedTimestamp != default);
    }

    public bool ValidateCmsData(ICanonicalMetricSeries cms)
    {
        if (cms == null)
            return false;

        // Check for valid metric ID, unit, samples
        return cms.MetricId != null && cms.Unit != null && cms.Samples != null;
    }
}
