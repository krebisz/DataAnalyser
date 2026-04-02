using System;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Events;

namespace DataVisualiser.UI.SyncfusionViews;

public sealed class SyncfusionChartsViewCoordinator
{
    public const string ReachabilityExportNotWiredMessage = "Reachability export is not wired for the Syncfusion tab yet.";
    public const string ManagedChartKey = ChartControllerKeys.SyncfusionSunburst;

    public string GetReachabilityExportMessage()
    {
        return ReachabilityExportNotWiredMessage;
    }

    public bool ShouldRenderAfterSubtypeSelectionChange(bool isApplyingSelectionSync, bool hasLoadedData, ChartDataContext? context)
    {
        return !isApplyingSelectionSync && hasLoadedData && context != null;
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
