using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.State;

namespace DataVisualiser.VNext.Application;

public sealed class ReasoningSessionCoordinator
{
    private readonly IReasoningEngine _engine;
    private readonly object _sync = new();
    private ReasoningSessionState _state = ReasoningSessionState.Empty;

    public ReasoningSessionCoordinator(IReasoningEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public ReasoningSessionState State
    {
        get
        {
            lock (_sync)
                return _state;
        }
    }

    public void ApplyMetricType(string? metricType)
    {
        lock (_sync)
            _state = ReasoningSessionTransitions.ApplyMetricTypeChange(_state, metricType);
    }

    public void ApplySeries(IReadOnlyList<MetricSeriesRequest> series)
    {
        lock (_sync)
            _state = ReasoningSessionTransitions.ApplySeriesSelection(_state, series);
    }

    public void ApplyDateRange(DateTime? from, DateTime? to)
    {
        lock (_sync)
            _state = ReasoningSessionTransitions.ApplyDateRange(_state, from, to);
    }

    public void ApplyResolution(string? resolutionTableName)
    {
        lock (_sync)
            _state = ReasoningSessionTransitions.ApplyResolution(_state, resolutionTableName);
    }

    public void ApplyMainDisplayMode(ChartDisplayMode displayMode)
    {
        lock (_sync)
            _state = _state with
            {
                Presentation = _state.Presentation with
                {
                    MainChartDisplayMode = displayMode
                }
            };
    }

    public void ApplyWorkflowPlan(IReadOnlyList<SeriesOperationRequest> plannedOperations, string? consumerIntent = null)
    {
        lock (_sync)
            _state = ReasoningSessionTransitions.ApplyWorkflowPlan(_state, plannedOperations, consumerIntent);
    }

    public void ApplyWorkflowPlan(WorkflowPlanRequest workflowPlan)
    {
        lock (_sync)
            _state = ReasoningSessionTransitions.ApplyWorkflowPlan(_state, workflowPlan);
    }

    public void Clear()
    {
        lock (_sync)
            _state = ReasoningSessionTransitions.ApplyClear(_state);
    }

    public async Task<MetricLoadSnapshot> LoadAsync(CancellationToken cancellationToken = default)
    {
        MetricSelectionRequest request;

        lock (_sync)
        {
            if (!_state.Selection.IsComplete)
                throw new InvalidOperationException("Selection is incomplete and cannot be loaded.");

            _state = ReasoningSessionTransitions.ApplyLoadPending(_state);
            request = _state.Selection.ToRequest();
        }

        try
        {
            var snapshot = await _engine.LoadAsync(request, cancellationToken);
            lock (_sync)
                _state = ReasoningSessionTransitions.ApplyLoadSuccess(_state, snapshot);

            return snapshot;
        }
        catch (Exception ex)
        {
            lock (_sync)
                _state = ReasoningSessionTransitions.ApplyLoadFailure(_state, ex.Message);
            throw;
        }
    }

    public ChartProgram BuildMainProgram()
    {
        var (_, presentation) = GetLoadedSnapshotAndPresentation();
        return BuildProgram(ChartProgramRequest.MainProgram(presentation.MainChartDisplayMode));
    }

    public ChartProgram BuildNormalizedProgram()
    {
        return BuildProgram(ChartProgramRequest.Normalized());
    }

    public ChartProgram BuildDifferenceProgram()
    {
        return BuildProgram(ChartProgramRequest.Difference());
    }

    public ChartProgram BuildRatioProgram()
    {
        return BuildProgram(ChartProgramRequest.Ratio());
    }

    public ChartProgram BuildWorkflowProgram(string? title = null)
    {
        var (snapshot, presentation, workflow) = GetLoadedSnapshotPresentationAndWorkflow();
        if (workflow.PlannedOperations.Count == 0)
            throw new InvalidOperationException("No workflow operations are available.");

        var request = ChartProgramRequest.Transform(
            title ?? workflow.TitleOverride ?? $"{snapshot.Request.MetricType ?? "Derived"} transform",
            workflow.PlannedOperations,
            presentation.MainChartDisplayMode);

        return BuildProgram(request);
    }

    public ChartProgram BuildProgram(ChartProgramRequest request)
    {
        var (snapshot, _) = GetLoadedSnapshotAndPresentation();
        return _engine.BuildProgram(snapshot, request);
    }

    private (MetricLoadSnapshot Snapshot, PresentationState Presentation) GetLoadedSnapshotAndPresentation()
    {
        var (snapshot, presentation, _) = GetLoadedSnapshotPresentationAndWorkflow();
        return (snapshot, presentation);
    }

    private (MetricLoadSnapshot Snapshot, PresentationState Presentation, WorkflowState Workflow) GetLoadedSnapshotPresentationAndWorkflow()
    {
        lock (_sync)
        {
            if (_state.Load.Snapshot == null)
                throw new InvalidOperationException("No loaded snapshot is available.");

            return (_state.Load.Snapshot, _state.Presentation, _state.Workflow);
        }
    }
}
