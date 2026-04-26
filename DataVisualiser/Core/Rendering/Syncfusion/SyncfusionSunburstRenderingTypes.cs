using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Syncfusion;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

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

public sealed record SyncfusionSunburstRenderSurface(
    ISyncfusionSunburstChartController Controller,
    bool IsVisible);

public static class SyncfusionSunburstRenderPlanBuilder
{
    public static ChartRenderPlan Build(SyncfusionSunburstChartRenderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var roots = request.Items
            .GroupBy(item => item.Bucket, StringComparer.OrdinalIgnoreCase)
            .Select(bucket => new ChartHierarchyNodePlan(
                $"bucket:{bucket.Key}",
                bucket.Key,
                bucket.Sum(item => item.Value),
                bucket
                    .GroupBy(item => item.Submetric, StringComparer.OrdinalIgnoreCase)
                    .Select(submetric => new ChartHierarchyNodePlan(
                        $"bucket:{bucket.Key}:submetric:{submetric.Key}",
                        submetric.Key,
                        submetric.Sum(item => item.Value),
                        Array.Empty<ChartHierarchyNodePlan>(),
                        new Dictionary<string, string>
                        {
                            ["Bucket"] = bucket.Key,
                            ["Submetric"] = submetric.Key
                        }))
                    .ToArray(),
                new Dictionary<string, string>
                {
                    ["Bucket"] = bucket.Key
                }))
            .ToArray();

        var renderedNodeCount = CountNodes(roots);
        var sourceSignature = $"{request.Route}:{request.BucketCount}:{request.SelectionCount}:{request.From:O}:{request.To:O}:{request.Items.Count}";
        var metadata = new Dictionary<string, string>
        {
            ["Adapter"] = nameof(SyncfusionSunburstRenderPlanAdapter),
            ["ProgramKind"] = ChartProgramKind.SyncfusionSunburst.ToString(),
            ["Route"] = request.Route.ToString(),
            ["BucketCount"] = request.BucketCount.ToString(),
            ["SelectionCount"] = request.SelectionCount.ToString()
        };
        ChartRenderPlanVocabularyMetadata.AddTo(
            metadata,
            ChartProgramKind.SyncfusionSunburst,
            sourceSignature,
            deliveryTarget: "SyncfusionSunburst");

        return new ChartRenderPlan(
            $"{SyncfusionSunburstBackendKey.SyncfusionWpfHierarchy}:{request.Route}:{request.BucketCount}:{request.SelectionCount}:{request.From:O}:{request.To:O}:{request.Items.Count}",
            ChartProgramKind.SyncfusionSunburst,
            ChartRenderPlanKind.Hierarchy,
            ChartDisplayMode.Regular,
            "Syncfusion Sunburst",
            request.From ?? DateTime.MinValue,
            request.To ?? DateTime.MinValue,
            sourceSignature,
            Array.Empty<ChartSeriesPlan>(),
            roots,
            new RenderDensityPlan(
                ChartRenderDensityMode.FullFidelity,
                request.Items.Count,
                renderedNodeCount,
                request.BucketCount),
            new ChartInteractionPlan(
                SupportsZoom: false,
                SupportsPan: false,
                SupportsTooltips: true,
                SupportsSelection: true,
                SupportsViewportRefinement: false),
            metadata);
    }

    private static int CountNodes(IEnumerable<ChartHierarchyNodePlan> nodes)
    {
        var count = 0;
        foreach (var node in nodes)
        {
            count++;
            count += CountNodes(node.Children);
        }

        return count;
    }
}
