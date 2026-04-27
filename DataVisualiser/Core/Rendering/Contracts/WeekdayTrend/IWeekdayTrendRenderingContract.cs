using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public interface IWeekdayTrendRenderingContract
{
    IReadOnlyList<WeekdayTrendBackendQualification> GetBackendQualificationMatrix();

    WeekdayTrendRenderingCapabilities GetCapabilities(WeekdayTrendRenderingRoute route);

    ChartRenderAdapterResult Render(WeekdayTrendChartRenderRequest request, WeekdayTrendChartRenderHost host);

    void Clear(WeekdayTrendChartRenderHost host);

    void ResetView(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host);

    bool HasRenderableContent(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host);
}
