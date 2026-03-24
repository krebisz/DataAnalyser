using System.Windows;

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

    private static bool TryVisibilityTransition(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
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

            if (!contract.HasRenderableContent(route, host))
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

    private static bool TryResetView(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
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

    private static bool TryOffscreenTransition(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
        ICollection<string> failures)
    {
        var cartesianWidth = host.CartesianChart.Width;
        var cartesianHeight = host.CartesianChart.Height;
        var cartesianActualWidth = host.CartesianChart.ActualWidth;
        var cartesianActualHeight = host.CartesianChart.ActualHeight;

        try
        {
            host.CartesianChart.Width = 0;
            host.CartesianChart.Height = 0;
            host.CartesianChart.Measure(new Size(0, 0));
            host.CartesianChart.Arrange(new Rect(0, 0, 0, 0));
            host.CartesianChart.UpdateLayout();

            host.CartesianChart.Width = cartesianWidth;
            host.CartesianChart.Height = cartesianHeight;

            var restoreWidth = cartesianWidth > 0 ? cartesianWidth : Math.Max(cartesianActualWidth, 1);
            var restoreHeight = cartesianHeight > 0 ? cartesianHeight : Math.Max(cartesianActualHeight, 1);
            host.CartesianChart.Measure(new Size(restoreWidth, restoreHeight));
            host.CartesianChart.Arrange(new Rect(0, 0, restoreWidth, restoreHeight));
            host.CartesianChart.UpdateLayout();

            if (!contract.HasRenderableContent(route, host))
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

    private static bool TryClear(
        IDistributionRenderingContract contract,
        DistributionChartRenderHost host,
        DistributionRenderingRoute route,
        ICollection<string> failures,
        string stage)
    {
        try
        {
            contract.Clear(host);

            if (contract.HasRenderableContent(route, host))
            {
                failures.Add($"{stage}: content remained after clear");
                return false;
            }

            if (host.CartesianChart.Tag != null)
            {
                failures.Add($"{stage}: cartesian tag remained after clear");
                return false;
            }

            if (host.CartesianChart.DataTooltip != null)
            {
                failures.Add($"{stage}: cartesian tooltip remained after clear");
                return false;
            }

            if (host.GetPolarTooltip()?.IsOpen == true)
            {
                failures.Add($"{stage}: polar tooltip remained open after clear");
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
