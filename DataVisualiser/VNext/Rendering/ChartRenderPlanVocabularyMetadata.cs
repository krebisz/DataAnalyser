using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public static class ChartRenderPlanVocabularyMetadata
{
    public static IReadOnlyDictionary<string, string> Build(
        ChartProgramKind programKind,
        string sourceSignature,
        ChartDisplayMode displayMode = ChartDisplayMode.Regular,
        string? deliveryTarget = null,
        int overlayCount = 0,
        int interactionCount = 0)
    {
        if (string.IsNullOrWhiteSpace(sourceSignature))
            sourceSignature = "<unknown>";

        var programRequest = CreateProgramRequest(programKind, displayMode);
        var capability = CapabilityRequest.FromProgramRequest(programRequest);
        var delivery = CreateDelivery(programKind, deliveryTarget);
        var provenance = ProvenanceDescriptor.Projected(sourceSignature, AnalyticalAuthority.Legacy);
        var intentSignature = string.Join("::",
            sourceSignature,
            programRequest.Kind,
            capability.Signature,
            delivery.Signature,
            provenance.Signature);

        return new Dictionary<string, string>
        {
            [ChartRenderPlanMetadataKeys.IntentSignature] = intentSignature,
            [ChartRenderPlanMetadataKeys.ProvenanceSignature] = provenance.Signature,
            [ChartRenderPlanMetadataKeys.ConsumerKind] = delivery.ConsumerKind.ToString(),
            [ChartRenderPlanMetadataKeys.DeliveryTarget] = delivery.DeliveryTarget,
            [ChartRenderPlanMetadataKeys.CapabilityKind] = capability.CapabilityKind.ToString(),
            [ChartRenderPlanMetadataKeys.CompositionKind] = capability.CompositionKind.ToString(),
            [ChartRenderPlanMetadataKeys.OverlayCount] = overlayCount.ToString(),
            [ChartRenderPlanMetadataKeys.InteractionCount] = interactionCount.ToString()
        };
    }

    public static void AddTo(
        IDictionary<string, string> metadata,
        ChartProgramKind programKind,
        string sourceSignature,
        ChartDisplayMode displayMode = ChartDisplayMode.Regular,
        string? deliveryTarget = null,
        int overlayCount = 0,
        int interactionCount = 0)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        foreach (var pair in Build(programKind, sourceSignature, displayMode, deliveryTarget, overlayCount, interactionCount))
            metadata[pair.Key] = pair.Value;
    }

    private static ChartProgramRequest CreateProgramRequest(ChartProgramKind kind, ChartDisplayMode displayMode)
    {
        return kind switch
        {
            ChartProgramKind.Main => ChartProgramRequest.MainProgram(displayMode),
            ChartProgramKind.Normalized => ChartProgramRequest.Normalized(),
            ChartProgramKind.Difference => ChartProgramRequest.Difference(),
            ChartProgramKind.Ratio => ChartProgramRequest.Ratio(),
            ChartProgramKind.Transform => ChartProgramRequest.Transform("Transform", []),
            ChartProgramKind.Distribution => ChartProgramRequest.Distribution(),
            ChartProgramKind.WeekdayTrend => ChartProgramRequest.WeekdayTrend(),
            ChartProgramKind.BarPie => ChartProgramRequest.BarPie(),
            ChartProgramKind.SyncfusionSunburst => ChartProgramRequest.SyncfusionSunburst(),
            _ => new ChartProgramRequest(kind, displayMode)
        };
    }

    private static ConsumerDeliveryContract CreateDelivery(ChartProgramKind kind, string? deliveryTarget)
    {
        return kind == ChartProgramKind.SyncfusionSunburst
            ? ConsumerDeliveryContract.HierarchyChart(kind, deliveryTarget ?? "SyncfusionSunburst")
            : ConsumerDeliveryContract.Chart(kind, deliveryTarget ?? DefaultChartDeliveryTarget(kind));
    }

    private static string DefaultChartDeliveryTarget(ChartProgramKind kind)
    {
        return kind switch
        {
            ChartProgramKind.Main => "MainChart",
            ChartProgramKind.Normalized => "NormalizedChart",
            ChartProgramKind.Difference or ChartProgramKind.Ratio => "DiffRatioChart",
            ChartProgramKind.Transform => "TransformChart",
            ChartProgramKind.Distribution => "DistributionChart",
            ChartProgramKind.WeekdayTrend => "WeekdayTrendChart",
            ChartProgramKind.BarPie => "BarPieChart",
            _ => "ChartSurface"
        };
    }
}
