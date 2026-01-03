using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace DataVisualiser.Core.Rendering.Helpers;

/// <summary>
///     Custom tooltip manager for Weekly Distribution Chart that shows interval breakdown with percentages and counts.
///     Uses DataHover event to display custom popup tooltip.
/// </summary>
public class WeeklyDistributionTooltip : IDisposable
{
    private const int HoverCheckIntervalMs = 100; // Check every 100ms
    private const int HoverTimeoutMs = 300; // Hide if no valid hover for 300ms AND mouse moved away
    private readonly CartesianChart _chart;
    private readonly Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> _dayIntervalData;

    private readonly string[] _dayNames =
    {
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday"
    };

    private readonly Popup _tooltipPopup;
    private DispatcherTimer? _hoverCheckTimer;
    private int _lastValidDayIndex = -1; // Track which day we last hovered over
    private DateTime _lastValidHoverTime;

    public WeeklyDistributionTooltip(CartesianChart chart, Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> dayIntervalData)
    {
        _chart = chart ?? throw new ArgumentNullException(nameof(chart));
        _dayIntervalData = dayIntervalData ?? new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

        // Find parent window for popup
        Window? parentWindow = null;
        DependencyObject current = chart;
        while (current != null && parentWindow == null)
        {
            current = VisualTreeHelper.GetParent(current);
            parentWindow = current as Window;
        }

        // Create popup for tooltip
        _tooltipPopup = new Popup
        {
            Placement = PlacementMode.RelativePoint,
            PlacementTarget = chart,
            StaysOpen = false, // Close when focus is lost
            AllowsTransparency = true,
            PopupAnimation = PopupAnimation.Fade
        };

        // Subscribe to chart events
        _chart.DataHover += OnChartDataHover;
        _chart.DataClick += OnChartDataClick; // Also hide on click
        _chart.MouseLeave += OnChartMouseLeave;
        _chart.MouseMove += OnChartMouseMove; // Track mouse movement

        // Also handle popup mouse leave - but don't hide immediately
        // Allow mouse to move to popup without hiding
        _tooltipPopup.MouseEnter += OnPopupMouseEnter;

        // Start timer to periodically check if we're still hovering over valid data
        _hoverCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(HoverCheckIntervalMs)
        };
        _hoverCheckTimer.Tick += OnHoverCheckTimerTick;
        _hoverCheckTimer.Start();

