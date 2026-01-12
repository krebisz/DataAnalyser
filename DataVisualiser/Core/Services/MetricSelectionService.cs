using System.Configuration;
using DataFileReader.Canonical;
using DataVisualiser.Core.Data.Repositories;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.Core.Data;

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


    //public async Task<(ICanonicalMetricSeries? PrimaryCms, ICanonicalMetricSeries? SecondaryCms, IEnumerable<MetricData> PrimaryLegacy, IEnumerable<MetricData> SecondaryLegacy)> LoadMetricDataWithCmsAsync(MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to, string tableName)
    //{
    //    var dataFetcher = new DataFetcher(_connectionString);
    //    var cmsService = new CmsDataService(_connectionString);

    //    // Calculate optimal max records for performance optimization (if enabled via config)
    //    int? maxRecords = null;
    //    var enableLimiting = ConfigurationManager.AppSettings["DataVisualiser:EnableSqlResultLimiting"];
    //    if (bool.TryParse(enableLimiting, out var isEnabled) && isEnabled)
    //        maxRecords = MathHelper.CalculateOptimalMaxRecords(from, to);

    //    // -----------------------
    //    // Legacy loads with result limiting
    //    // -----------------------
    //    var primaryLegacyTask = dataFetcher.GetHealthMetricsDataByBaseType(primarySelection.MetricType, primarySelection.QuerySubtype, from, to, tableName, maxRecords);
    //    var secondaryLegacyTask = secondarySelection != null ? dataFetcher.GetHealthMetricsDataByBaseType(secondarySelection.MetricType, secondarySelection.QuerySubtype, from, to, tableName, maxRecords) : Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

    //    // ---------------------------------
    //    // Canonical ID resolution (explicit)
    //    // ---------------------------------
    //    var primaryCanonicalId = !string.Equals(primarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase) ? CanonicalMetricMapping.FromLegacyFields(primarySelection.MetricType, primarySelection.QuerySubtype) : null;
    //    var secondaryCanonicalId = secondarySelection != null && !string.Equals(secondarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase) ? CanonicalMetricMapping.FromLegacyFields(secondarySelection.MetricType, secondarySelection.QuerySubtype) : null;

    //    // ------------------------
    //    // CMS availability checks
    //    // ------------------------
    //    Task<IReadOnlyList<ICanonicalMetricSeries>>? primaryCmsTask = null;
    //    Task<IReadOnlyList<ICanonicalMetricSeries>>? secondaryCmsTask = null;

    //    if (primaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(primaryCanonicalId))
    //        primaryCmsTask = cmsService.GetCmsByCanonicalIdAsync(primaryCanonicalId, from, to);

    //    if (secondaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(secondaryCanonicalId))
    //        secondaryCmsTask = cmsService.GetCmsByCanonicalIdAsync(secondaryCanonicalId, from, to);

    //    // -------------------------
    //    // Await everything together
    //    // -------------------------
    //    await Task.WhenAll(primaryLegacyTask, secondaryLegacyTask, primaryCmsTask ?? Task.CompletedTask, secondaryCmsTask ?? Task.CompletedTask);

    //    return (PrimaryCms: primaryCmsTask?.Result.FirstOrDefault(), SecondaryCms: secondaryCmsTask?.Result.FirstOrDefault(), PrimaryLegacy: primaryLegacyTask.Result, SecondaryLegacy: secondaryLegacyTask.Result);
    //}
    public async Task<( ICanonicalMetricSeries? PrimaryCms, ICanonicalMetricSeries? SecondaryCms, IEnumerable<MetricData> PrimaryLegacy, IEnumerable<MetricData> SecondaryLegacy)> LoadMetricDataWithCmsAsync(MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to, string tableName)
    {
        var dataFetcher = new DataFetcher(_connectionString);
        var cmsService = new CmsDataService(_connectionString);

        var maxRecords = ResolveMaxRecords(from, to);

        var legacyTasks = StartLegacyLoadTasks(dataFetcher, primarySelection, secondarySelection, from, to, tableName, maxRecords);

        var cmsTasks = await StartCmsLoadTasksAsync(cmsService, primarySelection, secondarySelection, from, to);

        await Task.WhenAll(legacyTasks.Primary, legacyTasks.Secondary, cmsTasks.Primary ?? Task.CompletedTask, cmsTasks.Secondary ?? Task.CompletedTask);

        return (PrimaryCms: cmsTasks.Primary?.Result.FirstOrDefault(), SecondaryCms: cmsTasks.Secondary?.Result.FirstOrDefault(), PrimaryLegacy: legacyTasks.Primary.Result, SecondaryLegacy: legacyTasks.Secondary.Result);
    }

    private static int? ResolveMaxRecords(DateTime from, DateTime to)
    {
        var enableLimiting = ConfigurationManager.AppSettings["DataVisualiser:EnableSqlResultLimiting"];

        if (bool.TryParse(enableLimiting, out var isEnabled) && isEnabled)
            return MathHelper.CalculateOptimalMaxRecords(from, to);

        return null;
    }

    private static( Task<IEnumerable<MetricData>> Primary, Task<IEnumerable<MetricData>> Secondary) StartLegacyLoadTasks(DataFetcher dataFetcher, MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to, string tableName, int? maxRecords)
    {
        var primaryTask = dataFetcher.GetHealthMetricsDataByBaseType(primarySelection.MetricType, primarySelection.QuerySubtype, from, to, tableName, maxRecords);

        var secondaryTask = secondarySelection != null ? dataFetcher.GetHealthMetricsDataByBaseType(secondarySelection.MetricType, secondarySelection.QuerySubtype, from, to, tableName, maxRecords) : Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

        return (primaryTask, secondaryTask);
    }

    private static async Task<( Task<IReadOnlyList<ICanonicalMetricSeries>>? Primary, Task<IReadOnlyList<ICanonicalMetricSeries>>? Secondary)> StartCmsLoadTasksAsync(CmsDataService cmsService, MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to)
    {
        Task<IReadOnlyList<ICanonicalMetricSeries>>? primaryTask = null;
        Task<IReadOnlyList<ICanonicalMetricSeries>>? secondaryTask = null;

        var primaryCanonicalId = !string.Equals(primarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase) ? CanonicalMetricMapping.FromLegacyFields(primarySelection.MetricType, primarySelection.QuerySubtype) : null;

        if (primaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(primaryCanonicalId))
            primaryTask = cmsService.GetCmsByCanonicalIdAsync(primaryCanonicalId, from, to);

        if (secondarySelection != null)
        {
            var secondaryCanonicalId = !string.Equals(secondarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase) ? CanonicalMetricMapping.FromLegacyFields(secondarySelection.MetricType, secondarySelection.QuerySubtype) : null;

            if (secondaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(secondaryCanonicalId))
                secondaryTask = cmsService.GetCmsByCanonicalIdAsync(secondaryCanonicalId, from, to);
        }

        return (primaryTask, secondaryTask);
    }

    private (SamplingMode Mode, int? TargetSamples, int? MaxRecords)
            ResolveDataLoadStrategy(DateTime from, DateTime to, long recordCount)
    {
        var enableSampling =
                bool.TryParse(
                        ConfigurationManager.AppSettings["DataVisualiser:EnableSqlSampling"],
                        out var samplingEnabled) && samplingEnabled;

        var samplingThreshold =
                int.TryParse(
                        ConfigurationManager.AppSettings["DataVisualiser:SamplingThreshold"],
                        out var threshold)
                        ? threshold
                        : 5000;

        var targetSamples =
                int.TryParse(
                        ConfigurationManager.AppSettings["DataVisualiser:TargetSamplePoints"],
                        out var samples)
                        ? samples
                        : 2000;

        var enableLimiting =
                bool.TryParse(
                        ConfigurationManager.AppSettings["DataVisualiser:EnableSqlResultLimiting"],
                        out var limitingEnabled) && limitingEnabled;

        if (enableSampling && recordCount > samplingThreshold)
        {
            System.Diagnostics.Debug.WriteLine($"[Sampling] Activated UniformOverTime | Records={recordCount}, Target={targetSamples}, Range={from:yyyy-MM-dd}→{to:yyyy-MM-dd}");

            return (SamplingMode.UniformOverTime, targetSamples, null);
        }


        if (enableLimiting)
            return (SamplingMode.None, null,
                    MathHelper.CalculateOptimalMaxRecords(from, to));

        return (SamplingMode.None, null, null);
    }



    // ------------------------------------------------------------
    // LOAD METRIC DATA (PRIMARY + SECONDARY)
    // ------------------------------------------------------------
    public async Task<(IEnumerable<MetricData> Primary, IEnumerable<MetricData> Secondary)> LoadMetricDataAsync(string baseType, string? primarySubtype, string? secondarySubtype, DateTime from, DateTime to, string tableName)
    {
        var dataFetcher = new DataFetcher(_connectionString);

        // PRIMARY
        var primaryCount = await dataFetcher.GetRecordCount(baseType, primarySubtype);
        var primaryStrategy = ResolveDataLoadStrategy(from, to, primaryCount);

        // SECONDARY
        (long recordCount, SamplingMode Mode, int? TargetSamples, int? MaxRecords) _;
        var secondaryStrategy = secondarySubtype != null
                ? ResolveDataLoadStrategy(
                        from,
                        to,
                        await dataFetcher.GetRecordCount(baseType, secondarySubtype))
                : (SamplingMode.None, null, null);

        // LOAD
        var primaryTask = dataFetcher.GetHealthMetricsDataByBaseType(
                baseType,
                primarySubtype,
                from,
                to,
                tableName,
                primaryStrategy.MaxRecords,
                primaryStrategy.Mode,
                primaryStrategy.TargetSamples);

        var secondaryTask = secondarySubtype != null
                ? dataFetcher.GetHealthMetricsDataByBaseType(
                        baseType,
                        secondarySubtype,
                        from,
                        to,
                        tableName,
                        secondaryStrategy.MaxRecords,
                        secondaryStrategy.Mode,
                        secondaryStrategy.TargetSamples)
                : Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

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