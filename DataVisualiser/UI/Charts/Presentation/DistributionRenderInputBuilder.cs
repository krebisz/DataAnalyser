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

internal sealed class DistributionRenderInputBuilder
{
    private readonly MainWindowViewModel _viewModel;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly VNextSeriesLoadCoordinator _vnextCoordinator;
    private readonly MetricSeriesSelectionCache _selectionCache = new();

    public DistributionRenderInputBuilder(MainWindowViewModel viewModel, MetricSelectionService metricSelectionService, VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _vnextCoordinator = vnextCoordinator ?? new VNextSeriesLoadCoordinator(metricSelectionService);
    }

    public void ClearCache() => _selectionCache.Clear();

    public async Task<DistributionRenderInput?> BuildAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        var (data, cmsSeries) = await ResolveDistributionDataAsync(ctx, selectedSeries);
        if (data == null || (data.Count == 0 && cmsSeries == null))
            return null;

        var displayName = ResolveDistributionDisplayName(ctx, selectedSeries);
        return new DistributionRenderInput(selectedSeries, data, cmsSeries, displayName);
    }

    private Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveDistributionDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        return VNextDataResolutionHelper.ResolveSeriesDataAsync(
            ctx, selectedSeries, _selectionCache, tableName, _vnextCoordinator,
            ChartProgramKind.Distribution, EvidenceRuntimePath.VNextDistribution,
            runtime => _viewModel.ChartState.SetFamilyRuntime(ChartProgramKind.Distribution, runtime),
            async (sel, from, to, table) =>
            {
                var (cms, _, data, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(sel, null, from, to, table);
                return (data.ToList(), cms);
            });
    }

    private static string ResolveDistributionDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveDisplayName(ctx, selectedSeries);
    }

    public static ChartDataContext BuildDistributionContext(ChartDataContext ctx, DistributionRenderInput renderInput)
    {
        return new ChartDataContext
        {
            PrimaryCms = renderInput.CmsSeries,
            Data1 = renderInput.Data,
            DisplayName1 = renderInput.DisplayName,
            MetricType = renderInput.SelectedSeries?.MetricType ?? ctx.MetricType,
            PrimaryMetricType = renderInput.SelectedSeries?.MetricType ?? ctx.PrimaryMetricType,
            PrimarySubtype = renderInput.SelectedSeries?.Subtype,
            DisplayPrimaryMetricType = renderInput.SelectedSeries?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = renderInput.SelectedSeries?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
            From = ctx.From,
            To = ctx.To
        };
    }
}

internal sealed record DistributionRenderInput(
    MetricSeriesSelection? SelectedSeries,
    IReadOnlyList<MetricData> Data,
    ICanonicalMetricSeries? CmsSeries,
    string DisplayName);
