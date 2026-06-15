using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Export;
using DataVisualiser.UI.Theming;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.OperationChain;

public partial class OperationChainWorkbenchView : UserControl
{
    private const string SummaryStatusBrushResourceKey = "ThemeSecondaryTextBrush";
    private const string SummaryErrorBrushResourceKey = "ThemeValidationErrorBrush";
    private readonly List<TransformEquationTerm> _equationTerms = [];
    private readonly List<TransformWorkbenchInputSelectionRow> _inputRows = [];
    private readonly TransformComputationOutputRenderer _outputRenderer = new();
    private readonly SelectableGridRowToggleCoordinator _resultGridToggleCoordinator = new();
    private readonly TransformSessionMilestoneRecorder _sessionMilestoneRecorder = new();
    private MetricSelectionService? _metricSelectionService;
    private TransformWorkbenchService? _inputLoader;
    private readonly TransformEvidenceExportService _exportService = new(new EvidenceExportWriter(), new EvidenceExportPathResolver());
    private IReadOnlyList<MetricNameOption> _metricOptions = [];
    private TransformComputationResult? _currentComputationResult;
    private TransformEvidenceExportSnapshot? _lastExportSnapshot;
    private bool _isInitializing;
    private bool _isApplyingDateRange;
    private bool _isThemeAttached;

    public OperationChainWorkbenchView()
    {
        InitializeComponent();
        _ = _outputRenderer.ClearAsync(OutputChart);
        Loaded += OnLoaded;
        Loaded += OnThemeLoaded;
        Unloaded += OnThemeUnloaded;
    }

    public async void DisplayResult(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var presentation = TransformWorkbenchPresenter.Build(result);
        _currentComputationResult = TransformComputationResultProjector.FromOperationChainResult(result);
        SetSummaryStatus(presentation.Summary);
        SetResultRows(presentation.DatasetRows);
        EvidenceText.Text = presentation.Evidence;
        await _outputRenderer.RenderAsync(OutputChart, _currentComputationResult);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_inputLoader != null)
            return;

