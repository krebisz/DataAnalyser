using DataVisualiser.UI.Charts.Interfaces;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class WeekdayTrendChartControllerAdapter : CartesianChartControllerAdapterBase<IWeekdayTrendChartController>, IWeekdayTrendChartControllerExtras
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly IWeekdayTrendChartController _controller;
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly VNextSeriesLoadCoordinator _vnextCoordinator;
    private readonly IWeekdayTrendRenderingContract _weekdayTrendRenderingContract;
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombo;

    public WeekdayTrendChartControllerAdapter(IWeekdayTrendChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, Func<IStrategyCutOverService?> getStrategyCutOverService, IWeekdayTrendRenderingContract weekdayTrendRenderingContract, VNextSeriesLoadCoordinator? vnextCoordinator = null)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
        _weekdayTrendRenderingContract = weekdayTrendRenderingContract ?? throw new ArgumentNullException(nameof(weekdayTrendRenderingContract));
        _vnextCoordinator = vnextCoordinator ?? new VNextSeriesLoadCoordinator(metricSelectionService);
    }

    public CartesianChart PolarChart => _controller.PolarChart;

    public override void ClearCache()
    {
        _selectionCache.Clear();
    }

    public override string Key => ChartControllerKeys.WeeklyTrend;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderWeekdayTrendAsync(context);
    }

    public override void Clear(ChartState state)
    {
        RenderingHostLifecycleAdapterHelper.Clear(CreateRenderHost, _weekdayTrendRenderingContract.Clear);
    }

    public override void ResetZoom()
    {
        RenderingHostLifecycleAdapterHelper.ResetView(ResolveRenderingRoute, CreateRenderHost, _weekdayTrendRenderingContract.ResetView);
    }

    public override bool HasSeries(ChartState state)
    {
        return RenderingHostLifecycleAdapterHelper.HasRenderableContent(ResolveRenderingRoute, CreateRenderHost, _weekdayTrendRenderingContract.HasRenderableContent);
    }

    public override void UpdateSubtypeOptions()
    {
        var combo = _controller.SubtypeCombo;
        if (combo == null)
            return;

        _isUpdatingSubtypeCombo = true;
        try
        {
            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            var seriesSelection = MetricSeriesSelectionAdapterHelper.PopulateSubtypeCombo(combo, selectedSeries, _viewModel.ChartState.SelectedWeekdayTrendSeries);

            if (seriesSelection == null)
            {
                _viewModel.ChartState.SelectedWeekdayTrendSeries = null;
            }
            else if (_isInitializing())
                _viewModel.ChartState.SelectedWeekdayTrendSeries = seriesSelection;
            else
                _viewModel.SetWeekdayTrendSeries(seriesSelection);
        }
        finally
        {
            _isUpdatingSubtypeCombo = false;
        }
    }

    public void InitializeControls()
    {
        ChartComboItemHelper.Populate(_controller.AverageWindowCombo, new[]
        {
                ("Running Mean", (object)WeekdayTrendAverageWindow.RunningMean),
                ("Weekly", (object)WeekdayTrendAverageWindow.Weekly),
                ("Monthly", (object)WeekdayTrendAverageWindow.Monthly)
        });

        SelectWeekdayTrendAverageWindow(_viewModel.ChartState.WeekdayTrendAverageWindow);
    }

    public void UpdateChartTypeVisibility()
    {
        if (!_viewModel.ChartState.IsWeeklyTrendVisible)
        {
            _controller.Chart.Visibility = Visibility.Collapsed;
            _controller.PolarChart.Visibility = Visibility.Collapsed;
            return;
        }

        var mode = _viewModel.ChartState.WeekdayTrendChartMode;
        if (mode == WeekdayTrendChartMode.Polar)
        {
            _controller.Chart.Visibility = Visibility.Collapsed;
            _controller.PolarChart.Visibility = Visibility.Visible;
            _controller.ChartTypeToggleButton.Content = "Scatter";
        }
        else
        {
            _controller.Chart.Visibility = Visibility.Visible;
            _controller.PolarChart.Visibility = Visibility.Collapsed;
            _controller.ChartTypeToggleButton.Content = mode == WeekdayTrendChartMode.Scatter ? "Cartesian" : "Polar";
        }
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleWeeklyTrend();
    }

    public void OnDayToggled(object? sender, WeekdayTrendDayToggleEventArgs e)
    {
        switch (e.Day)
        {
            case DayOfWeek.Monday:
                _viewModel.SetWeeklyTrendMondayVisible(e.IsChecked);
                break;
            case DayOfWeek.Tuesday:
                _viewModel.SetWeeklyTrendTuesdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Wednesday:
                _viewModel.SetWeeklyTrendWednesdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Thursday:
                _viewModel.SetWeeklyTrendThursdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Friday:
                _viewModel.SetWeeklyTrendFridayVisible(e.IsChecked);
                break;
            case DayOfWeek.Saturday:
                _viewModel.SetWeeklyTrendSaturdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Sunday:
                _viewModel.SetWeeklyTrendSundayVisible(e.IsChecked);
                break;
        }
    }

    public void OnAverageToggled(object? sender, WeekdayTrendAverageToggleEventArgs e)
    {
        _viewModel.SetWeeklyTrendAverageVisible(e.IsChecked);
    }

    public void OnAverageWindowChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        if (_controller.AverageWindowCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is WeekdayTrendAverageWindow window)
            _viewModel.SetWeeklyTrendAverageWindow(window);
    }

    public void OnSubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombo)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.SubtypeCombo);
        _viewModel.SetWeekdayTrendSeries(selection);
    }

    public async void OnChartTypeToggleRequested(object? sender, EventArgs e)
    {
        using var _ = _beginUiBusyScope();
        _viewModel.ToggleWeekdayTrendChartType();
        UpdateChartTypeVisibility();

        if (_viewModel.ChartState.IsWeeklyTrendVisible && _viewModel.ChartState.LastContext != null)
        {
            var result = ComputeWeekdayTrend(_viewModel.ChartState.LastContext);
            if (result != null)
                RenderWeekdayTrendChart(result);
        }
    }

    private void SelectWeekdayTrendAverageWindow(WeekdayTrendAverageWindow window)
    {
        _ = ChartComboItemHelper.TrySelectByTag(_controller.AverageWindowCombo, tag => tag is WeekdayTrendAverageWindow option && option == window);
    }

    private async Task RenderWeekdayTrendAsync(ChartDataContext ctx)
    {
        if (!_viewModel.ChartState.IsWeeklyTrendVisible)
            return;

        var selectedSeries = ResolveSelectedWeekdayTrendSeries(ctx);
        var data = await ResolveWeekdayTrendDataAsync(ctx, selectedSeries);
        if (data == null || data.Count == 0)
            return;

        var displayName = ResolveWeekdayTrendDisplayName(ctx, selectedSeries);
        var trendContext = new ChartDataContext
        {
                Data1 = data,
                DisplayName1 = displayName,
                MetricType = selectedSeries?.MetricType ?? ctx.MetricType,
                PrimaryMetricType = selectedSeries?.MetricType ?? ctx.PrimaryMetricType,
                PrimarySubtype = selectedSeries?.Subtype,
                DisplayPrimaryMetricType = selectedSeries?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
                DisplayPrimarySubtype = selectedSeries?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
                From = ctx.From,
                To = ctx.To
        };

        var result = ComputeWeekdayTrend(trendContext);
        if (result != null)
            RenderWeekdayTrendChart(result);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveWeekdayTrendDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return null;

        if (selectedSeries == null)
            return ctx.Data1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_selectionCache.TryGetData(cacheKey, out var cached))
            return cached;

        return await LoadFreshWeekdayTrendDataAsync(selectedSeries, ctx.From, ctx.To, tableName, cacheKey);
    }

    private async Task<IReadOnlyList<MetricData>?> LoadFreshWeekdayTrendDataAsync(
        MetricSeriesSelection selectedSeries, DateTime from, DateTime to, string tableName, string cacheKey)
    {
        var vnextResult = await _vnextCoordinator.LoadAsync(selectedSeries, from, to, tableName, ChartProgramKind.WeekdayTrend);
        if (vnextResult.Success && vnextResult.Data != null)
        {
            _viewModel.ChartState.LastWeekdayTrendLoadRuntime = new LoadRuntimeState(
                EvidenceRuntimePath.VNextWeekdayTrend,
                vnextResult.RequestSignature ?? string.Empty,
                vnextResult.SnapshotSignature,
                vnextResult.ProgramKind,
                vnextResult.ProgramSourceSignature,
                null, null, false);

            var data = vnextResult.Data is List<MetricData> list ? list : vnextResult.Data.ToList();
            _selectionCache.SetData(cacheKey, data);
            return data;
        }

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, from, to, tableName);
        var legacyData = primaryData.ToList();
        _selectionCache.SetData(cacheKey, legacyData);

        _viewModel.ChartState.LastWeekdayTrendLoadRuntime = new LoadRuntimeState(
            EvidenceRuntimePath.Legacy,
            vnextResult.RequestSignature ?? string.Empty,
            null, null, null, null,
            vnextResult.FailureReason, false);

        return legacyData;
    }

    private MetricSeriesSelection? ResolveSelectedWeekdayTrendSeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(!_isUpdatingSubtypeCombo, _controller.SubtypeCombo, _viewModel.ChartState.SelectedWeekdayTrendSeries, ctx);
    }

    private static string ResolveWeekdayTrendDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveDisplayName(ctx, selectedSeries);
    }

    private WeekdayTrendResult? ComputeWeekdayTrend(ChartDataContext ctx)
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

    private void RenderWeekdayTrendChart(WeekdayTrendResult result)
    {
        var renderTarget = RenderingHostLifecycleAdapterHelper.CreateTarget(ResolveRenderingRoute, CreateRenderHost);
        _weekdayTrendRenderingContract.Render(
            new WeekdayTrendChartRenderRequest(renderTarget.Route, result, _viewModel.ChartState),
            renderTarget.Host);
    }

    private WeekdayTrendRenderingRoute ResolveRenderingRoute()
    {
        return WeekdayTrendRenderingRouteResolver.Resolve(_viewModel.ChartState.WeekdayTrendChartMode);
    }

    private WeekdayTrendChartRenderHost CreateRenderHost()
    {
        return new WeekdayTrendChartRenderHost(_controller.Chart, _controller.PolarChart, _viewModel.ChartState);
    }
}
