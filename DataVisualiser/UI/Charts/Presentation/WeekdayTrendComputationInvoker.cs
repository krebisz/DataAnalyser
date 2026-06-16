using DataFileReader.Canonical;
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
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly TemporalMetricSeriesInputBuilder _inputBuilder;

    public WeekdayTrendComputationInvoker(
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService,
        VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
        _inputBuilder = new TemporalMetricSeriesInputBuilder(viewModel, metricSelectionService, vnextCoordinator);
    }

    public void ClearCache() => _inputBuilder.ClearCache();

    public async Task<WeekdayTrendResult?> ComputeAsync(ChartDataContext sourceCtx, MetricSeriesSelection? selectedSeries, string displayName)
    {
        return await ComputeAsync(CreateRequest(sourceCtx, selectedSeries, displayName));
    }

    public async Task<WeekdayTrendResult?> ComputeAsync(WeekdayTrendComputationRequest request)
    {
        var input = await _inputBuilder.BuildAsync(request.InputRequest);
        if (input == null || input.Data.Count == 0)
            return null;

        return ComputeFromContext(input.RenderingContext);
    }

    public WeekdayTrendComputationRequest CreateRequest(ChartDataContext ctx, MetricSeriesSelection? selectedSeries, string displayName)
    {
        return new WeekdayTrendComputationRequest(
            _inputBuilder.CreateRequest(
                ctx,
                selectedSeries,
                displayName,
                ChartProgramKind.WeekdayTrend,
                EvidenceRuntimePath.VNextWeekdayTrend,
                async (metricSelectionService, sel, from, to, table) =>
                {
                    var (primary, _) = await metricSelectionService.LoadMetricDataAsync(sel.MetricType, sel.QuerySubtype, null, from, to, table);
                    return ((IReadOnlyList<MetricData>)primary.ToList(), (ICanonicalMetricSeries?)null);
                }));
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
    TemporalMetricSeriesInputRequest InputRequest);
