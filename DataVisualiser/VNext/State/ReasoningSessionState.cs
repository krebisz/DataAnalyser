namespace DataVisualiser.VNext.State;

public sealed record ReasoningSessionState(
    SelectionState Selection,
    LoadState Load,
    PresentationState Presentation,
    WorkflowState Workflow)
{
    public static ReasoningSessionState Empty { get; } = new(
        SelectionState.Empty,
        LoadState.Empty,
        PresentationState.Default,
        WorkflowState.Default);
}