        _isInitializing = true;
        try
        {
            var shared = SharedMainWindowViewModelProvider.GetOrCreate(ResolveConnectionString());
            _metricSelectionService = shared.MetricSelectionService;
            _inputLoader = new TransformWorkbenchService(_metricSelectionService);

            InitializeResolutionOptions(shared);
            InitializeOperationOptions();
            InitializeDateRange(shared);
            EnsureInputRowCount(1);
            RefreshEquationInputOptions();
            await LoadMetricOptionsAsync();
            SetSummaryStatus("Select inputs and an operation, then compute an Operation Chain result.");
            _sessionMilestoneRecorder.RecordInitialized(_inputRows.Count, ResolveResolutionTableName());
        }
        catch (Exception ex)
        {
            _sessionMilestoneRecorder.RecordInitializationFailed(ex.Message);
            SetSummaryError($"Operation Chain initialization failed: {ex.Message}");
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private void InitializeResolutionOptions(SharedMainWindowViewModelContext shared)
    {
        ResolutionCombo.Items.Clear();
        foreach (var item in new[] { "All", "Hourly", "Daily", "Weekly", "Monthly", "Yearly" })
            ResolutionCombo.Items.Add(item);

        ResolutionCombo.SelectedItem = ChartUiHelper.GetResolutionFromTableName(shared.MetricState.ResolutionTableName);
        if (ResolutionCombo.SelectedItem == null)
            ResolutionCombo.SelectedItem = "All";
    }

    private void InitializeOperationOptions()
    {
        TransformOperationOptions.Populate(OperationCombo, new TransformWorkbenchOperationProvider());
        OperationCombo.SelectedIndex = 0;
    }

    private void InitializeDateRange(SharedMainWindowViewModelContext shared)
    {
        FromDate.SelectedDate = shared.MetricState.FromDate ?? DateTime.UtcNow.Date.AddDays(-30);
        ToDate.SelectedDate = shared.MetricState.ToDate ?? DateTime.UtcNow.Date;
    }

    private async Task LoadMetricOptionsAsync()
    {
        if (_metricSelectionService == null)
            return;

        _metricOptions = await _metricSelectionService.LoadMetricTypesAsync(ResolveResolutionTableName());
        foreach (var row in _inputRows)
            ApplyMetricOptions(row.MetricCombo, _metricOptions);

        for (var index = 0; index < _inputRows.Count && _metricOptions.Count > 0; index++)
        {
            var row = _inputRows[index];
            row.MetricCombo.SelectedIndex = Math.Min(index, _metricOptions.Count - 1);
            await LoadSubtypeOptionsAsync(row);
        }

        await RefreshDateRangeForSelectedInputsAsync();
        RefreshEquationInputOptions();
    }

    private static void ApplyMetricOptions(ComboBox combo, IReadOnlyList<MetricNameOption> options)
    {
        combo.Items.Clear();
        foreach (var option in options)
            combo.Items.Add(option);
    }

    private void EnsureInputRowCount(int count)
    {
        while (_inputRows.Count < count)
            AddInputRow();
    }

    private void AddInputRow()
    {
        var index = _inputRows.Count + 1;
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var label = new TextBlock
        {
            Text = $"Input {index}",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        var metricCombo = CreateInputCombo();
        var subtypeCombo = CreateInputCombo();
        var removeButton = new Button
        {
            Content = "Remove",
            Padding = new Thickness(10, 4, 10, 4),
            Margin = new Thickness(8, 0, 0, 0)
        };

        Grid.SetColumn(label, 0);
        Grid.SetColumn(metricCombo, 1);
        Grid.SetColumn(subtypeCombo, 2);
        Grid.SetColumn(removeButton, 3);
        grid.Children.Add(label);
        grid.Children.Add(metricCombo);
        grid.Children.Add(subtypeCombo);
        grid.Children.Add(removeButton);

        var row = new TransformWorkbenchInputSelectionRow(grid, label, metricCombo, subtypeCombo);
        _inputRows.Add(row);
        InputRowsPanel.Children.Add(grid);

        metricCombo.SelectionChanged += async (_, _) =>
        {
            if (_isInitializing)
                return;

            ClearEquationTerms();
            await LoadSubtypeOptionsAsync(row);
            await RefreshDateRangeForSelectedInputsAsync();
            RefreshEquationInputOptions();
        };
        subtypeCombo.SelectionChanged += (_, _) =>
        {
            if (!_isInitializing)
            {
                ClearEquationTerms();
                RefreshEquationInputOptions();
            }
        };
        removeButton.Click += (_, _) => RemoveInputRow(row);

        ApplyMetricOptions(metricCombo, _metricOptions);
    }

    private static ComboBox CreateInputCombo() =>
        new()
        {
            Margin = new Thickness(0, 0, 8, 0),
            DisplayMemberPath = "Display",
            SelectedValuePath = "Value"
        };

    private void RemoveInputRow(TransformWorkbenchInputSelectionRow row)
    {
        if (_inputRows.Count <= 1)
        {
            _sessionMilestoneRecorder.RecordInputRemoveBlocked();
            SetSummaryError("Operation Chain requires at least one selected input.");
            return;
        }

        _inputRows.Remove(row);
        InputRowsPanel.Children.Remove(row.Container);
        RenumberInputRows();
        ClearEquationTerms();
        RefreshEquationInputOptions();
        _sessionMilestoneRecorder.RecordInputRemoved(_inputRows.Count);
    }

    private void RenumberInputRows()
    {
        for (var index = 0; index < _inputRows.Count; index++)
            _inputRows[index].Label.Text = $"Input {index + 1}";
    }

    private async void OnAddInputClicked(object sender, RoutedEventArgs e)
    {
        var rowIndex = _inputRows.Count;
        AddInputRow();
        var row = _inputRows[rowIndex];
        if (_metricOptions.Count > 0)
        {
            row.MetricCombo.SelectedIndex = Math.Min(rowIndex, _metricOptions.Count - 1);
            await LoadSubtypeOptionsAsync(row);
            RefreshEquationInputOptions();
        }

        _sessionMilestoneRecorder.RecordInputAdded(_inputRows.Count);
    }

    private void OnOperationChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private async void OnResolutionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        _sessionMilestoneRecorder.RecordResolutionChanged(ResolutionCombo.SelectedItem?.ToString() ?? "All");
        await LoadMetricOptionsAsync();
    }

    private async Task LoadSubtypeOptionsAsync(TransformWorkbenchInputSelectionRow row)
    {
        if (_metricSelectionService == null || row.MetricCombo.SelectedItem is not MetricNameOption metric)
            return;

        row.SubtypeCombo.Items.Clear();
        var subtypes = await _metricSelectionService.LoadSubtypesAsync(metric.Value, ResolveResolutionTableName());
        foreach (var subtype in subtypes)
            row.SubtypeCombo.Items.Add(subtype);

        if (row.SubtypeCombo.Items.Count > 0)
            row.SubtypeCombo.SelectedIndex = 0;
    }

    private async void OnLoadInputsClicked(object sender, RoutedEventArgs e)
    {
        if (_inputLoader == null)
        {
            SetSummaryError("Operation Chain is still initializing.");
            return;
        }

        LoadInputsButton.IsEnabled = false;
        SetSummaryStatus("Computing Operation Chain result...");
        string operationTag = "Unknown";
        string? equation = null;
        var inputCount = 0;
        try
        {
            var inputs = TransformInputSelectionResolver.ResolveRequests(_inputRows);
            inputCount = inputs.Count;
            var from = FromDate.SelectedDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var to = ToDate.SelectedDate ?? DateTime.UtcNow.Date;
            var resolution = ResolveResolutionTableName();
            operationTag = ResolveOperationTag();
            equation = _equationTerms.Count > 0 ? EquationText.Text : null;
            _sessionMilestoneRecorder.RecordComputeRequested(operationTag, inputCount, equation);
            var result = _equationTerms.Count > 0
                ? await ComputeEquationAsync(inputs, from, to, resolution)
                : await _inputLoader.ComputeAsync(inputs, from, to, resolution, operationTag);

            DisplayComputationResult(result);
            _sessionMilestoneRecorder.RecordComputeCompleted(operationTag, inputCount, result.Rows.Count, equation);
            _lastExportSnapshot = new TransformEvidenceExportSnapshot(
                inputs,
                _equationTerms.Count > 0 ? EquationText.Text : operationTag,
                _equationTerms.Count > 0 ? EquationText.Text : ResolveOperationLabel(),
                from,
                to,
                resolution,
                result);
        }
        catch (TransformEquationValidationException ex)
        {
            _sessionMilestoneRecorder.RecordInvalidEquation(ex.Message, equation ?? EquationText.Text);
            SetSummaryError($"Invalid equation: {ex.Message}");
            await _outputRenderer.ClearAsync(OutputChart);
        }
        catch (Exception ex)
        {
            _sessionMilestoneRecorder.RecordComputeFailed(operationTag, inputCount, ex.Message, equation ?? EquationText.Text);
            SetSummaryError($"Operation Chain compute failed: {ex.Message}");
            await _outputRenderer.ClearAsync(OutputChart);
        }
        finally
        {
            LoadInputsButton.IsEnabled = true;
        }
    }

    private void OnExportEvidenceClicked(object sender, RoutedEventArgs e)
    {
        if (_lastExportSnapshot == null)
        {
            _sessionMilestoneRecorder.RecordEvidenceExportFailed("No computed Operation Chain result is available.");
            SetSummaryError("Compute an Operation Chain result before exporting evidence.");
            return;
        }

        try
        {
            _sessionMilestoneRecorder.RecordEvidenceExportRequested(_lastExportSnapshot.OperationTag, _lastExportSnapshot.Inputs.Count);
            var result = _exportService.Export(_lastExportSnapshot, DateTime.UtcNow);
            _sessionMilestoneRecorder.RecordEvidenceExportCompleted(_lastExportSnapshot.OperationTag, _lastExportSnapshot.Inputs.Count, result.FilePath);
            SetSummaryStatus($"Operation Chain evidence exported to {result.FilePath}.");
        }
        catch (Exception ex)
        {
            _sessionMilestoneRecorder.RecordEvidenceExportFailed(ex.Message);
            SetSummaryError($"Operation Chain evidence export failed: {ex.Message}");
        }
    }

    private async Task<TransformComputationResult> ComputeEquationAsync(
        IReadOnlyList<MetricSeriesRequest> inputs,
        DateTime from,
        DateTime to,
        string resolution)
    {
        if (_inputLoader == null)
            throw new InvalidOperationException("Operation Chain is still initializing.");

        var compiled = TransformEquationCompiler.Compile(_equationTerms, inputs.Count);
        if (!compiled.IsValid)
            throw new TransformEquationValidationException(compiled.Error);

        return await _inputLoader.ComputeAsync(
            inputs,
            from,
            to,
            resolution,
            compiled.Steps,
            compiled.Title);
    }

    private void OnThemeLoaded(object sender, RoutedEventArgs e)
    {
        if (_isThemeAttached)
            return;

        AppThemeService.Default.ThemeChanged += OnThemeChanged;
        _isThemeAttached = true;
        ApplyOutputChartTheme();
    }

    private void OnThemeUnloaded(object sender, RoutedEventArgs e)
    {
        if (!_isThemeAttached)
            return;

        AppThemeService.Default.ThemeChanged -= OnThemeChanged;
        _isThemeAttached = false;
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(ApplyOutputChartTheme);
            return;
        }

        ApplyOutputChartTheme();
    }

