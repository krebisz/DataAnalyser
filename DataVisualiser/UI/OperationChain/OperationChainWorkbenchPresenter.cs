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
                .SelectMany(dataset => dataset.Timeline.Select((timestamp, index) => new OperationChainDatasetRow(
                    dataset.Label,
                    timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    FormatValue(dataset.RawValues[index]),
                    FormatValue(dataset.SmoothedValues[index]))))
                .ToArray(),
            $"Plan: {result.Evidence.PlanSignature} | Trace: {result.Evidence.TraceSignature} | Contract: {result.Evidence.ContractSignature}");
    }

    private static string FormatValue(double value) =>
        double.IsNaN(value) ? "NaN" : value.ToString("G6");
}

internal sealed record OperationChainWorkbenchPresentation(
    string Summary,
    IReadOnlyList<OperationChainTraceRow> TraceRows,
    IReadOnlyList<OperationChainDatasetRow> DatasetRows,
    string Evidence);

internal sealed record OperationChainTraceRow(string StepLabel, string Detail);

internal sealed record OperationChainDatasetRow(string Dataset, string Timestamp, string Raw, string Smoothed);
