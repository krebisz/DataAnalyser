using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost.Evidence;

internal sealed class EvidenceTransformParityEvaluator
{
    private readonly Func<string?> _getSelectedTransformOperation;
    private readonly EvidenceTransformParityDataResolver _dataResolver;

    internal EvidenceTransformParityEvaluator(
        MetricSelectionService metricSelectionService,
        Func<string?> getSelectedTransformOperation)
    {
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _getSelectedTransformOperation = getSelectedTransformOperation ?? throw new ArgumentNullException(nameof(getSelectedTransformOperation));
        _dataResolver = new EvidenceTransformParityDataResolver(metricSelectionService);
    }

    internal async Task<TransformParitySnapshot> BuildAsync(ChartState chartState, MetricState metricState, ChartDataContext? ctx)
    {
        if (ctx == null)
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No chart context available" };

        var operation = _getSelectedTransformOperation();
        if (string.IsNullOrWhiteSpace(operation))
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No transform operation selected" };

        var (primarySelection, secondarySelection) = EvidenceTransformParityDataResolver.ResolveSelections(chartState, ctx);
        var primaryData = await _dataResolver.ResolveAsync(metricState, ctx, primarySelection);
        if (primaryData == null || primaryData.Count == 0)
            return new TransformParitySnapshot { Status = "Unavailable", Reason = "No primary data available for transform" };

        var isUnary = EvidenceTransformParityComputer.IsUnaryTransform(operation);
        IReadOnlyList<MetricData>? secondaryData = null;
        if (!isUnary)
        {
            secondaryData = await _dataResolver.ResolveAsync(metricState, ctx, secondarySelection);
            if (secondaryData == null || secondaryData.Count == 0)
                return new TransformParitySnapshot { Status = "Unavailable", Reason = "No secondary data available for binary transform" };
        }

        var result = isUnary
            ? EvidenceTransformParityComputer.ComputeUnary(primaryData, operation)
            : EvidenceTransformParityComputer.ComputeBinary(primaryData, secondaryData!, operation);
        return new TransformParitySnapshot
        {
            Status = "Completed",
            Operation = operation,
            IsUnary = isUnary,
            ExpressionAvailable = result.ExpressionAvailable,
            LegacySamples = result.LegacySamples,
            NewSamples = result.NewSamples,
            Result = result.Result
        };
    }
}
