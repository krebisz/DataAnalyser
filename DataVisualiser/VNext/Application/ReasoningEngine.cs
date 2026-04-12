using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class ReasoningEngine : IReasoningEngine
{
    private readonly LegacyMetricViewGateway _gateway;
    private readonly ChartProgramPlanner _planner;

    public ReasoningEngine(LegacyMetricViewGateway gateway, ChartProgramPlanner planner)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _planner = planner ?? throw new ArgumentNullException(nameof(planner));
    }

    public Task<MetricLoadSnapshot> LoadAsync(MetricSelectionRequest request, CancellationToken cancellationToken = default)
    {
        return _gateway.LoadAsync(request, cancellationToken);
    }

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request)
    {
        return _planner.BuildProgram(snapshot, request);
    }

    public ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular)
    {
        return _planner.BuildMainProgram(snapshot, displayMode);
    }

    public ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot)
    {
        return _planner.BuildNormalizedProgram(snapshot);
    }

    public ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot)
    {
        return _planner.BuildDifferenceProgram(snapshot);
    }

    public ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot)
    {
        return _planner.BuildRatioProgram(snapshot);
    }
}
