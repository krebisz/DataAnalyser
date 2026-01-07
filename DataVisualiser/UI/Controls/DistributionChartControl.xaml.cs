using System.Windows;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Common user control for distribution charts (weekly, hourly, etc.)
///     Abstracts the common XAML structure and event handling
/// </summary>
public partial class DistributionChartControl : UserControl
{
    public static readonly DependencyProperty ChartTitleTextProperty = DependencyProperty.Register(nameof(ChartTitleText), typeof(string), typeof(DistributionChartControl), new PropertyMetadata(string.Empty, OnChartTitleTextChanged));

    public static readonly DependencyProperty XAxisLabelsProperty = DependencyProperty.Register(nameof(XAxisLabels), typeof(string), typeof(DistributionChartControl), new PropertyMetadata(string.Empty, OnXAxisLabelsChanged));

    public static readonly DependencyProperty XAxisTitleProperty = DependencyProperty.Register(nameof(XAxisTitle), typeof(string), typeof(DistributionChartControl), new PropertyMetadata("Time", OnXAxisTitleChanged));

    public static readonly DependencyProperty DefaultIntervalCountProperty = DependencyProperty.Register(nameof(DefaultIntervalCount), typeof(int), typeof(DistributionChartControl), new PropertyMetadata(25, OnDefaultIntervalCountChanged));

    public DistributionChartControl()
    {
        InitializeComponent();
    }

    public string ChartTitleText
    {
        get => (string)GetValue(ChartTitleTextProperty);
        set => SetValue(ChartTitleTextProperty, value);
    }

    public string XAxisLabels
    {
        get => (string)GetValue(XAxisLabelsProperty);
        set => SetValue(XAxisLabelsProperty, value);
    }

    public string XAxisTitle
    {
        get => (string)GetValue(XAxisTitleProperty);
        set => SetValue(XAxisTitleProperty, value);
    }

    public int DefaultIntervalCount
    {
        get => (int)GetValue(DefaultIntervalCountProperty);
        set => SetValue(DefaultIntervalCountProperty, value);
    }

    public CartesianChart ChartControl => Chart;

    public StackPanel ChartPanelControl => ChartPanel;

    public Button ToggleButton => ChartToggleButton;

    public bool IsFrequencyShadingEnabled => FrequencyShadingRadio.IsChecked == true;

    public int SelectedIntervalCount
    {
        get
        {
            if (IntervalCountCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string tagValue && int.TryParse(tagValue, out var intervalCount))
                return intervalCount;
            return DefaultIntervalCount;
        }
    }

    public event EventHandler?      ToggleRequested;
    public event EventHandler?      DisplayModeChanged;
    public event EventHandler<int>? IntervalCountChanged;

    public void SetVisibility(bool isVisible)
    {
        ChartPanel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        ChartToggleButton.Content = isVisible ? "Hide" : "Show";
    }

    public void SetDisplayMode(bool useFrequencyShading)
    {
        FrequencyShadingRadio.IsChecked = useFrequencyShading;
        SimpleRangeRadio.IsChecked = !useFrequencyShading;
    }

    public void SetIntervalCount(int intervalCount)
    {
        foreach (ComboBoxItem item in IntervalCountCombo.Items)
            if (item.Tag is string tagValue && int.TryParse(tagValue, out var value) && value == intervalCount)
            {
                IntervalCountCombo.SelectedItem = item;
                break;
            }
    }

    private static void OnChartTitleTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DistributionChartControl control)
            control.ChartTitle.Text = (string)e.NewValue;
    }

    private static void OnXAxisLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DistributionChartControl control && control.Chart.AxisX.Count > 0)
        {
            var labels = ((string)e.NewValue).Split(',');
            control.Chart.AxisX[0].Labels = labels;
        }
    }

    private static void OnXAxisTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DistributionChartControl control && control.Chart.AxisX.Count > 0)
            control.Chart.AxisX[0].Title = (string)e.NewValue;
    }

    private static void OnDefaultIntervalCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DistributionChartControl control)
            control.SetIntervalCount((int)e.NewValue);
    }

    private void OnChartToggle(object sender, RoutedEventArgs e)
    {
        ToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisplayModeChanged(object sender, RoutedEventArgs e)
    {
        DisplayModeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnIntervalCountChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IntervalCountCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string tagValue && int.TryParse(tagValue, out var intervalCount))
            IntervalCountChanged?.Invoke(this, intervalCount);
    }
}