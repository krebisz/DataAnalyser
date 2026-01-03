using DataFileReader.Canonical;
using DataFileReader.Helper;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Data.Repositories;

/// <summary>
///     Service for fetching Canonical Metric Series data for DataVisualiser.
///     Phase 4 Integration: This service provides CMS-based data access alongside
///     the legacy HealthMetricData path. It enables explicit opt-in to CMS workflows.
///     Architecture:
///     - Uses HealthMetricToCmsMapper to convert stored HealthMetric records to CMS
///     - Provides adapter methods to convert CMS back to HealthMetricData for compatibility
///     - Supports parallel operation with legacy DataFetcher
/// </summary>
public class CmsDataService
{
    private readonly HealthMetricToCmsMapper _cmsMapper;
    private readonly string _connectionString;
    private readonly DataFetcher _legacyFetcher;

    public CmsDataService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        _connectionString = connectionString;
        _legacyFetcher = new DataFetcher(connectionString);
        _cmsMapper = new HealthMetricToCmsMapper(new CanonicalMetricIdentityResolver());
    }

    /// <summary>
    ///     Fetches CMS data for a specific canonical metric identity.
    /// </summary>
    public async Task<IReadOnlyList<ICanonicalMetricSeries>> GetCmsByCanonicalIdAsync(string canonicalMetricId, DateTime from, DateTime to)
    {
        // For Phase 4, we still read from HealthMetrics table and convert to CMS
        // In future phases, CMS may be stored directly

        // Determine which metric type/subtype to query based on canonical ID
        var (metricType, subtype) = CanonicalMetricMapping.ToLegacyFields(canonicalMetricId);
        if (metricType == null)
            return Array.Empty<ICanonicalMetricSeries>();

        // Fetch legacy data
        var legacyData = await _legacyFetcher.GetHealthMetricsDataByBaseType(metricType, subtype, from, to);

        // Convert to HealthMetric records (simplified - in practice, you'd query HealthMetric directly)
        var healthMetrics = legacyData.Select(d => new HealthMetric
        {
            Provider = d.Provider ?? "Unknown",
            MetricType = metricType,
            MetricSubtype = subtype ?? string.Empty,
            SourceFile = "Database",
            NormalizedTimestamp = d.NormalizedTimestamp,
            Value = d.Value,
            Unit = d.Unit ?? string.Empty
        }).
            ToList();

        // Map to CMS
        return _cmsMapper.Map(healthMetrics);
    }

    /// <summary>
    ///     Fetches CMS data and converts to HealthMetricData for backward compatibility.
    ///     This allows gradual migration of strategies.
    /// </summary>
    public async Task<IEnumerable<HealthMetricData>> GetHealthMetricDataFromCmsAsync(string canonicalMetricId, DateTime from, DateTime to)
    {
        var cmsList = await GetCmsByCanonicalIdAsync(canonicalMetricId, from, to);

        // Convert CMS back to HealthMetricData for compatibility
        return CmsConversionHelper.ConvertMultipleCmsToHealthMetricData(cmsList, from, to);
    }

    /// <summary>
    ///     Checks if CMS data is available for a given canonical metric ID.
    /// </summary>
    public async Task<bool> IsCmsAvailableAsync(string canonicalMetricId)
    {
        var (metricType, _) = CanonicalMetricMapping.ToLegacyFields(canonicalMetricId);
        if (metricType == null)
            return false;

        // Check if data exists in legacy table
        var count = await _legacyFetcher.GetRecordCount(metricType);
        return count > 0;
    }
}