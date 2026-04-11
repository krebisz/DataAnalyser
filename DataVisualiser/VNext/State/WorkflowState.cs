namespace DataVisualiser.VNext.State;

public sealed record WorkflowState(
    string? TransformOperation,
    string? ConsumerIntent)
{
    public static WorkflowState Default { get; } = new(null, null);
}
