using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace DataVisualiser.Charts.Helpers
{
    /// <summary>
    /// Custom tooltip manager for Weekly Distribution Chart that shows interval breakdown with percentages and counts.
    /// Uses DataHover event to display custom popup tooltip.
    /// </summary>
    public class WeeklyDistributionTooltip : IDisposable
    {
        private readonly Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> _dayIntervalData;
        private readonly CartesianChart _chart;
        private readonly Popup _tooltipPopup;
        private readonly string[] _dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        private System.Windows.Threading.DispatcherTimer? _hoverCheckTimer;
        private DateTime _lastValidHoverTime;
        private int _lastValidDayIndex = -1; // Track which day we last hovered over
        private const int HoverCheckIntervalMs = 100; // Check every 100ms
        private const int HoverTimeoutMs = 300; // Hide if no valid hover for 300ms AND mouse moved away

        public WeeklyDistributionTooltip(
            CartesianChart chart,
            Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> dayIntervalData)
        {
            _chart = chart ?? throw new ArgumentNullException(nameof(chart));
            _dayIntervalData = dayIntervalData ?? new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

            // Find parent window for popup
            Window? parentWindow = null;
            DependencyObject current = chart;
            while (current != null && parentWindow == null)
            {
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
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
            _hoverCheckTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(HoverCheckIntervalMs)
            };
            _hoverCheckTimer.Tick += OnHoverCheckTimerTick;
            _hoverCheckTimer.Start();
            
            _lastValidHoverTime = DateTime.MinValue;
        }
        
        private void OnHoverCheckTimerTick(object? sender, EventArgs e)
        {
            if (_tooltipPopup == null || !_tooltipPopup.IsOpen || _chart == null)
            {
                return;
            }
            
            // Check if mouse is still over the chart
            var mousePos = Mouse.GetPosition(_chart);
            var isOverChart = mousePos.X >= 0 && mousePos.Y >= 0 &&
                              mousePos.X <= _chart.ActualWidth &&
                              mousePos.Y <= _chart.ActualHeight;
            
            if (!isOverChart)
            {
                // Mouse has left the chart - hide immediately
                HideTooltip();
                return;
            }
            
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
            // If chartPoint is null or invalid, hide tooltip immediately
            // This means we've moved off a data point
            if (chartPoint == null || chartPoint.SeriesView == null)
            {
                HideTooltip();
                return;
            }

            // The x-axis index (0-6) represents the day of week
            int dayIndex = (int)Math.Round(chartPoint.X);
            if (dayIndex < 0 || dayIndex > 6)
            {
                // Invalid day index - hide tooltip
                HideTooltip();
                return;
            }

            // Get interval data for this day
            if (!_dayIntervalData.TryGetValue(dayIndex, out var intervals) || intervals == null || intervals.Count == 0)
            {
                // No data for this day - hide tooltip
                HideTooltip();
                return;
            }

            // Valid hover - update timestamp and day index
            _lastValidHoverTime = DateTime.Now;
            _lastValidDayIndex = dayIndex;

            // Create and show tooltip content
            var tooltipContent = CreateTooltipContent(dayIndex, intervals);
            _tooltipPopup.Child = tooltipContent;

            // Position tooltip near mouse
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
                if (mousePos.X < 0 || mousePos.Y < 0 ||
                    mousePos.X > _chart.ActualWidth ||
                    mousePos.Y > _chart.ActualHeight)
                {
                    HideTooltip();
                    return;
                }

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
            _tooltipPopup.IsOpen = false;
            // Reset hover time and day index so timer doesn't immediately re-show it
            _lastValidHoverTime = DateTime.MinValue;
            _lastValidDayIndex = -1;
        }

        private FrameworkElement CreateTooltipContent(int dayIndex, List<(double Min, double Max, int Count, double Percentage)> intervals)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                Margin = new Thickness(8)
            };

            // Day header
            var dayHeader = new TextBlock
            {
                Text = _dayNames[dayIndex],
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50))
            };
            stackPanel.Children.Add(dayHeader);

            // Total count header
            int totalCount = intervals.Sum(i => i.Count);
            var totalHeader = new TextBlock
            {
                Text = $"Total Values: {totalCount}",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromRgb(70, 70, 70))
            };
            stackPanel.Children.Add(totalHeader);

            // Separator
            var separator = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                Margin = new Thickness(0, 0, 0, 6)
            };
            stackPanel.Children.Add(separator);

            // Interval breakdown
            var intervalsPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                MaxHeight = 400 // Limit height and allow scrolling if needed
            };

            // Sort intervals by Min value (ascending)
            var sortedIntervals = intervals.OrderBy(i => i.Min).ToList();

            foreach (var interval in sortedIntervals)
            {
                // Only show intervals that have data or are within the day's range
                if (interval.Count == 0 && interval.Percentage == 0)
                    continue;

                var intervalPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                // Interval range
                var rangeText = new TextBlock
                {
                    Text = $"[{interval.Min:F2} - {interval.Max:F2}]",
                    FontSize = 11,
                    FontFamily = new FontFamily("Consolas"),
                    Width = 120,
                    Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60))
                };
                intervalPanel.Children.Add(rangeText);

                // Percentage
                var percentageText = new TextBlock
                {
                    Text = $"{interval.Percentage:F1}%",
                    FontSize = 11,
                    Width = 60,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(8, 0, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
                };
                intervalPanel.Children.Add(percentageText);

                // Count
                var countText = new TextBlock
                {
                    Text = $"Count: {interval.Count}",
                    FontSize = 11,
                    Margin = new Thickness(8, 0, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80))
                };
                intervalPanel.Children.Add(countText);

                intervalsPanel.Children.Add(intervalPanel);
            }

            // Wrap in ScrollViewer if there are many intervals
            if (sortedIntervals.Count > 15)
            {
                var scrollViewer = new ScrollViewer
                {
                    Content = intervalsPanel,
                    MaxHeight = 400,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                stackPanel.Children.Add(scrollViewer);
            }
            else
            {
                stackPanel.Children.Add(intervalsPanel);
            }

            // Wrap in border
            var border = new Border
            {
                Child = stackPanel,
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    ShadowDepth = 4,
                    Opacity = 0.3,
                    BlurRadius = 5
                }
            };

            return border;
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
    }
}

