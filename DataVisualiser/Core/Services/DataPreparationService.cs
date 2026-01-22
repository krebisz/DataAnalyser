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
    public IReadOnlyList<MetricData> PrepareLegacyData(IEnumerable<MetricData>? source, DateTime from, DateTime to)
    {
        if (source == null)
            return Array.Empty<MetricData>();

        return source.Where(d => d != null && d.Value.HasValue && d.NormalizedTimestamp >= from && d.NormalizedTimestamp <= to).OrderBy(d => d.NormalizedTimestamp).ToList();
    }

    public IReadOnlyList<MetricSample> PrepareCmsData(ICanonicalMetricSeries? cms, DateTime from, DateTime to)
    {
        if (cms == null || cms.Samples.Count == 0)
            return Array.Empty<MetricSample>();

        return cms.Samples.Where(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= from && s.Timestamp.LocalDateTime <= to).OrderBy(s => s.Timestamp.LocalDateTime).ToList();
    }

    public IReadOnlyList<MetricData> ConvertCmsToLegacy(ICanonicalMetricSeries cms, DateTime from, DateTime to)
    {
        if (cms == null)
            throw new ArgumentNullException(nameof(cms));

        // Use existing helper for consistency
        return CmsConversionHelper.ConvertSamplesToHealthMetricData(cms, from, to).ToList();
    }
}