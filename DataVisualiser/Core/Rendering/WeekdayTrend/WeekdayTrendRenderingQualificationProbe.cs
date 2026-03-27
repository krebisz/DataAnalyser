using System.Windows;
using DataVisualiser.Core.Rendering;

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
        return RenderingQualificationProbeSupport.TryRender(
            () => contract.Render(request, host),
            () => HasRenderedState(contract, route, host),
            failures,
            stage,
            () =>
            {
                if (!contract.HasRenderableContent(route, host))
                    contract.ResetView(route, host);
            });
    }

    private static bool TryVisibilityTransition(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryVisibilityTransition(
            [host.CartesianChart, host.PolarChart],
            () => HasRenderedState(contract, route, host),
            failures);
    }

    private static bool TryOffscreenTransition(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryOffscreenTransition(
            ResolveTargetChart(route, host),
            () => HasRenderedState(contract, route, host),
            failures);
    }

    private static bool TryResetView(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryResetView(() => contract.ResetView(route, host), failures);
    }

    private static bool TryClear(
        IWeekdayTrendRenderingContract contract,
        WeekdayTrendChartRenderHost host,
        WeekdayTrendRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryClear(
            () => contract.Clear(host),
            () => contract.HasRenderableContent(route, host),
            failures,
            "clear");
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

public sealed record WeekdayTrendRenderingQualificationProbeResult(
    WeekdayTrendRenderingRoute Route,
    bool InitialRenderPassed,
    bool RepeatedUpdatePassed,
    bool VisibilityTransitionPassed,
    bool OffscreenTransitionPassed,
    bool ResetViewPassed,
    bool ClearPassed,
    IReadOnlyList<string> Failures)
{
    public bool Passed =>
        InitialRenderPassed &&
        RepeatedUpdatePassed &&
        VisibilityTransitionPassed &&
        OffscreenTransitionPassed &&
        ResetViewPassed &&
        ClearPassed;
}