    private void ApplyOutputChartTheme() =>
        ChartThemeStylingHelper.ApplyCartesianChartTheme(OutputChart);

    private async void OnClearClicked(object sender, RoutedEventArgs e)
    {
        _lastExportSnapshot = null;
        _currentComputationResult = null;
        _resultGridToggleCoordinator.Reset();
        _sessionMilestoneRecorder.RecordClearRequested();
        ClearEquationTerms();
        ClearEquationErrorState();
        await ResetControlSelectionsToDefaultsAsync();
        ResultGridTitle.Text = "Result";
        SetResultRows(null);
        EvidenceText.Text = string.Empty;
        ClearEquationErrorState();
        _ = _outputRenderer.ClearAsync(OutputChart);
    }

    private async Task ResetControlSelectionsToDefaultsAsync()
    {
        var wasInitializing = _isInitializing;
        _isInitializing = true;
        try
        {
            ResetInputRowsToInitialState();
            ResolutionCombo.SelectedItem = "All";
            FromDate.SelectedDate = null;
            ToDate.SelectedDate = null;
            if (OperationCombo.Items.Count > 0)
                OperationCombo.SelectedIndex = 0;

            EquationInputCombo.Items.Clear();

            foreach (var row in _inputRows)
            {
                row.MetricCombo.SelectedIndex = -1;
                row.SubtypeCombo.SelectedIndex = -1;
                row.SubtypeCombo.Items.Clear();
            }

            await LoadMetricOptionsAsync();
        }
        finally
        {
            _isInitializing = wasInitializing;
        }
    }

