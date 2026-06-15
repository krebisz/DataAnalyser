using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformOperationChainWorkbenchPresenter
{
    public static TransformOperationChainWorkbenchPresentation Build(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new TransformOperationChainWorkbenchPresentation(
            $"{result.DerivedDatasets.Count} derived dataset(s) from {result.Plan.SourceSeriesSignatures.Count} source series.",
            result.Trace.Entries
                .Select(entry => new TransformOperationChainTraceRow(
                    $"Step {entry.StepIndex + 1}",
                    $"{entry.OperationKind} -> {entry.OutputDatasetId}; lossiness: {entry.Lossiness}"))
                .ToArray(),
            result.DerivedDatasets
                .SelectMany(dataset => TransformGridRowProjector.BuildComputedRows(
                    ComputedSeriesResult.FromDerivedDataset(dataset),
                    dataset.Label))
                .ToArray(),
            $"Plan: {result.Evidence.PlanSignature} | Trace: {result.Evidence.TraceSignature} | Contract: {result.Evidence.ContractSignature}");
    }
}

internal sealed record TransformOperationChainWorkbenchPresentation(
    string Summary,
    IReadOnlyList<TransformOperationChainTraceRow> TraceRows,
    IReadOnlyList<TransformResultGridRow> DatasetRows,
    string Evidence);

internal sealed record TransformOperationChainTraceRow(string StepLabel, string Detail);
