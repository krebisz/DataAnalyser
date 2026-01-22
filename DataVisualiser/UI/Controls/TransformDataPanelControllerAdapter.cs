using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public sealed class TransformDataPanelControllerAdapter : IChartController, IChartCacheController, ITransformPanelControllerExtras, ICartesianChartSurface
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;
    private readonly TransformDataPanelController _controller;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _subtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly MainWindowViewModel _viewModel;
    private bool _isTransformSelectionPendingLoad;
    private bool _isUpdatingTransformSubtypeCombos;

    public TransformDataPanelControllerAdapter(TransformDataPanelController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, ChartUpdateCoordinator chartUpdateCoordinator)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
    }

    public CartesianChart Chart => _controller.ChartTransformResult;

    public void ClearCache()
    {
        _subtypeCache.Clear();
    }

    public string Key => "Transform";
    public bool RequiresPrimaryData => true;
    public bool RequiresSecondaryData => false;
    public ChartPanelController Panel => _controller.Panel;
    public ButtonBase ToggleButton => _controller.ToggleButton;

    public void Initialize()
    {
    }

    public Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsTransformPanelVisible)
            return Task.CompletedTask;

        PopulateTransformGrids(context);
        return Task.CompletedTask;
    }

    public void Clear(ChartState state)
    {
        ClearTransformGrids(state);
    }

    public void ResetZoom()
    {
        ChartHelper.ResetZoom(_controller.ChartTransformResult);
    }

    public void CompleteSelectionsPendingLoad()
    {
        _isTransformSelectionPendingLoad = false;
    }

    public void ResetSelectionsPendingLoad()
    {
        _isTransformSelectionPendingLoad = true;
        _viewModel.ChartState.SelectedTransformPrimarySeries = null;
        _viewModel.ChartState.SelectedTransformSecondarySeries = null;

        if (_controller.TransformPrimarySubtypeCombo == null || _controller.TransformSecondarySubtypeCombo == null)
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            _controller.TransformPrimarySubtypeCombo.Items.Clear();
            _controller.TransformSecondarySubtypeCombo.Items.Clear();
            _controller.TransformPrimarySubtypeCombo.IsEnabled = false;
            _controller.TransformSecondarySubtypeCombo.IsEnabled = false;
            _controller.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
            _controller.TransformPrimarySubtypeCombo.SelectedItem = null;
            _controller.TransformSecondarySubtypeCombo.SelectedItem = null;
            _controller.TransformOperationCombo.SelectedItem = null;
            _controller.TransformComputeButton.IsEnabled = false;
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    public void HandleVisibilityOnlyToggle(ChartDataContext? context)
    {
        if (!_viewModel.ChartState.IsTransformPanelVisible)
            return;

        if (context != null && ShouldRenderCharts(context))
            PopulateTransformGrids(context, false);

        UpdateTransformSubtypeOptions();
    }

    public void UpdateTransformSubtypeOptions()
    {
        if (!CanUpdateTransformSubtypeOptions())
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            ClearTransformSubtypeCombos();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                HandleNoSelectedSeries();
                return;
            }

            PopulateTransformSubtypeCombos(selectedSeries);
            UpdatePrimaryTransformSubtype(selectedSeries);
            UpdateSecondaryTransformSubtype(selectedSeries);

            UpdateTransformComputeButtonState();
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    public void UpdateTransformComputeButtonState()
    {
        if (_isTransformSelectionPendingLoad)
        {
            _controller.TransformComputeButton.IsEnabled = false;
            return;
        }

        if (_controller.TransformOperationCombo.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not string operationTag)
        {
            _controller.TransformComputeButton.IsEnabled = false;
            return;
        }

        var ctx = _viewModel.ChartState.LastContext;
        if (ctx == null)
        {
            _controller.TransformComputeButton.IsEnabled = false;
            return;
        }

        var hasSecondary = HasSecondaryData(ctx);
        var hasSecondSubtype = ResolveSelectedTransformSecondarySeries(ctx) != null;
        var isUnary = operationTag == "Log" || operationTag == "Sqrt";
        var isBinary = operationTag == "Add" || operationTag == "Subtract" || operationTag == "Divide";

        _controller.TransformComputeButton.IsEnabled = (isUnary && ctx.Data1 != null) || (isBinary && hasSecondary && hasSecondSubtype);
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleTransformPanel();
    }

    public void OnOperationChanged(object? sender, EventArgs e)
    {
        UpdateTransformComputeButtonState();
    }

    public void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(_controller.TransformPrimarySubtypeCombo);
        _viewModel.SetTransformPrimarySeries(selection);

        UpdateTransformComputeButtonState();
    }

    public void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(_controller.TransformSecondarySubtypeCombo);
        _viewModel.SetTransformSecondarySeries(selection);

        UpdateTransformComputeButtonState();
    }

    public async void OnComputeRequested(object? sender, EventArgs e)
    {
        if (_isTransformSelectionPendingLoad)
            return;

        if (_viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        if (!TryGetSelectedOperation(out var operationTag))
            return;

        using var _ = _beginUiBusyScope();
        await ExecuteTransformOperation(ctx, operationTag);
    }

    private void PopulateTransformGrids(ChartDataContext ctx, bool resetResults = true)
    {
        PopulateTransformGrid(ctx.Data1, _controller.TransformGrid1, _controller.TransformGrid1Title, ctx.DisplayName1 ?? "Primary Data", true);

        var hasSecondary = HasSecondaryData(ctx) && !string.IsNullOrEmpty(ctx.SecondarySubtype) && ctx.Data2 != null;

        if (hasSecondary)
        {
            _controller.TransformGrid2Panel.Visibility = Visibility.Visible;

            PopulateTransformGrid(ctx.Data2, _controller.TransformGrid2, _controller.TransformGrid2Title, ctx.DisplayName2 ?? "Secondary Data", false);

            SetBinaryTransformOperationsEnabled(true);
        }
        else
        {
            _controller.TransformGrid2Panel.Visibility = Visibility.Collapsed;
            _controller.TransformGrid2.ItemsSource = null;
            SetBinaryTransformOperationsEnabled(false);
        }

        if (resetResults)
            ResetTransformResultState();
    }

    private void PopulateTransformGrid(IEnumerable<MetricData>? data, DataGrid grid, TextBlock title, string titleText, bool alwaysVisible)
    {
        if (data == null && !alwaysVisible)
            return;

        var rows = data?.Where(d => d.Value.HasValue)
                       .OrderBy(d => d.NormalizedTimestamp)
                       .Select(d => new
                       {
                               Timestamp = d.NormalizedTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                               Value = d.Value!.Value.ToString("F4")
                       })
                       .ToList();

        grid.ItemsSource = rows;
        title.Text = titleText;

        if (grid.Columns.Count >= 2)
        {
            grid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            grid.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        }
    }

    private void SetBinaryTransformOperationsEnabled(bool enabled)
    {
        var binaryItems = _controller.TransformOperationCombo.Items.Cast<ComboBoxItem>().Where(i => i.Tag?.ToString() == "Add" || i.Tag?.ToString() == "Subtract");

        foreach (var item in binaryItems)
            item.IsEnabled = enabled;
    }

    private void ResetTransformResultState()
    {
        _controller.TransformGrid3Panel.Visibility = Visibility.Collapsed;
        _controller.TransformChartContentPanel.Visibility = Visibility.Collapsed;
        _controller.TransformGrid3.ItemsSource = null;
        _controller.TransformComputeButton.IsEnabled = false;
    }

    private bool CanUpdateTransformSubtypeOptions()
    {
        if (_controller.TransformPrimarySubtypeCombo == null || _controller.TransformSecondarySubtypeCombo == null)
            return false;

        return !_isTransformSelectionPendingLoad;
    }

    private void ClearTransformSubtypeCombos()
    {
        _controller.TransformPrimarySubtypeCombo.Items.Clear();
        _controller.TransformSecondarySubtypeCombo.Items.Clear();
    }

    private void HandleNoSelectedSeries()
    {
        _controller.TransformPrimarySubtypeCombo.IsEnabled = false;
        _controller.TransformSecondarySubtypeCombo.IsEnabled = false;
        _controller.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;

        _viewModel.ChartState.SelectedTransformPrimarySeries = null;
        _viewModel.ChartState.SelectedTransformSecondarySeries = null;

        _controller.TransformPrimarySubtypeCombo.SelectedItem = null;
        _controller.TransformSecondarySubtypeCombo.SelectedItem = null;
    }

    private void PopulateTransformSubtypeCombos(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        foreach (var selection in selectedSeries)
        {
            _controller.TransformPrimarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));

            _controller.TransformSecondarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
        }

        _controller.TransformPrimarySubtypeCombo.IsEnabled = true;
    }

    private void UpdatePrimaryTransformSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        var primaryCurrent = _viewModel.ChartState.SelectedTransformPrimarySeries;

        var primarySelection = primaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, primaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? primaryCurrent : selectedSeries[0];

        var primaryItem = FindSeriesComboItem(_controller.TransformPrimarySubtypeCombo, primarySelection) ?? _controller.TransformPrimarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

        _controller.TransformPrimarySubtypeCombo.SelectedItem = primaryItem;
        _viewModel.ChartState.SelectedTransformPrimarySeries = primarySelection;
    }

    private void UpdateSecondaryTransformSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        if (selectedSeries.Count > 1)
        {
            _controller.TransformSecondarySubtypePanel.Visibility = Visibility.Visible;
            _controller.TransformSecondarySubtypeCombo.IsEnabled = true;

            var secondaryCurrent = _viewModel.ChartState.SelectedTransformSecondarySeries;

            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? secondaryCurrent : selectedSeries[1];

            var secondaryItem = FindSeriesComboItem(_controller.TransformSecondarySubtypeCombo, secondarySelection) ?? _controller.TransformSecondarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

            _controller.TransformSecondarySubtypeCombo.SelectedItem = secondaryItem;
            _viewModel.ChartState.SelectedTransformSecondarySeries = secondarySelection;
        }
        else
        {
            _controller.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
            _controller.TransformSecondarySubtypeCombo.IsEnabled = false;
            _controller.TransformSecondarySubtypeCombo.SelectedItem = null;
            _viewModel.ChartState.SelectedTransformSecondarySeries = null;
        }
    }

    private bool TryGetSelectedOperation(out string operationTag)
    {
        operationTag = string.Empty;
        if (_controller.TransformOperationCombo.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not string tag)
            return false;

        operationTag = tag;
        return true;
    }

    private async Task ExecuteTransformOperation(ChartDataContext ctx, string operationTag)
    {
        var isUnary = operationTag == "Log" || operationTag == "Sqrt";
        var isBinary = operationTag == "Add" || operationTag == "Subtract" || operationTag == "Divide";
        var hasSecondary = HasSecondaryData(ctx) && ResolveSelectedTransformSecondarySeries(ctx) != null;

        var (primaryData, secondaryData, transformContext) = await ResolveTransformDataAsync(ctx);
        if (primaryData == null)
            return;

        if (isUnary)
            await ComputeUnaryTransform(primaryData, operationTag, transformContext);
        else if (isBinary && hasSecondary && secondaryData != null)
            await ComputeBinaryTransform(primaryData, secondaryData, operationTag, transformContext);
    }

    private async Task ComputeUnaryTransform(IEnumerable<MetricData> data, string operation, ChartDataContext transformContext)
    {
        var allDataList = data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

        if (allDataList.Count == 0)
            return;

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);
        List<double> computedResults;
        List<IReadOnlyList<MetricData>> metricsList;

        if (expression == null)
        {
            Debug.WriteLine($"[Transform] UNARY - Using LEGACY approach for operation: {operation}");
            var op = operation switch
            {
                    "Log" => UnaryOperators.Logarithm,
                    "Sqrt" => UnaryOperators.SquareRoot,
                    _ => x => x
            };
            var allValues = allDataList.Select(d => (double)d.Value!.Value).ToList();
            computedResults = MathHelper.ApplyUnaryOperation(allValues, op);
            metricsList = new List<IReadOnlyList<MetricData>>
            {
                    allDataList
            };
        }
        else
        {
            Debug.WriteLine($"[Transform] UNARY - Using NEW infrastructure for operation: {operation}, expression built successfully");
            metricsList = new List<IReadOnlyList<MetricData>>
            {
                    allDataList
            };
            computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);
            Debug.WriteLine($"[Transform] UNARY - Evaluated {computedResults.Count} results using TransformExpressionEvaluator");
        }

        await RenderTransformResults(allDataList, computedResults, operation, metricsList, transformContext);
    }

    private async Task RenderTransformResults(List<MetricData> dataList, List<double> results, string operation, List<IReadOnlyList<MetricData>> metrics, ChartDataContext transformContext)
    {
        var resultData = TransformExpressionEvaluator.CreateTransformResultData(dataList, results);
        PopulateTransformResultGrid(resultData);

        if (resultData.Count == 0)
            return;

        ShowTransformResultPanels();
        await PrepareTransformChartLayout();
        await RenderTransformChart(dataList, results, operation, metrics, transformContext);
        await FinalizeTransformChartRendering();
    }

    private void PopulateTransformResultGrid(List<object> resultData)
    {
        _controller.TransformGrid3.ItemsSource = resultData;
        if (_controller.TransformGrid3.Columns.Count >= 2)
        {
            _controller.TransformGrid3.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            _controller.TransformGrid3.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        }
    }

    private void ShowTransformResultPanels()
    {
        _controller.TransformGrid3Panel.Visibility = Visibility.Visible;
        _controller.TransformChartContentPanel.Visibility = Visibility.Visible;
    }

    private async Task PrepareTransformChartLayout()
    {
        _controller.TransformChartContentPanel.UpdateLayout();
        await _controller.Dispatcher.InvokeAsync(() =>
                {
                },
                DispatcherPriority.Render);
        await CalculateAndSetTransformChartWidth();
        Debug.WriteLine($"[TransformChart] Before render - ActualWidth={_controller.ChartTransformResult.ActualWidth}, ActualHeight={_controller.ChartTransformResult.ActualHeight}, IsVisible={_controller.ChartTransformResult.IsVisible}, PanelVisible={_controller.TransformChartContentPanel.Visibility}");
    }

    private async Task FinalizeTransformChartRendering()
    {
        _controller.ChartTransformResult.Update(true, true);
        _controller.TransformChartContentPanel.UpdateLayout();
        await _controller.Dispatcher.InvokeAsync(() =>
                {
                    _controller.ChartTransformResult.InvalidateVisual();
                    _controller.ChartTransformResult.Update(true, true);
                },
                DispatcherPriority.Render);
        Debug.WriteLine($"[TransformChart] After render - ActualWidth={_controller.ChartTransformResult.ActualWidth}, ActualHeight={_controller.ChartTransformResult.ActualHeight}, SeriesCount={_controller.ChartTransformResult.Series?.Count ?? 0}");
    }

    private async Task CalculateAndSetTransformChartWidth()
    {
        await _controller.Dispatcher.InvokeAsync(() =>
                {
                    if (_controller.TransformChartContainer == null)
                        return;

                    var parentStackPanel = _controller.TransformChartContainer.Parent as FrameworkElement;
                    if (parentStackPanel?.Parent is not FrameworkElement parentContainer)
                        return;

                    var usedWidth = CalculateUsedWidthForTransformGrids();
                    usedWidth += 40;

                    var availableWidth = parentContainer.ActualWidth > 0 ? parentContainer.ActualWidth : 1800;
                    var chartWidth = Math.Max(400, availableWidth - usedWidth - 40);
                    _controller.TransformChartContainer.Width = chartWidth;

                    Debug.WriteLine($"[TransformChart] Calculated width - parentWidth={parentContainer.ActualWidth}, usedWidth={usedWidth}, chartWidth={chartWidth}");
                },
                DispatcherPriority.Render);
    }

    private double CalculateUsedWidthForTransformGrids()
    {
        double usedWidth = 0;

        var grid1StackPanel = _controller.TransformGrid1.Parent as FrameworkElement;
        usedWidth += grid1StackPanel?.ActualWidth > 0 ? grid1StackPanel.ActualWidth : 250;

        if (_controller.TransformGrid2Panel.IsVisible)
            usedWidth += _controller.TransformGrid2Panel.ActualWidth > 0 ? _controller.TransformGrid2Panel.ActualWidth : 250;

        if (_controller.TransformGrid3Panel.IsVisible)
            usedWidth += _controller.TransformGrid3Panel.ActualWidth > 0 ? _controller.TransformGrid3Panel.ActualWidth : 250;

        return usedWidth;
    }

    private async Task RenderTransformChart(List<MetricData> dataList, List<double> results, string operation, List<IReadOnlyList<MetricData>> metrics, ChartDataContext transformContext)
    {
        if (dataList.Count == 0 || results.Count == 0)
            return;

        var from = transformContext.From != default ? transformContext.From : dataList.Min(d => d.NormalizedTimestamp);
        var to = transformContext.To != default ? transformContext.To : dataList.Max(d => d.NormalizedTimestamp);

        var label = TransformExpressionEvaluator.GenerateTransformLabel(operation, metrics, transformContext);

        var strategy = new TransformResultStrategy(dataList, results, label, from, to);

        var operationTag = _controller.TransformOperationCombo.SelectedItem is ComboBoxItem item ? item.Tag?.ToString() ?? "Transform" : "Transform";
        var operationType = operationTag == "Subtract" ? "-" : operationTag == "Add" ? "+" : operationTag == "Divide" ? "/" : null;
        var isOperationChart = operationTag == "Subtract" || operationTag == "Add" || operationTag == "Divide";

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(_controller.ChartTransformResult, strategy, label, null, 400, transformContext.PrimaryMetricType ?? transformContext.MetricType, transformContext.PrimarySubtype, transformContext.SecondarySubtype, operationType, isOperationChart, transformContext.SecondaryMetricType, transformContext.DisplayPrimaryMetricType, transformContext.DisplaySecondaryMetricType, transformContext.DisplayPrimarySubtype, transformContext.DisplaySecondarySubtype);
    }

    private async Task ComputeBinaryTransform(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string operation, ChartDataContext transformContext)
    {
        var allData1List = PrepareMetricData(data1);
        var allData2List = PrepareMetricData(data2);

        if (allData1List.Count == 0 || allData2List.Count == 0)
            return;

        var alignedData = TransformExpressionEvaluator.AlignMetricsByTimestamp(allData1List, allData2List);

        if (alignedData.Item1.Count == 0 || alignedData.Item2.Count == 0)
            return;

        var computation = ComputeBinaryResults(alignedData, operation);

        await RenderTransformResults(alignedData.Item1, computation.Results, operation, computation.MetricsList, transformContext);
    }

    private static List<MetricData> PrepareMetricData(IEnumerable<MetricData> data)
    {
        return data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
    }

    private static(List<double> Results, List<IReadOnlyList<MetricData>> MetricsList) ComputeBinaryResults((List<MetricData> Item1, List<MetricData> Item2) alignedData, string operation)
    {
        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                alignedData.Item1,
                alignedData.Item2
        };

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);

        if (expression == null)
        {
            Debug.WriteLine($"[Transform] BINARY - Using LEGACY approach for operation: {operation}");

            var op = operation switch
            {
                    "Add" => BinaryOperators.Sum,
                    "Subtract" => BinaryOperators.Difference,
                    "Divide" => BinaryOperators.Ratio,
                    _ => (a, b) => a
            };

            var values1 = alignedData.Item1.Select(d => (double)d.Value!.Value).ToList();

            var values2 = alignedData.Item2.Select(d => (double)d.Value!.Value).ToList();

            var results = MathHelper.ApplyBinaryOperation(values1, values2, op);

            Debug.WriteLine($"[Transform] BINARY - Legacy computation completed: {results.Count} results");

            return (results, metricsList);
        }

        Debug.WriteLine($"[Transform] BINARY - Using NEW infrastructure for operation: {operation}");

        var computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);

        Debug.WriteLine($"[Transform] BINARY - Evaluated {computedResults.Count} results using TransformExpressionEvaluator");

        return (computedResults, metricsList);
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveTransformDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedTransformPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedTransformSecondarySeries(ctx);

        var primaryData = await ResolveTransformDataAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;

        if (secondarySelection != null)
            secondaryData = await ResolveTransformDataAsync(ctx, secondarySelection);

        var displayName1 = ResolveTransformDisplayName(ctx, primarySelection);
        var displayName2 = ResolveTransformDisplayName(ctx, secondarySelection);

        var transformContext = new ChartDataContext
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

        return (primaryData, secondaryData, transformContext);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveTransformDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
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
        var cacheKey = BuildTransformCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_subtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _subtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedTransformPrimarySeries(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && _controller.TransformPrimarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(_controller.TransformPrimarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformPrimarySeries != null)
            return _viewModel.ChartState.SelectedTransformPrimarySeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedTransformSecondarySeries(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && _controller.TransformSecondarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(_controller.TransformSecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformSecondarySeries != null)
            return _viewModel.ChartState.SelectedTransformSecondarySeries;

        var metricType = ctx.SecondaryMetricType ?? ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.SecondarySubtype);
    }

    private static string ResolveTransformDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildTransformCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
    }

    private void ClearTransformGrids(ChartState state)
    {
        _controller.TransformGrid1.ItemsSource = null;
        _controller.TransformGrid2.ItemsSource = null;
        _controller.TransformGrid3.ItemsSource = null;
        _controller.TransformGrid2Panel.Visibility = Visibility.Collapsed;
        _controller.TransformGrid3Panel.Visibility = Visibility.Collapsed;
        _controller.TransformChartContentPanel.Visibility = Visibility.Collapsed;
        _controller.TransformComputeButton.IsEnabled = false;
        ChartHelper.ClearChart(_controller.ChartTransformResult, state.ChartTimestamps);
    }

    private static bool ShouldRenderCharts(ChartDataContext? ctx)
    {
        return ctx != null && ctx.Data1 != null && ctx.Data1.Any();
    }

    private static bool HasSecondaryData(ChartDataContext ctx)
    {
        return ctx.Data2 != null && ctx.Data2.Any();
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
        return combo.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag is MetricSeriesSelection candidate && string.Equals(candidate.DisplayKey, selection.DisplayKey, StringComparison.OrdinalIgnoreCase));
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