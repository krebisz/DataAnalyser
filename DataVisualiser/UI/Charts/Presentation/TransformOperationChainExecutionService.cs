using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformOperationChainExecutionService(MetricLoadSnapshotGateway gateway)
{
    public async Task<OperationChainResult> ExecuteAsync(
        MetricSelectionRequest selection,
        IReadOnlyList<OperationChainStep> steps,
        string title,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(steps);
        if (steps.Count == 0)
            throw new ArgumentException("At least one transform operation-chain step is required.", nameof(steps));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Transform operation-chain title cannot be empty.", nameof(title));

        var executor = new OperationChainExecutor(new TransformSnapshotReasoningEngine(gateway));
        return await executor.ExecuteAsync(
            new OperationChainRequest(selection, steps, title: title),
            cancellationToken);
    }
}
