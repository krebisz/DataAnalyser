using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Defaults;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using UiChartRenderModel = DataVisualiser.UI.Charts.Rendering.UiChartRenderModel;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Adapters;

public sealed class BarPieChartControllerAdapter : ChartControllerAdapterBase, IBarPieChartControllerExtras
{
    private readonly IBarPieChartController _controller;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly IChartRendererResolver _rendererResolver;
    private readonly IChartSurfaceFactory _surfaceFactory;
    private readonly MainWindowViewModel _viewModel;
    private IChartRenderer? _renderer;
    private IChartSurface? _surface;

    public BarPieChartControllerAdapter(
        IBarPieChartController controller,
        MainWindowViewModel viewModel,
        Func<bool> isInitializing,
        MetricSelectionService metricSelectionService,
        IChartRendererResolver rendererResolver,
        IChartSurfaceFactory surfaceFactory)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _rendererResolver = rendererResolver ?? throw new ArgumentNullException(nameof(rendererResolver));
        _surfaceFactory = surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory));
    }

    public void InitializeControls()
    {
        _controller.BucketCountCombo.Items.Clear();
        for (var i = 1; i <= 20; i++)
            _controller.BucketCountCombo.Items.Add(new ComboBoxItem
            {
                    Content = i.ToString(),
                    Tag = i
            });

        SelectBarPieBucketCount(_viewModel.ChartState.BarPieBucketCount);
    }

    public Task RenderIfVisibleAsync()
    {
        if (!_viewModel.ChartState.IsBarPieVisible)
            return Task.CompletedTask;

        return RenderBarPieChartAsync();
    }

    public override string Key => ChartControllerKeys.BarPie;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderBarPieChartAsync();
    }

    public override void Clear(ChartState state)
    {
        EnsureSurfaceAndRenderer();
        _ = _renderer!.ApplyAsync(_surface!, CreateEmptyBarPieModel());
    }

    public override void ResetZoom()
    {
        var chart = FindCartesianChart(_controller.Panel.ChartContentPanel);
        if (chart != null)
            ChartSurfaceHelper.ResetZoom(chart);
    }

    public override bool HasSeries(ChartState state)
    {
        return false;
    }

    public override void UpdateSubtypeOptions()
    {
    }

    public override void ClearCache()
    {
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleBarPie();
    }

    public async void OnDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        if (_viewModel.ChartState.IsBarPieVisible)
            await RenderBarPieChartAsync();
    }

    public async void OnBucketCountChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        if (_controller.BucketCountCombo.SelectedItem is ComboBoxItem selectedItem && TryGetIntervalCount(selectedItem.Tag, out var bucketCount))
            _viewModel.ChartState.BarPieBucketCount = bucketCount;

        if (_viewModel.ChartState.IsBarPieVisible)
            await RenderBarPieChartAsync();
    }

    private void EnsureSurfaceAndRenderer()
    {
        _surface ??= _surfaceFactory.Create(Key, _controller.Panel);
        _renderer ??= _rendererResolver.ResolveRenderer(Key);
    }

    private async Task RenderBarPieChartAsync()
    {
        EnsureSurfaceAndRenderer();

        if (!_viewModel.ChartState.IsBarPieVisible)
        {
            await _renderer!.ApplyAsync(_surface!, CreateEmptyBarPieModel());
            return;
        }

        var isPieMode = _controller.PieModeRadio.IsChecked == true;
        var model = await BuildBarPieRenderModelAsync(isPieMode);
        await _renderer!.ApplyAsync(_surface!, model);
    }

    private async Task<UiChartRenderModel> BuildBarPieRenderModelAsync(bool isPieMode)
    {
        var selections = GetDistinctSelectedSeries();
        if (selections.Count == 0)
            return CreateEmptyBarPieModel();

        if (!TryResolveBarPieDateRange(out var from, out var to))
            return CreateEmptyBarPieModel();

        var bucketCount = ResolveBarPieBucketCount(from, to);
        var bucketPlan = BuildBarPieBucketPlan(from, to, bucketCount);

        var seriesTotals = await LoadBarPieSeriesTotalsAsync(selections, from, to, bucketPlan);
        if (seriesTotals.Count == 0)
            return CreateEmptyBarPieModel();

        var paletteKey = _controller;
        ColourPalette.Reset(paletteKey);

        var coloredSeries = seriesTotals.Select(data => new BarPieSeriesValues(data.Selection, data.Totals, ColourPalette.Next(paletteKey))).ToList();

        if (isPieMode)
        {
            var facets = bucketPlan.Buckets.Select(bucket =>
                                   {
                                       var series = coloredSeries.Select(item => new ChartSeriesModel
                                                                 {
                                                                         Name = item.Selection.DisplayName,
                                                                         SeriesType = ChartSeriesType.Pie,
                                                                         Values = new[]
                                                                         {
                                                                                 item.Totals[bucket.Index]
                                                                         },
                                                                         Color = item.Color
                                                                 })
                                                                 .ToList();

                                       return new ChartFacetModel
                                       {
                                               Title = bucket.Label,
                                               Series = series
                                       };
                                   })
                                   .ToList();

            return new UiChartRenderModel
            {
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

        var barSeries = coloredSeries.Select(item => new ChartSeriesModel
                                     {
                                             Name = item.Selection.DisplayName,
                                             SeriesType = ChartSeriesType.Column,
                                             Values = item.Totals,
                                             Color = item.Color
                                     })
                                     .ToList();

        return new UiChartRenderModel
        {
                Title = ChartUiDefaults.BarPieChartTitle,
                IsVisible = _viewModel.ChartState.IsBarPieVisible,
                Series = barSeries,
                AxesX = new[]
                {
                        new ChartAxisModel
                        {
                                Title = "Interval",
                                Labels = bucketPlan.Buckets.Select(bucket => bucket.Label).ToList()
                        }
                },
                AxesY = new[]
                {
                        new ChartAxisModel
                        {
                                Title = "Value"
                        }
                },
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

    private UiChartRenderModel CreateEmptyBarPieModel()
    {
        return new UiChartRenderModel
        {
                Title = ChartUiDefaults.BarPieChartTitle,
                IsVisible = _viewModel.ChartState.IsBarPieVisible,
                Series = Array.Empty<ChartSeriesModel>(),
                Facets = Array.Empty<ChartFacetModel>()
        };
    }

    private List<MetricSeriesSelection> GetDistinctSelectedSeries()
    {
        return _viewModel.MetricState.SelectedSeries.GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase).Select(group => group.First()).ToList();
    }

    private async Task<IReadOnlyList<BarPieSeriesTotals>> LoadBarPieSeriesTotalsAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to, BarPieBucketPlan plan)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var useCms = CmsConfiguration.ShouldUseCms("BarPieStrategy");
        var tasks = selections.Select(selection => LoadBarPieSeriesTotalsAsync(selection, from, to, tableName, plan, useCms)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(result => result != null).Select(result => result!).ToList();
    }

    private async Task<BarPieSeriesTotals?> LoadBarPieSeriesTotalsAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName, BarPieBucketPlan plan, bool useCms)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            if (useCms)
            {
                var (primaryCms, _, primaryLegacy, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);
                if (primaryCms != null && primaryCms.Samples.Count > 0)
                    return new BarPieSeriesTotals(selection, BuildBucketTotals(primaryCms, plan));

                var fallbackLegacy = primaryLegacy?.ToList() ?? new List<MetricData>();
                if (fallbackLegacy.Count == 0)
                    return null;

                return new BarPieSeriesTotals(selection, BuildBucketTotals(fallbackLegacy, plan));
            }

            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new BarPieSeriesTotals(selection, BuildBucketTotals(dataList, plan));
        }
        catch
        {
            return null;
        }
    }

    private int ResolveBarPieBucketCount(DateTime from, DateTime to)
    {
        const int bucketMax = 20;

        if (to <= from)
            return 1;

        var bucketCount = _viewModel.ChartState.BarPieBucketCount;
        return Math.Max(1, Math.Min(bucketCount, bucketMax));
    }

    private static BarPieBucketPlan BuildBarPieBucketPlan(DateTime from, DateTime to, int bucketCount)
    {
        if (to < from)
            (from, to) = (to, from);

        bucketCount = Math.Max(1, bucketCount);
        var totalTicks = Math.Max(1, (to - from).Ticks);
        var bucketTicks = totalTicks / (double)bucketCount;

        var buckets = new List<BarPieBucket>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            var startTicks = from.Ticks + (long)Math.Floor(i * bucketTicks);
            var endTicks = i == bucketCount - 1 ? to.Ticks : from.Ticks + (long)Math.Floor((i + 1) * bucketTicks);

            if (endTicks < startTicks)
                endTicks = startTicks;

            var start = new DateTime(startTicks);
            var end = new DateTime(Math.Min(endTicks, to.Ticks));
            buckets.Add(new BarPieBucket(i, start, end, $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}"));
        }

        return new BarPieBucketPlan(from, to, bucketTicks, buckets);
    }

    private static double?[] BuildBucketTotals(IReadOnlyList<MetricData> data, BarPieBucketPlan plan)
    {
        var totals = new double?[plan.Buckets.Count];
        var sums = new double[plan.Buckets.Count];
        var counts = new int[plan.Buckets.Count];

        foreach (var point in data)
        {
            if (!point.Value.HasValue)
                continue;

            var index = ResolveBucketIndex(point.NormalizedTimestamp, plan);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)point.Value.Value;
            counts[index] += 1;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = counts[i] > 0 ? sums[i] / counts[i] : null;

        return totals;
    }

    private static double?[] BuildBucketTotals(ICanonicalMetricSeries series, BarPieBucketPlan plan)
    {
        var totals = new double?[plan.Buckets.Count];
        var sums = new double[plan.Buckets.Count];
        var counts = new int[plan.Buckets.Count];

        foreach (var sample in series.Samples)
        {
            if (!sample.Value.HasValue)
                continue;

            var timestamp = sample.Timestamp.LocalDateTime;
            var index = ResolveBucketIndex(timestamp, plan);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)sample.Value.Value;
            counts[index] += 1;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = counts[i] > 0 ? sums[i] / counts[i] : null;

        return totals;
    }

    private static int ResolveBucketIndex(DateTime timestamp, BarPieBucketPlan plan)
    {
        if (timestamp < plan.From || timestamp > plan.To)
            return -1;

        if (plan.BucketTicks <= 0)
            return 0;

        var offsetTicks = timestamp.Ticks - plan.From.Ticks;
        var index = (int)Math.Floor(offsetTicks / plan.BucketTicks);
        if (index < 0)
            return -1;
        if (index >= plan.Buckets.Count)
            return plan.Buckets.Count - 1;

        return index;
    }

    private bool TryResolveBarPieDateRange(out DateTime from, out DateTime to)
    {
        if (_viewModel.MetricState.FromDate.HasValue && _viewModel.MetricState.ToDate.HasValue)
        {
            from = _viewModel.MetricState.FromDate.Value;
            to = _viewModel.MetricState.ToDate.Value;
            return true;
        }

        var context = _viewModel.ChartState.LastContext;
        if (context != null && context.From != default && context.To != default)
        {
            from = context.From;
            to = context.To;
            return true;
        }

        from = default;
        to = default;
        return false;
    }

    private void SelectBarPieBucketCount(int bucketCount)
    {
        foreach (var item in _controller.BucketCountCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is int taggedInterval && taggedInterval == bucketCount)
            {
                _controller.BucketCountCombo.SelectedItem = item;
                return;
            }
    }

    private static bool TryGetIntervalCount(object? tag, out int intervalCount)
    {
        switch (tag)
        {
            case int direct:
                intervalCount = direct;
                return true;
            case string tagValue when int.TryParse(tagValue, out var parsed):
                intervalCount = parsed;
                return true;
            default:
                intervalCount = 0;
                return false;
        }
    }

    private sealed record BarPieSeriesTotals(MetricSeriesSelection Selection, double?[] Totals);

    private sealed record BarPieSeriesValues(MetricSeriesSelection Selection, double?[] Totals, Color Color);

    private sealed record BarPieBucket(int Index, DateTime Start, DateTime End, string Label);

    private sealed record BarPieBucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<BarPieBucket> Buckets);

    private static CartesianChart? FindCartesianChart(DependencyObject? root)
    {
        if (root == null)
            return null;

        if (root is CartesianChart chart)
            return chart;

        if (root is ContentPresenter presenter && presenter.Content is DependencyObject presented)
        {
            var presentedChart = FindCartesianChart(presented);
            if (presentedChart != null)
                return presentedChart;
        }

        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            var found = FindCartesianChart(child);
            if (found != null)
                return found;
        }

        foreach (var child in LogicalTreeHelper.GetChildren(root))
        {
            if (child is not DependencyObject dependencyChild)
                continue;

            var found = FindCartesianChart(dependencyChild);
            if (found != null)
                return found;
        }

        return null;
    }
}
