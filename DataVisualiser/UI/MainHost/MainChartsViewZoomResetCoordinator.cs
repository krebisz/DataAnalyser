using System.Windows;
using System.Windows.Media;
using DataVisualiser.UI.Charts.Interfaces;

namespace DataVisualiser.UI.MainHost;

internal sealed class MainChartsViewZoomResetCoordinator
{
    public sealed class Actions(Action<string, string, MessageBoxImage> trackHostMessage)
    {
        public Action<string, string, MessageBoxImage> TrackHostMessage { get; } = trackHostMessage ?? throw new ArgumentNullException(nameof(trackHostMessage));
    }

    public void ResetRegisteredCharts(IReadOnlyList<IChartController> controllers, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(controllers);
        ArgumentNullException.ThrowIfNull(actions);

        foreach (var controller in controllers)
        {
            if (!ShouldReset(controller))
                continue;

            try
            {
                controller.ResetZoom();
            }
            catch (Exception ex)
            {
                actions.TrackHostMessage(
                    "Reset Zoom",
                    $"Failed to reset zoom for chart '{controller.Key}': {ex.Message}",
                    MessageBoxImage.Warning);
            }
        }
    }

    internal static bool ShouldReset(IChartController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        if (controller is not IWpfChartPanelHost host)
            return true;

        return PresentationSource.FromVisual(host.ChartContentPanel) != null;
    }
}
