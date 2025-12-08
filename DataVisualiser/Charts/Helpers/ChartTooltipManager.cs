using DataVisualiser.Charts.Helpers;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DataVisualiser.Charts.Helpers
{
    /// <summary>
    /// Manages tooltip display and vertical line indicators for one or more CartesianChart instances.
    /// Provides a reusable, modular solution for chart hover interactions.
    /// </summary>
    public class ChartTooltipManager : IDisposable
    {
        private readonly Popup _hoverPopup;
        private readonly TextBlock _timestampText;
        private readonly Dictionary<CartesianChart, TextBlock> _chartTextBlocks;
        private readonly Dictionary<CartesianChart, AxisSection?> _chartVerticalLines;
        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
        private readonly Dictionary<CartesianChart, string> _chartLabels;
        private readonly Window _parentWindow;

        /// <summary>
        /// Initializes a new instance of ChartTooltipManager.
        /// </summary>
        /// <param name="parentWindow">The parent window that will host the tooltip popup.</param>
        /// <param name="chartLabels">Optional dictionary mapping charts to their display labels (e.g., "Main", "Norm", "Diff", "Ratio").</param>
        public ChartTooltipManager(Window parentWindow, Dictionary<CartesianChart, string>? chartLabels = null)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
            _chartTextBlocks = new Dictionary<CartesianChart, TextBlock>();
            _chartVerticalLines = new Dictionary<CartesianChart, AxisSection?>();
            _chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            _chartLabels = chartLabels ?? new Dictionary<CartesianChart, string>();

            // Create the shared hover popup UI
            _timestampText = ChartHelper.SetHoverText(true);
            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children = { _timestampText }
            };

            var border = ChartHelper.CreateBorder(stack);
            _hoverPopup = CreatePopup(border);
        }

        /// <summary>
        /// Attaches a chart to this tooltip manager. The chart will display tooltips and vertical lines on hover.
        /// </summary>
        /// <param name="chart">The chart to attach.</param>
        /// <param name="label">Optional label for this chart (e.g., "Main", "Norm"). If not provided, uses the chart's name or a default.</param>
        public void AttachChart(CartesianChart chart, string? label = null)
        {
            if (chart == null) return;
            if (_chartTextBlocks.ContainsKey(chart)) return; // Already attached

            // Create a TextBlock for this chart's values
            var textBlock = ChartHelper.SetHoverText();
            _chartTextBlocks[chart] = textBlock;

            // Add the TextBlock to the popup's StackPanel
            if (_hoverPopup.Child is Border border && border.Child is StackPanel stackPanel)
            {
                stackPanel.Children.Add(textBlock);
            }

            // Store the label
            if (!string.IsNullOrEmpty(label))
            {
                _chartLabels[chart] = label;
            }
            else if (!_chartLabels.ContainsKey(chart))
            {
                _chartLabels[chart] = chart.Name ?? "Chart";
            }

            // Initialize vertical line tracking
            _chartVerticalLines[chart] = null;

            // Subscribe to events
            chart.DataHover += OnChartDataHover;
            chart.MouseLeave += OnChartMouseLeave;
        }

        /// <summary>
        /// Detaches a chart from this tooltip manager, removing event handlers and cleaning up resources.
        /// </summary>
        /// <param name="chart">The chart to detach.</param>
        public void DetachChart(CartesianChart chart)
        {
            if (chart == null) return;
            if (!_chartTextBlocks.ContainsKey(chart)) return; // Not attached

            // Unsubscribe from events
            chart.DataHover -= OnChartDataHover;
            chart.MouseLeave -= OnChartMouseLeave;

            // Remove vertical line if it exists
            if (_chartVerticalLines.TryGetValue(chart, out var verticalLine) && verticalLine != null)
            {
                ChartHelper.RemoveAxisSection(ref chart, verticalLine);
            }

            // Remove TextBlock from popup
            if (_hoverPopup.Child is Border border && border.Child is StackPanel stackPanel)
            {
                if (_chartTextBlocks.TryGetValue(chart, out var textBlock))
                {
                    stackPanel.Children.Remove(textBlock);
                }
            }

            // Clean up dictionaries
            _chartTextBlocks.Remove(chart);
            _chartVerticalLines.Remove(chart);
            _chartTimestamps.Remove(chart);
            _chartLabels.Remove(chart);
        }

        /// <summary>
        /// Updates the timestamp data for a chart. This should be called whenever chart data is updated.
        /// </summary>
        /// <param name="chart">The chart whose timestamps are being updated.</param>
        /// <param name="timestamps">The list of timestamps corresponding to the chart's data points.</param>
        public void UpdateChartTimestamps(CartesianChart chart, List<DateTime> timestamps)
        {
            if (chart == null) return;
            _chartTimestamps[chart] = timestamps ?? new List<DateTime>();
        }

        /// <summary>
        /// Clears the timestamp data for a chart.
        /// </summary>
        /// <param name="chart">The chart whose timestamps should be cleared.</param>
        public void ClearChartTimestamps(CartesianChart chart)
        {
            if (chart == null) return;
            _chartTimestamps.Remove(chart);
        }

        /// <summary>
        /// Updates the label for a chart.
        /// </summary>
        /// <param name="chart">The chart whose label should be updated.</param>
        /// <param name="label">The new label for the chart.</param>
        public void UpdateChartLabel(CartesianChart chart, string label)
        {
            if (chart == null) return;
            if (_chartLabels.ContainsKey(chart))
            {
                _chartLabels[chart] = label;
            }
        }

        /// <summary>
        /// Handles the DataHover event from any attached chart.
        /// </summary>
        private void OnChartDataHover(object? sender, ChartPoint chartPoint)
        {
            if (chartPoint == null || sender is not CartesianChart chart) return;

            int index = (int)Math.Round(chartPoint.X);

            // Update timestamp text
            string timestampText = GetTimestampTextForIndex(index);
            if (_timestampText != null)
            {
                _timestampText.Text = timestampText;
            }

            // Update text for each attached chart
            var charts = _chartTextBlocks.Keys.ToArray();
            foreach (var attachedChart in charts)
            {
                if (_chartTextBlocks.TryGetValue(attachedChart, out var textBlock))
                {
                    // Use the new formatted method that shows: Primary Smoothed, Secondary Smoothed, Primary Raw, Secondary Raw
                    string chartValues = ChartHelper.GetChartValuesFormattedAtIndex(attachedChart, index);
                    textBlock.Text = chartValues;
                }
            }

            // Position the popup
            ChartHelper.PositionHoverPopup(_hoverPopup);

            // Update vertical lines for all attached charts
            foreach (var attachedChart in charts)
            {
                if (_chartVerticalLines.TryGetValue(attachedChart, out var verticalLine))
                {
                    var chartRef = attachedChart; // Local variable for ref parameter
                    ChartHelper.UpdateVerticalLineForChart(ref chartRef, index, ref verticalLine);
                    _chartVerticalLines[attachedChart] = verticalLine;
                }
            }
        }

        /// <summary>
        /// Handles the MouseLeave event from any attached chart.
        /// </summary>
        private void OnChartMouseLeave(object? sender, MouseEventArgs e)
        {
            ClearHoverVisuals();
        }

        /// <summary>
        /// Clears the tooltip popup and all vertical lines.
        /// </summary>
        private void ClearHoverVisuals()
        {
            if (_hoverPopup != null && _hoverPopup.IsOpen)
            {
                _hoverPopup.IsOpen = false;
            }

            // Remove vertical lines from all charts
            var charts = _chartVerticalLines.Keys.ToArray();
            foreach (var chart in charts)
            {
                if (_chartVerticalLines.TryGetValue(chart, out var verticalLine))
                {
                    var chartRef = chart; // Local variable for ref parameter
                    ChartHelper.RemoveAxisSection(ref chartRef, verticalLine);
                    _chartVerticalLines[chart] = null;
                }
            }
        }

        /// <summary>
        /// Gets the timestamp text for a given index by searching through all attached charts' timestamp data.
        /// </summary>
        private string GetTimestampTextForIndex(int index)
        {
            foreach (var kvp in _chartTimestamps)
            {
                var timestamps = kvp.Value;
                if (index >= 0 && index < timestamps.Count)
                {
                    return timestamps[index].ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            return "Timestamp: N/A";
        }

        /// <summary>
        /// Creates a Popup control with the specified child content.
        /// </summary>
        private Popup CreatePopup(Border border)
        {
            var popup = new Popup
            {
                Child = border,
                Placement = PlacementMode.Mouse,
                StaysOpen = true,
                AllowsTransparency = true,
                PlacementTarget = _parentWindow
            };

            return popup;
        }

        /// <summary>
        /// Disposes of resources and detaches all charts.
        /// </summary>
        public void Dispose()
        {
            // Detach all charts
            var charts = _chartTextBlocks.Keys.ToArray();
            foreach (var chart in charts)
            {
                DetachChart(chart);
            }

            // Close and clear popup
            if (_hoverPopup != null)
            {
                _hoverPopup.IsOpen = false;
            }

            _chartTextBlocks.Clear();
            _chartVerticalLines.Clear();
            _chartTimestamps.Clear();
            _chartLabels.Clear();
        }
    }
}

