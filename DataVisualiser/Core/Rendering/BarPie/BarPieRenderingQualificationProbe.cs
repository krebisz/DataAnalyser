namespace DataVisualiser.Core.Rendering.BarPie;

public sealed class BarPieRenderingQualificationProbe
{
    public async Task<BarPieRenderingQualificationProbeResult> ProbeAsync(
        IBarPieRenderingContract contract,
        BarPieChartRenderHost host,
        BarPieChartRenderRequest initialRequest,
        BarPieChartRenderRequest updateRequest,
        BarPieChartRenderRequest restoreRequest)
    {
        if (contract == null)
            throw new ArgumentNullException(nameof(contract));
        if (host == null)
            throw new ArgumentNullException(nameof(host));
        if (initialRequest == null)
            throw new ArgumentNullException(nameof(initialRequest));
        if (updateRequest == null)
            throw new ArgumentNullException(nameof(updateRequest));
        if (restoreRequest == null)
            throw new ArgumentNullException(nameof(restoreRequest));

        var failures = new List<string>();
        var route = initialRequest.Route;

        var initialRenderPassed = await TryRenderAsync(contract, host, initialRequest, route, failures, "initial render");
        var repeatedUpdatePassed = initialRenderPassed && await TryRenderAsync(contract, host, initialRequest, route, failures, "repeated update");
        var visibilityTransitionPassed = repeatedUpdatePassed &&
            await TryVisibilityTransitionAsync(contract, host, updateRequest, restoreRequest, route, failures);
        var resetViewPassed = visibilityTransitionPassed && TryResetView(contract, host, route, failures);
        var clearPassed = resetViewPassed && await TryClearAsync(contract, host, route, failures, "clear");

        var disposalPassed = false;
        if (clearPassed)
        {
            var disposalRenderPassed = await TryRenderAsync(contract, host, initialRequest, route, failures, "pre-disposal render");
            disposalPassed = disposalRenderPassed && await TryClearAsync(contract, host, route, failures, "disposal/close cleanup");
        }

        return new BarPieRenderingQualificationProbeResult(
            route,
            initialRenderPassed,
            repeatedUpdatePassed,
            visibilityTransitionPassed,
            resetViewPassed,
            clearPassed,
            disposalPassed,
            failures);
    }

    private static async Task<bool> TryRenderAsync(
        IBarPieRenderingContract contract,
        BarPieChartRenderHost host,
        BarPieChartRenderRequest request,
        BarPieRenderingRoute route,
        ICollection<string> failures,
        string stage)
    {
        try
        {
            await contract.RenderAsync(request, host);
            if (!contract.HasRenderableContent(route, host))
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

    private static async Task<bool> TryVisibilityTransitionAsync(
        IBarPieRenderingContract contract,
        BarPieChartRenderHost host,
        BarPieChartRenderRequest hiddenRequest,
        BarPieChartRenderRequest restoreRequest,
        BarPieRenderingRoute route,
        ICollection<string> failures)
    {
        try
        {
            await contract.RenderAsync(hiddenRequest, host);
            await contract.RenderAsync(restoreRequest, host);

            if (!contract.HasRenderableContent(route, host))
            {
                failures.Add("visibility transition: content did not survive visible/hidden update cycle");
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

    private static bool TryResetView(
        IBarPieRenderingContract contract,
        BarPieChartRenderHost host,
        BarPieRenderingRoute route,
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

    private static async Task<bool> TryClearAsync(
        IBarPieRenderingContract contract,
        BarPieChartRenderHost host,
        BarPieRenderingRoute route,
        ICollection<string> failures,
        string stage)
    {
        try
        {
            await contract.ClearAsync(host);

            if (contract.HasRenderableContent(route, host))
            {
                failures.Add($"{stage}: content remained after clear");
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
}
