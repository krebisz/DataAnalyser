using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.UI.Charts.Helpers;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed class WeekdayTrendRenderingContract : IWeekdayTrendRenderingContract
{
    private static readonly IReadOnlyList<WeekdayTrendBackendQualification> QualificationMatrix =
    [
        new WeekdayTrendBackendQualification(
            WeekdayTrendBackendKey.LiveChartsWpfCartesian,
            "WeekdayTrend.Cartesian.LiveChartsWpf",
            WeekdayTrendRenderingQualification.Qualified,
            WeekdayTrendRenderingRoute.Cartesian,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new WeekdayTrendBackendQualification(
            WeekdayTrendBackendKey.LiveChartsWpfPolar,
            "WeekdayTrend.Polar.LiveChartsWpf",
            WeekdayTrendRenderingQualification.Qualified,
            WeekdayTrendRenderingRoute.Polar,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new WeekdayTrendBackendQualification(
            WeekdayTrendBackendKey.LiveChartsWpfScatter,
            "WeekdayTrend.Scatter.LiveChartsWpf",
            WeekdayTrendRenderingQualification.Qualified,
            WeekdayTrendRenderingRoute.Scatter,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true)
    ];

    private readonly WeekdayTrendChartUpdateCoordinator _updateCoordinator;

    public WeekdayTrendRenderingContract(WeekdayTrendChartUpdateCoordinator updateCoordinator)
    {
        _updateCoordinator = updateCoordinator ?? throw new ArgumentNullException(nameof(updateCoordinator));
    }

    public IReadOnlyList<WeekdayTrendBackendQualification> GetBackendQualificationMatrix()
    {
        return QualificationMatrix;
    }

    public WeekdayTrendRenderingCapabilities GetCapabilities(WeekdayTrendRenderingRoute route)
    {
        var qualification = QualificationMatrix.FirstOrDefault(entry => entry.ActiveRoute == route);
        if (qualification == null)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown weekday trend rendering route.");

        return new WeekdayTrendRenderingCapabilities(
            qualification.PathKey,
            qualification.Qualification,
            qualification.SupportsRender,
            qualification.SupportsUpdate,
            qualification.SupportsResetView,
            qualification.SupportsClear,
            qualification.SupportsLifecycleSafety);
    }

    public void Render(WeekdayTrendChartRenderRequest request, WeekdayTrendChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        _updateCoordinator.UpdateChart(request.Result, request.ChartState, host.CartesianChart, host.PolarChart);
    }

    public void Clear(WeekdayTrendChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        ChartSurfaceHelper.ClearCartesian(host.CartesianChart, host.ChartState);
        ChartSurfaceHelper.ClearCartesian(host.PolarChart, host.ChartState);
    }

    public void ResetView(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        if (_updateCoordinator.TryRefitActiveChart())
            return;

        switch (route)
        {
            case WeekdayTrendRenderingRoute.Polar:
                ChartSurfaceHelper.ResetZoom(host.PolarChart);
                return;
            case WeekdayTrendRenderingRoute.Cartesian:
            case WeekdayTrendRenderingRoute.Scatter:
                ChartSurfaceHelper.ResetZoom(host.CartesianChart);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown weekday trend rendering route.");
        }
    }

    public bool HasRenderableContent(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return route switch
        {
            WeekdayTrendRenderingRoute.Polar => ChartSurfaceHelper.HasSeries(host.PolarChart),
            WeekdayTrendRenderingRoute.Cartesian => ChartSurfaceHelper.HasSeries(host.CartesianChart),
            WeekdayTrendRenderingRoute.Scatter => ChartSurfaceHelper.HasSeries(host.CartesianChart),
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown weekday trend rendering route.")
        };
    }
}
