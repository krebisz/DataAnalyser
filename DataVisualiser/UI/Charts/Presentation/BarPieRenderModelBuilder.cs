using System.Windows.Media;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Computation.BucketedSeries;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;
using UiChartRenderModel = DataVisualiser.UI.Charts.Presentation.UiChartRenderModel;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class BarPieRenderModelBuilder
{
    private readonly BucketedSeriesLegacyTotalsLoader _legacyTotalsLoader;
    private readonly object _paletteKey;
    private readonly VNextSeriesLoadCoordinator _vnextCoordinator;
    private readonly MainWindowViewModel _viewModel;

    public BarPieRenderModelBuilder(MainWindowViewModel viewModel, MetricSelectionService metricSelectionService, object paletteKey, VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _legacyTotalsLoader = new BucketedSeriesLegacyTotalsLoader(metricSelectionService);
        _paletteKey = paletteKey ?? throw new ArgumentNullException(nameof(paletteKey));
        _vnextCoordinator = vnextCoordinator ?? new VNextSeriesLoadCoordinator(metricSelectionService);
    }

    public async Task<UiChartRenderModel> BuildAsync(bool isPieMode)
    {
        var selections = BucketedSeriesInputPlanner.GetDistinctSelectedSeries(_viewModel);
        if (selections.Count == 0)
            return CreateEmptyModel();

        if (!BucketedSeriesInputPlanner.TryResolveDateRange(_viewModel, out var from, out var to))
            return CreateEmptyModel();

        var bucketPlan = BucketedSeriesInputPlanner.BuildBucketPlan(_viewModel, from, to);

        var seriesTotals = await LoadSeriesTotalsAsync(selections, from, to, bucketPlan);
        if (seriesTotals.Count == 0)
            return CreateEmptyModel();

        ColourPalette.Reset(_paletteKey);
        var coloredSeries = seriesTotals.Select(data => new BarPieSeriesValues(data.Selection, data.Totals, ColourPalette.Next(_paletteKey))).ToList();

        return isPieMode
            ? BuildPieModel(bucketPlan, coloredSeries)
            : BuildBarModel(bucketPlan, coloredSeries);
    }

    private UiChartRenderModel BuildPieModel(BucketedSeriesBucketPlan bucketPlan, IReadOnlyList<BarPieSeriesValues> coloredSeries)
    {
        var facets = bucketPlan.Buckets.Select(bucket =>
                               {
                                   var series = coloredSeries.Select(item => new ChartSeriesModel
                                   {
                                       Name = item.Selection.DisplayName,
                                       SeriesType = ChartSeriesType.Pie,
                                       Values = [item.Totals[bucket.Index]],
                                       Color = item.Color
                                   }).ToList();

                                   return new ChartFacetModel
                                   {
                                       Title = bucket.Label,
                                       Series = series
                                   };
                               })
                               .ToList();

        return new UiChartRenderModel
        {
            ChartName = RenderingDefaults.BarPieChartName,
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _viewModel.ChartState.IsBarPieVisible,
            Facets = facets,
            Legend = new ChartLegendModel
            {
                IsVisible = true,
                Placement = ChartLegendPlacement.Right
            },
            Interactions = new ChartInteractionModel
            {
                Hoverable = ChartUiDefaults.DefaultHoverable
            }
        };
    }

    private UiChartRenderModel BuildBarModel(BucketedSeriesBucketPlan bucketPlan, IReadOnlyList<BarPieSeriesValues> coloredSeries)
    {
        var barSeries = coloredSeries.Select(item => new ChartSeriesModel
        {
            Name = item.Selection.DisplayName,
            SeriesType = ChartSeriesType.Column,
            Values = item.Totals,
            Color = item.Color
        }).ToList();

        return new UiChartRenderModel
        {
            ChartName = RenderingDefaults.BarPieChartName,
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _viewModel.ChartState.IsBarPieVisible,
            Series = barSeries,
            AxesX =
            [
                new ChartAxisModel
                {
                    Title = "Interval",
                    Labels = bucketPlan.Buckets.Select(bucket => bucket.Label).ToList()
                }
            ],
            AxesY =
            [
                new ChartAxisModel
                {
                    Title = "Value"
                }
            ],
            Legend = new ChartLegendModel
            {
                IsVisible = true,
                Placement = ChartLegendPlacement.Right
            },
            Interactions = new ChartInteractionModel
            {
                EnableZoomX = true,
                EnablePanX = true,
                Hoverable = ChartUiDefaults.DefaultHoverable
            }
        };
    }

    private UiChartRenderModel CreateEmptyModel()
    {
        return new UiChartRenderModel
        {
            ChartName = RenderingDefaults.BarPieChartName,
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _viewModel.ChartState.IsBarPieVisible,
            Series = [],
            Facets = []
        };
    }

    private async Task<IReadOnlyList<BucketedSeriesTotals>> LoadSeriesTotalsAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to, BucketedSeriesBucketPlan plan)
    {
        var tableName = BucketedSeriesInputPlanner.ResolveTableName(_viewModel);
        var useCms = CmsConfiguration.ShouldUseCms("BarPieStrategy");
        var tasks = selections.Select(selection => LoadSeriesTotalsAsync(selection, from, to, tableName, plan, useCms)).ToList();
        var results = await Task.WhenAll(tasks);

        var lastVNext = results.FirstOrDefault(r => r?.Runtime?.RuntimePath == EvidenceRuntimePath.VNextBarPie);
        var lastLegacy = results.FirstOrDefault(r => r?.Runtime?.RuntimePath == EvidenceRuntimePath.Legacy);
        var barPieRuntime = lastVNext?.Runtime ?? lastLegacy?.Runtime;
        if (barPieRuntime != null)
            _viewModel.ChartState.SetFamilyRuntime(ChartProgramKind.BarPie, barPieRuntime);

        return results.Where(result => result != null).Select(result => result!).ToList();
    }

    private async Task<BucketedSeriesTotals?> LoadSeriesTotalsAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName, BucketedSeriesBucketPlan plan, bool useCms)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            var vnextResult = await _vnextCoordinator.LoadAsync(selection, from, to, tableName, ChartProgramKind.BarPie);
            if (vnextResult.Success && vnextResult.Data != null && vnextResult.Data.Count > 0)
            {
                var runtime = LoadRuntimeState.FromVNextSuccess(
                    EvidenceRuntimePath.VNextBarPie, vnextResult.RequestSignature,
                    vnextResult.SnapshotSignature, vnextResult.ProgramKind, vnextResult.ProgramSourceSignature);

                if (useCms && vnextResult.CmsSeries != null && vnextResult.CmsSeries.Samples.Count > 0)
                    return new BucketedSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(vnextResult.CmsSeries, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count), runtime);

                return new BucketedSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(vnextResult.Data.ToList(), plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count), runtime);
            }

            return await LoadSeriesTotalsLegacyAsync(selection, from, to, tableName, plan, useCms, vnextResult.FailureReason);
        }
        catch
        {
            return null;
        }
    }

    private Task<BucketedSeriesTotals?> LoadSeriesTotalsLegacyAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName, BucketedSeriesBucketPlan plan, bool useCms, string? vnextFailureReason)
    {
        var runtime = LoadRuntimeState.LegacyFallback(string.Empty, vnextFailureReason);
        return _legacyTotalsLoader.LoadAsync(selection, from, to, tableName, plan, useCms, runtime);
    }

    private sealed record BarPieSeriesValues(MetricSeriesSelection Selection, double?[] Totals, Color Color);
}
