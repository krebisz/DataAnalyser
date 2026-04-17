using System.Configuration;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using DataVisualiser.Core.Data.Repositories;

namespace DataVisualiser.UI.Admin;

public partial class AdminMetricsManagerView : UserControl
{
    private readonly AdminMetricsManagerCoordinator _coordinator;
    private bool _filterRefreshPending;

    public AdminMetricsManagerView()
    {
        InitializeComponent();

        _coordinator = new AdminMetricsManagerCoordinator(
            new DataFetcherAdminMetricsRepository(new DataFetcher(GetConnectionString())),
            new AdminSessionMilestoneRecorder());
        _coordinator.RowsChanged += OnCoordinatorRowsChanged;
        CountsGrid.ItemsSource = _coordinator.Rows;

        Loaded += OnLoaded;
    }

    private static string GetConnectionString()
    {
        return ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_coordinator.IsLoading)
            return;

        await ReloadMetricTypesAndDataAsync();
    }

    private async void OnReloadClicked(object sender, RoutedEventArgs e)
    {
        _coordinator.RecordReloadRequested(GetSelectedMetricTypeForLog());
        await ReloadMetricTypesAndDataAsync();
    }

    private async void OnMetricTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_coordinator.IsLoading)
            return;

        _coordinator.RecordMetricTypeChanged(GetSelectedMetricTypeForLog());
        await ReloadCountsAsync();
    }

    private void OnHideDisabledToggled(object sender, RoutedEventArgs e)
    {
        _coordinator.SetHideDisabled(HideDisabledCheckBox.IsChecked == true);
        ScheduleRowFilterRefresh();
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        SetBusy(true);
        var result = await _coordinator.SaveAsync();
        StatusText.Text = result.StatusText;
        SetBusy(false);
        UpdateSaveButtonState();
        if (result.Success)
            ScheduleRowFilterRefresh();
    }

    private async Task ReloadMetricTypesAndDataAsync()
    {
        try
        {
            SetBusy(true);
            StatusText.Text = "Loading metric types...";

            var metricTypes = await _coordinator.LoadMetricTypesAsync();
            MetricTypeCombo.ItemsSource = metricTypes;
            MetricTypeCombo.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Load failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }

        await ReloadCountsAsync();
    }

    private async Task ReloadCountsAsync()
    {
        var metricType = GetSelectedMetricTypeForLog();

        SetBusy(true);
        SaveButton.IsEnabled = false;
        StatusText.Text = metricType == null ? "Loading all metric/submetric rows..." : $"Loading rows for {metricType}...";

        var result = await _coordinator.ReloadCountsAsync(metricType);
        StatusText.Text = result.StatusText;
        SetBusy(false);
        UpdateSaveButtonState();
        ScheduleRowFilterRefresh();
    }

    private void OnCoordinatorRowsChanged(object? sender, AdminRowsChangedEventArgs e)
    {
        SaveButton.IsEnabled = e.CanSave;
        if (e.DisabledChanged)
            ScheduleRowFilterRefresh();
    }

    private string? GetSelectedMetricTypeForLog()
    {
        var selected = MetricTypeCombo.SelectedItem as string;
        return string.Equals(selected, AdminMetricsManagerCoordinator.AllMetricTypesToken, StringComparison.OrdinalIgnoreCase) ? null : selected;
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
                }),
                DispatcherPriority.Background);
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

        view.Filter = _coordinator.HideDisabled ? _coordinator.ShouldIncludeRow : null;

        view.Refresh();
        FilterStatusText.Visibility = Visibility.Collapsed;
    }

    private void UpdateSaveButtonState()
    {
        SaveButton.IsEnabled = _coordinator.CanSave;
    }

    private void SetBusy(bool isBusy)
    {
        ReloadButton.IsEnabled = !isBusy;
        MetricTypeCombo.IsEnabled = !isBusy;
        CountsGrid.IsEnabled = !isBusy;
        if (isBusy)
            SaveButton.IsEnabled = false;
    }
}
