using System.Windows;
using System.Windows.Media;
using DataVisualiser.UI.Charts.Interfaces;

namespace DataVisualiser.UI.MainHost.Coordination;

internal sealed class MainChartsViewZoomResetCoordinator
{
    public sealed record Result(int ResetCount, int SkippedCount, int FailureCount);

    public sealed class Actions(Action<string, string, MessageBoxImage> trackHostMessage)
    {
        public Action<string, string, MessageBoxImage> TrackHostMessage { get; } = trackHostMessage ?? throw new ArgumentNullException(nameof(trackHostMessage));
    }

    public Result ResetRegisteredCharts(IReadOnlyList<IChartController> controllers, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(controllers);
        ArgumentNullException.ThrowIfNull(actions);

        var resetCount = 0;
        var skippedCount = 0;
        var failureCount = 0;

        foreach (var controller in controllers)
        {
            if (!ShouldReset(controller))
            {
                skippedCount++;
                continue;
            }

            try
            {
                controller.ResetZoom();
                resetCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                actions.TrackHostMessage(
                    "Reset Zoom",
                    $"Failed to reset zoom for chart '{controller.Key}': {ex.Message}",
                    MessageBoxImage.Warning);
            }
        }

        return new Result(resetCount, skippedCount, failureCount);
    }

    internal static bool ShouldReset(IChartController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (controller is not IWpfChartPanelHost host)
            return true;

        return PresentationSource.FromVisual(host.ChartContentPanel) != null;
    }
}
