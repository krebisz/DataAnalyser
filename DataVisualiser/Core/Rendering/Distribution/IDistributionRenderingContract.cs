namespace DataVisualiser.Core.Rendering.Distribution;

public interface IDistributionRenderingContract
{
    IReadOnlyList<DistributionBackendQualification> GetBackendQualificationMatrix();

    DistributionRenderingCapabilities GetCapabilities(DistributionRenderingRoute route);

    Task RenderAsync(DistributionChartRenderRequest request, DistributionChartRenderHost host);

    void Clear(DistributionChartRenderHost host);

    void ResetView(DistributionRenderingRoute route, DistributionChartRenderHost host);

    bool HasRenderableContent(DistributionRenderingRoute route, DistributionChartRenderHost host);
}
