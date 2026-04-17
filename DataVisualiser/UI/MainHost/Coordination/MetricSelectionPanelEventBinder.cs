using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MetricSelectionPanelEventBinder
{
    public sealed record Actions(
        Action LoadData,
        Action ResetZoom,
        Action Clear,
        Action ExportReachability,
        Action ToggleTheme,
        Action AddSubtype,
        SelectionChangedEventHandler ResolutionSelectionChanged,
        SelectionChangedEventHandler MetricTypeSelectionChanged,
        EventHandler<SelectionChangedEventArgs> FromDateChanged,
        EventHandler<SelectionChangedEventArgs> ToDateChanged,
        RoutedEventHandler CmsToggleChanged,
        RoutedEventHandler CmsStrategyToggled);

    public void Bind(MetricSelectionPanel panel, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentNullException.ThrowIfNull(actions);

        panel.LoadDataRequested += (_, _) => actions.LoadData();
        panel.ResetZoomRequested += (_, _) => actions.ResetZoom();
        panel.ClearRequested += (_, _) => actions.Clear();
        panel.ExportReachabilityRequested += (_, _) => actions.ExportReachability();
        panel.ThemeToggleRequested += (_, _) => actions.ToggleTheme();
        panel.AddSubtypeRequested += (_, _) => actions.AddSubtype();
        panel.ResolutionSelectionChanged += actions.ResolutionSelectionChanged;
        panel.MetricTypeSelectionChanged += actions.MetricTypeSelectionChanged;
        panel.FromDateChanged += actions.FromDateChanged;
        panel.ToDateChanged += actions.ToDateChanged;
        panel.CmsToggleChanged += actions.CmsToggleChanged;
        panel.CmsStrategyToggled += actions.CmsStrategyToggled;
    }
}
