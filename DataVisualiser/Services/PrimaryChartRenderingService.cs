using DataVisualiser.Charts;
using DataVisualiser.Charts.Rendering;
using DataVisualiser.Data.Repositories;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.Services.ChartRendering;
using DataVisualiser.Services.Implementations;
using DataVisualiser.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Services
{
    /// <summary>
    /// Service for rendering the primary (main) chart with support for multi-metric scenarios.
    /// Extracts complex logic from MainWindow to improve testability and maintainability.
    /// </summary>
    public sealed class PrimaryChartRenderingService
    {
        private readonly ChartRenderingOrchestrator _orchestrator;
        private readonly string _connectionString;

        public PrimaryChartRenderingService(
            ChartUpdateCoordinator chartUpdateCoordinator,
            WeeklyDistributionService weeklyDistributionService,
            IStrategyCutOverService strategyCutOverService,
            string connectionString)
        {
            _orchestrator = new ChartRenderingOrchestrator(
                chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator)),
                weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService)),
                strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService)));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Renders the primary chart with support for additional subtypes.
        /// </summary>
        public async Task RenderPrimaryChartAsync(
            ChartDataContext ctx,
            CartesianChart chartMain,
            IEnumerable<HealthMetricData> data1,
            IEnumerable<HealthMetricData>? data2,
            string displayName1,
            string displayName2,
            DateTime from,
            DateTime to,
            string? metricType = null,
            IReadOnlyList<string>? selectedSubtypes = null,
            string? resolutionTableName = null)
        {
            if (ctx == null || chartMain == null)
                return;

            // Build initial series list for multi-metric routing
            var (series, labels) = BuildInitialSeriesList(data1, data2, displayName1, displayName2);

            // Load additional subtypes if more than 2 are selected
            await LoadAdditionalSubtypesAsync(series, labels, metricType, from, to, selectedSubtypes, resolutionTableName);

            // Extract additional series (beyond the first 2 from context)
            IReadOnlyList<IEnumerable<HealthMetricData>>? additionalSeries = null;
            IReadOnlyList<string>? additionalLabels = null;

            if (series.Count > 2)
            {
                additionalSeries = series.Skip(2).ToList();
                additionalLabels = labels.Skip(2).ToList();
            }

            // Use orchestrator to render the chart
            await _orchestrator.RenderPrimaryChart(ctx, chartMain, additionalSeries, additionalLabels);
        }

        /// <summary>
        /// Builds the initial series list and labels from primary and secondary data.
        /// </summary>
        private (List<IEnumerable<HealthMetricData>> series, List<string> labels) BuildInitialSeriesList(
            IEnumerable<HealthMetricData> data1,
            IEnumerable<HealthMetricData>? data2,
            string displayName1,
            string displayName2)
        {
            var series = new List<IEnumerable<HealthMetricData>> { data1 };
            var labels = new List<string> { displayName1 };

            if (data2 != null && data2.Any())
            {
                series.Add(data2);
                labels.Add(displayName2);
            }

            return (series, labels);
        }

        /// <summary>
        /// Loads additional subtype data (subtypes 3, 4, etc.) and adds them to the series and labels lists.
        /// </summary>
        private async Task LoadAdditionalSubtypesAsync(
            List<IEnumerable<HealthMetricData>> series,
            List<string> labels,
            string? metricType,
            DateTime from,
            DateTime to,
            IReadOnlyList<string>? selectedSubtypes,
            string? resolutionTableName)
        {
            if (selectedSubtypes == null || selectedSubtypes.Count <= 2 || string.IsNullOrEmpty(metricType))
                return;

            var dataFetcher = new DataFetcher(_connectionString);
            var tableName = resolutionTableName ?? "HealthMetrics";

            // Load data for subtypes 3, 4, etc.
            for (int i = 2; i < selectedSubtypes.Count; i++)
            {
                var subtype = selectedSubtypes[i];
                if (string.IsNullOrWhiteSpace(subtype))
                    continue;

                try
                {
                    var additionalData = await dataFetcher.GetHealthMetricsDataByBaseType(
                        metricType,
                        subtype,
                        from,
                        to,
                        tableName);

                    if (additionalData != null && additionalData.Any())
                    {
                        series.Add(additionalData);
                        labels.Add($"{metricType}:{subtype}");
                    }
                }
                catch
                {
                    // Skip if loading fails
                }
            }
        }
    }
}

