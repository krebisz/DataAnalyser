using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Reusable composite controller for chart panels.
///     Encapsulates the common structure: header with title/toggle, behavioral controls, and chart content.
///     Each chart panel can use this controller and provide its own rendering logic and behavioral controls.
/// </summary>
public partial class ChartPanelController : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ChartPanelController), new PropertyMetadata(UiDefaults.ChartTitleDefault, OnTitleChanged));

    public static readonly DependencyProperty IsChartVisibleProperty = DependencyProperty.Register(nameof(IsChartVisible), typeof(bool), typeof(ChartPanelController), new PropertyMetadata(false, OnVisibilityChanged));

    public static readonly DependencyProperty HasBehavioralControlsProperty = DependencyProperty.Register(nameof(HasBehavioralControls), typeof(bool), typeof(ChartPanelController), new PropertyMetadata(false));

    public static readonly DependencyProperty RenderingContextProperty = DependencyProperty.Register(nameof(RenderingContext), typeof(IChartRenderingContext), typeof(ChartPanelController), new PropertyMetadata(null, OnRenderingContextChanged));

    private IChartRenderingContext? _renderingContext;

    public ChartPanelController()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsChartVisible
    {
        get => (bool)GetValue(IsChartVisibleProperty);
        set => SetValue(IsChartVisibleProperty, value);
    }

    public bool HasBehavioralControls
    {
        get => (bool)GetValue(HasBehavioralControlsProperty);
        set => SetValue(HasBehavioralControlsProperty, value);
    }

    public IChartRenderingContext? RenderingContext
    {
        get => (IChartRenderingContext?)GetValue(RenderingContextProperty);
        set => SetValue(RenderingContextProperty, value);
    }

    /// <summary>
    ///     Gets the current chart data context from the rendering context.
    /// </summary>
    public ChartDataContext? ChartDataContext => _renderingContext?.CurrentDataContext;

    /// <summary>
    ///     Gets the chart state from the rendering context.
    /// </summary>
    public ChartState? ChartState => _renderingContext?.ChartState;

    /// <summary>
    ///     Gets the content panel that contains the chart.
    ///     Useful for direct manipulation if needed.
    /// </summary>
    public Panel ChartContentPanel => ContentPanel;

    /// <summary>
    ///     Gets the toggle button for enabling/disabling the chart panel.
    /// </summary>
    public Button ToggleButtonControl => ToggleButton;

    public event EventHandler? ToggleRequested;

    /// <summary>
    ///     Sets the header controls (e.g., operation toggle button for DiffRatio chart).
    /// </summary>
    public void SetHeaderControls(UIElement? controls)
    {
        HeaderControlsPresenter.Content = controls;
    }

    /// <summary>
    ///     Sets the behavioral controls section (e.g., normalization radio buttons, weekday checkboxes).
    /// </summary>
    public void SetBehavioralControls(UIElement? controls)
    {
        BehavioralControlsPresenter.Content = controls;
        HasBehavioralControls = controls != null;
        BehavioralControlsPresenter.Visibility = controls != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Sets the chart content (the actual LiveCharts.Wpf.CartesianChart or other chart control).
    /// </summary>
    public void SetChartContent(UIElement? chart)
    {
        ChartContentPresenter.Content = chart;
    }

    /// <summary>
    ///     Checks if secondary data is available (convenience method).
    /// </summary>
    public bool HasSecondaryData()
    {
        return _renderingContext != null && _renderingContext.HasSecondaryData(ChartDataContext);
    }

    /// <summary>
    ///     Checks if charts should be rendered (convenience method).
    /// </summary>
    public bool ShouldRenderCharts()
    {
        return _renderingContext != null && _renderingContext.ShouldRenderCharts(ChartDataContext);
    }

    private void OnToggleButtonClick(object sender, RoutedEventArgs e)
    {
        ToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ChartPanelController controller)
            controller.TitleTextBlock.Text = e.NewValue?.ToString() ?? UiDefaults.ChartTitleDefault;
    }

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ChartPanelController controller)
        {
            var isVisible = (bool)e.NewValue;
            controller.ContentPanel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            controller.ToggleButton.Content = isVisible ? UiDefaults.ToggleHideLabel : UiDefaults.ToggleShowLabel;
        }
    }

    private static void OnRenderingContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ChartPanelController controller)
            controller._renderingContext = e.NewValue as IChartRenderingContext;
    }
}
