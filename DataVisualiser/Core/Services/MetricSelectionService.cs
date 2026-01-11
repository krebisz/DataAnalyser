using System.Configuration;
using DataFileReader.Canonical;
using DataVisualiser.Core.Data.Repositories;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services;

public class MetricSelectionService
{
    private readonly CmsDataService _cms; // <-- add
    private readonly string _connectionString;

    public MetricSelectionService(string connectionString)
    {
        _connectionString = connectionString;
        _cms = new CmsDataService(connectionString); // <-- add
    }


    public async Task<(ICanonicalMetricSeries? PrimaryCms, ICanonicalMetricSeries? SecondaryCms, IEnumerable<MetricData> PrimaryLegacy, IEnumerable<MetricData> SecondaryLegacy)> LoadMetricDataWithCmsAsync(MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to, string tableName)
    {
        var dataFetcher = new DataFetcher(_connectionString);
        var cmsService = new CmsDataService(_connectionString);

        // Calculate optimal max records for performance optimization (if enabled via config)
        int? maxRecords = null;
        var enableLimiting = ConfigurationManager.AppSettings["DataVisualiser:EnableSqlResultLimiting"];
        if (bool.TryParse(enableLimiting, out var isEnabled) && isEnabled)
            maxRecords = MathHelper.CalculateOptimalMaxRecords(from, to);

        // -----------------------
        // Legacy loads with result limiting
        // -----------------------
        var primaryLegacyTask = dataFetcher.GetHealthMetricsDataByBaseType(primarySelection.MetricType, primarySelection.QuerySubtype, from, to, tableName, maxRecords);
        var secondaryLegacyTask = secondarySelection != null
                ? dataFetcher.GetHealthMetricsDataByBaseType(secondarySelection.MetricType, secondarySelection.QuerySubtype, from, to, tableName, maxRecords)
                : Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

        // ---------------------------------
        // Canonical ID resolution (explicit)
        // ---------------------------------
        var primaryCanonicalId = !string.Equals(primarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase)
                ? CanonicalMetricMapping.FromLegacyFields(primarySelection.MetricType, primarySelection.QuerySubtype)
                : null;
        var secondaryCanonicalId = secondarySelection != null && !string.Equals(secondarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase)
                ? CanonicalMetricMapping.FromLegacyFields(secondarySelection.MetricType, secondarySelection.QuerySubtype)
                : null;

        // ------------------------
        // CMS availability checks
        // ------------------------
        Task<IReadOnlyList<ICanonicalMetricSeries>>? primaryCmsTask = null;
        Task<IReadOnlyList<ICanonicalMetricSeries>>? secondaryCmsTask = null;

        if (primaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(primaryCanonicalId))
            primaryCmsTask = cmsService.GetCmsByCanonicalIdAsync(primaryCanonicalId, from, to);

        if (secondaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(secondaryCanonicalId))
            secondaryCmsTask = cmsService.GetCmsByCanonicalIdAsync(secondaryCanonicalId, from, to);

        // -------------------------
        // Await everything together
        // -------------------------
        await Task.WhenAll(primaryLegacyTask, secondaryLegacyTask, primaryCmsTask ?? Task.CompletedTask, secondaryCmsTask ?? Task.CompletedTask);

        return (PrimaryCms: primaryCmsTask?.Result.FirstOrDefault(), SecondaryCms: secondaryCmsTask?.Result.FirstOrDefault(), PrimaryLegacy: primaryLegacyTask.Result, SecondaryLegacy: secondaryLegacyTask.Result);
    }


    // ------------------------------------------------------------
    // LOAD METRIC DATA (PRIMARY + SECONDARY)
    // ------------------------------------------------------------
    public async Task<(IEnumerable<MetricData> Primary, IEnumerable<MetricData> Secondary)> LoadMetricDataAsync(string baseType, string? primarySubtype, string? secondarySubtype, DateTime from, DateTime to, string tableName)
    {
        var dataFetcher = new DataFetcher(_connectionString);

        // Calculate optimal max records for performance optimization (if enabled via config)
        int? maxRecords = null;
        var enableLimiting = ConfigurationManager.AppSettings["DataVisualiser:EnableSqlResultLimiting"];
        if (bool.TryParse(enableLimiting, out var isEnabled) && isEnabled)
            maxRecords = MathHelper.CalculateOptimalMaxRecords(from, to);

        var primaryTask = dataFetcher.GetHealthMetricsDataByBaseType(baseType, primarySubtype, from, to, tableName, maxRecords);

        var secondaryTask = dataFetcher.GetHealthMetricsDataByBaseType(baseType, secondarySubtype, from, to, tableName, maxRecords);

        await Task.WhenAll(primaryTask, secondaryTask);

        return (primaryTask.Result, secondaryTask.Result);
    }

    // ------------------------------------------------------------
    // LOAD METRIC TYPES (BASE METRIC TYPES)
    // ------------------------------------------------------------
    public async Task<List<string>> LoadMetricTypesAsync(string tableName)
    {
        var dataFetcher = new DataFetcher(_connectionString);
        var baseMetricTypes = await dataFetcher.GetBaseMetricTypes(tableName);

        return baseMetricTypes.ToList();
    }

    // ------------------------------------------------------------
    // LOAD SUBTYPES
    // ------------------------------------------------------------
    public async Task<List<string>> LoadSubtypesAsync(string metricType, string tableName)
    {
        var dataFetcher = new DataFetcher(_connectionString);
        var subtypes = await dataFetcher.GetSubtypesForBaseType(metricType, tableName);

        return subtypes.ToList();
    }

    // ------------------------------------------------------------
    // LOAD DATE RANGE
    // ------------------------------------------------------------
    public async Task<(DateTime MinDate, DateTime MaxDate)?> LoadDateRangeAsync(string metricType, string? subtype, string tableName)
    {
        var dataFetcher = new DataFetcher(_connectionString);
        var dateRange = await dataFetcher.GetBaseTypeDateRange(metricType, subtype, tableName);

        return dateRange;
    }
}
