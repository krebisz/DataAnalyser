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
        var data = await ResolveDataAsync(sourceCtx, selectedSeries);
        if (data == null || data.Count == 0)
            return null;

        var trendContext = BuildTrendContext(sourceCtx, selectedSeries, data, displayName);
        return ComputeFromContext(trendContext);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (data, _) = await VNextDataResolutionHelper.ResolveSeriesDataAsync(
            ctx, selectedSeries, _selectionCache, tableName, _vnextCoordinator,
            ChartProgramKind.WeekdayTrend, EvidenceRuntimePath.VNextWeekdayTrend,
            runtime => _viewModel.ChartState.SetFamilyRuntime(ChartProgramKind.WeekdayTrend, runtime),
            async (sel, from, to, table) =>
            {
                var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(sel.MetricType, sel.QuerySubtype, null, from, to, table);
                return ((IReadOnlyList<MetricData>)primary.ToList(), (ICanonicalMetricSeries?)null);
            });
        return data;
    }

    private static ChartDataContext BuildTrendContext(ChartDataContext sourceCtx, MetricSeriesSelection? selectedSeries, IReadOnlyList<MetricData> data, string displayName)
    {
        return new ChartDataContext
        {
            Data1 = data,
            DisplayName1 = displayName,
            MetricType = selectedSeries?.MetricType ?? sourceCtx.MetricType,
            PrimaryMetricType = selectedSeries?.MetricType ?? sourceCtx.PrimaryMetricType,
            PrimarySubtype = selectedSeries?.Subtype,
            DisplayPrimaryMetricType = selectedSeries?.DisplayMetricType ?? sourceCtx.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = selectedSeries?.DisplaySubtype ?? sourceCtx.DisplayPrimarySubtype,
            From = sourceCtx.From,
            To = sourceCtx.To
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