    private void ResetInputRowsToInitialState()
    {
        while (_inputRows.Count > 1)
        {
            var row = _inputRows[^1];
            _inputRows.RemoveAt(_inputRows.Count - 1);
            InputRowsPanel.Children.Remove(row.Container);
        }

        EnsureInputRowCount(1);
        RenumberInputRows();
    }

    private void OnAddEquationTermClicked(object sender, RoutedEventArgs e)
    {
        if (EquationInputCombo.SelectedItem is not TransformInputOption input)
        {
            SetSummaryError("Select an input before adding to the equation.");
            return;
        }

        var operationTag = ResolveOperationTag();
        if (!TransformEquationCompiler.IsSupportedEquationOperation(operationTag))
        {
            SetSummaryError($"Operation '{ResolveOperationLabel()}' is not valid in the equation builder.");
            return;
        }

        _equationTerms.Add(new TransformEquationTerm(
            operationTag,
            ResolveOperationLabel(),
            input.Index,
            input.Display,
            input.EquationLabel));
        EquationText.Text = TransformEquationCompiler.BuildExpression(_equationTerms);
        _sessionMilestoneRecorder.RecordEquationUpdated(EquationText.Text, _equationTerms.Count);
        SetSummaryStatus("Equation updated.");
    }

    private void OnClearEquationClicked(object sender, RoutedEventArgs e)
    {
        ClearEquationTerms();
        _sessionMilestoneRecorder.RecordEquationCleared();
        SetSummaryStatus("Equation cleared.");
    }

    private void OnOutputChartToggleClicked(object sender, RoutedEventArgs e)
    {
        var isVisible = OutputChartContentPanel.Visibility == Visibility.Visible;
        OutputChartContentPanel.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;
        OutputChartToggleButton.Content = isVisible ? UiDefaults.ToggleShowLabel : UiDefaults.ToggleHideLabel;
        _sessionMilestoneRecorder.RecordOutputChartVisibilityChanged(OutputChartContentPanel.Visibility == Visibility.Visible);
    }

    private void OnOutputChartResetZoomClicked(object sender, RoutedEventArgs e)
    {
        _outputRenderer.ResetZoom(OutputChart);
        _sessionMilestoneRecorder.RecordOutputChartResetZoom();
    }

    private async Task RefreshDateRangeForSelectedInputsAsync()
    {
        if (_inputLoader == null || _isApplyingDateRange)
            return;

        if (!TransformInputSelectionResolver.TryResolveRequests(_inputRows, out var inputs))
            return;

        var range = await _inputLoader.ResolveDateRangeAsync(inputs, ResolveResolutionTableName());
        if (range == null)
            return;

        _isApplyingDateRange = true;
        try
        {
            FromDate.SelectedDate = range.From;
            ToDate.SelectedDate = range.To;
        }
        finally
        {
            _isApplyingDateRange = false;
        }
    }

    private async void DisplayComputationResult(TransformComputationResult result)
    {
        _currentComputationResult = result;
        SetSummaryStatus(result.Summary);
        ResultGridTitle.Text = result.Title;
        SetResultRows(result.Rows);
        EvidenceText.Text = result.Evidence ?? $"Transform evidence: {result.ComputationEvidence?.TraceSignature}";
        await _outputRenderer.RenderAsync(OutputChart, result);
    }

