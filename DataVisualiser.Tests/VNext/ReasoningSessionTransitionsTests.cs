using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.State;

namespace DataVisualiser.Tests.VNext;

public sealed class ReasoningSessionTransitionsTests
{
    [Fact]
    public void ApplyMetricTypeChange_ShouldClearSeriesAndInvalidateLoad()
    {
        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var state = new ReasoningSessionState(
            new SelectionState(request.MetricType, request.Series, request.From, request.To, request.ResolutionTableName),
            new LoadState(LoadLifecycle.Loaded, new MetricLoadSnapshot(request, Array.Empty<MetricSeriesSnapshot>(), DateTime.UtcNow), null),
            PresentationState.Default,
            WorkflowState.Default);

        var updated = ReasoningSessionTransitions.ApplyMetricTypeChange(state, "SkinTemperature");

        Assert.Equal("SkinTemperature", updated.Selection.MetricType);
        Assert.Empty(updated.Selection.Series);
        Assert.Equal(LoadLifecycle.Empty, updated.Load.Lifecycle);
    }

    [Fact]
    public void ApplySeriesSelection_ShouldInvalidateMismatchedSnapshot()
    {
        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var snapshot = new MetricLoadSnapshot(request, Array.Empty<MetricSeriesSnapshot>(), DateTime.UtcNow);
        var state = new ReasoningSessionState(
            new SelectionState(request.MetricType, request.Series, request.From, request.To, request.ResolutionTableName),
            new LoadState(LoadLifecycle.Loaded, snapshot, null),
            PresentationState.Default,
            WorkflowState.Default);

        var updated = ReasoningSessionTransitions.ApplySeriesSelection(
            state,
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")]);

        Assert.Equal(LoadLifecycle.Empty, updated.Load.Lifecycle);
    }

    [Fact]
    public void ApplyClear_ShouldResetSelectionAndLoad()
    {
        var state = new ReasoningSessionState(
            new SelectionState("Weight", [new MetricSeriesRequest("Weight", "morning")], DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, "HealthMetrics"),
            new LoadState(LoadLifecycle.Loaded, null, null),
            PresentationState.Default,
            WorkflowState.Default);

        var updated = ReasoningSessionTransitions.ApplyClear(state);

        Assert.Equal(LoadLifecycle.Empty, updated.Load.Lifecycle);
        Assert.Null(updated.Selection.MetricType);
        Assert.Empty(updated.Selection.Series);
    }

    [Fact]
    public void ApplyWorkflowPlan_ShouldReplaceWorkflowOperationsAndIntent()
    {
        var state = ReasoningSessionState.Empty;
        var workflowPlan = new WorkflowPlanRequest(
        [
            SeriesOperationRequest.Normalize(0, "morning-normalized", "Morning normalized"),
            SeriesOperationRequest.Difference(0, 1, "Delta")
        ],
        "transform",
        "Weight transform");

        var updated = ReasoningSessionTransitions.ApplyWorkflowPlan(state, workflowPlan);

        Assert.Equal("transform", updated.Workflow.ConsumerIntent);
        Assert.Equal("Weight transform", updated.Workflow.TitleOverride);
        Assert.Equal(2, updated.Workflow.PlannedOperations.Count);
        Assert.Equal(SeriesOperationKind.Normalize, updated.Workflow.PlannedOperations[0].Kind);
        Assert.Equal(SeriesOperationKind.Difference, updated.Workflow.PlannedOperations[1].Kind);
    }
}
