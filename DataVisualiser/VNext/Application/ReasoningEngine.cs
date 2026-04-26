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

    public async Task<AnalyticalExecutionResult> ExecuteAsync(AnalyticalIntent intent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);

        var snapshot = await LoadAsync(intent.Selection, cancellationToken);
        var program = BuildProgram(snapshot, intent);
        return new AnalyticalExecutionResult(intent, snapshot, program);
    }

    public async Task<AnalyticalResultSet> ExecuteAsync(AnalyticalIntentSet intentSet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intentSet);

        var snapshot = await LoadAsync(intentSet.Selection, cancellationToken);
        var results = intentSet.Intents
            .Select(intent => new AnalyticalExecutionResult(intent, snapshot, BuildProgram(snapshot, intent)))
            .ToArray();

        return new AnalyticalResultSet(intentSet.Selection, results);
    }

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request)
    {
        return _planner.BuildProgram(snapshot, request);
    }

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, AnalyticalIntent intent)
    {
        return _planner.BuildProgram(snapshot, intent);
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
