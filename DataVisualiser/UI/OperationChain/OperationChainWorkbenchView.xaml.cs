using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Export;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.OperationChain;

public partial class OperationChainWorkbenchView : UserControl
{
    private readonly List<OperationChainInputSelectionRow> _inputRows = [];
    private MetricSelectionService? _metricSelectionService;
    private OperationChainInputGridLoadService? _inputLoader;
    private readonly OperationChainEvidenceExportService _exportService = new(new EvidenceExportWriter(), new EvidenceExportPathResolver());
    private IReadOnlyList<MetricNameOption> _metricOptions = [];
    private OperationChainEvidenceExportSnapshot? _lastExportSnapshot;
    private bool _isInitializing;
    private bool _isApplyingDateRange;

    public OperationChainWorkbenchView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public void DisplayResult(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var presentation = OperationChainWorkbenchPresenter.Build(result);
        SummaryText.Text = presentation.Summary;
        ResultGrid.ItemsSource = presentation.DatasetRows;
        EvidenceText.Text = presentation.Evidence;
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
            _inputLoader = new OperationChainInputGridLoadService(_metricSelectionService);

            InitializeResolutionOptions(shared);
            InitializeOperationOptions();
            InitializeDateRange(shared);
            EnsureInputRowCount(3);
            await LoadMetricOptionsAsync();
            SummaryText.Text = "Select inputs and an operation, then compute an Operation Chain result.";
        }
        catch (Exception ex)
        {
            SummaryText.Text = $"Operation Chain initialization failed: {ex.Message}";
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
        OperationCombo.Items.Clear();
        AddOperation("Logarithm", "Log");
        AddOperation("Square Root", "Sqrt");
        AddOperation("Add (+)", "Add");
        AddOperation("Subtract (-)", "Subtract");
        AddOperation("Divide (/)", "Divide");
        AddOperation("Ternary Sum (+ +)", "Sum3");
        AddOperation("Correlation", "Correlation");
        AddOperation("Ternary Sum Correlation", "Sum3Correlation");
        OperationCombo.SelectedIndex = 2;
    }

    private void AddOperation(string content, string tag) =>
        OperationCombo.Items.Add(new ComboBoxItem { Content = content, Tag = tag });

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

        var row = new OperationChainInputSelectionRow(grid, label, metricCombo, subtypeCombo);
        _inputRows.Add(row);
        InputRowsPanel.Children.Add(grid);

        metricCombo.SelectionChanged += async (_, _) =>
        {
            if (_isInitializing)
                return;

            await LoadSubtypeOptionsAsync(row);
            await RefreshDateRangeForSelectedInputsAsync();
        };
        subtypeCombo.SelectionChanged += async (_, _) =>
        {
            if (!_isInitializing)
                await RefreshDateRangeForSelectedInputsAsync();
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

    private void RemoveInputRow(OperationChainInputSelectionRow row)
    {
        if (_inputRows.Count <= 2)
        {
            SummaryText.Text = "Operation Chain requires at least two selected inputs.";
            return;
        }

        _inputRows.Remove(row);
        InputRowsPanel.Children.Remove(row.Container);
        RenumberInputRows();
        _ = RefreshDateRangeForSelectedInputsAsync();
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
            await RefreshDateRangeForSelectedInputsAsync();
        }
    }

    private async void OnOperationChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitializing)
            await RefreshDateRangeForSelectedInputsAsync();
    }

    private async void OnResolutionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        await LoadMetricOptionsAsync();
    }

    private async Task LoadSubtypeOptionsAsync(OperationChainInputSelectionRow row)
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
            SummaryText.Text = "Operation Chain is still initializing.";
            return;
        }

        LoadInputsButton.IsEnabled = false;
        SummaryText.Text = "Computing Operation Chain result...";
        try
        {
            var inputs = ResolveInputRequests();
            var from = FromDate.SelectedDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var to = ToDate.SelectedDate ?? DateTime.UtcNow.Date;
            var resolution = ResolveResolutionTableName();
            var operationTag = ResolveOperationTag();
            var result = await _inputLoader.ComputeAsync(inputs, from, to, resolution, operationTag);

            DisplayComputationResult(result);
            _lastExportSnapshot = new OperationChainEvidenceExportSnapshot(
                inputs,
                operationTag,
                ResolveOperationLabel(),
                from,
                to,
                resolution,
                result);
        }
        catch (Exception ex)
        {
            SummaryText.Text = $"Operation Chain compute failed: {ex.Message}";
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
            SummaryText.Text = "Compute an Operation Chain result before exporting evidence.";
            return;
        }

        try
        {
            var result = _exportService.Export(_lastExportSnapshot, DateTime.UtcNow);
            SummaryText.Text = $"Operation Chain evidence exported to {result.FilePath}.";
        }
        catch (Exception ex)
        {
            SummaryText.Text = $"Operation Chain evidence export failed: {ex.Message}";
        }
    }

    private IReadOnlyList<MetricSeriesRequest> ResolveInputRequests()
    {
        var inputs = new List<MetricSeriesRequest>();
        for (var index = 0; index < _inputRows.Count; index++)
            inputs.Add(ResolveInputRequest(_inputRows[index], $"Input {index + 1}"));

        if (inputs.Count < 2)
            throw new InvalidOperationException("Operation Chain requires at least two inputs.");

        return inputs;
    }

    private static MetricSeriesRequest ResolveInputRequest(OperationChainInputSelectionRow row, string label)
    {
        if (row.MetricCombo.SelectedItem is not MetricNameOption metric)
            throw new InvalidOperationException($"{label} requires a metric.");

        if (row.SubtypeCombo.SelectedItem is not MetricNameOption subtype)
            throw new InvalidOperationException($"{label} requires a submetric.");

        return new MetricSeriesRequest(metric.Value, subtype.Value, metric.Display, subtype.Display);
    }

    private async Task RefreshDateRangeForSelectedInputsAsync()
    {
        if (_inputLoader == null || _isApplyingDateRange)
            return;

        if (!TryResolveInputRequests(out var inputs))
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

    private bool TryResolveInputRequests(out IReadOnlyList<MetricSeriesRequest> inputs)
    {
        inputs = Array.Empty<MetricSeriesRequest>();
        if (_inputRows.Count < 2)
            return false;

        var resolved = new List<MetricSeriesRequest>();
        foreach (var row in _inputRows)
        {
            if (row.MetricCombo.SelectedItem is not MetricNameOption metric ||
                row.SubtypeCombo.SelectedItem is not MetricNameOption subtype)
                return false;

            resolved.Add(new MetricSeriesRequest(metric.Value, subtype.Value, metric.Display, subtype.Display));
        }

        inputs = resolved;
        return true;
    }

    private void DisplayComputationResult(OperationChainComputationGridResult result)
    {
        SummaryText.Text = result.Summary;
        ResultGridTitle.Text = result.Title;
        ResultGrid.ItemsSource = result.Rows;
        EvidenceText.Text = result.Evidence ?? $"Operation Chain evidence: {result.Result?.Evidence.TraceSignature}";
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

    private sealed record OperationChainInputSelectionRow(
        Grid Container,
        TextBlock Label,
        ComboBox MetricCombo,
        ComboBox SubtypeCombo);
}
