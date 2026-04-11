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
        var (snapshot, presentation) = GetLoadedSnapshotAndPresentation();
        return _engine.BuildMainProgram(snapshot, presentation.MainChartDisplayMode);
    }

    public ChartProgram BuildNormalizedProgram()
    {
        var (snapshot, _) = GetLoadedSnapshotAndPresentation();
        return _engine.BuildNormalizedProgram(snapshot);
    }

    public ChartProgram BuildDifferenceProgram()
    {
        var (snapshot, _) = GetLoadedSnapshotAndPresentation();
        return _engine.BuildDifferenceProgram(snapshot);
    }

    public ChartProgram BuildRatioProgram()
    {
        var (snapshot, _) = GetLoadedSnapshotAndPresentation();
        return _engine.BuildRatioProgram(snapshot);
    }

    private (MetricLoadSnapshot Snapshot, PresentationState Presentation) GetLoadedSnapshotAndPresentation()
    {
        lock (_sync)
        {
            if (_state.Load.Snapshot == null)
                throw new InvalidOperationException("No loaded snapshot is available.");

            return (_state.Load.Snapshot, _state.Presentation);
        }
    }
}