    private void SetSummaryStatus(string message)
    {
        SummaryText.SetResourceReference(TextBlock.ForegroundProperty, SummaryStatusBrushResourceKey);
        SummaryText.Text = message;
    }

    private void SetSummaryError(string message)
    {
        SummaryText.SetResourceReference(TextBlock.ForegroundProperty, SummaryErrorBrushResourceKey);
        SummaryText.Text = message;
    }

    private void ClearEquationErrorState()
    {
        SetSummaryStatus("Select inputs and an operation, then compute an Operation Chain result.");
    }

    private void SetResultRows(IReadOnlyList<TransformResultGridRow>? rows)
    {
        _resultGridToggleCoordinator.Reset();
        var selectableRows = new List<TransformSelectableResultGridRow>();
        if (rows != null)
        {
            foreach (var row in rows)
                selectableRows.Add(new TransformSelectableResultGridRow(row));
        }

        ResultGrid.ItemsSource = selectableRows.Count == 0 ? null : selectableRows;
        SmoothedResultColumn.Visibility = selectableRows.Any(row => !string.IsNullOrWhiteSpace(row.Smoothed)) == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void OnResultGridPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_resultGridToggleCoordinator.HandlePreviewMouseLeftButtonDown(ResultGrid, e))
            return;

        await RefreshOutputChartForIncludedRowsAsync();
    }

    private async void OnResultGridKeyDown(object sender, KeyEventArgs e)
    {
        if (!_resultGridToggleCoordinator.HandleKeyDown(ResultGrid, e))
            return;

        await RefreshOutputChartForIncludedRowsAsync();
    }

    private async Task RefreshOutputChartForIncludedRowsAsync()
    {
        if (_currentComputationResult == null)
            return;

        var rows = GetSelectableResultRows();
        if (rows.Count == 0)
        {
            await _outputRenderer.ClearAsync(OutputChart);
            return;
        }

        var includedRows = TransformResultRowSelection.ResolveIncludedRows(rows);
        var includedCount = TransformResultRowSelection.CountIncluded(includedRows);
        _sessionMilestoneRecorder.RecordResultRowsToggled(includedCount, includedRows.Count);
        if (!includedRows.Any(included => included))
        {
            await _outputRenderer.ClearAsync(OutputChart);
            return;
        }

        await _outputRenderer.RenderAsync(OutputChart, _currentComputationResult, includedRows);
    }

    private IReadOnlyList<TransformSelectableResultGridRow> GetSelectableResultRows() =>
        ResultGrid.ItemsSource is IEnumerable<TransformSelectableResultGridRow> rows
            ? rows.ToArray()
            : Array.Empty<TransformSelectableResultGridRow>();

    private void ClearEquationTerms()
    {
        _equationTerms.Clear();
        EquationText.Text = string.Empty;
    }

    private void RefreshEquationInputOptions()
    {
        var selectedIndex = EquationInputCombo.SelectedItem is TransformInputOption selected
            ? selected.Index
            : 0;

        EquationInputCombo.Items.Clear();
        foreach (var option in TransformInputSelectionResolver.BuildInputOptions(_inputRows))
            EquationInputCombo.Items.Add(option);

        if (EquationInputCombo.Items.Count > 0)
            EquationInputCombo.SelectedIndex = Math.Min(selectedIndex, EquationInputCombo.Items.Count - 1);
    }

    private string ResolveOperationTag()
    {
        if (OperationCombo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            return tag;

        throw new InvalidOperationException("Operation Chain requires an operation.");
    }

    private string ResolveOperationLabel()
    {
        if (OperationCombo.SelectedItem is ComboBoxItem item)
            return item.Content?.ToString() ?? ResolveOperationTag();

        return ResolveOperationTag();
    }

    private string ResolveResolutionTableName()
    {
        return ChartUiHelper.GetTableNameFromResolution(ResolutionCombo);
    }

    private static string ResolveConnectionString()
    {
        return ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
    }

    private sealed record TransformWorkbenchInputSelectionRow(
        Grid Container,
        TextBlock Label,
        ComboBox MetricCombo,
        ComboBox SubtypeCombo) : ITransformInputSelection
    {
        public MetricNameOption? SelectedMetric => MetricCombo.SelectedItem as MetricNameOption;
        public MetricNameOption? SelectedSubtype => SubtypeCombo.SelectedItem as MetricNameOption;
    }

    private sealed class TransformEquationValidationException : InvalidOperationException
    {
        public TransformEquationValidationException(string? message)
            : base(message)
        {
        }
    }

}
