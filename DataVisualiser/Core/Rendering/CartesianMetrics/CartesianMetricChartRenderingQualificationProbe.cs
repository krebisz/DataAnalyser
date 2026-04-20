using System.Windows;
using DataVisualiser.Core.Rendering;

namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed class CartesianMetricChartRenderingQualificationProbe
{
    public async Task<CartesianMetricChartRenderingQualificationProbeResult> ProbeAsync(
        ICartesianMetricChartRenderingContract contract,
        CartesianMetricChartRenderHost host,
        CartesianMetricChartRenderRequest initialRequest,
        CartesianMetricChartRenderRequest? updateRequest = null)
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

        var initialRenderPassed = await TryRenderAsync(contract, host, initialRequest, route, failures, "initial render");
        var repeatedUpdatePassed = initialRenderPassed && await TryRenderAsync(contract, host, rerenderRequest, route, failures, "repeated update");
        var visibilityTransitionPassed = repeatedUpdatePassed && TryVisibilityTransition(contract, host, route, failures);
        var offscreenTransitionPassed = visibilityTransitionPassed && TryOffscreenTransition(contract, host, route, failures);
        var resetViewPassed = offscreenTransitionPassed && TryResetView(contract, host, route, failures);
        var clearPassed = resetViewPassed && TryClear(contract, host, route, failures);

        return new CartesianMetricChartRenderingQualificationProbeResult(
            route,
            initialRenderPassed,
            repeatedUpdatePassed,
            visibilityTransitionPassed,
            offscreenTransitionPassed,
            resetViewPassed,
            clearPassed,
            failures);
    }

    private static async Task<bool> TryRenderAsync(
        ICartesianMetricChartRenderingContract contract,
        CartesianMetricChartRenderHost host,
        CartesianMetricChartRenderRequest request,
        CartesianMetricChartRoute route,
        ICollection<string> failures,
        string stage)
    {
        return await RenderingQualificationProbeSupport.TryRenderAsync(
            () => contract.RenderAsync(request, host),
            () => HasRenderedState(contract, route, host),
            failures,
            stage);
    }

    private static bool TryVisibilityTransition(
        ICartesianMetricChartRenderingContract contract,
        CartesianMetricChartRenderHost host,
        CartesianMetricChartRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryVisibilityTransition(
            [host.Chart],
            () => HasRenderedState(contract, route, host),
            failures);
    }

    private static bool TryOffscreenTransition(
        ICartesianMetricChartRenderingContract contract,
        CartesianMetricChartRenderHost host,
        CartesianMetricChartRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryOffscreenTransition(
            host.Chart,
            () => HasRenderedState(contract, route, host),
            failures);
    }

    private static bool TryResetView(
        ICartesianMetricChartRenderingContract contract,
        CartesianMetricChartRenderHost host,
        CartesianMetricChartRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryResetView(() => contract.ResetView(route, host), failures);
    }

    private static bool TryClear(
        ICartesianMetricChartRenderingContract contract,
        CartesianMetricChartRenderHost host,
        CartesianMetricChartRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryClear(
            () => contract.Clear(route, host),
            () => contract.HasRenderableContent(route, host),
            failures,
            "clear");
    }

    private static bool HasRenderedState(
        ICartesianMetricChartRenderingContract contract,
        CartesianMetricChartRoute route,
        CartesianMetricChartRenderHost host)
    {
        if (contract.HasRenderableContent(route, host))
            return true;

        return host.Chart.AxisX.Count > 0 && host.Chart.AxisY.Count > 0;
    }
}

public sealed record CartesianMetricChartRenderingQualificationProbeResult(
    CartesianMetricChartRoute Route,
    bool InitialRenderPassed,
    bool RepeatedUpdatePassed,
    bool VisibilityTransitionPassed,
    bool OffscreenTransitionPassed,
    bool ResetViewPassed,
    bool ClearPassed,
    IReadOnlyList<string> Failures)
{
    public bool Passed => InitialRenderPassed
                          && RepeatedUpdatePassed
                          && VisibilityTransitionPassed
                          && OffscreenTransitionPassed
                          && ResetViewPassed
                          && ClearPassed
                          && Failures.Count == 0;
}
