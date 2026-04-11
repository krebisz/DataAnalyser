using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Data.Repositories;

public class DataFetcher : IMetricSelectionDataQueries
{
    private readonly DataFetcherAdminQueries _adminQueries;
    private readonly DataFetcherDateRangeQueries _dateRangeQueries;
    private readonly DataFetcherMetricCatalogQueries _metricCatalogQueries;
    private readonly DataFetcherMetricDataQueries _metricDataQueries;

    public DataFetcher(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        _metricCatalogQueries = new DataFetcherMetricCatalogQueries(connectionString);
        _metricDataQueries = new DataFetcherMetricDataQueries(connectionString);
        _dateRangeQueries = new DataFetcherDateRangeQueries(connectionString);
        _adminQueries = new DataFetcherAdminQueries(connectionString);
    }

    public Task<IEnumerable<dynamic>> GetCombinedData(string[] tables, DateTime from, DateTime to) => _metricDataQueries.GetCombinedData(tables, from, to);
    public Task<IEnumerable<string>> GetMetricTypes() => _metricCatalogQueries.GetMetricTypes();
    public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName = DataAccessDefaults.DefaultTableName) => _metricCatalogQueries.GetBaseMetricTypeOptions(tableName);
    public Task<IReadOnlyList<string>> GetCountsMetricTypesForAdmin() => _adminQueries.GetCountsMetricTypesForAdmin();
    public Task<IReadOnlyList<HealthMetricsCountEntry>> GetHealthMetricsCountsForAdmin(string? metricType = null) => _adminQueries.GetHealthMetricsCountsForAdmin(metricType);
    public Task<int> UpdateHealthMetricsCountsForAdmin(IEnumerable<HealthMetricsCountEntry> updates) => _adminQueries.UpdateHealthMetricsCountsForAdmin(updates);
    public Task<(DateTime MinDate, DateTime MaxDate)?> GetMetricTypeDateRange(string metricType) => _dateRangeQueries.GetMetricTypeDateRange(metricType);
    public Task<IEnumerable<MetricData>> GetHealthMetricsData(string metricType, DateTime from, DateTime to) => _metricDataQueries.GetHealthMetricsData(metricType, from, to);
    public Task<IEnumerable<string>> GetBaseMetricTypes(string tableName = DataAccessDefaults.DefaultTableName) => _metricCatalogQueries.GetBaseMetricTypes(tableName);
    public Task<IEnumerable<string>> GetSubtypesForBaseType(string baseType, string tableName = DataAccessDefaults.DefaultTableName) => _metricCatalogQueries.GetSubtypesForBaseType(baseType, tableName);
    public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName = DataAccessDefaults.DefaultTableName) => _metricCatalogQueries.GetSubtypeOptionsForBaseType(baseType, tableName);
    public Task<IEnumerable<string>> GetAllSubtypes(string tableName = DataAccessDefaults.DefaultTableName) => _metricCatalogQueries.GetAllSubtypes(tableName);
    public Task<IEnumerable<MetricNameOption>> GetAllSubtypeOptions(string tableName = DataAccessDefaults.DefaultTableName) => _metricCatalogQueries.GetAllSubtypeOptions(tableName);
    public Task<Dictionary<string, List<string>>> GetMetricTypesByBaseType() => _metricCatalogQueries.GetMetricTypesByBaseType();
    public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype = null, DateTime? from = null, DateTime? to = null, string tableName = DataAccessDefaults.DefaultTableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null) => _metricDataQueries.GetHealthMetricsDataByBaseType(baseType, subtype, from, to, tableName, maxRecords, samplingMode, targetSamples);
    public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype = null, string tableName = DataAccessDefaults.DefaultTableName) => _dateRangeQueries.GetBaseTypeDateRange(baseType, subtype, tableName);
    public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => _dateRangeQueries.GetBaseTypeDateRangeFromCounts(baseType, subtypes);
    public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName = DataAccessDefaults.DefaultTableName) => _dateRangeQueries.GetBaseTypeDateRangeForSubtypes(baseType, subtypes, tableName);
    public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => _dateRangeQueries.GetRecordCount(metricType, metricSubtype);
    public Task<Dictionary<(string MetricType, string? MetricSubtype), long>> GetAllRecordCounts() => _dateRangeQueries.GetAllRecordCounts();
    public Task<Dictionary<string, long>> GetRecordCountsByMetricType() => _dateRangeQueries.GetRecordCountsByMetricType();
    public Task<Dictionary<string, long>> GetRecordCountsBySubtype(string metricType) => _dateRangeQueries.GetRecordCountsBySubtype(metricType);
}
