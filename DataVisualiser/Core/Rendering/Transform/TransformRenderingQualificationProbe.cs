using System.Windows;
using DataVisualiser.Core.Rendering;

namespace DataVisualiser.Core.Rendering.Transform;

public sealed class TransformRenderingQualificationProbe
{
    public async Task<TransformRenderingQualificationProbeResult> ProbeAsync(
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformChartRenderRequest initialRequest,
        TransformChartRenderRequest? updateRequest = null)
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

        return new TransformRenderingQualificationProbeResult(
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
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformChartRenderRequest request,
        TransformRenderingRoute route,
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
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryVisibilityTransition(
            [host.Chart],
            () => HasRenderedState(contract, route, host),
            failures);
    }

    private static bool TryOffscreenTransition(
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryOffscreenTransition(
            host.Chart,
            () => HasRenderedState(contract, route, host),
            failures);
    }

    private static bool TryResetView(
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryResetView(() => contract.ResetView(route, host), failures);
    }

    private static bool TryClear(
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryClear(
            () => contract.Clear(route, host),
            () => contract.HasRenderableContent(route, host),
            failures,
            "clear");
    }

    private static bool HasRenderedState(
        ITransformRenderingContract contract,
        TransformRenderingRoute route,
        TransformChartRenderHost host)
    {
        if (contract.HasRenderableContent(route, host))
            return true;

        return host.Chart.AxisX.Count > 0 && host.Chart.AxisY.Count > 0;
    }
}
