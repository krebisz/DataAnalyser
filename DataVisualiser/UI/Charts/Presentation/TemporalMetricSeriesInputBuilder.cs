using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TemporalMetricSeriesInputBuilder
{
    private readonly MainWindowViewModel _viewModel;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly VNextSeriesLoadCoordinator _vnextCoordinator;
    private readonly MetricSeriesSelectionCache _selectionCache = new();

    public TemporalMetricSeriesInputBuilder(
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService,
        VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _vnextCoordinator = vnextCoordinator ?? new VNextSeriesLoadCoordinator(metricSelectionService);
    }

    public void ClearCache() => _selectionCache.Clear();

    public async Task<TemporalMetricSeriesInput?> BuildAsync(TemporalMetricSeriesInputRequest request)
    {
        var resolution = await VNextDataResolutionHelper.ResolveSeriesDataAsync(request.SeriesResolution);
        if (resolution.Data == null)
            return null;

        return new TemporalMetricSeriesInput(
            request.SelectedSeries,
            resolution.Data,
            resolution.Cms,
            request.DisplayName,
            request.From,
            request.To,
            BuildContext(request.SourceContext, request.SelectedSeries, resolution.Data, resolution.Cms, request.DisplayName, request.From, request.To));
    }

    public TemporalMetricSeriesInputRequest CreateRequest(
        ChartDataContext ctx,
        MetricSeriesSelection? selectedSeries,
        string displayName,
        ChartProgramKind programKind,
        EvidenceRuntimePath runtimePath,
        TemporalMetricSeriesLegacyLoad legacyLoad)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        return new TemporalMetricSeriesInputRequest(
            selectedSeries,
            SeriesResolutionRequest.FromContext(
                ctx,
                selectedSeries,
                _selectionCache,
                tableName,
                _vnextCoordinator,
                programKind,
                runtimePath,
                runtime => _viewModel.ChartState.SetFamilyRuntime(programKind, runtime),
                (sel, from, to, table) => legacyLoad(_metricSelectionService, sel, from, to, table)),
            displayName,
            ctx.From,
            ctx.To,
            ctx);
    }

    public static ChartDataContext BuildContext(
        ChartDataContext sourceContext,
        MetricSeriesSelection? selectedSeries,
        IReadOnlyList<MetricData> data,
        ICanonicalMetricSeries? cmsSeries,
        string displayName,
        DateTime from,
        DateTime to)
    {
        return new ChartDataContext
        {
            PrimaryCms = cmsSeries,
            Data1 = data,
            DisplayName1 = displayName,
            MetricType = selectedSeries?.MetricType ?? sourceContext.MetricType,
            PrimaryMetricType = selectedSeries?.MetricType ?? sourceContext.PrimaryMetricType,
            PrimarySubtype = selectedSeries?.Subtype,
            DisplayPrimaryMetricType = selectedSeries?.DisplayMetricType ?? sourceContext.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = selectedSeries?.DisplaySubtype ?? sourceContext.DisplayPrimarySubtype,
            From = from,
            To = to
        };
    }
}

internal delegate Task<(IReadOnlyList<MetricData> Data, ICanonicalMetricSeries? Cms)> TemporalMetricSeriesLegacyLoad(
    MetricSelectionService metricSelectionService,
    MetricSeriesSelection selection,
    DateTime from,
    DateTime to,
    string tableName);

internal sealed record TemporalMetricSeriesInputRequest(
    MetricSeriesSelection? SelectedSeries,
    SeriesResolutionRequest SeriesResolution,
    string DisplayName,
    DateTime From,
    DateTime To,
    ChartDataContext SourceContext);

internal sealed record TemporalMetricSeriesInput(
    MetricSeriesSelection? SelectedSeries,
    IReadOnlyList<MetricData> Data,
    ICanonicalMetricSeries? CmsSeries,
    string DisplayName,
    DateTime From,
    DateTime To,
    ChartDataContext RenderingContext);
