namespace DataVisualiser.VNext.Contracts;

public enum OperationChainPlanningStatus
{
    WithinBudget = 0,
    ExceedsBudget = 1
}

public sealed record OperationChainPlanningBudget(
    int MaxSteps = 16,
    int MaxInputReferences = 64,
    int MaxWorkingSetSize = 64);

public sealed record OperationChainPlanningAssessment(
    OperationChainPlanningStatus Status,
    int StepCount,
    int InputReferenceCount,
    int WorkingSetSize,
    string ReplaySignature,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    public IReadOnlyDictionary<string, string> ResolvedMetadata { get; } = Metadata == null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(Metadata);
}

public static class OperationChainPlanningRules
{
    public static OperationChainPlanningAssessment Assess(
        MetricSelectionRequest selection,
        IReadOnlyList<OperationChainStep> steps,
        OperationChainPlanningBudget? budget = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(steps);

        budget ??= new OperationChainPlanningBudget();
        var stepCount = steps.Count;
        var inputReferenceCount = steps.Sum(step => step.Operation.InputIndexes.Count);
        var workingSetSize = selection.Series.Count + stepCount;
        var status = stepCount <= budget.MaxSteps &&
                     inputReferenceCount <= budget.MaxInputReferences &&
                     workingSetSize <= budget.MaxWorkingSetSize
            ? OperationChainPlanningStatus.WithinBudget
            : OperationChainPlanningStatus.ExceedsBudget;

        return new OperationChainPlanningAssessment(
            status,
            stepCount,
            inputReferenceCount,
            workingSetSize,
            BuildReplaySignature(selection, steps),
            new Dictionary<string, string>
            {
                ["MaxSteps"] = budget.MaxSteps.ToString(),
                ["MaxInputReferences"] = budget.MaxInputReferences.ToString(),
                ["MaxWorkingSetSize"] = budget.MaxWorkingSetSize.ToString(),
                ["Authoritative"] = "False"
            });
    }

    private static string BuildReplaySignature(
        MetricSelectionRequest selection,
        IReadOnlyList<OperationChainStep> steps)
    {
        return $"{selection.Signature}::{string.Join("|", steps.Select(step => step.Signature))}";
    }
}
