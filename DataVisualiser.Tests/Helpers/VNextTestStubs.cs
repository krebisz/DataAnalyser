using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.Helpers;

public static class VNextTestStubs
{
    public static ReasoningSessionCoordinator CreateSessionCoordinator()
    {
        var loader = new StubMetricSeriesLoader();
        var gateway = new LegacyMetricViewGateway(loader);
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var engine = new ReasoningEngine(gateway, planner);
        return new ReasoningSessionCoordinator(engine);
    }

    public sealed class StubMetricSeriesLoader : IMetricSeriesLoader
    {
        public Task<LoadedMetricSeries> LoadAsync(
            MetricSeriesRequest request, DateTime from, DateTime to,
            string resolutionTableName, CancellationToken cancellationToken = default)
        {
            var value = string.Equals(request.QuerySubtype, "evening", StringComparison.OrdinalIgnoreCase) ? 2m : 1m;
            return Task.FromResult(new LoadedMetricSeries(
                [new MetricData { NormalizedTimestamp = from, Value = value }], null));
        }
    }
}
