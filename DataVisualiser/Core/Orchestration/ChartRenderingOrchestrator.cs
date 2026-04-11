using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration.DistributionCharts;
using DataVisualiser.Core.Orchestration.MainChart;
using DataVisualiser.Core.Orchestration.SecondaryCharts;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration;

/// <summary>
///     Orchestrates chart rendering operations while keeping chart-family-specific
///     behavior behind explicit preparation/selection/render stages.
/// </summary>
public sealed class ChartRenderingOrchestrator
{
    private readonly string? _connectionString;
    private readonly IDistributionChartOrchestrationPipeline _distributionChartOrchestrationPipeline;
    private readonly IDistributionService _hourlyDistributionService;
    private readonly MetricSelectionService? _metricSelectionService;
    private readonly IMainChartOrchestrationPipeline _mainChartOrchestrationPipeline;
    private readonly IUserNotificationService _notificationService;
    private readonly ISecondaryMetricChartOrchestrationPipeline _secondaryMetricChartOrchestrationPipeline;
    private readonly IStrategyCutOverService _strategyCutOverService;
    private readonly IDistributionService _weeklyDistributionService;
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;

    public ChartRenderingOrchestrator(
        ChartUpdateCoordinator chartUpdateCoordinator,
        IDistributionService weeklyDistributionService,
        IDistributionService hourlyDistributionService,
        IStrategyCutOverService strategyCutOverService,
        IUserNotificationService notificationService,
        string? connectionString = null)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _connectionString = connectionString;
        _mainChartOrchestrationPipeline = CreateMainChartOrchestrationPipeline(metricSelectionService: null);
        _secondaryMetricChartOrchestrationPipeline = CreateSecondaryMetricChartOrchestrationPipeline();
        _distributionChartOrchestrationPipeline = CreateDistributionChartOrchestrationPipeline();
    }

    public ChartRenderingOrchestrator(
        ChartUpdateCoordinator chartUpdateCoordinator,
        IDistributionService weeklyDistributionService,
        IDistributionService hourlyDistributionService,
        IStrategyCutOverService strategyCutOverService,
        MetricSelectionService metricSelectionService,
        IUserNotificationService notificationService,
        string? connectionString = null)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _connectionString = connectionString;
        _mainChartOrchestrationPipeline = CreateMainChartOrchestrationPipeline(metricSelectionService);
        _secondaryMetricChartOrchestrationPipeline = CreateSecondaryMetricChartOrchestrationPipeline();
        _distributionChartOrchestrationPipeline = CreateDistributionChartOrchestrationPipeline();
    }

    public async Task RenderChartsFromContext(
        ChartDataContext ctx,
        ChartState chartState,
        CartesianChart chartMain,
        CartesianChart chartNorm,
        CartesianChart chartDiffRatio,
        CartesianChart chartDistribution)
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

    public async Task RenderSingleChart(
        string chartName,
        ChartDataContext ctx,
        ChartState chartState,
        CartesianChart chartMain,
        CartesianChart chartNorm,
        CartesianChart chartDiffRatio,
        CartesianChart chartDistribution)
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
                    await RenderDiffRatioChartAsync(ctx, chartDiffRatio, chartState);
                break;

            case "Distribution":
            case "WeeklyTrend":
                await RenderDistributionChartsIfVisible(ctx, chartState, chartDistribution);
                break;
        }
    }

    public Task RenderNormalizedChartAsync(ChartDataContext ctx, CartesianChart chartNorm, ChartState chartState)
    {
        return _secondaryMetricChartOrchestrationPipeline.RenderAsync(
            new SecondaryMetricChartRenderRequest(ctx, chartState, SecondaryMetricChartRoute.Normalized),
            chartNorm);
    }

    public Task RenderDiffRatioChartAsync(ChartDataContext ctx, CartesianChart chartDiffRatio, ChartState chartState)
    {
        if (!chartState.IsDiffRatioVisible)
            return Task.CompletedTask;

        return _secondaryMetricChartOrchestrationPipeline.RenderAsync(
            new SecondaryMetricChartRenderRequest(
                ctx,
                chartState,
                chartState.IsDiffRatioDifferenceMode ? SecondaryMetricChartRoute.Difference : SecondaryMetricChartRoute.Ratio),
            chartDiffRatio);
    }

    public Task RenderWeeklyDistributionChartAsync(ChartDataContext ctx, CartesianChart chartWeekly, ChartState chartState)
    {
        return _distributionChartOrchestrationPipeline.RenderAsync(
            new DistributionChartOrchestrationRequest(ctx, chartState, DistributionMode.Weekly),
            chartWeekly);
    }

    public Task RenderDistributionChartAsync(ChartDataContext ctx, CartesianChart chartDistribution, ChartState chartState, DistributionMode mode)
    {
        return _distributionChartOrchestrationPipeline.RenderAsync(
            new DistributionChartOrchestrationRequest(ctx, chartState, mode),
            chartDistribution);
    }

    public async Task<ChartDataContext?> RenderPrimaryChart(
        ChartDataContext ctx,
        CartesianChart chartMain,
        IReadOnlyList<IEnumerable<MetricData>>? additionalSeries = null,
        IReadOnlyList<string>? additionalLabels = null,
        bool isStacked = false,
        bool isCumulative = false,
        IReadOnlyList<SeriesResult>? overlaySeries = null)
    {
        if (ctx == null || chartMain == null)
            return null;

        var preparedData = await _mainChartOrchestrationPipeline.RenderAsync(
            new MainChartRenderRequest(
                ctx,
                IsStacked: isStacked,
                IsCumulative: isCumulative,
                OverlaySeries: overlaySeries,
                AdditionalSeries: additionalSeries,
                AdditionalLabels: additionalLabels),
            chartMain);
        return preparedData.WorkingContext;
    }

    public async Task<ChartDataContext?> RenderPrimaryChartAsync(
        ChartDataContext ctx,
        CartesianChart chartMain,
        IEnumerable<MetricData> data1,
        IEnumerable<MetricData>? data2,
        string displayName1,
        string displayName2,
        DateTime from,
        DateTime to,
        string? metricType = null,
        IReadOnlyList<MetricSeriesSelection>? selectedSeries = null,
        string? resolutionTableName = null,
        bool isStacked = false,
        bool isCumulative = false,
        IReadOnlyList<SeriesResult>? overlaySeries = null)
    {
        if (ctx == null || chartMain == null)
            return null;

        var requestedContext = BuildPrimaryRequestContext(ctx, data1, data2, displayName1, displayName2, from, to, metricType);

        var preparedData = await _mainChartOrchestrationPipeline.RenderAsync(
            new MainChartRenderRequest(
                requestedContext,
                selectedSeries,
                resolutionTableName,
                isStacked,
                isCumulative,
                overlaySeries),
            chartMain);
        return preparedData.WorkingContext;
    }

    private async Task RenderPrimaryIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartMain)
    {
        if (!chartState.IsMainVisible)
            return;

        var (isStacked, isCumulative) = ResolveMainChartDisplayMode(chartState.MainChartDisplayMode);
        await RenderPrimaryChart(ctx, chartMain, isStacked: isStacked, isCumulative: isCumulative);
    }

    private async Task RenderSecondaryChartsIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartNorm, CartesianChart? chartDiffRatio)
    {
        if (chartState.IsNormalizedVisible)
            await RenderNormalizedChartAsync(ctx, chartNorm, chartState);

        if (chartState.IsDiffRatioVisible && chartDiffRatio != null)
            await RenderDiffRatioChartAsync(ctx, chartDiffRatio, chartState);
    }

    private static void ClearSecondaryCharts(CartesianChart chartNorm, CartesianChart chartDiffRatio, ChartState chartState)
    {
        ChartHelper.ClearChart(chartNorm, chartState.ChartTimestamps);
        ChartHelper.ClearChart(chartDiffRatio, chartState.ChartTimestamps);
    }

    private async Task RenderDistributionChartsIfVisible(ChartDataContext ctx, ChartState chartState, CartesianChart chartDistribution)
    {
        if (chartState.IsDistributionVisible)
            await RenderDistributionChartAsync(ctx, chartDistribution, chartState, chartState.SelectedDistributionMode);
    }

    private IMainChartOrchestrationPipeline CreateMainChartOrchestrationPipeline(MetricSelectionService? metricSelectionService)
    {
        var preparationStage = new MainChartPreparationStage(metricSelectionService, _connectionString);
        var strategySelectionStage = new MainChartStrategySelectionStage(
            new StrategySelectionService(_strategyCutOverService, _connectionString ?? string.Empty));
        var renderInvocationStage = new MainChartRenderInvocationStage(_chartUpdateCoordinator);

        return new MainChartOrchestrationPipeline(preparationStage, strategySelectionStage, renderInvocationStage);
    }

    private ISecondaryMetricChartOrchestrationPipeline CreateSecondaryMetricChartOrchestrationPipeline()
    {
        var strategySelectionStage = new SecondaryMetricChartStrategySelectionStage(_strategyCutOverService);
        var renderInvocationStage = new SecondaryMetricChartRenderInvocationStage(_chartUpdateCoordinator);
        return new SecondaryMetricChartOrchestrationPipeline(strategySelectionStage, renderInvocationStage, _notificationService);
    }

    private IDistributionChartOrchestrationPipeline CreateDistributionChartOrchestrationPipeline()
    {
        var preparationStage = new DistributionChartPreparationStage(_weeklyDistributionService, _hourlyDistributionService);
        var renderInvocationStage = new DistributionChartRenderInvocationStage();
        return new DistributionChartOrchestrationPipeline(preparationStage, renderInvocationStage, _notificationService);
    }

    private static ChartDataContext BuildPrimaryRequestContext(
        ChartDataContext context,
        IEnumerable<MetricData> data1,
        IEnumerable<MetricData>? data2,
        string displayName1,
        string displayName2,
        DateTime from,
        DateTime to,
        string? metricType)
    {
        return new ChartDataContext
        {
            PrimaryCms = context.PrimaryCms,
            SecondaryCms = context.SecondaryCms,
            CmsSeries = context.CmsSeries,
            Data1 = data1.ToList(),
            Data2 = data2?.ToList(),
            Timestamps = context.Timestamps,
            RawValues1 = context.RawValues1,
            RawValues2 = context.RawValues2,
            SmoothedValues1 = context.SmoothedValues1,
            SmoothedValues2 = context.SmoothedValues2,
            DifferenceValues = context.DifferenceValues,
            RatioValues = context.RatioValues,
            NormalizedValues1 = context.NormalizedValues1,
            NormalizedValues2 = context.NormalizedValues2,
            DisplayName1 = displayName1,
            DisplayName2 = displayName2,
            ActualSeriesCount = context.ActualSeriesCount,
            MetricType = metricType ?? context.MetricType,
            PrimaryMetricType = context.PrimaryMetricType,
            SecondaryMetricType = context.SecondaryMetricType,
            PrimarySubtype = context.PrimarySubtype,
            SecondarySubtype = context.SecondarySubtype,
            DisplayPrimaryMetricType = context.DisplayPrimaryMetricType,
            DisplaySecondaryMetricType = context.DisplaySecondaryMetricType,
            DisplayPrimarySubtype = context.DisplayPrimarySubtype,
            DisplaySecondarySubtype = context.DisplaySecondarySubtype,
            From = from,
            To = to
        };
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

    private static bool ShouldRenderCharts(ChartDataContext? ctx)
    {
        return ctx != null && ctx.Data1 != null && ctx.Data1.Any();
    }

    private static bool HasSecondaryData(ChartDataContext ctx)
    {
        return ctx.Data2 != null && ctx.Data2.Any();
    }
}
