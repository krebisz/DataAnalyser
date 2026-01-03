using DataVisualiser.Charts;
using DataVisualiser.Charts.Helpers;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Services.ChartRendering;

/// <summary>
///     Orchestrates chart rendering operations, extracting complex rendering logic
///     from MainWindow to improve maintainability and testability.
///     Handles multi-chart rendering, visibility management, and chart-specific rendering strategies.
/// </summary>
public sealed class ChartRenderingOrchestrator
{
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;
    private readonly IStrategyCutOverService _strategyCutOverService;
    private readonly WeeklyDistributionService _weeklyDistributionService;

    public ChartRenderingOrchestrator(ChartUpdateCoordinator chartUpdateCoordinator, WeeklyDistributionService weeklyDistributionService, IStrategyCutOverService strategyCutOverService)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
    }

    /// <summary>
    ///     Renders all charts based on the provided context and visibility state.
    /// </summary>
    public async Task RenderChartsFromContext(ChartDataContext ctx, ChartState chartState, CartesianChart chartMain, CartesianChart chartNorm, CartesianChart chartDiffRatio, CartesianChart chartWeekly)
    {
        if (!ShouldRenderCharts(ctx))
            return;

        var hasSecondaryData = HasSecondaryData(ctx);

        await RenderPrimaryIfVisible(ctx, chartState, chartMain);

        if (hasSecondaryData)
            await RenderSecondaryChartsIfVisible(ctx, chartState, chartNorm, chartDiffRatio);
        else
            ClearSecondaryCharts(chartNorm, chartDiffRatio, chartState);

        await RenderWeeklyChartsIfVisible(ctx, chartState, chartWeekly);
    }

    private async Task RenderPrimaryIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartMain)
    {
        if (chartState.IsMainVisible)
            await RenderPrimaryChart(ctx, chartMain);
    }

    private async Task RenderSecondaryChartsIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartNorm, CartesianChart chartDiffRatio)
    {
        if (chartState.IsNormalizedVisible)
            await RenderNormalized(ctx, chartNorm, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype, chartState.SelectedNormalizationMode);

        if (chartState.IsDiffRatioVisible)
            await RenderDiffRatio(ctx, chartDiffRatio, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype, chartState);
    }

    private static void ClearSecondaryCharts(CartesianChart chartNorm, CartesianChart chartDiffRatio, ChartState chartState)
    {
        ChartHelper.ClearChart(chartNorm, chartState.ChartTimestamps);
        ChartHelper.ClearChart(chartDiffRatio, chartState.ChartTimestamps);
    }

    private async Task RenderWeeklyChartsIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartWeekly)
    {
        if (chartState.IsWeeklyVisible)
            await RenderWeeklyDistribution(ctx, chartWeekly, chartState);

        if (chartState.IsWeeklyTrendVisible)
            RenderWeeklyTrend(ctx);
    }


    /// <summary>
    ///     Renders a single chart by name.
    /// </summary>
    public async Task RenderSingleChart(string chartName, ChartDataContext ctx, ChartState chartState, CartesianChart chartMain, CartesianChart chartNorm, CartesianChart chartDiffRatio, CartesianChart chartWeekly)
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

            case "Weekly":
            case "WeeklyTrend":
                await RenderWeeklyChartsIfVisible(ctx, chartState, chartWeekly);
                break;
        }
    }


    /// <summary>
    ///     Renders the primary (main) chart using StrategyCutOverService.
    ///     Handles single, combined, and multi-metric strategies.
    /// </summary>
    public async Task RenderPrimaryChart(ChartDataContext ctx, CartesianChart chartMain, IReadOnlyList<IEnumerable<HealthMetricData>>? additionalSeries = null, IReadOnlyList<string>? additionalLabels = null)
    {
        if (ctx == null || chartMain == null)
            return;

        var (series, labels) = BuildSeriesAndLabels(ctx, additionalSeries, additionalLabels);

        var strategy = CreatePrimaryStrategy(ctx, series, labels, out var secondaryLabel);

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartMain, strategy, labels[0], secondaryLabel, 400, ctx.MetricType, ctx.PrimarySubtype, secondaryLabel != null ? ctx.SecondarySubtype : null, isOperationChart: false);
    }

    private static (List<IEnumerable<HealthMetricData>> Series, List<string> Labels) BuildSeriesAndLabels(ChartDataContext ctx, IReadOnlyList<IEnumerable<HealthMetricData>>? additionalSeries, IReadOnlyList<string>? additionalLabels)
    {
        var series = new List<IEnumerable<HealthMetricData>>
        {
            ctx.Data1 ?? Array.Empty<HealthMetricData>()
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
                if (additionalSeries[i] != null && additionalSeries[i].
                        Any())
                {
                    series.Add(additionalSeries[i]);
                    labels.Add(additionalLabels[i]);
                }

        return (series, labels);
    }

    private IChartComputationStrategy CreatePrimaryStrategy(ChartDataContext ctx, List<IEnumerable<HealthMetricData>> series, List<string> labels, out string? secondaryLabel)
    {
        secondaryLabel = null;

        if (series.Count > 2)
            return _strategyCutOverService.CreateStrategy(StrategyType.MultiMetric, ctx, new StrategyCreationParameters
            {
                LegacySeries = series,
                Labels = labels,
                From = ctx.From,
                To = ctx.To
            });

        if (series.Count == 2)
        {
            secondaryLabel = labels[1];

            return _strategyCutOverService.CreateStrategy(StrategyType.CombinedMetric, ctx, new StrategyCreationParameters
            {
                LegacyData1 = series[0],
                LegacyData2 = series[1],
                Label1 = labels[0],
                Label2 = labels[1],
                From = ctx.From,
                To = ctx.To
            });
        }

        return _strategyCutOverService.CreateStrategy(StrategyType.SingleMetric, ctx, new StrategyCreationParameters
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

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartNorm, strategy, $"{ctx.DisplayName1} ~ {ctx.DisplayName2}", minHeight: 400, metricType: metricType, primarySubtype: primarySubtype, secondarySubtype: secondarySubtype, operationType: "~", isOperationChart: true);
    }

    private async Task RenderDiffRatio(ChartDataContext ctx, CartesianChart chartDiffRatio, string? metricType, string? primarySubtype, string? secondarySubtype, ChartState? chartState = null)
    {
        if (ctx.Data1 == null || ctx.Data2 == null)
            return;

        var isDifferenceMode = chartState?.IsDiffRatioDifferenceMode ?? true;
        var operation = isDifferenceMode ? "Subtract" : "Divide";
        var operationSymbol = isDifferenceMode ? "-" : "/";

        // Use transform infrastructure to compute the operation
        var allData1List = ctx.Data1.Where(d => d.Value.HasValue).
            OrderBy(d => d.NormalizedTimestamp).
            ToList();

        var allData2List = ctx.Data2.Where(d => d.Value.HasValue).
            OrderBy(d => d.NormalizedTimestamp).
            ToList();

        if (allData1List.Count == 0 || allData2List.Count == 0)
            return;

        // Align data by timestamp
        var alignedData = TransformExpressionEvaluator.AlignMetricsByTimestamp(allData1List, allData2List);
        if (alignedData.Item1.Count == 0 || alignedData.Item2.Count == 0)
            return;

        // Build expression and evaluate
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
        List<double> computedResults;
        var metricsList = new List<IReadOnlyList<HealthMetricData>>
        {
            alignedData.Item1,
            alignedData.Item2
        };

        if (expression == null)
        {
            // Fallback to legacy approach
            var op = operation switch
            {
                "Subtract" => BinaryOperators.Difference,
                "Divide" => BinaryOperators.Ratio,
                _ => (a, b) => a
            };

            var allValues1 = alignedData.Item1.Select(d => (double)d.Value!.Value).
                ToList();
            var allValues2 = alignedData.Item2.Select(d => (double)d.Value!.Value).
                ToList();
            computedResults = MathHelper.ApplyBinaryOperation(allValues1, allValues2, op);
        }
        else
        {
            computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);
        }

        // Generate label
        var label = TransformExpressionEvaluator.GenerateTransformLabel(operation, metricsList, ctx);

        // Create strategy and render
        var strategy = new TransformResultStrategy(alignedData.Item1, computedResults, label, ctx.From, ctx.To);

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartDiffRatio, strategy, label, null, 400, metricType, primarySubtype, secondarySubtype, operationSymbol, true);
    }

    private async Task RenderWeeklyDistribution(ChartDataContext ctx, CartesianChart chartWeekly, ChartState chartState)
    {
        await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(chartWeekly, ctx.Data1!, ctx.DisplayName1, ctx.From, ctx.To, 400, chartState.UseFrequencyShading, chartState.WeeklyIntervalCount);
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
}