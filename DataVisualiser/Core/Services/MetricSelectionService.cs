using DataFileReader.Canonical;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Data.Repositories;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services;

public class MetricSelectionService
{
    private readonly CmsDataService _cms;
    private readonly IMetricSelectionDataQueries _dataQueries;

    public MetricSelectionService(string connectionString)
    {
        _dataQueries = new DataFetcher(connectionString);
        _cms = new CmsDataService(connectionString);
    }

    public MetricSelectionService(IMetricSelectionDataQueries dataQueries, string connectionString)
    {
        _dataQueries = dataQueries ?? throw new ArgumentNullException(nameof(dataQueries));
        _cms = new CmsDataService(connectionString);
    }

    internal MetricSelectionService(IMetricSelectionDataQueries dataQueries, CmsDataService cmsDataService)
    {
        _dataQueries = dataQueries ?? throw new ArgumentNullException(nameof(dataQueries));
        _cms = cmsDataService ?? throw new ArgumentNullException(nameof(cmsDataService));
    }
    public async Task<(ICanonicalMetricSeries? PrimaryCms, ICanonicalMetricSeries? SecondaryCms, IEnumerable<MetricData> PrimaryLegacy, IEnumerable<MetricData> SecondaryLegacy)> LoadMetricDataWithCmsAsync(MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to, string tableName)
    {
        var primaryCountTask = _dataQueries.GetRecordCount(primarySelection.MetricType, primarySelection.QuerySubtype);
        var secondaryCountTask = secondarySelection != null ? _dataQueries.GetRecordCount(secondarySelection.MetricType, secondarySelection.QuerySubtype) : Task.FromResult(0L);

        await Task.WhenAll(primaryCountTask, secondaryCountTask);

        var primaryStrategy = ResolveDataLoadStrategy(from, to, primaryCountTask.Result);
        var secondaryStrategy = secondarySelection != null ? ResolveDataLoadStrategy(from, to, secondaryCountTask.Result) : new MetricDataLoadStrategy(SamplingMode.None, null, null);

        var legacyTasks = StartLegacyLoadTasks(_dataQueries, primarySelection, secondarySelection, from, to, tableName, primaryStrategy, secondaryStrategy);

        var cmsTasks = await StartCmsLoadTasksAsync(_cms, primarySelection, secondarySelection, from, to, tableName, primaryStrategy, secondaryStrategy);

        await Task.WhenAll(legacyTasks.Primary, legacyTasks.Secondary, cmsTasks.Primary ?? Task.CompletedTask, cmsTasks.Secondary ?? Task.CompletedTask);

        return (PrimaryCms: cmsTasks.Primary?.Result.FirstOrDefault(), SecondaryCms: cmsTasks.Secondary?.Result.FirstOrDefault(), PrimaryLegacy: legacyTasks.Primary.Result, SecondaryLegacy: legacyTasks.Secondary.Result);
    }

    private static(Task<IEnumerable<MetricData>> Primary, Task<IEnumerable<MetricData>> Secondary) StartLegacyLoadTasks(IMetricSelectionDataQueries dataQueries, MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to, string tableName, MetricDataLoadStrategy primaryStrategy, MetricDataLoadStrategy secondaryStrategy)
    {
        var primaryTask = dataQueries.GetHealthMetricsDataByBaseType(primarySelection.MetricType, primarySelection.QuerySubtype, from, to, tableName, primaryStrategy.MaxRecords, primaryStrategy.Mode, primaryStrategy.TargetSamples);

        var secondaryTask = secondarySelection != null ? dataQueries.GetHealthMetricsDataByBaseType(secondarySelection.MetricType, secondarySelection.QuerySubtype, from, to, tableName, secondaryStrategy.MaxRecords, secondaryStrategy.Mode, secondaryStrategy.TargetSamples) : Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

        return (primaryTask, secondaryTask);
    }

    private static async Task<(Task<IReadOnlyList<ICanonicalMetricSeries>>? Primary, Task<IReadOnlyList<ICanonicalMetricSeries>>? Secondary)> StartCmsLoadTasksAsync(CmsDataService cmsService, MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, DateTime from, DateTime to, string tableName, MetricDataLoadStrategy primaryStrategy, MetricDataLoadStrategy secondaryStrategy)
    {
        Task<IReadOnlyList<ICanonicalMetricSeries>>? primaryTask = null;
        Task<IReadOnlyList<ICanonicalMetricSeries>>? secondaryTask = null;

        var primaryCanonicalId = !string.Equals(primarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase) ? CanonicalMetricMapping.FromLegacyFields(primarySelection.MetricType, primarySelection.QuerySubtype) : null;

        if (primaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(primaryCanonicalId))
            primaryTask = cmsService.GetCmsByCanonicalIdAsync(primaryCanonicalId, from, to, tableName, primaryStrategy.MaxRecords, primaryStrategy.Mode, primaryStrategy.TargetSamples);

        if (secondarySelection != null)
        {
            var secondaryCanonicalId = !string.Equals(secondarySelection.MetricType, "(All)", StringComparison.OrdinalIgnoreCase) ? CanonicalMetricMapping.FromLegacyFields(secondarySelection.MetricType, secondarySelection.QuerySubtype) : null;

            if (secondaryCanonicalId != null && await cmsService.IsCmsAvailableAsync(secondaryCanonicalId))
                secondaryTask = cmsService.GetCmsByCanonicalIdAsync(secondaryCanonicalId, from, to, tableName, secondaryStrategy.MaxRecords, secondaryStrategy.Mode, secondaryStrategy.TargetSamples);
        }

        return (primaryTask, secondaryTask);
    }

