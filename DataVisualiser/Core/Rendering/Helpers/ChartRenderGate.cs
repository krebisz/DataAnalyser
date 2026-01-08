using System.Windows;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

/// <summary>
///     Defers chart rendering until the control is visible and measured.
///     Ensures consistent behavior when charts render while collapsed.
/// </summary>
public sealed class ChartRenderGate
{
    private readonly HashSet<CartesianChart> _attachedCharts = new();
    private readonly Dictionary<CartesianChart, Action> _pendingRenders = new();

    public void ExecuteWhenReady(CartesianChart chart, Action render)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));
        if (render == null)
            throw new ArgumentNullException(nameof(render));

        _pendingRenders[chart] = render;

        if (IsChartReady(chart))
        {
            _pendingRenders.Remove(chart);
            Detach(chart);
            render();
            return;
        }

        Attach(chart);
    }

    private static bool IsChartReady(CartesianChart chart)
    {
        return chart.IsVisible && chart.IsLoaded && chart.ActualWidth > 0 && chart.ActualHeight > 0;
    }

    private void Attach(CartesianChart chart)
    {
        if (_attachedCharts.Contains(chart))
            return;

        _attachedCharts.Add(chart);
        chart.Loaded += OnChartLoaded;
        chart.IsVisibleChanged += OnChartVisibleChanged;
        chart.SizeChanged += OnChartSizeChanged;
    }

    private void Detach(CartesianChart chart)
    {
        if (!_attachedCharts.Remove(chart))
            return;

        chart.Loaded -= OnChartLoaded;
        chart.IsVisibleChanged -= OnChartVisibleChanged;
        chart.SizeChanged -= OnChartSizeChanged;
    }

    private void OnChartLoaded(object? sender, RoutedEventArgs e)
    {
        TryRender(sender as CartesianChart);
    }

    private void OnChartVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        TryRender(sender as CartesianChart);
    }

    private void OnChartSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        TryRender(sender as CartesianChart);
    }

    private void TryRender(CartesianChart? chart)
    {
        if (chart == null)
            return;

        if (!_pendingRenders.TryGetValue(chart, out var render))
            return;

        if (!IsChartReady(chart))
            return;

        _pendingRenders.Remove(chart);
        Detach(chart);
        render();
    }
}