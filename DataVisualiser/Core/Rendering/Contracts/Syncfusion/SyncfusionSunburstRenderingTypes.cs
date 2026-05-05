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

public sealed record SyncfusionSunburstItem(
    string Bucket,
    string Submetric,
    double Value);

public interface ISyncfusionSunburstRenderTarget
{
    void SetItems(IReadOnlyList<SyncfusionSunburstItem> items);

    bool HasItems { get; }
}

public sealed record SyncfusionSunburstCapabilityContract : IAnalyticalCapabilityContract
{
    public SyncfusionSunburstCapabilityContract(
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery)
    {
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(delivery);

        if (programRequest.Kind != ChartProgramKind.SyncfusionSunburst)
            throw new ArgumentException("SyncfusionSunburst capability contracts must use a SyncfusionSunburst program request.", nameof(programRequest));
        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("SyncfusionSunburst delivery contract must target the SyncfusionSunburst program kind.", nameof(delivery));

        ProgramRequest = programRequest;
        Capability = capability;
        Delivery = delivery;
    }

    public ChartProgramRequest ProgramRequest { get; }
    public CapabilityRequest Capability { get; }
    public ConsumerDeliveryContract Delivery { get; }

    public static SyncfusionSunburstCapabilityContract Create()
    {
        var programRequest = ChartProgramRequest.SyncfusionSunburst();
        return new SyncfusionSunburstCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.HierarchyChart(programRequest.Kind, "SyncfusionSunburst"));
    }
}

public sealed record SyncfusionSunburstChartRenderRequest(
    SyncfusionSunburstRenderingRoute Route,
    IReadOnlyList<SyncfusionSunburstItem> Items,
    int BucketCount,
    int SelectionCount,
    DateTime? From,
    DateTime? To,
    SyncfusionSunburstCapabilityContract? CapabilityContract = null,
    VNextUiConsumptionContract? ConsumptionContract = null);

public sealed record SyncfusionSunburstChartRenderHost(
    ISyncfusionSunburstRenderTarget Target,
    bool IsVisible);

public sealed record SyncfusionSunburstRenderSurface(
    ISyncfusionSunburstRenderTarget Target,
    bool IsVisible);

public static class SyncfusionSunburstVNextConsumptionContractBuilder
{
    public static VNextUiConsumptionContract Build(
        SyncfusionSunburstChartRenderRequest request,
        ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(plan);

        var capabilityContract = request.CapabilityContract ?? SyncfusionSunburstCapabilityContract.Create();
        var provider = ConsumerProviderContracts.SyncfusionSunburst;

        return ChartRenderPlanConsumptionContractBuilder.Build(
            plan,
            capabilityContract,
            provider,
            new Dictionary<string, string>
            {
                ["SyncfusionSunburst.Route"] = request.Route.ToString(),
                ["SyncfusionSunburst.BucketCount"] = request.BucketCount.ToString(),
                ["SyncfusionSunburst.SelectionCount"] = request.SelectionCount.ToString(),
                ["SyncfusionSunburst.ItemCount"] = request.Items.Count.ToString()
            },
            "SyncfusionSunburst");
    }
}

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
        var capabilityContract = request.CapabilityContract ?? SyncfusionSunburstCapabilityContract.Create();
        var metadata = new Dictionary<string, string>
        {
            ["Adapter"] = nameof(SyncfusionSunburstRenderPlanAdapter),
            ["ProgramKind"] = capabilityContract.ProgramRequest.Kind.ToString(),
            ["Route"] = request.Route.ToString(),
            ["BucketCount"] = request.BucketCount.ToString(),
            ["SelectionCount"] = request.SelectionCount.ToString()
        };
        ChartRenderPlanVocabularyMetadata.AddTo(
            metadata,
            capabilityContract.ProgramRequest,
            capabilityContract.Capability,
            capabilityContract.Delivery,
            sourceSignature);

        return new ChartRenderPlan(
            $"{SyncfusionSunburstBackendKey.SyncfusionWpfHierarchy}:{request.Route}:{request.BucketCount}:{request.SelectionCount}:{request.From:O}:{request.To:O}:{request.Items.Count}",
            capabilityContract.ProgramRequest.Kind,
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
