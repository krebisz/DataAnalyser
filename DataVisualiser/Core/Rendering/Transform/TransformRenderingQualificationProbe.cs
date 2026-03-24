using System.Windows;

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
        try
        {
            await contract.RenderAsync(request, host);
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
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
        ICollection<string> failures)
    {
        var visibility = host.Chart.Visibility;

        try
        {
            host.Chart.Visibility = Visibility.Collapsed;
            host.Chart.Visibility = visibility;

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
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
        ICollection<string> failures)
    {
        var width = host.Chart.Width;
        var height = host.Chart.Height;
        var actualWidth = host.Chart.ActualWidth;
        var actualHeight = host.Chart.ActualHeight;

        try
        {
            host.Chart.Width = 0;
            host.Chart.Height = 0;
            host.Chart.Measure(new Size(0, 0));
            host.Chart.Arrange(new Rect(0, 0, 0, 0));
            host.Chart.UpdateLayout();

            host.Chart.Width = width;
            host.Chart.Height = height;

            var restoreWidth = width > 0 ? width : Math.Max(actualWidth, 1);
            var restoreHeight = height > 0 ? height : Math.Max(actualHeight, 1);
            host.Chart.Measure(new Size(restoreWidth, restoreHeight));
            host.Chart.Arrange(new Rect(0, 0, restoreWidth, restoreHeight));
            host.Chart.UpdateLayout();

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
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
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
        ITransformRenderingContract contract,
        TransformChartRenderHost host,
        TransformRenderingRoute route,
        ICollection<string> failures)
    {
        try
        {
            contract.Clear(route, host);

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
