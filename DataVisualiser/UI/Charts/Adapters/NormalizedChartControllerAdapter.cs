using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using System.Windows;
using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Adapters;

public sealed class NormalizedChartControllerAdapter : CartesianChartControllerAdapterBase<INormalizedChartController>
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly INormalizedChartController _controller;
    private readonly ICartesianMetricChartRenderingContract _renderingContract;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombos;

    public NormalizedChartControllerAdapter(INormalizedChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, ICartesianMetricChartRenderingContract renderingContract)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _renderingContract = renderingContract ?? throw new ArgumentNullException(nameof(renderingContract));
    }

    public override void ClearCache()
    {
        _selectionCache.Clear();
    }

    public override string Key => ChartControllerKeys.Normalized;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => true;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderNormalizedAsync(context);
    }

    public override void UpdateSubtypeOptions()
    {
        if (!CanUpdateNormalizedSubtypeOptions())
            return;

        _isUpdatingSubtypeCombos = true;
        try
        {
            ClearNormalizedSubtypeCombos();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                HandleNoSelectedNormalizedSeries();
                return;
            }

            PopulateNormalizedSubtypeCombos(selectedSeries);
            UpdatePrimaryNormalizedSubtype(selectedSeries);
            UpdateSecondaryNormalizedSubtype(selectedSeries);
        }
        finally
        {
            _isUpdatingSubtypeCombos = false;
        }
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleNorm();
    }

    public override void Clear(ChartState state)
    {
        _renderingContract.Clear(CartesianMetricChartRoute.Normalized, CreateRenderHost());
    }

    public override void ResetZoom()
    {
        _renderingContract.ResetView(CartesianMetricChartRoute.Normalized, CreateRenderHost());
    }

    public override bool HasSeries(ChartState state)
    {
        return _renderingContract.HasRenderableContent(CartesianMetricChartRoute.Normalized, CreateRenderHost());
    }

    public async void OnNormalizationModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        try
        {
            ApplySelectedNormalizationMode();
            await RerenderNormalizedIfVisibleAsync();
        }
        catch
        {
            // intentional: mode change shouldn't hard-fail the UI
        }
    }

    public async void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombos)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.NormalizedPrimarySubtypeCombo);
        _viewModel.SetNormalizedPrimarySeries(selection);

        await RenderNormalizedFromSelectionAsync();
    }

    public async void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombos)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.NormalizedSecondarySubtypeCombo);
        _viewModel.SetNormalizedSecondarySeries(selection);

        await RenderNormalizedFromSelectionAsync();
    }

    private async Task RenderNormalizedFromSelectionAsync()
    {
        if (!_viewModel.ChartState.IsNormalizedVisible || _viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        await RenderNormalizedAsync(ctx);
    }

    private async Task RenderNormalizedAsync(ChartDataContext ctx)
    {
        var (primaryData, secondaryData, normalizedContext) = await ResolveNormalizedDataAsync(ctx);
        if (primaryData == null || secondaryData == null)
            return;

        UpdateNormalizedPanelTitle(normalizedContext);
        await _renderingContract.RenderAsync(
            new CartesianMetricChartRenderRequest(CartesianMetricChartRoute.Normalized, normalizedContext),
            CreateRenderHost());
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveNormalizedDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedNormalizedPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedNormalizedSecondarySeries(ctx);

        var (primaryData, primaryCms) = await ResolveNormalizedSeriesAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;
        ICanonicalMetricSeries? secondaryCms = null;
        if (secondarySelection != null)
        {
            var resolvedSecondary = await ResolveNormalizedSeriesAsync(ctx, secondarySelection);
            secondaryData = resolvedSecondary.Data;
            secondaryCms = resolvedSecondary.Cms;
        }

        var normalizedContext = BuildNormalizedContext(ctx, primarySelection, secondarySelection, primaryData, secondaryData, primaryCms, secondaryCms);

        return (primaryData, secondaryData, normalizedContext);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveNormalizedSeriesAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return (null, null);

        if (ctx.Data1 != null && MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (ctx.Data2 != null && MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2, ctx.SecondaryCms as ICanonicalMetricSeries);

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_selectionCache.TryGetDataWithCms(cacheKey, out var cached, out var cachedCms))
            return (cached, cachedCms);

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selectedSeries, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _selectionCache.SetDataWithCms(cacheKey, data, primaryCms);
        return (data, primaryCms);
    }

    private MetricSeriesSelection? ResolveSelectedNormalizedPrimarySeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionCache.ResolveSelection(!_isUpdatingSubtypeCombos,
                _controller.NormalizedPrimarySubtypeCombo,
                _viewModel.ChartState.SelectedNormalizedPrimarySeries,
                () =>
                {
                    var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
                    if (string.IsNullOrWhiteSpace(metricType))
                        return null;

                    return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
                });
    }

    private MetricSeriesSelection? ResolveSelectedNormalizedSecondarySeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionCache.ResolveSelection(!_isUpdatingSubtypeCombos,
                _controller.NormalizedSecondarySubtypeCombo,
                _viewModel.ChartState.SelectedNormalizedSecondarySeries,
                () =>
                {
                    var metricType = ctx.SecondaryMetricType;
                    if (string.IsNullOrWhiteSpace(metricType))
                        return null;

                    return new MetricSeriesSelection(metricType, ctx.SecondarySubtype);
                });
    }

    private static string ResolveNormalizedDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private void ApplySelectedNormalizationMode()
    {
        if (_controller.NormZeroToOneRadio.IsChecked == true)
            _viewModel.SetNormalizationMode(NormalizationMode.ZeroToOne);
        else if (_controller.NormPercentOfMaxRadio.IsChecked == true)
            _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
        else if (_controller.NormRelativeToMaxRadio.IsChecked == true)
            _viewModel.SetNormalizationMode(NormalizationMode.RelativeToMax);
    }

    private async Task RerenderNormalizedIfVisibleAsync()
    {
        var ctx = _viewModel.ChartState.LastContext;
        if (!_viewModel.ChartState.IsNormalizedVisible || ctx?.Data1 == null || ctx.Data2 == null)
            return;

        using var _ = _beginUiBusyScope();
        await RenderNormalizedAsync(ctx);
    }

    private static ChartDataContext BuildNormalizedContext(ChartDataContext ctx, MetricSeriesSelection? primarySelection, MetricSeriesSelection? secondarySelection, IReadOnlyList<MetricData>? primaryData, IReadOnlyList<MetricData>? secondaryData, ICanonicalMetricSeries? primaryCms, ICanonicalMetricSeries? secondaryCms)
    {
        return new ChartDataContext
        {
                Data1 = primaryData,
                Data2 = secondaryData,
                PrimaryCms = primaryCms,
                SecondaryCms = secondaryCms,
                DisplayName1 = ResolveNormalizedDisplayName(ctx, primarySelection),
                DisplayName2 = ResolveNormalizedDisplayName(ctx, secondarySelection),
                MetricType = primarySelection?.MetricType ?? ctx.MetricType,
                PrimaryMetricType = primarySelection?.MetricType ?? ctx.PrimaryMetricType,
                PrimarySubtype = primarySelection?.Subtype,
                SecondaryMetricType = secondarySelection?.MetricType ?? ctx.SecondaryMetricType,
                SecondarySubtype = secondarySelection?.Subtype,
                DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
                DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
                DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? ctx.DisplaySecondaryMetricType,
                DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? ctx.DisplaySecondarySubtype,
                From = ctx.From,
                To = ctx.To
        };
    }

    private void UpdateNormalizedPanelTitle(ChartDataContext ctx)
    {
        var leftName = ctx.DisplayName1 ?? string.Empty;
        var rightName = ctx.DisplayName2 ?? string.Empty;
        _controller.Panel.Title = $"{leftName} ~ {rightName}";
    }

    private bool CanUpdateNormalizedSubtypeOptions()
    {
        return _controller.NormalizedPrimarySubtypeCombo != null && _controller.NormalizedSecondarySubtypeCombo != null;
    }

    private void ClearNormalizedSubtypeCombos()
    {
        _controller.NormalizedPrimarySubtypeCombo.Items.Clear();
        _controller.NormalizedSecondarySubtypeCombo.Items.Clear();
    }

    private void HandleNoSelectedNormalizedSeries()
    {
        _controller.NormalizedPrimarySubtypeCombo.IsEnabled = false;
        _controller.NormalizedSecondarySubtypeCombo.IsEnabled = false;
        _controller.NormalizedSecondarySubtypePanel.Visibility = Visibility.Collapsed;

        _viewModel.ChartState.SelectedNormalizedPrimarySeries = null;
        _viewModel.ChartState.SelectedNormalizedSecondarySeries = null;

        _controller.NormalizedPrimarySubtypeCombo.SelectedItem = null;
        _controller.NormalizedSecondarySubtypeCombo.SelectedItem = null;
    }

    private void PopulateNormalizedSubtypeCombos(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        ChartSubtypeComboHelper.PopulateCombo(_controller.NormalizedPrimarySubtypeCombo, selectedSeries);
        ChartSubtypeComboHelper.PopulateCombo(_controller.NormalizedSecondarySubtypeCombo, selectedSeries);
    }

    private void UpdatePrimaryNormalizedSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        var primaryCurrent = _viewModel.ChartState.SelectedNormalizedPrimarySeries;
        var primarySelection = ChartSubtypeComboHelper.ResolveSelection(selectedSeries, primaryCurrent) ?? selectedSeries[0];
        ChartSubtypeComboHelper.SelectComboItem(_controller.NormalizedPrimarySubtypeCombo, primarySelection);
        _viewModel.ChartState.SelectedNormalizedPrimarySeries = primarySelection;
    }

    private void UpdateSecondaryNormalizedSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        if (selectedSeries.Count > 1)
        {
            _controller.NormalizedSecondarySubtypePanel.Visibility = Visibility.Visible;
            _controller.NormalizedSecondarySubtypeCombo.IsEnabled = true;

            var secondaryCurrent = _viewModel.ChartState.SelectedNormalizedSecondarySeries;
            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? secondaryCurrent : selectedSeries[1];

            ChartSubtypeComboHelper.SelectComboItem(_controller.NormalizedSecondarySubtypeCombo, secondarySelection);
            _viewModel.ChartState.SelectedNormalizedSecondarySeries = secondarySelection;
        }
        else
        {
            _controller.NormalizedSecondarySubtypePanel.Visibility = Visibility.Collapsed;
            _controller.NormalizedSecondarySubtypeCombo.IsEnabled = false;
            _controller.NormalizedSecondarySubtypeCombo.SelectedItem = null;
            _viewModel.ChartState.SelectedNormalizedSecondarySeries = null;
        }
    }

    private CartesianMetricChartRenderHost CreateRenderHost()
    {
        return new CartesianMetricChartRenderHost(_controller.Chart, _viewModel.ChartState);
    }
}
