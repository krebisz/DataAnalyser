using DataVisualiser.Core.Services;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.VNext.Application;

public static class ReasoningEngineFactory
{
    public static ReasoningSessionCoordinator CreateCoordinator(MetricSelectionService metricSelectionService)
    {
        ArgumentNullException.ThrowIfNull(metricSelectionService);

        var loader = new MetricSelectionServiceSeriesLoader(metricSelectionService);
        var gateway = new LegacyMetricViewGateway(loader);
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var engine = new ReasoningEngine(gateway, planner);

        return new ReasoningSessionCoordinator(engine);
    }
}
