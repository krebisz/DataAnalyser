using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeWindowLayout();
    }

    private void InitializeWindowLayout()
    {
        Left = 0;
        Top = 0;
    }

    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != MainTabControl)
            return;

        if (MainTabControl.SelectedItem is not TabItem selectedTab)
            return;

        var tabName = selectedTab.Header?.ToString() ?? "Unknown";
        var context = SharedMainWindowViewModelProvider.Current;
        if (context == null)
            return;

        context.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = "TabSwitched",
            Outcome = "Info",
            MetricType = context.MetricState.SelectedMetricType,
            SelectedSeriesCount = context.MetricState.SelectedSeries.Count,
            SelectedDisplayKeys = context.MetricState.SelectedSeries.Select(series => series.DisplayKey).ToList(),
            Note = $"Switched to {tabName} tab."
        });
    }
}
