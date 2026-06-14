using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Export;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.OperationChain;

internal sealed class OperationChainEvidenceExportService(
    EvidenceExportWriter writer,
    IEvidenceExportPathResolver pathResolver)
{
    private const string ExportFileNamePrefix = "operation-chain";

    public OperationChainEvidenceExportResult Export(
        OperationChainEvidenceExportSnapshot snapshot,
        DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var payload = OperationChainEvidenceExportPayload.FromSnapshot(snapshot, utcNow);
        var result = writer.Write(payload, pathResolver.ResolveDocumentsDirectory(), utcNow, ExportFileNamePrefix);
        return new OperationChainEvidenceExportResult(result.FilePath);
    }
}

internal sealed record OperationChainEvidenceExportResult(string FilePath);

internal sealed record OperationChainEvidenceExportSnapshot(
    IReadOnlyList<MetricSeriesRequest> Inputs,
    string OperationTag,
    string OperationLabel,
    DateTime From,
    DateTime To,
    string ResolutionTableName,
    OperationChainComputationGridResult Result);

internal sealed record OperationChainEvidenceExportPayload(
    DateTime ExportedAtUtc,
    string ExportScope,
    OperationChainEvidenceDiagnostics Diagnostics)
{
    public static OperationChainEvidenceExportPayload FromSnapshot(
        OperationChainEvidenceExportSnapshot snapshot,
        DateTime utcNow)
    {
        var result = snapshot.Result.Result;
        var consumedInputIndexes = ResolveConsumedInputIndexes(snapshot).ToArray();
        var consumedInputSet = consumedInputIndexes.ToHashSet();
        var finalDataset = result?.DerivedDatasets.LastOrDefault();
        var finalTraceEntry = result?.Trace.Entries.LastOrDefault();

        return new OperationChainEvidenceExportPayload(
            utcNow,
            "OperationChain",
            new OperationChainEvidenceDiagnostics(
                snapshot.OperationTag,
                snapshot.OperationLabel,
                finalTraceEntry?.OperationKind.ToString(),
                finalTraceEntry?.Metadata.TryGetValue(ConstructionMetadataKeys.OperationId, out var operationId) == true ? operationId : null,
                finalDataset?.OperationSignature,
                snapshot.ResolutionTableName,
                snapshot.From,
                snapshot.To,
                snapshot.Inputs
                    .Select((input, index) => new OperationChainInputEvidence(
                        input.MetricType,
                        input.Subtype,
                        input.DisplayMetricType,
                        input.DisplaySubtype,
                        input.DisplayName,
                        input.SignatureToken,
                        consumedInputSet.Contains(index)))
                    .ToArray(),
                snapshot.Result.Title,
                snapshot.Result.Summary,
                snapshot.Result.Evidence,
                OperationChainCorrelationEvidence.FromSummary(snapshot.Result.Correlation),
                result?.Evidence.SourceSignature,
                result?.Evidence.PlanSignature,
                result?.Evidence.TraceSignature,
                result?.Evidence.ContractSignature,
                result?.Evidence.SourceSeriesSignatures ?? [],
                result?.Evidence.DerivedDatasetIds ?? [],
                result?.Evidence.Metadata ?? new Dictionary<string, string>(),
                finalDataset?.Id,
                finalDataset?.Label,
                finalDataset?.SourceSeriesSignatures ?? [],
                consumedInputIndexes,
                snapshot.Result.Rows.Count,
                snapshot.Result.Rows));
    }

    private static IReadOnlyList<int> ResolveConsumedInputIndexes(OperationChainEvidenceExportSnapshot snapshot)
    {
        var traceIndexes = snapshot.Result.Result?.Trace.Entries.LastOrDefault()?.InputIndexes;
        if (traceIndexes is { Count: > 0 })
            return traceIndexes;

        return snapshot.OperationTag switch
        {
            "Log" or "Sqrt" => [0],
            "Add" or "Subtract" or "Divide" or "Correlation" => [0, 1],
            "Sum3" => [0, 1, 2],
            "Sum3Correlation" => [0, 1, 2, 3],
            _ => Enumerable.Range(0, snapshot.Inputs.Count).ToArray()
        };
    }
}

internal sealed record OperationChainEvidenceDiagnostics(
    string OperationTag,
    string OperationLabel,
    string? OperationKind,
    string? OperationId,
    string? OperationSignature,
    string ResolutionTableName,
    DateTime From,
    DateTime To,
    IReadOnlyList<OperationChainInputEvidence> Inputs,
    string ResultTitle,
    string Summary,
    string? Evidence,
    OperationChainCorrelationEvidence? Correlation,
    string? SourceSignature,
    string? PlanSignature,
    string? TraceSignature,
    string? ContractSignature,
    IReadOnlyList<string> SourceSeriesSignatures,
    IReadOnlyList<string> DerivedDatasetIds,
    IReadOnlyDictionary<string, string> OperationChainEvidenceMetadata,
    string? FinalDatasetId,
    string? FinalDatasetLabel,
    IReadOnlyList<string> FinalDatasetSourceSeriesSignatures,
    IReadOnlyList<int> ConsumedInputIndexes,
    int ResultRowCount,
    IReadOnlyList<OperationChainResultGridRow> ResultRows);

internal sealed record OperationChainCorrelationEvidence(
    string Label,
    string SourceLabel,
    string TargetLabel,
    double Correlation,
    double ConfidenceLower,
    double ConfidenceUpper,
    int SampleCount)
{
    public static OperationChainCorrelationEvidence? FromSummary(TransformCorrelationSummary? summary) =>
        summary == null
            ? null
            : new OperationChainCorrelationEvidence(
                summary.Label,
                summary.SourceLabel,
                summary.TargetLabel,
                summary.Correlation,
                summary.ConfidenceLower,
                summary.ConfidenceUpper,
                summary.SampleCount);
}

internal sealed record OperationChainInputEvidence(
    string MetricType,
    string? Subtype,
    string? DisplayMetricType,
    string? DisplaySubtype,
    string DisplayName,
    string SignatureToken,
    bool IsConsumed);
