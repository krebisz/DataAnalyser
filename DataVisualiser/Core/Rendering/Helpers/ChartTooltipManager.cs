using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

/// <summary>
///     Manages tooltip display and vertical line indicators for one or more CartesianChart instances.
///     Provides a reusable, modular solution for chart hover interactions.
/// </summary>
public class ChartTooltipManager : IDisposable
{
    private readonly Dictionary<CartesianChart, string>         _chartLabels;
    private readonly Dictionary<CartesianChart, TextBlock>      _chartTextBlocks;
    private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
    private readonly Dictionary<CartesianChart, AxisSection?>   _chartVerticalLines;
    private readonly Popup                                      _hoverPopup;
    private readonly Window                                     _parentWindow;
    private readonly TextBlock                                  _timestampText;

    /// <summary>
    ///     Initializes a new instance of ChartTooltipManager.
    /// </summary>
    /// <param name="parentWindow">The parent window that will host the tooltip popup.</param>
    /// <param name="chartLabels">
    ///     Optional dictionary mapping charts to their display labels (e.g., "Main", "Norm", "Diff",
    ///     "Ratio").
    /// </param>
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
                Children =
                {
                        _timestampText
                }
        };

        var border = ChartHelper.CreateBorder(stack);
        _hoverPopup = CreatePopup(border);
    }

    /// <summary>
    ///     Disposes of resources and detaches all charts.
    /// </summary>
    public void Dispose()
    {
        // Detach all charts
        var charts = _chartTextBlocks.Keys.ToArray();
        foreach (var chart in charts)
            DetachChart(chart);

        // Close and clear popup
        if (_hoverPopup != null)
            _hoverPopup.IsOpen = false;

        _chartTextBlocks.Clear();
        _chartVerticalLines.Clear();
        _chartTimestamps.Clear();
        _chartLabels.Clear();
    }

    /// <summary>
    ///     Attaches a chart to this tooltip manager. The chart will display tooltips and vertical lines on hover.
    /// </summary>
    public void AttachChart(CartesianChart chart, string? label = null)
    {
        if (!CanAttach(chart))
            return;

        RegisterChart(chart, label);
        SubscribeChartEvents(chart);
    }

    private void RegisterChart(CartesianChart chart, string? label)
    {
        var textBlock = CreateAndRegisterTextBlock(chart);
        AddTextBlockToPopup(textBlock);
        RegisterChartLabel(chart, label);
        InitializeVerticalLineTracking(chart);
    }


    /// <summary>
    ///     Detaches a chart from this tooltip manager, removing event handlers and cleaning up resources.
    /// </summary>
    public void DetachChart(CartesianChart chart)
    {
        if (!CanDetach(chart))
            return;

        UnsubscribeChartEvents(chart);
        RemoveChartVisuals(chart);
        CleanupChartState(chart);
    }

    private void RemoveChartVisuals(CartesianChart chart)
    {
        RemoveVerticalLine(chart);
        RemoveTextBlockFromPopup(chart);
    }


    private bool CanAttach(CartesianChart? chart)
    {
        return chart != null && !_chartTextBlocks.ContainsKey(chart);
    }

    private bool CanDetach(CartesianChart? chart)
    {
        return chart != null && _chartTextBlocks.ContainsKey(chart);
    }

    private TextBlock CreateAndRegisterTextBlock(CartesianChart chart)
    {
        var textBlock = ChartHelper.SetHoverText();
        _chartTextBlocks[chart] = textBlock;
        return textBlock;
    }

    private void AddTextBlockToPopup(TextBlock textBlock)
    {
        if (_hoverPopup.Child is Border border && border.Child is StackPanel stackPanel)
            stackPanel.Children.Add(textBlock);
    }

    private void RemoveTextBlockFromPopup(CartesianChart chart)
    {
        if (_hoverPopup.Child is Border border && border.Child is StackPanel stackPanel && _chartTextBlocks.TryGetValue(chart, out var textBlock))
            stackPanel.Children.Remove(textBlock);
    }

    private void RegisterChartLabel(CartesianChart chart, string? label)
    {
        if (!string.IsNullOrEmpty(label))
            _chartLabels[chart] = label;
        else if (!_chartLabels.ContainsKey(chart))
            _chartLabels[chart] = chart.Name ?? "Chart";
    }

    private void InitializeVerticalLineTracking(CartesianChart chart)
    {
        _chartVerticalLines[chart] = null;
    }

    private void RemoveVerticalLine(CartesianChart chart)
    {
        if (_chartVerticalLines.TryGetValue(chart, out var verticalLine) && verticalLine != null)
            ChartHelper.RemoveAxisSection(chart, verticalLine);
    }

    private void SubscribeChartEvents(CartesianChart chart)
    {
        chart.DataHover += OnChartDataHover;
        chart.MouseLeave += OnChartMouseLeave;
    }

    private void UnsubscribeChartEvents(CartesianChart chart)
    {
        chart.DataHover -= OnChartDataHover;
        chart.MouseLeave -= OnChartMouseLeave;
    }

    private void CleanupChartState(CartesianChart chart)
    {
        _chartTextBlocks.Remove(chart);
        _chartVerticalLines.Remove(chart);
        _chartTimestamps.Remove(chart);
        _chartLabels.Remove(chart);
    }


    /// <summary>
    ///     Updates the timestamp data for a chart. This should be called whenever chart data is updated.
    /// </summary>
    /// <param name="chart">The chart whose timestamps are being updated.</param>
    /// <param name="timestamps">The list of timestamps corresponding to the chart's data points.</param>
    public void UpdateChartTimestamps(CartesianChart chart, List<DateTime> timestamps)
    {
        if (chart == null)
            return;
        _chartTimestamps[chart] = timestamps ?? new List<DateTime>();
    }

    /// <summary>
    ///     Clears the timestamp data for a chart.
    /// </summary>
    /// <param name="chart">The chart whose timestamps should be cleared.</param>
    public void ClearChartTimestamps(CartesianChart chart)
    {
        if (chart == null)
            return;
        _chartTimestamps.Remove(chart);
    }

    /// <summary>
    ///     Updates the label for a chart.
    /// </summary>
    /// <param name="chart">The chart whose label should be updated.</param>
    /// <param name="label">The new label for the chart.</param>
    public void UpdateChartLabel(CartesianChart chart, string label)
    {
        if (chart == null)
            return;
        if (_chartLabels.ContainsKey(chart))
            _chartLabels[chart] = label;
    }

    /// <summary>
    ///     Handles the DataHover event from any attached chart.
    /// </summary>
    private void OnChartDataHover(object? sender, ChartPoint chartPoint)
    {
        if (sender is not CartesianChart chart || chartPoint == null)
            return;

        var index = (int)Math.Round(chartPoint.X);

        UpdateTimestampText(index);
        UpdateChartValueTextBlocks(index);
        ShowHoverPopup();
        UpdateVerticalLines(index);
    }

    private void UpdateTimestampText(int index)
    {
        _timestampText.Text = GetTimestampTextForIndex(index);
    }

    private void UpdateChartValueTextBlocks(int index)
    {
        foreach (var (chart, textBlock) in _chartTextBlocks)
            textBlock.Text = ChartHelper.GetChartValuesFormattedAtIndex(chart, index);
    }

    private void ShowHoverPopup()
    {
        ChartHelper.PositionHoverPopup(_hoverPopup);
    }

    private void UpdateVerticalLines(int index)
    {
        foreach (var chart in _chartVerticalLines.Keys.ToArray())
            if (_chartVerticalLines.TryGetValue(chart, out var verticalLine))
            {
                var chartRef = chart;
                ChartHelper.UpdateVerticalLineForChart(ref chartRef, index, ref verticalLine);

                _chartVerticalLines[chart] = verticalLine;
            }
    }


    /// <summary>
    ///     Handles the MouseLeave event from any attached chart.
    /// </summary>
    private void OnChartMouseLeave(object? sender, MouseEventArgs e)
    {
        ClearHoverVisuals();
    }

    /// <summary>
    ///     Clears the tooltip popup and all vertical lines.
    /// </summary>
    private void ClearHoverVisuals()
    {
        if (_hoverPopup.IsOpen)
            _hoverPopup.IsOpen = false;

        foreach (var chart in _chartVerticalLines.Keys.ToArray())
        {
            RemoveVerticalLine(chart);
            _chartVerticalLines[chart] = null;
        }
    }


    /// <summary>
    ///     Gets the timestamp text for a given index by searching through all attached charts' timestamp data.
    /// </summary>
    private string GetTimestampTextForIndex(int index)
    {
        foreach (var kvp in _chartTimestamps)
        {
            var timestamps = kvp.Value;
            if (index >= 0 && index < timestamps.Count)
                return timestamps[index].
                        ToString("yyyy-MM-dd HH:mm:ss");
        }

        return "Timestamp: N/A";
    }

    /// <summary>
    ///     Creates a Popup control with the specified child content.
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
}
