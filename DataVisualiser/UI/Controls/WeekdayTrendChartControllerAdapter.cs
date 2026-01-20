using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public sealed class WeekdayTrendChartControllerAdapter : IChartController, IChartSubtypeOptionsController, IChartCacheController, IWeekdayTrendChartControllerExtras, IChartSeriesAvailability, ICartesianChartSurface
{
    private readonly WeekdayTrendChartController _controller;
    private readonly MainWindowViewModel _viewModel;
    private readonly Func<bool> _isInitializing;
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly WeekdayTrendChartUpdateCoordinator _updateCoordinator;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _subtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isUpdatingSubtypeCombo;

    public WeekdayTrendChartControllerAdapter(
        WeekdayTrendChartController controller,
        MainWindowViewModel viewModel,
        Func<bool> isInitializing,
        Func<IDisposable> beginUiBusyScope,
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService,
        WeekdayTrendChartUpdateCoordinator updateCoordinator)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
        _updateCoordinator = updateCoordinator ?? throw new ArgumentNullException(nameof(updateCoordinator));
    }

    public string Key => "WeeklyTrend";
    public bool RequiresPrimaryData => true;
    public bool RequiresSecondaryData => false;
    public ChartPanelController Panel => _controller.Panel;
    public ButtonBase ToggleButton => _controller.ToggleButton;
    public LiveCharts.Wpf.CartesianChart Chart => _controller.Chart;
    public LiveCharts.Wpf.CartesianChart PolarChart => _controller.PolarChart;

    public void Initialize()
    {
    }

    public void InitializeControls()
    {
        _controller.AverageWindowCombo.Items.Clear();
        AddWeekdayTrendAverageOption("Running Mean", WeekdayTrendAverageWindow.RunningMean);
        AddWeekdayTrendAverageOption("Weekly", WeekdayTrendAverageWindow.Weekly);
        AddWeekdayTrendAverageOption("Monthly", WeekdayTrendAverageWindow.Monthly);

        SelectWeekdayTrendAverageWindow(_viewModel.ChartState.WeekdayTrendAverageWindow);
    }

    public Task RenderAsync(ChartDataContext context)
    {
        return RenderWeekdayTrendAsync(context);
    }

    public void Clear(ChartState state)
    {
        ChartHelper.ClearChart(_controller.Chart, state.ChartTimestamps);
        ChartHelper.ClearChart(_controller.PolarChart, state.ChartTimestamps);
    }

    public void ResetZoom()
    {
        ChartHelper.ResetZoom(_controller.Chart);
        ChartHelper.ResetZoom(_controller.PolarChart);
    }

    public bool HasSeries(ChartState state)
    {
        return state.WeekdayTrendChartMode == WeekdayTrendChartMode.Polar
            ? HasSeriesInternal(_controller.PolarChart.Series)
            : HasSeriesInternal(_controller.Chart.Series);
    }

    private static bool HasSeriesInternal(System.Collections.IEnumerable? series)
    {
        if (series == null)
            return false;

        return series.Cast<object>().Any();
    }

    public void ClearCache()
    {
        _subtypeCache.Clear();
    }

    public void UpdateSubtypeOptions()
    {
        var combo = _controller.SubtypeCombo;
        if (combo == null)
            return;

        _isUpdatingSubtypeCombo = true;
        try
        {
            combo.Items.Clear();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                combo.IsEnabled = false;
                _viewModel.ChartState.SelectedWeekdayTrendSeries = null;
                combo.SelectedItem = null;
                return;
            }

            foreach (var selection in selectedSeries)
                combo.Items.Add(BuildSeriesComboItem(selection));

            combo.IsEnabled = true;

            var current = _viewModel.ChartState.SelectedWeekdayTrendSeries;
            var seriesSelection = current != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, current.DisplayKey, StringComparison.OrdinalIgnoreCase))
                ? current
                : selectedSeries[0];
            var weekdayItem = FindSeriesComboItem(combo, seriesSelection) ?? combo.Items.OfType<ComboBoxItem>().FirstOrDefault();
            combo.SelectedItem = weekdayItem;

            if (_isInitializing())
                _viewModel.ChartState.SelectedWeekdayTrendSeries = seriesSelection;
            else
                _viewModel.SetWeekdayTrendSeries(seriesSelection);
        }
        finally
        {
            _isUpdatingSubtypeCombo = false;
        }
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

        var selection = GetSeriesSelectionFromCombo(_controller.SubtypeCombo);
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

    private void AddWeekdayTrendAverageOption(string label, WeekdayTrendAverageWindow window)
    {
        _controller.AverageWindowCombo.Items.Add(new ComboBoxItem
        {
            Content = label,
            Tag = window
        });
    }

    private void SelectWeekdayTrendAverageWindow(WeekdayTrendAverageWindow window)
    {
        foreach (var item in _controller.AverageWindowCombo.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is WeekdayTrendAverageWindow option && option == window)
            {
                _controller.AverageWindowCombo.SelectedItem = item;
                break;
            }
        }
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

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildWeekdayTrendCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_subtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _subtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedWeekdayTrendSeries(ChartDataContext ctx)
    {
        if (_viewModel.ChartState.SelectedWeekdayTrendSeries != null)
            return _viewModel.ChartState.SelectedWeekdayTrendSeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private static string ResolveWeekdayTrendDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildWeekdayTrendCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
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
        _updateCoordinator.UpdateChart(result, _viewModel.ChartState, _controller.Chart, _controller.PolarChart);
    }

    private static ComboBoxItem BuildSeriesComboItem(MetricSeriesSelection selection)
    {
        return new ComboBoxItem
        {
            Content = selection.DisplayName,
            Tag = selection
        };
    }

    private static ComboBoxItem? FindSeriesComboItem(ComboBox combo, MetricSeriesSelection selection)
    {
        return combo.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(item => item.Tag is MetricSeriesSelection candidate && string.Equals(candidate.DisplayKey, selection.DisplayKey, StringComparison.OrdinalIgnoreCase));
    }

    private static MetricSeriesSelection? GetSeriesSelectionFromCombo(ComboBox combo)
    {
        if (combo.SelectedItem is ComboBoxItem item && item.Tag is MetricSeriesSelection selection)
            return selection;

        return combo.SelectedItem as MetricSeriesSelection;
    }

    private static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (!string.Equals(selection.MetricType, metricType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedSubtype = string.IsNullOrWhiteSpace(subtype) || subtype == "(All)" ? null : subtype;
        var selectionSubtype = selection.QuerySubtype;

        return string.Equals(selectionSubtype ?? string.Empty, normalizedSubtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
