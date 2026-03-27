using System.Windows;
using DataVisualiser.Core.Rendering;

namespace DataVisualiser.Core.Rendering.Distribution;

public sealed class DistributionRenderingQualificationProbe
{
    public async Task<DistributionRenderingQualificationProbeResult> ProbeAsync(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionChartRenderRequest initialRequest,
        DistributionChartRenderRequest? updateRequest = null)
    {
        if (contract == null)
            throw new ArgumentNullException(nameof(contract));
        if (host == null)
            throw new ArgumentNullException(nameof(host));
        if (initialRequest == null)
            throw new ArgumentNullException(nameof(initialRequest));

        var failures = new List<string>();
        var route = initialRequest.Route;
        var rerenderRequest = updateRequest ?? initialRequest;

        var initialRenderPassed = await TryRenderAsync(contract, host, initialRequest, route, failures, "initial render");
        var repeatedUpdatePassed = initialRenderPassed && await TryRenderAsync(contract, host, rerenderRequest, route, failures, "repeated update");
        var visibilityTransitionPassed = repeatedUpdatePassed && TryVisibilityTransition(contract, host, route, failures);
        var offscreenTransitionPassed = visibilityTransitionPassed && TryOffscreenTransition(contract, host, route, failures);
        var resetViewPassed = offscreenTransitionPassed && TryResetView(contract, host, route, failures);
        var clearPassed = resetViewPassed && TryClear(contract, host, route, failures, "clear");

        var disposalPassed = false;
        if (clearPassed)
        {
            var disposalRenderPassed = await TryRenderAsync(contract, host, rerenderRequest, route, failures, "pre-disposal render");
            disposalPassed = disposalRenderPassed && TryClear(contract, host, route, failures, "disposal/close cleanup");
        }

        return new DistributionRenderingQualificationProbeResult(
            route,
            initialRenderPassed,
            repeatedUpdatePassed,
            visibilityTransitionPassed,
            offscreenTransitionPassed,
            resetViewPassed,
            clearPassed,
            disposalPassed,
            failures);
    }

    private static async Task<bool> TryRenderAsync(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionChartRenderRequest request,
        DistributionRenderingRoute route,
        ICollection<string> failures,
        string stage)
    {
        return await RenderingQualificationProbeSupport.TryRenderAsync(
            () => contract.RenderAsync(request, host),
            () => contract.HasRenderableContent(route, host),
            failures,
            stage);
    }

    private static bool TryVisibilityTransition(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryVisibilityTransition(
            [host.CartesianChart, host.PolarChart],
            () => contract.HasRenderableContent(route, host),
            failures);
    }

    private static bool TryResetView(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryResetView(() => contract.ResetView(route, host), failures);
    }

    private static bool TryOffscreenTransition(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
        ICollection<string> failures)
    {
        return RenderingQualificationProbeSupport.TryOffscreenTransition(
            host.CartesianChart,
            () => contract.HasRenderableContent(route, host),
            failures);
    }

    private static bool TryClear(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
        ICollection<string> failures,
        string stage)
    {
        return RenderingQualificationProbeSupport.TryClear(
            () => contract.Clear(host),
            () => contract.HasRenderableContent(route, host),
            failures,
            stage,
            () => VerifyClearedState(host, failures, stage));
    }

    private static void VerifyClearedState(
        DistributionChartRenderHost host,
        ICollection<string> failures,
        string stage)
    {
        if (host.CartesianChart.Tag != null)
            failures.Add($"{stage}: cartesian tag remained after clear");

        if (host.CartesianChart.DataTooltip != null)
            failures.Add($"{stage}: cartesian tooltip remained after clear");

        if (host.GetPolarTooltip()?.IsOpen == true)
            failures.Add($"{stage}: polar tooltip remained open after clear");

        if (failures.Any(message => message.StartsWith(stage, StringComparison.Ordinal)))
            throw new InvalidOperationException($"{stage}: post-clear verification failed");
    }
}
