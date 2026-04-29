using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.BarPie;

public static class BarPieBackendKey
{
    public const string LiveChartsWpfColumn = "LiveChartsWpf.Column";
    public const string LiveChartsWpfPieFacet = "LiveChartsWpf.PieFacet";
    public const string EChartsPlaceholderColumn = "ECharts.Placeholder.Column";
    public const string EChartsPlaceholderPieFacet = "ECharts.Placeholder.PieFacet";
}

public enum BarPieRenderingRoute
{
    Column = 0,
    PieFacet = 1
}

public enum BarPieRenderingQualification
{
    Qualified = 0,
    UnqualifiedDebt = 1
}

public sealed record BarPieChartRenderRequest(
    BarPieRenderingRoute Route,
    UiChartRenderModel Model,
    BarPieCapabilityContract? CapabilityContract = null);

public sealed record BarPieCapabilityContract
{
    public BarPieCapabilityContract(
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery)
    {
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(delivery);

        if (programRequest.Kind != ChartProgramKind.BarPie)
            throw new ArgumentException("BarPie capability contracts must use a BarPie program request.", nameof(programRequest));
        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("BarPie delivery contract must target the BarPie program kind.", nameof(delivery));

        ProgramRequest = programRequest;
        Capability = capability;
        Delivery = delivery;
    }

    public ChartProgramRequest ProgramRequest { get; }
    public CapabilityRequest Capability { get; }
    public ConsumerDeliveryContract Delivery { get; }

    public static BarPieCapabilityContract Create()
    {
        var programRequest = ChartProgramRequest.BarPie();
        return new BarPieCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.Chart(programRequest.Kind, "BarPieChart"));
    }
}

public sealed record BarPieChartRenderHost(
    IChartSurface Surface,
    IChartRenderer Renderer,
    ChartRendererKind RendererKind,
    bool IsVisible);

public static class BarPieRenderPlanBuilder
{
    public static ChartRenderPlan Build(BarPieChartRenderRequest request, ChartRendererKind rendererKind)
    {
        ArgumentNullException.ThrowIfNull(request);

        var model = request.Model;
        var capabilityContract = request.CapabilityContract ?? BarPieCapabilityContract.Create();
        var planKind = request.Route == BarPieRenderingRoute.PieFacet
            ? ChartRenderPlanKind.Faceted
            : ChartRenderPlanKind.Cartesian;

        var backendKey = ResolveBackendKey(rendererKind, request.Route);
        var sourceSignature = $"{model.ChartName}:{request.Route}:{model.Title}:{model.Series.Count}:{model.Facets.Count}";
        var metadata = new Dictionary<string, string>
        {
            ["Adapter"] = "UiChartRenderPlanAdapter",
            [ChartRenderPlanMetadataKeys.BackendKey] = backendKey,
            ["ProgramKind"] = capabilityContract.ProgramRequest.Kind.ToString(),
            ["RendererKind"] = rendererKind.ToString(),
            ["Route"] = request.Route.ToString()
        };
        ChartRenderPlanVocabularyMetadata.AddTo(
            metadata,
            capabilityContract.ProgramRequest,
            capabilityContract.Capability,
            capabilityContract.Delivery,
            sourceSignature);

        return new ChartRenderPlan(
            $"{backendKey}:{model.ChartName}:{request.Route}:{model.Title}:{model.Series.Count}:{model.Facets.Count}",
            capabilityContract.ProgramRequest.Kind,
            planKind,
            ChartDisplayMode.Regular,
            model.Title ?? "Bar/Pie",
            DateTime.MinValue,
            DateTime.MinValue,
            sourceSignature,
            Array.Empty<ChartSeriesPlan>(),
            Array.Empty<ChartHierarchyNodePlan>(),
            new RenderDensityPlan(
                ChartRenderDensityMode.FullFidelity,
                SourcePointCount: CountRenderedPoints(model),
                RenderedPointCount: CountRenderedPoints(model),
                BucketCount: model.Series.Count + model.Facets.Count),
            new ChartInteractionPlan(
                SupportsZoom: request.Route == BarPieRenderingRoute.Column,
                SupportsPan: request.Route == BarPieRenderingRoute.Column,
                SupportsTooltips: true,
                SupportsSelection: true,
                SupportsViewportRefinement: false),
            metadata);
    }

    private static string ResolveBackendKey(ChartRendererKind rendererKind, BarPieRenderingRoute route)
    {
        return rendererKind switch
        {
            ChartRendererKind.LiveCharts when route == BarPieRenderingRoute.PieFacet => BarPieBackendKey.LiveChartsWpfPieFacet,
            ChartRendererKind.LiveCharts => BarPieBackendKey.LiveChartsWpfColumn,
            ChartRendererKind.ECharts when route == BarPieRenderingRoute.PieFacet => BarPieBackendKey.EChartsPlaceholderPieFacet,
            _ => BarPieBackendKey.EChartsPlaceholderColumn
        };
    }

    private static int CountRenderedPoints(UiChartRenderModel model)
    {
        return model.Series.Sum(series => series.Values.Count) +
               model.Facets.Sum(facet => facet.Series.Sum(series => series.Values.Count));
    }
}
