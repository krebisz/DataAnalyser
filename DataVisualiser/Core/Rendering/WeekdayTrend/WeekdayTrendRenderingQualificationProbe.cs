using System.Windows;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed class WeekdayTrendRenderingQualificationProbe
{
    public WeekdayTrendRenderingQualificationProbeResult Probe(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendChartRenderRequest initialRequest,
        WeekdayTrendChartRenderRequest? updateRequest = null)
    {
        if (contract == null)
            throw new ArgumentNullException(nameof(contract));
        if (host == null)
            throw new ArgumentNullException(nameof(host));
        if (initialRequest == null)
            throw new ArgumentNullException(nameof(initialRequest));

        var route = initialRequest.Route;
        var failures = new List<string>();
        var rerenderRequest = updateRequest ?? initialRequest;

        var initialRenderPassed = TryRender(contract, host, initialRequest, route, failures, "initial render");
        var repeatedUpdatePassed = initialRenderPassed && TryRender(contract, host, rerenderRequest, route, failures, "repeated update");
        var visibilityTransitionPassed = repeatedUpdatePassed && TryVisibilityTransition(contract, host, route, failures);
        var offscreenTransitionPassed = visibilityTransitionPassed && TryOffscreenTransition(contract, host, route, failures);
        var resetViewPassed = offscreenTransitionPassed && TryResetView(contract, host, route, failures);
        var clearPassed = resetViewPassed && TryClear(contract, host, route, failures);

        return new WeekdayTrendRenderingQualificationProbeResult(
            route,
            initialRenderPassed,
            repeatedUpdatePassed,
            visibilityTransitionPassed,
            offscreenTransitionPassed,
            resetViewPassed,
            clearPassed,
            failures);
    }

    private static bool TryRender(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendChartRenderRequest request,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures,
        string stage)
    {
        try
        {
            contract.Render(request, host);
            if (!contract.HasRenderableContent(route, host))
                contract.ResetView(route, host);

            if (!HasRenderedState(contract, route, host))
            {
                failures.Add($"{stage}: route rendered without content");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            failures.Add($"{stage}: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private static bool TryVisibilityTransition(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        var cartesianVisibility = host.CartesianChart.Visibility;
        var polarVisibility = host.PolarChart.Visibility;

        try
        {
            host.CartesianChart.Visibility = Visibility.Collapsed;
            host.PolarChart.Visibility = Visibility.Collapsed;
            host.CartesianChart.Visibility = cartesianVisibility;
            host.PolarChart.Visibility = polarVisibility;

            if (!HasRenderedState(contract, route, host))
            {
                failures.Add("visibility transition: content did not survive hide/show");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            failures.Add($"visibility transition: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private static bool TryOffscreenTransition(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        var targetChart = ResolveTargetChart(route, host);
        var width = targetChart.Width;
        var height = targetChart.Height;
        var actualWidth = targetChart.ActualWidth;
        var actualHeight = targetChart.ActualHeight;

        try
        {
            targetChart.Width = 0;
            targetChart.Height = 0;
            targetChart.Measure(new Size(0, 0));
            targetChart.Arrange(new Rect(0, 0, 0, 0));
            targetChart.UpdateLayout();

            targetChart.Width = width;
            targetChart.Height = height;

            var restoreWidth = width > 0 ? width : Math.Max(actualWidth, 1);
            var restoreHeight = height > 0 ? height : Math.Max(actualHeight, 1);
            targetChart.Measure(new Size(restoreWidth, restoreHeight));
            targetChart.Arrange(new Rect(0, 0, restoreWidth, restoreHeight));
            targetChart.UpdateLayout();

            if (!HasRenderedState(contract, route, host))
            {
                failures.Add("offscreen transition: content did not survive host collapse/restore");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            failures.Add($"offscreen transition: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private static bool TryResetView(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        try
        {
            contract.ResetView(route, host);
            return true;
        }
        catch (Exception ex)
        {
            failures.Add($"reset view: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private static bool TryClear(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        try
        {
            contract.Clear(host);

            if (contract.HasRenderableContent(route, host))
            {
                failures.Add("clear: content remained after clear");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            failures.Add($"clear: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private static LiveCharts.Wpf.CartesianChart ResolveTargetChart(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host)
    {
        return route == WeekdayTrendRenderingRoute.Polar
            ? host.PolarChart
            : host.CartesianChart;
    }

    private static bool HasRenderedState(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendRenderingRoute route,
        WeekdayTrendChartRenderHost host)
    {
        if (contract.HasRenderableContent(route, host))
            return true;

        var chart = ResolveTargetChart(route, host);
        return chart.AxisX.Count > 0 && chart.AxisY.Count > 0;
    }
}
