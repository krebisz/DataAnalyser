namespace DataVisualiser.VNext.Contracts;

public sealed record WorkflowPlanRequest
{
    public WorkflowPlanRequest(
        IReadOnlyList<SeriesOperationRequest> plannedOperations,
        string? consumerIntent = null,
        string? titleOverride = null)
    {
        PlannedOperations = plannedOperations?.ToArray() ?? throw new ArgumentNullException(nameof(plannedOperations));
        ConsumerIntent = consumerIntent;
        TitleOverride = titleOverride;
    }

    public IReadOnlyList<SeriesOperationRequest> PlannedOperations { get; }
    public string? ConsumerIntent { get; }
    public string? TitleOverride { get; }
}
