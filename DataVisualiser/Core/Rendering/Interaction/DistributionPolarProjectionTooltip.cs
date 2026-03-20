using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Interaction;

public sealed class DistributionPolarProjectionTooltip : IDisposable
{
    private const double HoverMoveTolerancePx = 8.0;

    private readonly CartesianChart _chart;
    private readonly DistributionModeDefinition _definition;
    private readonly DispatcherTimer _hoverCheckTimer;
    private readonly Popup _popup;
    private readonly DistributionRangeResult _rangeResult;
    private readonly TextBlock _text;
    private DateTime _lastValidHoverTime;
    private Point _lastValidHoverPosition;
    public DistributionPolarProjectionTooltip(CartesianChart chart, DistributionModeDefinition definition, DistributionRangeResult rangeResult)
    {
        _chart = chart ?? throw new ArgumentNullException(nameof(chart));
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _rangeResult = rangeResult ?? throw new ArgumentNullException(nameof(rangeResult));

        _text = new TextBlock
        {
                Foreground = Brushes.White,
                Margin = new Thickness(6, 4, 6, 4)
        };

        _popup = new Popup
        {
                Placement = PlacementMode.RelativePoint,
                PlacementTarget = chart,
                StaysOpen = false,
                AllowsTransparency = true,
                Child = new Border
                {
                        Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
                        CornerRadius = new CornerRadius(4),
                        BorderThickness = new Thickness(1),
                        BorderBrush = Brushes.Black,
                        Child = _text
                }
        };

        _hoverCheckTimer = new DispatcherTimer
        {
                Interval = TimeSpan.FromMilliseconds(RenderingDefaults.TooltipHoverCheckIntervalMs)
        };
        _hoverCheckTimer.Tick += OnHoverCheckTimerTick;
        _hoverCheckTimer.Start();

        _lastValidHoverTime = DateTime.MinValue;
        _lastValidHoverPosition = new Point(double.NaN, double.NaN);

        _chart.DataHover += OnChartDataHover;
        _chart.DataClick += OnChartDataClick;
        _chart.MouseLeave += OnChartMouseLeave;
    }

    public void Dispose()
    {
        _hoverCheckTimer.Stop();
        _hoverCheckTimer.Tick -= OnHoverCheckTimerTick;

        _chart.DataHover -= OnChartDataHover;
        _chart.DataClick -= OnChartDataClick;
        _chart.MouseLeave -= OnChartMouseLeave;

        HideTooltip();
    }

    private void OnChartDataHover(object? sender, ChartPoint chartPoint)
    {
        if (!TryResolveBucketIndex(chartPoint, out var bucketIndex))
        {
            HideTooltip();
            return;
        }

        var label = _definition.XAxisLabels[bucketIndex];
        var minValue = FormatValue(_rangeResult.Mins, bucketIndex);
        var maxValue = FormatValue(_rangeResult.Maxs, bucketIndex);
        var avgValue = FormatValue(_rangeResult.Averages, bucketIndex);
        var deltaValue = FormatValue(ResolveDelta(bucketIndex));

        _text.Text = $"{label}\nMin: {minValue}\nMax: {maxValue}\nAvg: {avgValue}\nDelta: {deltaValue}";

        var position = Mouse.GetPosition(_chart);
        _lastValidHoverPosition = position;
        _lastValidHoverTime = DateTime.UtcNow;

        _popup.HorizontalOffset = position.X + RenderingDefaults.HoverPopupOffsetPx;
        _popup.VerticalOffset = position.Y + RenderingDefaults.HoverPopupOffsetPx;
        _popup.IsOpen = true;
    }

    private void OnChartDataClick(object? sender, ChartPoint chartPoint)
    {
        HideTooltip();
    }

    private void OnChartMouseLeave(object? sender, MouseEventArgs e)
    {
        HideTooltip();
    }

    private void OnHoverCheckTimerTick(object? sender, EventArgs e)
    {
        if (!_popup.IsOpen)
            return;

        var currentMousePosition = Mouse.GetPosition(_chart);

        if (!IsMouseOverChart(currentMousePosition))
        {
            HideTooltip();
            return;
        }

        if (_lastValidHoverTime == DateTime.MinValue)
            return;

        var elapsed = DateTime.UtcNow - _lastValidHoverTime;
        if (elapsed.TotalMilliseconds <= RenderingDefaults.TooltipHoverTimeoutMs)
            return;

        if (GetDistance(currentMousePosition, _lastValidHoverPosition) > HoverMoveTolerancePx)
            HideTooltip();
    }

    private bool TryResolveBucketIndex(ChartPoint chartPoint, out int bucketIndex)
    {
        bucketIndex = -1;
        if (chartPoint?.SeriesView == null)
            return false;

        var title = chartPoint.SeriesView.Title;
        if (!string.Equals(title, "Min", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(title, "Max", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(title, "Avg", StringComparison.OrdinalIgnoreCase))
            return false;

        var bucketCount = _definition.XAxisLabels.Count;
        if (bucketCount <= 0)
            return false;

        bucketIndex = chartPoint.Key % bucketCount;
        if (bucketIndex < 0)
            bucketIndex += bucketCount;

        return bucketIndex >= 0 && bucketIndex < bucketCount;
    }

    private string FormatValue(IReadOnlyList<double> values, int index)
    {
        if (index < 0 || index >= values.Count || double.IsNaN(values[index]))
            return "n/a";

        return FormatValue(values[index]);
    }

    private string FormatValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return "n/a";

        var formatted = MathHelper.FormatDisplayedValue(value);
        return string.IsNullOrWhiteSpace(_rangeResult.Unit) ? formatted : $"{formatted} {_rangeResult.Unit}";
    }

    private double ResolveDelta(int bucketIndex)
    {
        if (bucketIndex < 0 || bucketIndex >= _rangeResult.Mins.Count || bucketIndex >= _rangeResult.Maxs.Count)
            return double.NaN;

        return _rangeResult.Maxs[bucketIndex] - _rangeResult.Mins[bucketIndex];
    }

    private void HideTooltip()
    {
        _popup.IsOpen = false;
        _lastValidHoverTime = DateTime.MinValue;
        _lastValidHoverPosition = new Point(double.NaN, double.NaN);
    }

    private bool IsMouseOverChart(Point position)
    {
        return position.X >= 0 &&
               position.Y >= 0 &&
               position.X <= _chart.ActualWidth &&
               position.Y <= _chart.ActualHeight;
    }

    private static double GetDistance(Point left, Point right)
    {
        if (double.IsNaN(left.X) || double.IsNaN(left.Y) || double.IsNaN(right.X) || double.IsNaN(right.Y))
            return 0;

        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
