using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class WeekdayTrendComputationInvoker
{
    private readonly MainWindowViewModel _viewModel;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly VNextSeriesLoadCoordinator _vnextCoordinator;
    private readonly MetricSeriesSelectionCache _selectionCache = new();

    public WeekdayTrendComputationInvoker(
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService,
        VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
        _vnextCoordinator = vnextCoordinator ?? new VNextSeriesLoadCoordinator(metricSelectionService);
    }

    public void ClearCache() => _selectionCache.Clear();

    public async Task<WeekdayTrendResult?> ComputeAsync(ChartDataContext sourceCtx, MetricSeriesSelection? selectedSeries, string displayName)
    {
        return await ComputeAsync(CreateRequest(sourceCtx, selectedSeries, displayName));
    }

    public async Task<WeekdayTrendResult?> ComputeAsync(WeekdayTrendComputationRequest request)
    {
        var resolution = await VNextDataResolutionHelper.ResolveSeriesDataAsync(request.SeriesResolution);
        if (resolution.Data == null || resolution.Data.Count == 0)
            return null;

        var trendContext = BuildTrendContext(request, resolution.Data);
        return ComputeFromContext(trendContext);
    }

    public WeekdayTrendComputationRequest CreateRequest(ChartDataContext ctx, MetricSeriesSelection? selectedSeries, string displayName)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        return new WeekdayTrendComputationRequest(
            selectedSeries,
            SeriesResolutionRequest.FromContext(
                ctx, selectedSeries, _selectionCache, tableName, _vnextCoordinator,
                ChartProgramKind.WeekdayTrend, EvidenceRuntimePath.VNextWeekdayTrend,
                runtime => _viewModel.ChartState.SetFamilyRuntime(ChartProgramKind.WeekdayTrend, runtime),
                async (sel, from, to, table) =>
                {
                    var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(sel.MetricType, sel.QuerySubtype, null, from, to, table);
                    return ((IReadOnlyList<MetricData>)primary.ToList(), (ICanonicalMetricSeries?)null);
                }),
            displayName,
            ctx.From,
            ctx.To,
            ctx.MetricType,
            ctx.PrimaryMetricType,
            ctx.DisplayPrimaryMetricType,
            ctx.DisplayPrimarySubtype);
    }

    private static ChartDataContext BuildTrendContext(WeekdayTrendComputationRequest request, IReadOnlyList<MetricData> data)
    {
        return new ChartDataContext
        {
            Data1 = data,
            DisplayName1 = request.DisplayName,
            MetricType = request.SelectedSeries?.MetricType ?? request.MetricType,
            PrimaryMetricType = request.SelectedSeries?.MetricType ?? request.PrimaryMetricType,
            PrimarySubtype = request.SelectedSeries?.Subtype,
            DisplayPrimaryMetricType = request.SelectedSeries?.DisplayMetricType ?? request.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = request.SelectedSeries?.DisplaySubtype ?? request.DisplayPrimarySubtype,
            From = request.From,
            To = request.To
        };
    }

    public WeekdayTrendResult? ComputeFromContext(ChartDataContext ctx)
    {
        var strategyCutOverService = _getStrategyCutOverService();
        if (strategyCutOverService == null)
            throw new InvalidOperationException("StrategyCutOverService is not initialized. Ensure InitializeChartPipeline() is called before using strategies.");

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = ctx.Data1 ?? Array.Empty<MetricData>(),
            Label1 = ctx.DisplayName1,
            From = ctx.From,
            To = ctx.To
        };

        var strategy = strategyCutOverService.CreateStrategy(StrategyType.WeekdayTrend, ctx, parameters);
        strategy.Compute();

        return strategy is IWeekdayTrendResultProvider provider ? provider.ExtendedResult : null;
    }
}

internal sealed record WeekdayTrendComputationRequest(
    MetricSeriesSelection? SelectedSeries,
    SeriesResolutionRequest SeriesResolution,
    string DisplayName,
    DateTime From,
    DateTime To,
    string? MetricType,
    string? PrimaryMetricType,
    string? DisplayPrimaryMetricType,
    string? DisplayPrimarySubtype);
