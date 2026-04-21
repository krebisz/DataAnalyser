using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Syncfusion;

namespace DataVisualiser.Core.Rendering.Syncfusion;

public static class SyncfusionSunburstBackendKey
{
    public const string SyncfusionWpfHierarchy = "SyncfusionWpf.Hierarchy";
}

public enum SyncfusionSunburstRenderingRoute
{
    Hierarchy = 0
}

public sealed record SyncfusionSunburstChartRenderRequest(
    SyncfusionSunburstRenderingRoute Route,
    IReadOnlyList<SunburstItem> Items,
    int BucketCount,
    int SelectionCount,
    DateTime? From,
    DateTime? To);

public sealed record SyncfusionSunburstChartRenderHost(
    ISyncfusionSunburstChartController Controller,
    bool IsVisible);
