using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DataVisualiser.Core.Data.Repositories;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Admin;

public partial class AdminMetricsManagerView : UserControl
{
    private const string AllMetricTypesToken = "(All)";
    private readonly ObservableCollection<EditableHealthMetricsCountEntry> _rows = new();
    private readonly DataFetcher _dataFetcher;
    private bool _isLoading;
    private bool _hideDisabled;
    private bool _filterRefreshPending;

    public AdminMetricsManagerView()
    {
        InitializeComponent();

        CountsGrid.ItemsSource = _rows;
        _dataFetcher = new DataFetcher(GetConnectionString());

        Loaded += OnLoaded;
    }

    private static string GetConnectionString()
    {
        return ConfigurationManager.AppSettings["HealthDB"]
               ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
            return;

        await ReloadMetricTypesAndDataAsync();
    }

    private async void OnReloadClicked(object sender, RoutedEventArgs e)
    {
        await ReloadMetricTypesAndDataAsync();
    }

    private async void OnMetricTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading)
            return;

        await ReloadCountsAsync();
    }

    private void OnHideDisabledToggled(object sender, RoutedEventArgs e)
    {
        _hideDisabled = HideDisabledCheckBox.IsChecked == true;
        ScheduleRowFilterRefresh();
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        var dirty = _rows.Where(r => r.IsDirty).ToList();
        if (dirty.Count == 0)
        {
            StatusText.Text = "No changes to save.";
            SaveButton.IsEnabled = false;
            return;
        }

        try
        {
            SetBusy(true);
            StatusText.Text = $"Saving {dirty.Count} row(s)...";

            var updates = dirty.Select(d => d.ToUpdate()).ToList();
            var affected = await _dataFetcher.UpdateHealthMetricsCountsForAdmin(updates);

            foreach (var row in dirty)
                row.AcceptChanges();

            StatusText.Text = $"Saved. Rows updated: {affected}.";

            ScheduleRowFilterRefresh();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Save failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
            UpdateSaveButtonState();
        }
    }

    private async Task ReloadMetricTypesAndDataAsync()
    {
        try
        {
            SetBusy(true);
            StatusText.Text = "Loading metric types...";

            _isLoading = true;
            var metricTypes = await _dataFetcher.GetCountsMetricTypesForAdmin();

            MetricTypeCombo.ItemsSource = new[] { AllMetricTypesToken }.Concat(metricTypes).ToList();
            MetricTypeCombo.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Load failed: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            SetBusy(false);
        }

        await ReloadCountsAsync();
    }

    private async Task ReloadCountsAsync()
    {
        try
        {
            SetBusy(true);
            SaveButton.IsEnabled = false;

            var selected = MetricTypeCombo.SelectedItem as string;
            var metricType = string.Equals(selected, AllMetricTypesToken, StringComparison.OrdinalIgnoreCase) ? null : selected;

            StatusText.Text = metricType == null
                    ? "Loading all metric/submetric rows..."
                    : $"Loading rows for {metricType}...";

            var rows = await _dataFetcher.GetHealthMetricsCountsForAdmin(metricType);

            foreach (var existing in _rows)
                existing.PropertyChanged -= OnRowPropertyChanged;

            _rows.Clear();
            foreach (var row in rows)
            {
                var editable = new EditableHealthMetricsCountEntry(row);
                editable.PropertyChanged += OnRowPropertyChanged;
                _rows.Add(editable);
            }

            StatusText.Text = $"Loaded {_rows.Count} row(s).";
            ScheduleRowFilterRefresh();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Load failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
            UpdateSaveButtonState();
        }
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EditableHealthMetricsCountEntry.IsDirty)
            or nameof(EditableHealthMetricsCountEntry.MetricTypeName)
            or nameof(EditableHealthMetricsCountEntry.MetricSubtypeName)
            or nameof(EditableHealthMetricsCountEntry.Disabled))
        {
            UpdateSaveButtonState();
            if (e.PropertyName == nameof(EditableHealthMetricsCountEntry.Disabled))
                ScheduleRowFilterRefresh();
        }
    }

    private void ScheduleRowFilterRefresh()
    {
        if (_filterRefreshPending)
            return;

        _filterRefreshPending = true;
        FilterStatusText.Visibility = Visibility.Visible;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _filterRefreshPending = false;
            RefreshRowFilter();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void RefreshRowFilter()
    {
        var view = CollectionViewSource.GetDefaultView(CountsGrid.ItemsSource);
        if (view == null)
        {
            FilterStatusText.Visibility = Visibility.Collapsed;
            return;
        }

        if (view is IEditableCollectionView editable && (editable.IsAddingNew || editable.IsEditingItem))
        {
            ScheduleRowFilterRefresh();
            return;
        }

        view.Filter = _hideDisabled
            ? item => item is EditableHealthMetricsCountEntry entry && !entry.Disabled
            : null;

        view.Refresh();
        FilterStatusText.Visibility = Visibility.Collapsed;
    }

    private void UpdateSaveButtonState()
    {
        SaveButton.IsEnabled = _rows.Any(r => r.IsDirty) && !_isLoading;
    }

    private void SetBusy(bool isBusy)
    {
        _isLoading = isBusy;
        ReloadButton.IsEnabled = !isBusy;
        MetricTypeCombo.IsEnabled = !isBusy;
        CountsGrid.IsEnabled = !isBusy;
        if (isBusy)
            SaveButton.IsEnabled = false;
    }

    private sealed class EditableHealthMetricsCountEntry : INotifyPropertyChanged
    {
        private readonly HealthMetricsCountEntry _original;
        private string _metricTypeName;
        private string _metricSubtypeName;
        private bool _disabled;
        private bool _isDirty;

        public EditableHealthMetricsCountEntry(HealthMetricsCountEntry source)
        {
            _original = source;
            MetricType = source.MetricType;
            MetricSubtype = source.MetricSubtype;
            RecordCount = source.RecordCount;
            MostRecentDateTime = source.MostRecentDateTime;

            _metricTypeName = source.MetricTypeName ?? string.Empty;
            _metricSubtypeName = source.MetricSubtypeName ?? string.Empty;
            _disabled = source.Disabled;
            _isDirty = false;
        }

        public string MetricType { get; }
        public string MetricSubtype { get; }
        public long RecordCount { get; }
        public DateTime? MostRecentDateTime { get; }

        public string MetricTypeName
        {
            get => _metricTypeName;
            set
            {
                if (value == _metricTypeName)
                    return;
                _metricTypeName = value ?? string.Empty;
                OnPropertyChanged();
                RecomputeDirty();
            }
        }

        public string MetricSubtypeName
        {
            get => _metricSubtypeName;
            set
            {
                if (value == _metricSubtypeName)
                    return;
                _metricSubtypeName = value ?? string.Empty;
                OnPropertyChanged();
                RecomputeDirty();
            }
        }

        public bool Disabled
        {
            get => _disabled;
            set
            {
                if (value == _disabled)
                    return;
                _disabled = value;
                OnPropertyChanged();
                RecomputeDirty();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (value == _isDirty)
                    return;
                _isDirty = value;
                OnPropertyChanged();
            }
        }

        public HealthMetricsCountEntry ToUpdate()
        {
            return new HealthMetricsCountEntry
            {
                MetricType = MetricType,
                MetricSubtype = MetricSubtype,
                MetricTypeName = MetricTypeName,
                MetricSubtypeName = MetricSubtypeName,
                Disabled = Disabled
            };
        }

        public void AcceptChanges()
        {
            _original.MetricTypeName = MetricTypeName;
            _original.MetricSubtypeName = MetricSubtypeName;
            _original.Disabled = Disabled;
            IsDirty = false;
        }

        private void RecomputeDirty()
        {
            IsDirty =
                !string.Equals((_original.MetricTypeName ?? string.Empty), MetricTypeName, StringComparison.Ordinal) ||
                !string.Equals((_original.MetricSubtypeName ?? string.Empty), MetricSubtypeName, StringComparison.Ordinal) ||
                _original.Disabled != Disabled;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
