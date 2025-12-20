using DataVisualiser.Data.Repositories;
using DataVisualiser.Models;

namespace DataVisualiser.Services
{
    public class MetricSelectionService
    {
        private readonly string _connectionString;

        public MetricSelectionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ------------------------------------------------------------
        // LOAD METRIC DATA (PRIMARY + SECONDARY)
        // ------------------------------------------------------------
        public async Task<(IEnumerable<HealthMetricData> Primary, IEnumerable<HealthMetricData> Secondary)> LoadMetricDataAsync(
            string baseType,
            string? primarySubtype,
            string? secondarySubtype,
            DateTime from,
            DateTime to,
            string tableName)
        {
            var dataFetcher = new DataFetcher(_connectionString);

            var primaryTask = dataFetcher.GetHealthMetricsDataByBaseType(
                baseType,
                primarySubtype,
                from,
                to,
                tableName);

            var secondaryTask = dataFetcher.GetHealthMetricsDataByBaseType(
                baseType,
                secondarySubtype,
                from,
                to,
                tableName);

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
        public async Task<(DateTime MinDate, DateTime MaxDate)?> LoadDateRangeAsync(
            string metricType,
            string? subtype,
            string tableName)
        {
            var dataFetcher = new DataFetcher(_connectionString);
            var dateRange = await dataFetcher.GetBaseTypeDateRange(metricType, subtype, tableName);

            return dateRange;
        }
    }
}
