using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class AnalyticalIntentFactory
{
    public AnalyticalIntent Main(
        MetricSelectionRequest selection,
        ChartDisplayMode displayMode = ChartDisplayMode.Regular,
        string deliveryTarget = "MainChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(
            selection,
            ChartProgramRequest.MainProgram(displayMode),
            deliveryTarget,
            overlays,
            interactions);
    }

    public AnalyticalIntent Normalized(
        MetricSelectionRequest selection,
        string deliveryTarget = "NormalizedChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(selection, ChartProgramRequest.Normalized(), deliveryTarget, overlays, interactions);
    }

    public AnalyticalIntent Difference(
        MetricSelectionRequest selection,
        string deliveryTarget = "DiffRatioChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(selection, ChartProgramRequest.Difference(), deliveryTarget, overlays, interactions);
    }

    public AnalyticalIntent Ratio(
        MetricSelectionRequest selection,
        string deliveryTarget = "DiffRatioChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(selection, ChartProgramRequest.Ratio(), deliveryTarget, overlays, interactions);
    }

    public AnalyticalIntent Transform(
        MetricSelectionRequest selection,
        string title,
        IReadOnlyList<SeriesOperationRequest> operations,
        ChartDisplayMode displayMode = ChartDisplayMode.Regular,
        string deliveryTarget = "TransformChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(
            selection,
            ChartProgramRequest.Transform(title, operations, displayMode),
            deliveryTarget,
            overlays,
            interactions);
    }

    public AnalyticalIntent Distribution(
        MetricSelectionRequest selection,
        string deliveryTarget = "DistributionChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(selection, ChartProgramRequest.Distribution(), deliveryTarget, overlays, interactions);
    }

    public AnalyticalIntent WeekdayTrend(
        MetricSelectionRequest selection,
        string deliveryTarget = "WeekdayTrendChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(selection, ChartProgramRequest.WeekdayTrend(), deliveryTarget, overlays, interactions);
    }

    public AnalyticalIntent BarPie(
        MetricSelectionRequest selection,
        string deliveryTarget = "BarPieChart",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(selection, ChartProgramRequest.BarPie(), deliveryTarget, overlays, interactions);
    }

    public AnalyticalIntent SyncfusionSunburst(
        MetricSelectionRequest selection,
        string deliveryTarget = "SyncfusionSunburst",
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        return Create(
            selection,
            ChartProgramRequest.SyncfusionSunburst(),
            deliveryTarget,
            overlays,
            interactions);
    }

    public AnalyticalIntent Create(
        MetricSelectionRequest selection,
        ChartProgramRequest programRequest,
        string? deliveryTarget = null,
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(programRequest);

        var delivery = CreateDelivery(programRequest.Kind, deliveryTarget);
        return AnalyticalIntent.FromRequests(
            selection,
            programRequest,
            delivery,
            ProvenanceDescriptor.FromSelection(selection),
            CapabilityRequest.FromProgramRequest(programRequest),
            overlays,
            interactions);
    }

    private static ConsumerDeliveryContract CreateDelivery(ChartProgramKind kind, string? deliveryTarget)
    {
        return kind == ChartProgramKind.SyncfusionSunburst
            ? ConsumerDeliveryContract.HierarchyChart(kind, deliveryTarget ?? "HierarchySurface")
            : ConsumerDeliveryContract.Chart(kind, deliveryTarget ?? "ChartSurface");
    }
}
