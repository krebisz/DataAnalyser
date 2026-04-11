using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Data;

public interface IMetricSelectionDataQueries
{
    Task<long> GetRecordCount(string metricType, string? metricSubtype = null);

    Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(
        string baseType,
        string? subtype,
        DateTime? from,
        DateTime? to,
        string tableName,
        int? maxRecords = null,
        SamplingMode samplingMode = SamplingMode.None,
        int? targetSamples = null);

    Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName);
    Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName);
    Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName);
    Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null);
    Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName);
}
