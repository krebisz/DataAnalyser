using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.UI;

public partial class MetricSelectionPanel : UserControl
{
    public MetricSelectionPanel()
    {
        InitializeComponent();
    }

    public event EventHandler? LoadDataRequested;
    public event EventHandler? ResetZoomRequested;
    public event EventHandler? ClearRequested;
    public event EventHandler? ExportReachabilityRequested;
    public event EventHandler? ThemeToggleRequested;
    public event EventHandler? AddSubtypeRequested;
    public event SelectionChangedEventHandler? ResolutionSelectionChanged;
    public event SelectionChangedEventHandler? MetricTypeSelectionChanged;
    public event EventHandler<SelectionChangedEventArgs>? FromDateChanged;
    public event EventHandler<SelectionChangedEventArgs>? ToDateChanged;
    public event RoutedEventHandler? CmsToggleChanged;
    public event RoutedEventHandler? CmsStrategyToggled;

    public ComboBox MetricTypeCombo => TablesCombo;
    public ComboBox PrimarySubtypeCombo => SubtypeCombo;
    public DatePicker FromDatePicker => FromDate;
    public DatePicker ToDatePicker => ToDate;
    public ComboBox ResolutionSelector => ResolutionCombo;
    public StackPanel SubtypePanel => TopControlMetricSubtypePanel;
    public Button ThemeToggle => ThemeToggleButton;

    public CheckBox CmsEnable => CmsEnableCheckBox;
    public CheckBox CmsSingle => CmsSingleCheckBox;
    public CheckBox CmsCombined => CmsCombinedCheckBox;
    public CheckBox CmsMulti => CmsMultiCheckBox;
    public CheckBox CmsNormalized => CmsNormalizedCheckBox;
    public CheckBox CmsWeekly => CmsWeeklyCheckBox;
    public CheckBox CmsWeekdayTrend => CmsWeekdayTrendCheckBox;
    public CheckBox CmsHourly => CmsHourlyCheckBox;
    public CheckBox CmsBarPie => CmsBarPieCheckBox;

    private void OnLoadDataClick(object sender, RoutedEventArgs e) => LoadDataRequested?.Invoke(this, EventArgs.Empty);
    private void OnResetZoomClick(object sender, RoutedEventArgs e) => ResetZoomRequested?.Invoke(this, EventArgs.Empty);
    private void OnClearClick(object sender, RoutedEventArgs e) => ClearRequested?.Invoke(this, EventArgs.Empty);
    private void OnExportReachabilityClick(object sender, RoutedEventArgs e) => ExportReachabilityRequested?.Invoke(this, EventArgs.Empty);
    private void OnThemeToggleClick(object sender, RoutedEventArgs e) => ThemeToggleRequested?.Invoke(this, EventArgs.Empty);
    private void OnAddSubtypeClick(object sender, RoutedEventArgs e) => AddSubtypeRequested?.Invoke(this, EventArgs.Empty);
    private void OnResolutionSelectionChanged(object sender, SelectionChangedEventArgs e) => ResolutionSelectionChanged?.Invoke(sender, e);
    private void OnMetricTypeSelectionChanged(object sender, SelectionChangedEventArgs e) => MetricTypeSelectionChanged?.Invoke(sender, e);
    private void OnFromDateChanged(object? sender, SelectionChangedEventArgs e) => FromDateChanged?.Invoke(sender, e);
    private void OnToDateChanged(object? sender, SelectionChangedEventArgs e) => ToDateChanged?.Invoke(sender, e);
    private void OnCmsToggleChanged(object sender, RoutedEventArgs e) => CmsToggleChanged?.Invoke(sender, e);
    private void OnCmsStrategyToggled(object sender, RoutedEventArgs e) => CmsStrategyToggled?.Invoke(sender, e);
}
