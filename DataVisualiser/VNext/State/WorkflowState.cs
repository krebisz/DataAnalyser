using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.State;

public sealed record WorkflowState(WorkflowPlanRequest? Plan)
{
    public string? ConsumerIntent => Plan?.ConsumerIntent;
    public string? TitleOverride => Plan?.TitleOverride;
    public IReadOnlyList<SeriesOperationRequest> PlannedOperations => Plan?.PlannedOperations ?? Array.Empty<SeriesOperationRequest>();

    public static WorkflowState Default { get; } = new((WorkflowPlanRequest?)null);
}