        _lastValidHoverTime = DateTime.MinValue;
    }


    public void Dispose()
    {
        // Stop and dispose timer
        if (_hoverCheckTimer != null)
        {
            _hoverCheckTimer.Stop();
            _hoverCheckTimer.Tick -= OnHoverCheckTimerTick;
            _hoverCheckTimer = null;
        }

        if (_chart != null)
        {
            _chart.DataHover -= OnChartDataHover;
            _chart.DataClick -= OnChartDataClick;
            _chart.MouseLeave -= OnChartMouseLeave;
            _chart.MouseMove -= OnChartMouseMove;
        }

        if (_tooltipPopup != null)
        {
            _tooltipPopup.MouseEnter -= OnPopupMouseEnter;
            _tooltipPopup.MouseLeave -= OnPopupMouseLeave;
            _tooltipPopup.IsOpen = false;
        }
    }

    private void OnHoverCheckTimerTick(object? sender, EventArgs e)
    {
        if (_tooltipPopup == null || !_tooltipPopup.IsOpen || _chart == null)
            return;

        // Check if mouse is still over the chart
        var mousePos = Mouse.GetPosition(_chart);
        var isOverChart = mousePos.X >= 0 && mousePos.Y >= 0 && mousePos.X <= _chart.ActualWidth && mousePos.Y <= _chart.ActualHeight;

        if (!isOverChart)
            // Mouse has left the chart - hide immediately
            HideTooltip();

        // Mouse is still over chart - check if we've received a valid hover recently
        // DataHover doesn't fire continuously, so we only hide if:
        // 1. No valid hover for a while AND
        // 2. Mouse has moved to a different area (we can't easily detect this, so we rely on timeout)
        // Actually, since DataHover doesn't fire continuously, we should keep tooltip open
        // as long as mouse is over chart. Only hide if mouse leaves chart or DataHover fires with invalid data.
        // So we'll only use the timer to hide if mouse has left chart (already handled above)
        // For now, don't auto-hide based on timeout if mouse is still over chart
    }

    private void OnPopupMouseEnter(object? sender, MouseEventArgs e)
    {
        // Keep tooltip open when mouse enters popup
        // This allows user to move mouse from chart to tooltip
    }

    private void OnChartDataHover(object? sender, ChartPoint chartPoint)
    {
        if (!TryResolveHoverContext(chartPoint, out var dayIndex, out var intervals))
        {
            HideTooltip();
            return;
        }

        UpdateHoverState(dayIndex);
        ShowTooltip(dayIndex, intervals);
    }

    private bool TryResolveHoverContext(ChartPoint chartPoint, out int dayIndex, out List<(double Min, double Max, int Count, double Percentage)> intervals)
    {
        dayIndex = -1;
        intervals = null!;

        if (chartPoint == null || chartPoint.SeriesView == null)
            return false;

        dayIndex = (int)Math.Round(chartPoint.X);
        if (dayIndex < 0 || dayIndex > 6)
            return false;

        if (!_dayIntervalData.TryGetValue(dayIndex, out intervals) || intervals == null || intervals.Count == 0)
            return false;

        return true;
    }

    private void UpdateHoverState(int dayIndex)
    {
        _lastValidHoverTime = DateTime.Now;
        _lastValidDayIndex = dayIndex;
    }

    private void ShowTooltip(int dayIndex, List<(double Min, double Max, int Count, double Percentage)> intervals)
    {
        _tooltipPopup.Child = CreateTooltipContent(dayIndex, intervals);

        var mousePos = Mouse.GetPosition(_chart);
        _tooltipPopup.HorizontalOffset = mousePos.X + 10;
        _tooltipPopup.VerticalOffset = mousePos.Y + 10;

        _tooltipPopup.IsOpen = true;
    }


    private void OnChartDataClick(object? sender, ChartPoint chartPoint)
    {
        // Hide tooltip when clicking on chart
        HideTooltip();
    }

    private void OnChartMouseMove(object? sender, MouseEventArgs e)
    {
        // Hide tooltip if mouse moves but we're not over valid data
        // This catches cases where mouse moves to empty areas of the chart
        if (_chart != null && _tooltipPopup != null && _tooltipPopup.IsOpen)
        {
            // Check if mouse is outside chart bounds
            var mousePos = e.GetPosition(_chart);
            if (mousePos.X < 0 || mousePos.Y < 0 || mousePos.X > _chart.ActualWidth || mousePos.Y > _chart.ActualHeight)
                HideTooltip();

            // If mouse is over chart but not over data, LiveCharts won't fire DataHover
            // We'll rely on MouseLeave to hide it, but also check if we're in an empty area
            // by verifying the mouse is still within reasonable bounds of data columns
            // (This is a fallback - MouseLeave should handle most cases)
        }
    }

    private void OnChartMouseLeave(object? sender, MouseEventArgs e)
    {
        // Immediately hide when mouse leaves chart
        HideTooltip();
    }

    private void OnPopupMouseLeave(object? sender, MouseEventArgs e)
    {
        // Hide tooltip when mouse leaves popup
        // This ensures tooltip closes when mouse moves away
        HideTooltip();
    }

    private void HideTooltip()
    {
        if (_tooltipPopup.IsOpen)
            _tooltipPopup.IsOpen = false;

        _lastValidHoverTime = DateTime.MinValue;
        _lastValidDayIndex = -1;
    }


    private FrameworkElement CreateTooltipContent(int dayIndex, List<(double Min, double Max, int Count, double Percentage)> intervals)
    {
        var stackPanel = CreateTooltipRoot();

        stackPanel.Children.Add(CreateDayHeader(dayIndex));
        stackPanel.Children.Add(CreateTotalHeader(intervals));
        stackPanel.Children.Add(CreateSeparator());

        var intervalPanel = CreateIntervalsPanel(intervals);

        if (intervals.Count > 15)
            stackPanel.Children.Add(new ScrollViewer
            {
                Content = intervalPanel,
                MaxHeight = 400,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            });
        else
            stackPanel.Children.Add(intervalPanel);

        return WrapTooltip(stackPanel);
    }

    private StackPanel CreateTooltipRoot()
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            Margin = new Thickness(8)
        };
    }

    private TextBlock CreateDayHeader(int dayIndex)
    {
        return new TextBlock
        {
            Text = _dayNames[dayIndex],
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 8),
            Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50))
        };
    }

    private TextBlock CreateTotalHeader(List<(double Min, double Max, int Count, double Percentage)> intervals)
    {
        var totalCount = intervals.Sum(i => i.Count);
        return new TextBlock
        {
            Text = $"Total Values: {totalCount}",
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 6),
            Foreground = new SolidColorBrush(Color.FromRgb(70, 70, 70))
        };
    }

    private UIElement CreateSeparator()
    {
        return new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
            Margin = new Thickness(0, 0, 0, 6)
        };
    }

    private StackPanel CreateIntervalsPanel(List<(double Min, double Max, int Count, double Percentage)> intervals)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            MaxHeight = 400
        };

        foreach (var interval in intervals.OrderBy(i => i.Min))
        {
            if (interval.Count == 0 && interval.Percentage == 0)
                continue;

            panel.Children.Add(CreateIntervalRow(interval));
        }

        return panel;
    }

    private UIElement CreateIntervalRow((double Min, double Max, int Count, double Percentage) interval)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 2, 0, 2)
        };

        panel.Children.Add(new TextBlock
        {
            Text = $"[{interval.Min:F2} - {interval.Max:F2}]",
            FontSize = 11,
            FontFamily = new FontFamily("Consolas"),
            Width = 120,
            Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60))
        });

        panel.Children.Add(new TextBlock
        {
            Text = $"{interval.Percentage:F1}%",
            FontSize = 11,
            Width = 60,
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(8, 0, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
        });

        panel.Children.Add(new TextBlock
        {
            Text = $"Count: {interval.Count}",
            FontSize = 11,
            Margin = new Thickness(8, 0, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
        });

        return panel;
    }

    private Border WrapTooltip(StackPanel content)
    {
        return new Border
        {
            Child = content,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = 4,
                Opacity = 0.3,
                BlurRadius = 5
            }
        };
    }
}