    private MetricDataLoadStrategy ResolveDataLoadStrategy(DateTime from, DateTime to, long recordCount)
    {
        return MetricDataLoadStrategyResolver.Resolve(from, to, recordCount);
    }


    // ------------------------------------------------------------
    // LOAD METRIC DATA (PRIMARY + SECONDARY)
    // ------------------------------------------------------------
    public async Task<(IEnumerable<MetricData> Primary, IEnumerable<MetricData> Secondary)> LoadMetricDataAsync(string baseType, string? primarySubtype, string? secondarySubtype, DateTime from, DateTime to, string tableName)
    {
        // PRIMARY
        var primaryCount = await _dataQueries.GetRecordCount(baseType, primarySubtype);
        var primaryStrategy = ResolveDataLoadStrategy(from, to, primaryCount);

        // SECONDARY
        var secondaryStrategy = secondarySubtype != null ? ResolveDataLoadStrategy(from, to, await _dataQueries.GetRecordCount(baseType, secondarySubtype)) : new MetricDataLoadStrategy(SamplingMode.None, null, null);

        // LOAD
        var primaryTask = _dataQueries.GetHealthMetricsDataByBaseType(baseType, primarySubtype, from, to, tableName, primaryStrategy.MaxRecords, primaryStrategy.Mode, primaryStrategy.TargetSamples);

        var secondaryTask = secondarySubtype != null ? _dataQueries.GetHealthMetricsDataByBaseType(baseType, secondarySubtype, from, to, tableName, secondaryStrategy.MaxRecords, secondaryStrategy.Mode, secondaryStrategy.TargetSamples) : Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());

        await Task.WhenAll(primaryTask, secondaryTask);

        return (primaryTask.Result, secondaryTask.Result);
    }

    // ------------------------------------------------------------
    // LOAD METRIC TYPES (BASE METRIC TYPES)
    // ------------------------------------------------------------
    public async Task<List<MetricNameOption>> LoadMetricTypesAsync(string tableName)
    {
        var baseMetricTypes = await _dataQueries.GetBaseMetricTypeOptions(tableName);

        return baseMetricTypes.ToList();
    }

    // ------------------------------------------------------------
    // LOAD SUBTYPES
    // ------------------------------------------------------------
    public async Task<List<MetricNameOption>> LoadSubtypesAsync(string metricType, string tableName)
    {
        var subtypes = await _dataQueries.GetSubtypeOptionsForBaseType(metricType, tableName);

        return subtypes.ToList();
    }

    // ------------------------------------------------------------
    // LOAD DATE RANGE
    // ------------------------------------------------------------
    public async Task<(DateTime MinDate, DateTime MaxDate)?> LoadDateRangeAsync(string metricType, string? subtype, string tableName)
    {
        var dateRange = await _dataQueries.GetBaseTypeDateRange(metricType, subtype, tableName);

        return dateRange;
    }

    /// <summary>
    ///     Loads a date range for the selected metric type, using the union of all selected subtypes.
    ///     Fast path uses HealthMetricsCounts; falls back to table scan only if needed.
    /// </summary>
    public async Task<(DateTime MinDate, DateTime MaxDate)?> LoadDateRangeForSelectionsAsync(string metricType, IReadOnlyCollection<MetricSeriesSelection> selections, string tableName)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        var subtypes = selections?
                .Where(selection => string.Equals(selection.MetricType, metricType, StringComparison.OrdinalIgnoreCase))
                .Select(selection => selection.QuerySubtype)
                .Where(subtype => !string.IsNullOrWhiteSpace(subtype) && !string.Equals(subtype, "(All)", StringComparison.OrdinalIgnoreCase))
                .Select(subtype => subtype!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                      ?? new List<string>();

        // Fast path: aggregated counts table.
        var countsRange = await _dataQueries.GetBaseTypeDateRangeFromCounts(metricType, subtypes);
        if (countsRange.HasValue)
            return countsRange;

        // Fallback: raw table scan using subtype union.
        return await _dataQueries.GetBaseTypeDateRangeForSubtypes(metricType, subtypes, tableName);
    }
}
