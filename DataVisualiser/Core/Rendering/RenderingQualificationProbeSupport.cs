using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.Core.Rendering;

internal static class RenderingQualificationProbeSupport
{
    public static async Task<bool> TryRenderAsync(
        Func<Task> render,
        Func<bool> hasRenderedState,
        ICollection<string> failures,
        string stage,
        Action? recoverMissingState = null)
    {
        try
        {
            await render();
            if (!TryEnsureRenderedState(hasRenderedState, recoverMissingState))
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

    public static bool TryRender(
        Action render,
        Func<bool> hasRenderedState,
        ICollection<string> failures,
        string stage,
        Action? recoverMissingState = null)
    {
        try
        {
            render();
            if (!TryEnsureRenderedState(hasRenderedState, recoverMissingState))
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

    public static bool TryVisibilityTransition(
        IEnumerable<UIElement> elements,
        Func<bool> hasRenderedState,
        ICollection<string> failures)
    {
        var targets = elements.ToList();
        var originalStates = targets.Select(element => element.Visibility).ToList();

        try
        {
            foreach (var element in targets)
                element.Visibility = Visibility.Collapsed;

            for (var i = 0; i < targets.Count; i++)
                targets[i].Visibility = originalStates[i];

            if (!hasRenderedState())
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

    public static bool TryOffscreenTransition(
        FrameworkElement target,
        Func<bool> hasRenderedState,
        ICollection<string> failures)
    {
        var width = target.Width;
        var height = target.Height;
        var actualWidth = target.ActualWidth;
        var actualHeight = target.ActualHeight;

        try
        {
            target.Width = 0;
            target.Height = 0;
            target.Measure(new Size(0, 0));
            target.Arrange(new Rect(0, 0, 0, 0));
            target.UpdateLayout();

            target.Width = width;
            target.Height = height;

            var restoreWidth = width > 0 ? width : Math.Max(actualWidth, 1);
            var restoreHeight = height > 0 ? height : Math.Max(actualHeight, 1);
            target.Measure(new Size(restoreWidth, restoreHeight));
            target.Arrange(new Rect(0, 0, restoreWidth, restoreHeight));
            target.UpdateLayout();

            if (!hasRenderedState())
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

    public static bool TryResetView(Action resetView, ICollection<string> failures)
    {
        try
        {
            resetView();
            return true;
        }
        catch (Exception ex)
        {
            failures.Add($"reset view: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    public static bool TryClear(
        Action clear,
        Func<bool> hasRenderableContent,
        ICollection<string> failures,
        string stage,
        Action? verifyClearedState = null)
    {
        try
        {
            clear();

            if (hasRenderableContent())
            {
                failures.Add($"{stage}: content remained after clear");
                return false;
            }

            verifyClearedState?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            failures.Add($"{stage}: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private static bool TryEnsureRenderedState(Func<bool> hasRenderedState, Action? recoverMissingState)
    {
        if (hasRenderedState())
            return true;

        recoverMissingState?.Invoke();
        return hasRenderedState();
    }
}
