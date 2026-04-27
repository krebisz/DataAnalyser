using System;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Events;

namespace DataVisualiser.UI.Charts.Syncfusion;

public sealed class SyncfusionChartsViewCoordinator
{
    public const string ReachabilityExportWiredMessage = "Reachability export is wired for the Syncfusion tab.";
    public const string ManagedChartKey = ChartControllerKeys.SyncfusionSunburst;

    public string GetReachabilityExportMessage()
    {
        return ReachabilityExportWiredMessage;
    }

    public bool ShouldRenderAfterSubtypeSelectionChange(bool isApplyingSelectionSync, bool canRenderCurrentSelection)
    {
        return !isApplyingSelectionSync && canRenderCurrentSelection;
    }

    public bool ShouldRenderAfterVisibilityOnlyToggle(ChartUpdateRequestedEventArgs args, ChartDataContext? context)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.IsVisibilityOnlyToggle &&
               IsRegisteredKey(args.ToggledChartName) &&
               args.ShowSyncfusionSunburst &&
               context != null;
    }

    public bool ShouldRenderWhenViewBecomesVisible(bool isInitializing, bool isVisible, bool isChartVisible, ChartDataContext? context)
    {
        return !isInitializing && isVisible && isChartVisible && context != null;
    }

    public static bool IsRegisteredKey(string? key)
    {
        return !string.IsNullOrWhiteSpace(key) &&
               string.Equals(key, ManagedChartKey, StringComparison.OrdinalIgnoreCase);
    }
}
