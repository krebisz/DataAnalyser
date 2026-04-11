using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.State;

public static class ReasoningSessionTransitions
{
    public static ReasoningSessionState ApplyMetricTypeChange(ReasoningSessionState state, string? metricType)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state with
        {
            Selection = state.Selection with
            {
                MetricType = metricType,
                Series = Array.Empty<MetricSeriesRequest>()
            },
            Load = LoadState.Empty
        };
    }

    public static ReasoningSessionState ApplySeriesSelection(ReasoningSessionState state, IReadOnlyList<MetricSeriesRequest> series)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(series);

        var nextSelection = state.Selection with { Series = series.ToArray() };
        return state with
        {
            Selection = nextSelection,
            Load = InvalidateIfSelectionDrifted(state.Load, nextSelection)
        };
    }

    public static ReasoningSessionState ApplyDateRange(ReasoningSessionState state, DateTime? from, DateTime? to)
    {
        ArgumentNullException.ThrowIfNull(state);

        var nextSelection = state.Selection with { From = from, To = to };
        return state with
        {
            Selection = nextSelection,
            Load = InvalidateIfSelectionDrifted(state.Load, nextSelection)
        };
    }

    public static ReasoningSessionState ApplyResolution(ReasoningSessionState state, string? resolutionTableName)
    {
        ArgumentNullException.ThrowIfNull(state);

        var nextSelection = state.Selection with { ResolutionTableName = resolutionTableName };
        return state with
        {
            Selection = nextSelection,
            Load = InvalidateIfSelectionDrifted(state.Load, nextSelection)
        };
    }

    public static ReasoningSessionState ApplyLoadPending(ReasoningSessionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state with
        {
            Load = new LoadState(LoadLifecycle.Pending, state.Load.Snapshot, null)
        };
    }

    public static ReasoningSessionState ApplyLoadSuccess(ReasoningSessionState state, MetricLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(snapshot);

        return state with
        {
            Load = new LoadState(LoadLifecycle.Loaded, snapshot, null)
        };
    }

    public static ReasoningSessionState ApplyLoadFailure(ReasoningSessionState state, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(state);

        return state with
        {
            Load = new LoadState(LoadLifecycle.Failed, null, errorMessage)
        };
    }

    public static ReasoningSessionState ApplyClear(ReasoningSessionState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return ReasoningSessionState.Empty with
        {
            Presentation = state.Presentation,
            Workflow = state.Workflow
        };
    }

    private static LoadState InvalidateIfSelectionDrifted(LoadState load, SelectionState selection)
    {
        if (load.Snapshot == null || !selection.IsComplete)
            return LoadState.Empty;

        return string.Equals(load.Snapshot.Signature, selection.ToRequest().Signature, StringComparison.Ordinal)
            ? load
            : LoadState.Empty;
    }
}
