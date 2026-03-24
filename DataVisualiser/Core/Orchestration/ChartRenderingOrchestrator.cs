using DataFileReader.Canonical;
using System.Linq;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
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
    private readonly IDistributionService _hourlyDistributionService;
    private readonly MetricSelectionService? _metricSelectionService;
    private readonly IStrategyCutOverService _strategyCutOverService;
    private readonly IDistributionService _weeklyDistributionService;

    public ChartRenderingOrchestrator(ChartUpdateCoordinator chartUpdateCoordinator, IDistributionService weeklyDistributionService, IDistributionService hourlyDistributionService, IStrategyCutOverService strategyCutOverService, string? connectionString = null)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _connectionString = connectionString;
    }

    public ChartRenderingOrchestrator(ChartUpdateCoordinator chartUpdateCoordinator, IDistributionService weeklyDistributionService, IDistributionService hourlyDistributionService, IStrategyCutOverService strategyCutOverService, MetricSelectionService metricSelectionService, string? connectionString = null)
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
    public async Task RenderPrimaryChart(ChartDataContext ctx, CartesianChart chartMain, IReadOnlyList<IEnumerable<MetricData>>? additionalSeries = null, IReadOnlyList<string>? additionalLabels = null, bool isStacked = false, bool isCumulative = false, IReadOnlyList<SeriesResult>? overlaySeries = null)
    {
        if (ctx == null || chartMain == null)
            return;

        var (series, labels) = BuildSeriesAndLabels(ctx, additionalSeries, additionalLabels);

        var strategy = CreatePrimaryStrategy(ctx, series, labels, ctx.CmsSeries, out var secondaryLabel);

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartMain, strategy, labels[0], secondaryLabel, 400, ctx.MetricType, ctx.PrimarySubtype, secondaryLabel != null ? ctx.SecondarySubtype : null, isOperationChart: false, secondaryMetricType: ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype, isStacked: isStacked, isCumulative: isCumulative, overlaySeries: overlaySeries);
    }

    /// <summary>
    ///     Renders the primary chart with support for additional subtypes.
    ///     Loads additional subtype data if more than 2 subtypes are selected.
    /// </summary>
    public async Task RenderPrimaryChartAsync(ChartDataContext ctx, CartesianChart chartMain, IEnumerable<MetricData> data1, IEnumerable<MetricData>? data2, string displayName1, string displayName2, DateTime from, DateTime to, string? metricType = null, IReadOnlyList<MetricSeriesSelection>? selectedSeries = null, string? resolutionTableName = null, bool isStacked = false, bool isCumulative = false, IReadOnlyList<SeriesResult>? overlaySeries = null)
    {
        if (ctx == null || chartMain == null)
            return;

        if (isStacked && selectedSeries != null)
        {
            var stackedSelections = selectedSeries.Where(selection => selection.QuerySubtype != null).ToList();
            var overlaySelections = selectedSeries.Where(selection => selection.QuerySubtype == null).ToList();

            if (stackedSelections.Count >= 2)
            {
                var (stackedSeries, stackedLabels, stackedCmsSeries) = await BuildSeriesFromSelectionsAsync(ctx, stackedSelections, resolutionTableName);
                if (stackedSeries.Count >= 2)
                {
                    var stackedContext = BuildMultiMetricContext(ctx, stackedCmsSeries, stackedSeries.Count);
                    var parameters = new StrategyCreationParameters
                    {
                            LegacySeries = stackedSeries,
                            CmsSeries = stackedContext.CmsSeries,
                            Labels = stackedLabels,
                            From = ctx.From,
                            To = ctx.To
                    };

                    var strategy = _strategyCutOverService.CreateStrategy(StrategyType.MultiMetric, stackedContext, parameters);
                    var computedOverlaySeries = overlaySeries ?? await BuildOverlaySeriesAsync(ctx, overlaySelections, resolutionTableName);

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartMain, strategy, stackedLabels[0], null, 400, ctx.MetricType, ctx.PrimarySubtype, null, isOperationChart: false, secondaryMetricType: ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype, isStacked: true, isCumulative: false, overlaySeries: computedOverlaySeries);

                    return;
                }
            }
        }

        // Build initial series list for multi-metric routing
        var (series, labels) = BuildInitialSeriesList(data1, data2, displayName1, displayName2);
        var cmsSeries = BuildInitialCmsSeries(ctx);

        // Load additional subtypes if more than 2 are selected
        await LoadAdditionalSubtypesAsync(series, labels, cmsSeries, metricType, from, to, selectedSeries, resolutionTableName);

        if (series.Count > 2)
        {
            var multiMetricContext = BuildMultiMetricContext(ctx, cmsSeries, series.Count);
            var strategy = CreatePrimaryStrategy(multiMetricContext, series, labels, multiMetricContext.CmsSeries, out _);

            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartMain, strategy, labels[0], null, 400, ctx.MetricType, ctx.PrimarySubtype, null, isOperationChart: false, secondaryMetricType: ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype, isStacked: isStacked, isCumulative: isCumulative, overlaySeries: overlaySeries);
            return;
        }

        // Use existing RenderPrimaryChart method
        await RenderPrimaryChart(ctx, chartMain, null, null, isStacked, isCumulative, overlaySeries);
    }

    private async Task<(List<IEnumerable<MetricData>> Series, List<string> Labels, List<ICanonicalMetricSeries> CmsSeries)> BuildSeriesFromSelectionsAsync(ChartDataContext ctx, IReadOnlyList<MetricSeriesSelection> selections, string? resolutionTableName)
    {
        var series = new List<IEnumerable<MetricData>>();
        var labels = new List<string>();
        var cmsSeries = new List<ICanonicalMetricSeries>();

        if (selections.Count == 0)
            return (series, labels, cmsSeries);

        var tableName = resolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var metricSelectionService = _metricSelectionService ?? new MetricSelectionService(_connectionString ?? string.Empty);

        foreach (var selection in selections)
        {
            var data = ResolveContextSeries(ctx, selection);
            var cms = ResolveContextCmsSeries(ctx, selection);
            if (data == null)
            {
                if (selection.QuerySubtype == null)
                    continue;

                if (string.IsNullOrWhiteSpace(selection.MetricType))
                    continue;

                var loaded = await metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, ctx.From, ctx.To, tableName);
                data = loaded.PrimaryLegacy.ToList();
                cms = loaded.PrimaryCms;
            }

            if (data == null || !data.Any())
                continue;

            series.Add(data);
            labels.Add(selection.DisplayName);
            if (cms != null)
                cmsSeries.Add(cms);
        }

        return (series, labels, cmsSeries);
    }

    private async Task<IReadOnlyList<SeriesResult>?> BuildOverlaySeriesAsync(ChartDataContext ctx, IReadOnlyList<MetricSeriesSelection> selections, string? resolutionTableName)
    {
        if (selections == null || selections.Count == 0)
            return null;

        var (overlaySeries, overlayLabels, _) = await BuildSeriesFromSelectionsAsync(ctx, selections, resolutionTableName);
        if (overlaySeries.Count == 0 || overlayLabels.Count == 0)
            return null;

        return BuildOverlaySeriesResults(overlaySeries, overlayLabels, ctx.From, ctx.To);
    }

    private static List<SeriesResult> BuildOverlaySeriesResults(IReadOnlyList<IEnumerable<MetricData>> series, IReadOnlyList<string> labels, DateTime from, DateTime to)
    {
        var results = new List<SeriesResult>();
        var smoothingService = new SmoothingService();

        for (var i = 0; i < Math.Min(series.Count, labels.Count); i++)
        {
            var orderedData = StrategyComputationHelper.FilterAndOrderByRange(series[i], from, to);
            if (orderedData.Count == 0)
                continue;

            var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();
            var rawValues = orderedData.Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN).ToList();
            var smoothedValues = smoothingService.SmoothSeries(orderedData, rawTimestamps, from, to).ToList();

            results.Add(new SeriesResult
            {
                    SeriesId = $"overlay_{i}",
                    DisplayName = labels[i],
                    Timestamps = rawTimestamps,
                    RawValues = rawValues,
                    Smoothed = smoothedValues
            });
        }

        return results;
    }

    private static IEnumerable<MetricData>? ResolveContextSeries(ChartDataContext ctx, MetricSeriesSelection selection)
    {
        if (IsMatchingSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsMatchingSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2;

        return null;
    }

    private static ICanonicalMetricSeries? ResolveContextCmsSeries(ChartDataContext ctx, MetricSeriesSelection selection)
    {
        if (IsMatchingSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.PrimaryCms as ICanonicalMetricSeries;

        if (IsMatchingSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.SecondaryCms as ICanonicalMetricSeries;

        return null;
    }

    private static bool IsMatchingSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(metricType) || string.IsNullOrWhiteSpace(selection.MetricType))
            return false;

        if (!string.Equals(metricType, selection.MetricType, StringComparison.OrdinalIgnoreCase))
            return false;

        var selectionSubtype = selection.Subtype ?? string.Empty;
        var ctxSubtype = subtype ?? string.Empty;

        return string.Equals(selectionSubtype, ctxSubtype, StringComparison.OrdinalIgnoreCase);
    }


    private static(bool IsStacked, bool IsCumulative) ResolveMainChartDisplayMode(MainChartDisplayMode mode)
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

    private IChartComputationStrategy CreatePrimaryStrategy(ChartDataContext ctx, List<IEnumerable<MetricData>> series, List<string> labels, IReadOnlyList<ICanonicalMetricSeries>? cmsSeries, out string? secondaryLabel)
    {
        secondaryLabel = null;

        if (series.Count > 2)
            return _strategyCutOverService.CreateStrategy(StrategyType.MultiMetric,
                    ctx,
                    new StrategyCreationParameters
                    {
                            LegacySeries = series,
                            CmsSeries = cmsSeries,
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

    private static List<ICanonicalMetricSeries> BuildInitialCmsSeries(ChartDataContext ctx)
    {
        var cmsSeries = new List<ICanonicalMetricSeries>(2);

        if (ctx.PrimaryCms is ICanonicalMetricSeries primaryCms)
            cmsSeries.Add(primaryCms);

        if (ctx.SecondaryCms is ICanonicalMetricSeries secondaryCms)
            cmsSeries.Add(secondaryCms);

        return cmsSeries;
    }

    private static ChartDataContext BuildMultiMetricContext(ChartDataContext ctx, IReadOnlyList<ICanonicalMetricSeries>? cmsSeries, int seriesCount)
    {
        return new ChartDataContext
        {
                PrimaryCms = ctx.PrimaryCms,
                SecondaryCms = ctx.SecondaryCms,
                CmsSeries = cmsSeries != null && cmsSeries.Count == seriesCount ? cmsSeries.ToList() : null,
                Data1 = ctx.Data1,
                Data2 = ctx.Data2,
                Timestamps = ctx.Timestamps,
                RawValues1 = ctx.RawValues1,
                RawValues2 = ctx.RawValues2,
                SmoothedValues1 = ctx.SmoothedValues1,
                SmoothedValues2 = ctx.SmoothedValues2,
                DifferenceValues = ctx.DifferenceValues,
                RatioValues = ctx.RatioValues,
                NormalizedValues1 = ctx.NormalizedValues1,
                NormalizedValues2 = ctx.NormalizedValues2,
                DisplayName1 = ctx.DisplayName1,
                DisplayName2 = ctx.DisplayName2,
                ActualSeriesCount = seriesCount,
                MetricType = ctx.MetricType,
                PrimaryMetricType = ctx.PrimaryMetricType,
                SecondaryMetricType = ctx.SecondaryMetricType,
                PrimarySubtype = ctx.PrimarySubtype,
                SecondarySubtype = ctx.SecondarySubtype,
                DisplayPrimaryMetricType = ctx.DisplayPrimaryMetricType,
                DisplaySecondaryMetricType = ctx.DisplaySecondaryMetricType,
                DisplayPrimarySubtype = ctx.DisplayPrimarySubtype,
                DisplaySecondarySubtype = ctx.DisplaySecondarySubtype,
                From = ctx.From,
                To = ctx.To
        };
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
        var operationSymbol = isDifferenceMode ? "-" : "/";
        var strategyType = isDifferenceMode ? StrategyType.Difference : StrategyType.Ratio;
        var label = $"{ctx.DisplayName1} {operationSymbol} {ctx.DisplayName2}";
        var strategy = _strategyCutOverService.CreateStrategy(strategyType,
            ctx,
            new StrategyCreationParameters
            {
                LegacyData1 = ctx.Data1,
                LegacyData2 = ctx.Data2,
                Label1 = ctx.DisplayName1,
                Label2 = ctx.DisplayName2,
                From = ctx.From,
                To = ctx.To
            });

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chartDiffRatio, strategy, label, null, 400, metricType, primarySubtype, secondarySubtype, operationSymbol, true, ctx.SecondaryMetricType, ctx.DisplayPrimaryMetricType, ctx.DisplaySecondaryMetricType, ctx.DisplayPrimarySubtype, ctx.DisplaySecondarySubtype);
    }


    private async Task RenderDistribution(ChartDataContext ctx, CartesianChart chartDistribution, ChartState chartState, DistributionMode mode)
    {
        var settings = chartState.GetDistributionSettings(mode);
        var cmsSeries = ctx.PrimaryCms as ICanonicalMetricSeries;
        switch (mode)
        {
            case DistributionMode.Weekly:
                await _weeklyDistributionService.UpdateDistributionChartAsync(chartDistribution, ctx.Data1!, ctx.DisplayName1, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount, cmsSeries);
                break;
            case DistributionMode.Hourly:
                await _hourlyDistributionService.UpdateDistributionChartAsync(chartDistribution, ctx.Data1!, ctx.DisplayName1, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount, cmsSeries);
                break;
            default:
                await _weeklyDistributionService.UpdateDistributionChartAsync(chartDistribution, ctx.Data1!, ctx.DisplayName1, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount, cmsSeries);
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
    private async Task LoadAdditionalSubtypesAsync(List<IEnumerable<MetricData>> series, List<string> labels, List<ICanonicalMetricSeries> cmsSeries, string? metricType, DateTime from, DateTime to, IReadOnlyList<MetricSeriesSelection>? selectedSeries, string? resolutionTableName)
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
                var (primaryCms, _, primary, _) = await metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);

                if (primary.Any())
                {
                    series.Add(primary);
                    labels.Add(selection.DisplayName);
                    if (primaryCms != null)
                        cmsSeries.Add(primaryCms);
                }
            }
            catch
            {
                // Skip if loading fails
            }
        }
    }
}
