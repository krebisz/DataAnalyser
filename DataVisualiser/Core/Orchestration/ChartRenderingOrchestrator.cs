using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Data.Repositories;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration;

/// <summary>
///     Orchestrates chart rendering operations, extracting complex rendering logic
///     from MainWindow to improve maintainability and testability.
///     Handles multi-chart rendering, visibility management, and chart-specific rendering strategies.
///     IMPORTANT:
///     Never fetch metric data directly in this orchestrator.
///     All data loading MUST go through MetricSelectionService
///     to ensure consistent sampling / limiting policy.
/// </summary>
public sealed class ChartRenderingOrchestrator
{
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;
    private readonly string? _connectionString;
    private readonly HourlyDistributionService _hourlyDistributionService;
    private readonly IStrategyCutOverService _strategyCutOverService;
    private readonly WeeklyDistributionService _weeklyDistributionService;
    private readonly MetricSelectionService? _metricSelectionService;

    public ChartRenderingOrchestrator(ChartUpdateCoordinator chartUpdateCoordinator, WeeklyDistributionService weeklyDistributionService, HourlyDistributionService hourlyDistributionService, IStrategyCutOverService strategyCutOverService, string? connectionString = null)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _connectionString = connectionString;
    }

    public ChartRenderingOrchestrator(ChartUpdateCoordinator chartUpdateCoordinator, WeeklyDistributionService weeklyDistributionService, HourlyDistributionService hourlyDistributionService, IStrategyCutOverService strategyCutOverService, MetricSelectionService metricSelectionService, string? connectionString = null)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _connectionString = connectionString;

    }
    /// <summary>
    ///     Renders all charts based on the provided context and visibility state.
    /// </summary>
    public async Task RenderChartsFromContext(ChartDataContext ctx, ChartState chartState, CartesianChart chartMain, CartesianChart chartNorm, CartesianChart chartDiffRatio, CartesianChart chartDistribution)
    {
        if (!ShouldRenderCharts(ctx))
            return;

        var hasSecondaryData = HasSecondaryData(ctx);

        await RenderPrimaryIfVisible(ctx, chartState, chartMain);

        if (hasSecondaryData)
            await RenderSecondaryChartsIfVisible(ctx, chartState, chartNorm, chartDiffRatio);
        else
            ClearSecondaryCharts(chartNorm, chartDiffRatio, chartState);

        await RenderDistributionChartsIfVisible(ctx, chartState, chartDistribution);
    }

    private async Task RenderPrimaryIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartMain)
    {
        if (chartState.IsMainVisible)
        {
            var (isStacked, isCumulative) = ResolveMainChartDisplayMode(chartState.MainChartDisplayMode);
            await RenderPrimaryChart(ctx, chartMain, isStacked: isStacked, isCumulative: isCumulative);
        }
    }

    private async Task RenderSecondaryChartsIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartNorm, CartesianChart? chartDiffRatio)
    {
        if (chartState.IsNormalizedVisible)
            await RenderNormalized(ctx, chartNorm, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype, chartState.SelectedNormalizationMode);

        if (chartState.IsDiffRatioVisible && chartDiffRatio != null)
            await RenderDiffRatio(ctx, chartDiffRatio, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype, chartState);
    }

    private static void ClearSecondaryCharts(CartesianChart chartNorm, CartesianChart chartDiffRatio, ChartState chartState)
    {
        ChartHelper.ClearChart(chartNorm, chartState.ChartTimestamps);
        ChartHelper.ClearChart(chartDiffRatio, chartState.ChartTimestamps);
    }

    private async Task RenderDistributionChartsIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartDistribution)
    {
        if (chartState.IsDistributionVisible)
            await RenderDistribution(ctx, chartDistribution, chartState, chartState.SelectedDistributionMode);

        if (chartState.IsWeeklyTrendVisible)
            RenderWeeklyTrend(ctx);
    }


    /// <summary>
    ///     Renders a single chart by name.
    /// </summary>
    public async Task RenderSingleChart(string chartName, ChartDataContext ctx, ChartState chartState, CartesianChart chartMain, CartesianChart chartNorm, CartesianChart chartDiffRatio, CartesianChart chartDistribution)
    {
        if (!ShouldRenderCharts(ctx))
            return;

        switch (chartName)
        {
            case "Main":
                await RenderPrimaryIfVisible(ctx, chartState, chartMain);
                break;

            case "Norm":
                if (HasSecondaryData(ctx))
                    await RenderSecondaryChartsIfVisible(ctx, chartState, chartNorm, null);
                break;

            case "DiffRatio":
                if (HasSecondaryData(ctx) && chartState.IsDiffRatioVisible)
                    await RenderDiffRatio(ctx, chartDiffRatio, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype, chartState);
                break;

            case "Distribution":
            case "WeeklyTrend":
                await RenderDistributionChartsIfVisible(ctx, chartState, chartDistribution);
                break;
        }
    }

    public Task RenderNormalizedChartAsync(ChartDataContext ctx, CartesianChart chartNorm, ChartState chartState)
    {
        return RenderNormalized(ctx, chartNorm, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype, chartState.SelectedNormalizationMode);
    }

    public Task RenderDiffRatioChartAsync(ChartDataContext ctx, CartesianChart chartDiffRatio, ChartState chartState)
    {
        if (!chartState.IsDiffRatioVisible)
            return Task.CompletedTask;

        return RenderDiffRatio(ctx, chartDiffRatio, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype, chartState);
    }

    public Task RenderWeeklyDistributionChartAsync(ChartDataContext ctx, CartesianChart chartWeekly, ChartState chartState)
    {
        return RenderDistribution(ctx, chartWeekly, chartState, DistributionMode.Weekly);
    }

    public Task RenderDistributionChartAsync(ChartDataContext ctx, CartesianChart chartDistribution, ChartState chartState, DistributionMode mode)
    {
        return RenderDistribution(ctx, chartDistribution, chartState, mode);
    }


    /// <summary>
    ///     Renders the primary (main) chart using StrategyCutOverService.
    ///     Handles single, combined, and multi-metric strategies.
    /// </summary>
    public async Task RenderPrimaryChart(ChartDataContext ctx, CartesianChart chartMain, IReadOnlyList<IEnumerable<MetricData>>? additionalSeries = null, IReadOnlyList<string>? additionalLabels = null, bool isStacked = false, bool isCumulative = false)
    {
        if (ctx == null || chartMain == null)
            return;

        var (series, labels) = BuildSeriesAndLabels(ctx, additionalSeries, additionalLabels);

        var strategy = CreatePrimaryStrategy(ctx, series, labels, out var secondaryLabel);

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartMain, strategy, labels[0], secondaryLabel, 400, ctx.MetricType, ctx.PrimarySubtype, secondaryLabel != null ? ctx.SecondarySubtype : null, isOperationChart: false, secondaryMetricType: ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype, isStacked: isStacked, isCumulative: isCumulative);
    }

    /// <summary>
    ///     Renders the primary chart with support for additional subtypes.
    ///     Loads additional subtype data if more than 2 subtypes are selected.
    /// </summary>
    public async Task RenderPrimaryChartAsync(ChartDataContext ctx, CartesianChart chartMain, IEnumerable<MetricData> data1, IEnumerable<MetricData>? data2, string displayName1, string displayName2, DateTime from, DateTime to, string? metricType = null, IReadOnlyList<MetricSeriesSelection>? selectedSeries = null, string? resolutionTableName = null, bool isStacked = false, bool isCumulative = false)
    {
        if (ctx == null || chartMain == null)
            return;

        // Build initial series list for multi-metric routing
        var (series, labels) = BuildInitialSeriesList(data1, data2, displayName1, displayName2);

        // Load additional subtypes if more than 2 are selected
        await LoadAdditionalSubtypesAsync(series, labels, metricType, from, to, selectedSeries, resolutionTableName);

        // Extract additional series (beyond the first 2 from context)
        IReadOnlyList<IEnumerable<MetricData>>? additionalSeries = null;
        IReadOnlyList<string>? additionalLabels = null;

        if (series.Count > 2)
        {
            additionalSeries = series.Skip(2).ToList();
            additionalLabels = labels.Skip(2).ToList();
        }

        // Use existing RenderPrimaryChart method
        await RenderPrimaryChart(ctx, chartMain, additionalSeries, additionalLabels, isStacked, isCumulative);
    }

    private static (bool IsStacked, bool IsCumulative) ResolveMainChartDisplayMode(MainChartDisplayMode mode)
    {
        return mode switch
        {
                MainChartDisplayMode.Stacked => (true, false),
                MainChartDisplayMode.Summed => (false, true),
                _ => (false, false)
        };
    }

    private static(List<IEnumerable<MetricData>> Series, List<string> Labels) BuildSeriesAndLabels(ChartDataContext ctx, IReadOnlyList<IEnumerable<MetricData>>? additionalSeries, IReadOnlyList<string>? additionalLabels)
    {
        var series = new List<IEnumerable<MetricData>>
        {
                ctx.Data1 ?? Array.Empty<MetricData>()
        };

        var labels = new List<string>
        {
                ctx.DisplayName1 ?? string.Empty
        };

        if (ctx.Data2 != null && ctx.Data2.Any())
        {
            series.Add(ctx.Data2);
            labels.Add(ctx.DisplayName2 ?? string.Empty);
        }

        if (additionalSeries != null && additionalLabels != null)
            for (var i = 0; i < Math.Min(additionalSeries.Count, additionalLabels.Count); i++)
                if (additionalSeries[i] != null && additionalSeries[i].Any())
                {
                    series.Add(additionalSeries[i]);
                    labels.Add(additionalLabels[i]);
                }

        return (series, labels);
    }

    private IChartComputationStrategy CreatePrimaryStrategy(ChartDataContext ctx, List<IEnumerable<MetricData>> series, List<string> labels, out string? secondaryLabel)
    {
        secondaryLabel = null;

        if (series.Count > 2)
            return _strategyCutOverService.CreateStrategy(StrategyType.MultiMetric,
                    ctx,
                    new StrategyCreationParameters
                    {
                            LegacySeries = series,
                            Labels = labels,
                            From = ctx.From,
                            To = ctx.To
                    });

        if (series.Count == 2)
        {
            secondaryLabel = labels[1];

            return _strategyCutOverService.CreateStrategy(StrategyType.CombinedMetric,
                    ctx,
                    new StrategyCreationParameters
                    {
                            LegacyData1 = series[0],
                            LegacyData2 = series[1],
                            Label1 = labels[0],
                            Label2 = labels[1],
                            From = ctx.From,
                            To = ctx.To
                    });
        }

        return _strategyCutOverService.CreateStrategy(StrategyType.SingleMetric,
                ctx,
                new StrategyCreationParameters
                {
                        LegacyData1 = series[0],
                        Label1 = labels[0],
                        From = ctx.From,
                        To = ctx.To
                });
    }


    private async Task RenderNormalized(ChartDataContext ctx, CartesianChart chartNorm, string? metricType, string? primarySubtype, string? secondarySubtype, NormalizationMode normalizationMode)
    {
        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = ctx.Data1,
                LegacyData2 = ctx.Data2,
                Label1 = ctx.DisplayName1,
                Label2 = ctx.DisplayName2,
                From = ctx.From,
                To = ctx.To,
                NormalizationMode = normalizationMode
        };

        var strategy = _strategyCutOverService.CreateStrategy(StrategyType.Normalized, ctx, parameters);

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartNorm, strategy, $"{ctx.DisplayName1} ~ {ctx.DisplayName2}", minHeight: 400, metricType: metricType, primarySubtype: primarySubtype, secondarySubtype: secondarySubtype, operationType: "~", isOperationChart: true, secondaryMetricType: ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype);
    }

    private async Task RenderDiffRatio(ChartDataContext ctx, CartesianChart chartDiffRatio, string? metricType, string? primarySubtype, string? secondarySubtype, ChartState? chartState = null)
    {
        if (ctx.Data1 == null || ctx.Data2 == null)
            return;

        var isDifferenceMode = chartState?.IsDiffRatioDifferenceMode ?? true;
        var operation = isDifferenceMode ? "Subtract" : "Divide";
        var operationSymbol = isDifferenceMode ? "-" : "/";

        if (TryBuildDiffRatioSeries(ctx, isDifferenceMode, out var derivedData, out var derivedValues))
        {
            var derivedLabel = TransformExpressionEvaluator.GenerateTransformLabel(operation, new List<IReadOnlyList<MetricData>> { ctx.Data1, ctx.Data2 }, ctx);
            var derivedStrategy = new TransformResultStrategy(derivedData, derivedValues, derivedLabel, ctx.From, ctx.To);

            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartDiffRatio, derivedStrategy, derivedLabel, null, 400, metricType, primarySubtype, secondarySubtype, operationSymbol, true, ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype);
            return;
        }

        var preparedData = PrepareAndAlignBinaryData(ctx.Data1, ctx.Data2);
        if (preparedData == null)
            return;

        var alignedData = preparedData.Value;

        var computation = ComputeBinaryResults(alignedData, operation);

        var label = TransformExpressionEvaluator.GenerateTransformLabel(operation, computation.MetricsList, ctx);

        var strategy = new TransformResultStrategy(alignedData.Item1, computation.Results, label, ctx.From, ctx.To);

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartDiffRatio, strategy, label, null, 400, metricType, primarySubtype, secondarySubtype, operationSymbol, true, ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype);
    }

    private static bool TryBuildDiffRatioSeries(ChartDataContext ctx, bool isDifferenceMode, out List<MetricData> data, out List<double> values)
    {
        data = new List<MetricData>();
        values = new List<double>();

        var timeline = ctx.Timestamps;
        var derivedValues = isDifferenceMode ? ctx.DifferenceValues : ctx.RatioValues;
        if (timeline == null || derivedValues == null || timeline.Count == 0 || derivedValues.Count == 0)
            return false;

        var count = Math.Min(timeline.Count, derivedValues.Count);
        if (count == 0)
            return false;

        var sample = ctx.Data1.FirstOrDefault(d => d.Value.HasValue)
                     ?? ctx.Data2.FirstOrDefault(d => d.Value.HasValue)
                     ?? ctx.Data1.FirstOrDefault()
                     ?? ctx.Data2.FirstOrDefault();

        data = new List<MetricData>(count);
        values = new List<double>(count);

        for (var i = 0; i < count; i++)
        {
            var value = derivedValues[i];
            values.Add(value);

            decimal? decimalValue = null;
            if (!double.IsNaN(value) && !double.IsInfinity(value))
                decimalValue = (decimal)value;

            data.Add(new MetricData
            {
                    NormalizedTimestamp = timeline[i],
                    Value = decimalValue,
                    Unit = sample?.Unit,
                    Provider = sample?.Provider
            });
        }

        return data.Count > 0 && values.Count > 0;
    }

    private static(List<MetricData> Item1, List<MetricData> Item2)? PrepareAndAlignBinaryData(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2)
    {
        var allData1List = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

        var allData2List = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

        if (allData1List.Count == 0 || allData2List.Count == 0)
            return null;

        var alignedData = TransformExpressionEvaluator.AlignMetricsByTimestamp(allData1List, allData2List);

        if (alignedData.Item1.Count == 0 || alignedData.Item2.Count == 0)
            return null;

        return alignedData;
    }

    private static( List<double> Results, List<IReadOnlyList<MetricData>> MetricsList) ComputeBinaryResults((List<MetricData> Item1, List<MetricData> Item2) alignedData, string operation)
    {
        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                alignedData.Item1,
                alignedData.Item2
        };

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);

        if (expression == null)
        {
            var op = operation switch
            {
                    "Subtract" => BinaryOperators.Difference,
                    "Divide" => BinaryOperators.Ratio,
                    _ => (a, b) => a
            };

            var values1 = alignedData.Item1.Select(d => (double)d.Value!.Value).ToList();

            var values2 = alignedData.Item2.Select(d => (double)d.Value!.Value).ToList();

            var results = MathHelper.ApplyBinaryOperation(values1, values2, op);

            return (results, metricsList);
        }

        var computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);

        return (computedResults, metricsList);
    }


    private async Task RenderDistribution(ChartDataContext ctx, CartesianChart chartDistribution, ChartState chartState, DistributionMode mode)
    {
        var settings = chartState.GetDistributionSettings(mode);
        switch (mode)
        {
            case DistributionMode.Weekly:
                await _weeklyDistributionService.UpdateDistributionChartAsync(chartDistribution, ctx.Data1!, ctx.DisplayName1, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount);
                break;
            case DistributionMode.Hourly:
                await _hourlyDistributionService.UpdateDistributionChartAsync(chartDistribution, ctx.Data1!, ctx.DisplayName1, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount);
                break;
            default:
                await _weeklyDistributionService.UpdateDistributionChartAsync(chartDistribution, ctx.Data1!, ctx.DisplayName1, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount);
                break;
        }
    }

    private void RenderWeeklyTrend(ChartDataContext ctx)
    {
        // Implementation extracted from MainWindow
        // TODO: Implement weekly trend rendering
    }

    private static bool ShouldRenderCharts(ChartDataContext? ctx)
    {
        return ctx != null && ctx.Data1 != null && ctx.Data1.Any();
    }

    private static bool HasSecondaryData(ChartDataContext ctx)
    {
        return ctx.Data2 != null && ctx.Data2.Any();
    }

    /// <summary>
    ///     Builds the initial series list and labels from primary and secondary data.
    /// </summary>
    private static(List<IEnumerable<MetricData>> series, List<string> labels) BuildInitialSeriesList(IEnumerable<MetricData> data1, IEnumerable<MetricData>? data2, string displayName1, string displayName2)
    {
        var series = new List<IEnumerable<MetricData>>
        {
                data1
        };
        var labels = new List<string>
        {
                displayName1
        };

        if (data2 != null && data2.Any())
        {
            series.Add(data2);
            labels.Add(displayName2);
        }

        return (series, labels);
    }

    /// <summary>
    ///     Loads additional subtype data (subtypes 3, 4, etc.) and adds them to the series and labels lists.
    /// </summary>
    private async Task LoadAdditionalSubtypesAsync(
            List<IEnumerable<MetricData>> series,
            List<string> labels,
            string? metricType,
            DateTime from,
            DateTime to,
            IReadOnlyList<MetricSeriesSelection>? selectedSeries,
            string? resolutionTableName)
    {
        if (selectedSeries == null || selectedSeries.Count <= 2 || string.IsNullOrEmpty(_connectionString))
            return;

        //var metricSelectionService = new MetricSelectionService(_connectionString);
        var metricSelectionService = _metricSelectionService;
        if (metricSelectionService == null)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            metricSelectionService = new MetricSelectionService(_connectionString);
        }

        var tableName = resolutionTableName ?? DataAccessDefaults.DefaultTableName;

        // Load data for subtypes 3, 4, etc. via MetricSelectionService
        for (var i = 2; i < selectedSeries.Count; i++)
        {
            var selection = selectedSeries[i];
            if (string.IsNullOrWhiteSpace(selection.MetricType))
                continue;

            try
            {
                var (primary, _) = await metricSelectionService.LoadMetricDataAsync(
                        selection.MetricType,
                        selection.QuerySubtype,
                        null,
                        from,
                        to,
                        tableName);

                if (primary.Any())
                {
                    series.Add(primary);
                    labels.Add(selection.DisplayName);
                }
            }
            catch
            {
                // Skip if loading fails
            }
        }
    }

}
