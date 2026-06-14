using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.OperationChain;

internal static class OperationChainWorkbenchPresenter
{
    public static OperationChainWorkbenchPresentation Build(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new OperationChainWorkbenchPresentation(
            $"{result.DerivedDatasets.Count} derived dataset(s) from {result.Plan.SourceSeriesSignatures.Count} source series.",
            result.Trace.Entries
                .Select(entry => new OperationChainTraceRow(
                    $"Step {entry.StepIndex + 1}",
                    $"{entry.OperationKind} -> {entry.OutputDatasetId}; lossiness: {entry.Lossiness}"))
                .ToArray(),
            result.DerivedDatasets
                .SelectMany(dataset => OperationChainGridRowProjector.BuildComputedRows(
                    ComputedSeriesResult.FromDerivedDataset(dataset),
                    dataset.Label))
                .ToArray(),
            $"Plan: {result.Evidence.PlanSignature} | Trace: {result.Evidence.TraceSignature} | Contract: {result.Evidence.ContractSignature}");
    }
}

internal sealed record OperationChainWorkbenchPresentation(
    string Summary,
    IReadOnlyList<OperationChainTraceRow> TraceRows,
    IReadOnlyList<OperationChainResultGridRow> DatasetRows,
    string Evidence);

internal sealed record OperationChainTraceRow(string StepLabel, string Detail);
