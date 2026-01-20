using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Controls;

public sealed class DiffRatioChartControllerAdapter : IChartController, IChartSubtypeOptionsController, IChartCacheController, IDiffRatioChartControllerExtras, IChartSeriesAvailability
{
    private readonly DiffRatioChartController _controller;
    private readonly MainWindowViewModel _viewModel;
    private readonly Func<bool> _isInitializing;
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;
    private readonly Func<ChartTooltipManager?> _getTooltipManager;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _subtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isUpdatingSubtypeCombos;

    public DiffRatioChartControllerAdapter(
        DiffRatioChartController controller,
        MainWindowViewModel viewModel,
        Func<bool> isInitializing,
        Func<IDisposable> beginUiBusyScope,
        MetricSelectionService metricSelectionService,
        Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator,
        Func<ChartTooltipManager?> getTooltipManager)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
        _getTooltipManager = getTooltipManager ?? throw new ArgumentNullException(nameof(getTooltipManager));
    }

    public string Key => "DiffRatio";
    public bool RequiresPrimaryData => true;
    public bool RequiresSecondaryData => true;
    public ChartPanelController Panel => _controller.Panel;
    public ButtonBase ToggleButton => _controller.ToggleButton;

    public void Initialize()
    {
    }

    public Task RenderAsync(ChartDataContext context)
    {
        return RenderDiffRatioAsync(context);
    }

    public void Clear(ChartState state)
    {
        ChartHelper.ClearChart(_controller.Chart, state.ChartTimestamps);
    }

    public void ResetZoom()
    {
        ChartHelper.ResetZoom(_controller.Chart);
    }

    public bool HasSeries(ChartState state)
    {
        return HasSeriesInternal(_controller.Chart.Series);
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
        if (!CanUpdateDiffRatioSubtypeOptions())
            return;

        _isUpdatingSubtypeCombos = true;
        try
        {
            ClearDiffRatioSubtypeCombos();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                HandleNoSelectedDiffRatioSeries();
                return;
            }

            PopulateDiffRatioSubtypeCombos(selectedSeries);
            UpdatePrimaryDiffRatioSubtype(selectedSeries);
            UpdateSecondaryDiffRatioSubtype(selectedSeries);
        }
        finally
        {
            _isUpdatingSubtypeCombos = false;
        }
    }

    public void UpdateOperationButton()
    {
        var isDifference = _viewModel.ChartState.IsDiffRatioDifferenceMode;

        var operationButton = _controller.OperationToggleButton;
        operationButton.Content = isDifference ? "/" : "-";
        operationButton.ToolTip = isDifference ? "Switch to Ratio (/)" : "Switch to Difference (-)";

        if (_controller.Chart.AxisY.Count > 0)
            _controller.Chart.AxisY[0].Title = isDifference ? "Difference" : "Ratio";
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleDiffRatio();
    }

    public async void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(_controller.PrimarySubtypeCombo);
        _viewModel.SetDiffRatioPrimarySeries(selection);

        await RenderDiffRatioFromSelectionAsync();
    }

    public async void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(_controller.SecondarySubtypeCombo);
        _viewModel.SetDiffRatioSecondarySeries(selection);

        await RenderDiffRatioFromSelectionAsync();
    }

    public async void OnOperationToggleRequested(object? sender, EventArgs e)
    {
        using var _ = _beginUiBusyScope();
        _viewModel.ToggleDiffRatioOperation();
        UpdateOperationButton();

        await RenderDiffRatioFromSelectionAsync();
    }

    private async Task RenderDiffRatioFromSelectionAsync()
    {
        if (!_viewModel.ChartState.IsDiffRatioVisible || _viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        await RenderDiffRatioAsync(ctx);
    }

    private async Task RenderDiffRatioAsync(ChartDataContext ctx)
    {
        var orchestrator = _getChartRenderingOrchestrator();
        if (orchestrator == null)
            return;

        var (primaryData, secondaryData, diffRatioContext) = await ResolveDiffRatioDataAsync(ctx);
        if (primaryData == null || secondaryData == null)
            return;

        UpdateDiffRatioPanelTitle(diffRatioContext);
        await orchestrator.RenderDiffRatioChartAsync(diffRatioContext, _controller.Chart, _viewModel.ChartState);
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveDiffRatioDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedDiffRatioPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedDiffRatioSecondarySeries(ctx);

        var primaryData = await ResolveDiffRatioDataAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;
        if (secondarySelection != null)
            secondaryData = await ResolveDiffRatioDataAsync(ctx, secondarySelection);

        var displayName1 = ResolveDiffRatioDisplayName(ctx, primarySelection);
        var displayName2 = ResolveDiffRatioDisplayName(ctx, secondarySelection);

        var diffRatioContext = new ChartDataContext
        {
            Data1 = primaryData,
            Data2 = secondaryData,
            DisplayName1 = displayName1,
            DisplayName2 = displayName2,
            MetricType = primarySelection?.MetricType ?? ctx.MetricType,
            PrimaryMetricType = primarySelection?.MetricType ?? ctx.PrimaryMetricType,
            SecondaryMetricType = secondarySelection?.MetricType ?? ctx.SecondaryMetricType,
            PrimarySubtype = primarySelection?.Subtype,
            SecondarySubtype = secondarySelection?.Subtype,
            DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? ctx.DisplaySecondaryMetricType,
            DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
            DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? ctx.DisplaySecondarySubtype,
            From = ctx.From,
            To = ctx.To
        };

        return (primaryData, secondaryData, diffRatioContext);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveDiffRatioDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
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
        var cacheKey = BuildDiffRatioCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_subtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _subtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedDiffRatioPrimarySeries(ChartDataContext ctx)
    {
        if (!_isUpdatingSubtypeCombos && _controller.PrimarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(_controller.PrimarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedDiffRatioPrimarySeries != null)
            return _viewModel.ChartState.SelectedDiffRatioPrimarySeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedDiffRatioSecondarySeries(ChartDataContext ctx)
    {
        if (!_isUpdatingSubtypeCombos && _controller.SecondarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(_controller.SecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedDiffRatioSecondarySeries != null)
            return _viewModel.ChartState.SelectedDiffRatioSecondarySeries;

        var metricType = ctx.SecondaryMetricType ?? ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.SecondarySubtype);
    }

    private static string ResolveDiffRatioDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildDiffRatioCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
    }

    private void UpdateDiffRatioPanelTitle(ChartDataContext ctx)
    {
        var leftName = ctx.DisplayName1 ?? string.Empty;
        var rightName = ctx.DisplayName2 ?? string.Empty;
        var operationSymbol = _viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/";
        _controller.Panel.Title = $"{leftName} {operationSymbol} {rightName}";

        var tooltipManager = _getTooltipManager();
        if (tooltipManager != null)
        {
            var label = !string.IsNullOrEmpty(rightName) ? $"{leftName} {operationSymbol} {rightName}" : leftName;
            tooltipManager.UpdateChartLabel(_controller.Chart, label);
        }
    }

    private bool CanUpdateDiffRatioSubtypeOptions()
    {
        return _controller.PrimarySubtypeCombo != null && _controller.SecondarySubtypeCombo != null;
    }

    private void ClearDiffRatioSubtypeCombos()
    {
        _controller.PrimarySubtypeCombo.Items.Clear();
        _controller.SecondarySubtypeCombo.Items.Clear();
    }

    private void HandleNoSelectedDiffRatioSeries()
    {
        _controller.PrimarySubtypeCombo.IsEnabled = false;
        _controller.SecondarySubtypeCombo.IsEnabled = false;
        _controller.SecondarySubtypePanel.Visibility = Visibility.Collapsed;

        _viewModel.ChartState.SelectedDiffRatioPrimarySeries = null;
        _viewModel.ChartState.SelectedDiffRatioSecondarySeries = null;

        _controller.PrimarySubtypeCombo.SelectedItem = null;
        _controller.SecondarySubtypeCombo.SelectedItem = null;
    }

    private void PopulateDiffRatioSubtypeCombos(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        foreach (var selection in selectedSeries)
        {
            _controller.PrimarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
            _controller.SecondarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
        }

        _controller.PrimarySubtypeCombo.IsEnabled = true;
    }

    private void UpdatePrimaryDiffRatioSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        var primaryCurrent = _viewModel.ChartState.SelectedDiffRatioPrimarySeries;
        var primarySelection = primaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, primaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase))
            ? primaryCurrent
            : selectedSeries[0];

        var primaryItem = FindSeriesComboItem(_controller.PrimarySubtypeCombo, primarySelection)
            ?? _controller.PrimarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

        _controller.PrimarySubtypeCombo.SelectedItem = primaryItem;
        _viewModel.ChartState.SelectedDiffRatioPrimarySeries = primarySelection;
    }

    private void UpdateSecondaryDiffRatioSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        if (selectedSeries.Count > 1)
        {
            _controller.SecondarySubtypePanel.Visibility = Visibility.Visible;
            _controller.SecondarySubtypeCombo.IsEnabled = true;

            var secondaryCurrent = _viewModel.ChartState.SelectedDiffRatioSecondarySeries;
            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase))
                ? secondaryCurrent
                : selectedSeries[1];

            var secondaryItem = FindSeriesComboItem(_controller.SecondarySubtypeCombo, secondarySelection)
                ?? _controller.SecondarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

            _controller.SecondarySubtypeCombo.SelectedItem = secondaryItem;
            _viewModel.ChartState.SelectedDiffRatioSecondarySeries = secondarySelection;
        }
        else
        {
            _controller.SecondarySubtypePanel.Visibility = Visibility.Collapsed;
            _controller.SecondarySubtypeCombo.IsEnabled = false;
            _controller.SecondarySubtypeCombo.SelectedItem = null;
            _viewModel.ChartState.SelectedDiffRatioSecondarySeries = null;
        }
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
