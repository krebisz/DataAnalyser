using System.Diagnostics;
using DataVisualiser.Core.Data.Repositories;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.Selection;

/// <summary>
///     Handles strategy selection logic, extracting complex conditional logic
///     from MainWindow to improve maintainability.
/// </summary>
public sealed class StrategySelectionService
{
    private readonly string _connectionString;
    private readonly IStrategyCutOverService _strategyCutOverService;

    public StrategySelectionService(IStrategyCutOverService strategyCutOverService, string connectionString)
    {
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    ///     Selects the appropriate computation strategy based on the number of series.
    ///     Returns the strategy and secondary label (if applicable).
    /// </summary>
    public (IChartComputationStrategy strategy, string? secondaryLabel) SelectComputationStrategy(List<IEnumerable<MetricData>> series, List<string> labels, ChartDataContext ctx, DateTime from, DateTime to)
    {
        string? secondaryLabel = null;
        IChartComputationStrategy strategy;

        var actualSeriesCount = series.Count;

        Debug.WriteLine($"[STRATEGY] ActualSeriesCount={actualSeriesCount}, SemanticMetricCount={ctx.SemanticMetricCount}, " + $"PrimaryCms={(ctx.PrimaryCms == null ? "NULL" : "SET")}, " + $"SecondaryCms={(ctx.SecondaryCms == null ? "NULL" : "SET")}");

        // Multi-metric strategy (3+ series)
        if (actualSeriesCount > 2)
        {
            strategy = CreateMultiMetricStrategy(ctx, series, labels, from, to);
        }
        // Combined metric strategy (2 series)
        else if (actualSeriesCount == 2)
        {
            secondaryLabel = labels[1];
            strategy = CreateCombinedMetricStrategy(ctx, series, labels, from, to);
        }
        // Single metric strategy (1 series)
        else
        {
            strategy = CreateSingleMetricStrategy(ctx, series[0], labels[0], from, to);
        }

        Debug.WriteLine($"[StrategySelection] actualSeriesCount={actualSeriesCount}, " + $"series.Count={series.Count}, " + $"strategy={strategy.GetType().Name}");

        return (strategy, secondaryLabel);
    }

    /// <summary>
    ///     Loads additional subtype data (subtypes 3, 4, etc.) and adds them to the series and labels lists.
    /// </summary>
    public async Task LoadAdditionalSubtypesAsync(List<IEnumerable<MetricData>> series, List<string> labels, string? metricType, string? resolutionTableName, DateTime from, DateTime to, List<string?> selectedSubtypes)
    {
        if (selectedSubtypes.Count <= 2 || string.IsNullOrEmpty(metricType))
            return;

        var dataFetcher = new DataFetcher(_connectionString);
        var tableName = resolutionTableName ?? "HealthMetrics";

        // Load data for subtypes 3, 4, etc.
        for (var i = 2; i < selectedSubtypes.Count; i++)
        {
            var subtype = selectedSubtypes[i];
            if (string.IsNullOrWhiteSpace(subtype))
                continue;

            try
            {
                var additionalData = await dataFetcher.GetHealthMetricsDataByBaseType(metricType, subtype, from, to, tableName);

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

    private IChartComputationStrategy CreateSingleMetricStrategy(ChartDataContext ctx, IEnumerable<MetricData> data, string label, DateTime from, DateTime to)
    {
        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = data,
            Label1 = label,
            From = from,
            To = to
        };

        return _strategyCutOverService.CreateStrategy(StrategyType.SingleMetric, ctx, parameters);
    }

    private IChartComputationStrategy CreateMultiMetricStrategy(ChartDataContext ctx, List<IEnumerable<MetricData>> series, List<string> labels, DateTime from, DateTime to)
    {
        var parameters = new StrategyCreationParameters
        {
            LegacySeries = series,
            Labels = labels,
            From = from,
            To = to
        };

        return _strategyCutOverService.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);
    }

    private IChartComputationStrategy CreateCombinedMetricStrategy(ChartDataContext ctx, List<IEnumerable<MetricData>> series, List<string> labels, DateTime from, DateTime to)
    {
        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = series[0],
            LegacyData2 = series[1],
            Label1 = labels[0],
            Label2 = labels[1],
            From = from,
            To = to
        };

        // Use the cut-over service to create the strategy, which respects toggle settings
        return _strategyCutOverService.CreateStrategy(StrategyType.CombinedMetric, ctx, parameters);
    }
}
