using DataVisualiser.UI.Export;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformEvidenceExportService(
    EvidenceExportWriter writer,
    IEvidenceExportPathResolver pathResolver)
{
    private const string ExportFileNamePrefix = "operation-chain";
    private const string ExportScope = "OperationChain";

    public TransformEvidenceExportResult Export(
        TransformEvidenceExportSnapshot snapshot,
        DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var payload = TransformEvidenceExportPayload.FromSnapshot(snapshot, utcNow, ExportScope);
        var result = writer.Write(payload, pathResolver.ResolveDocumentsDirectory(), utcNow, ExportFileNamePrefix);
        return new TransformEvidenceExportResult(result.FilePath);
    }
}

internal sealed record TransformEvidenceExportResult(string FilePath);

internal sealed record TransformEvidenceExportSnapshot(
    IReadOnlyList<MetricSeriesRequest> Inputs,
    string OperationTag,
    string OperationLabel,
    DateTime From,
    DateTime To,
    string ResolutionTableName,
    TransformComputationResult Result);

internal sealed record TransformEvidenceExportPayload(
    DateTime ExportedAtUtc,
    string ExportScope,
    TransformEvidenceDiagnostics Diagnostics)
{
    public static TransformEvidenceExportPayload FromSnapshot(
        TransformEvidenceExportSnapshot snapshot,
        DateTime utcNow,
        string exportScope)
    {
        var computationEvidence = snapshot.Result.ComputationEvidence;
        var consumedInputIndexes = ResolveConsumedInputIndexes(snapshot).ToArray();
        var consumedInputSet = consumedInputIndexes.ToHashSet();

        return new TransformEvidenceExportPayload(
            utcNow,
            exportScope,
            new TransformEvidenceDiagnostics(
                snapshot.OperationTag,
                snapshot.OperationLabel,
                computationEvidence?.OperationKind?.ToString(),
                computationEvidence?.OperationId,
                computationEvidence?.OperationSignature,
                snapshot.ResolutionTableName,
                snapshot.From,
                snapshot.To,
                snapshot.Inputs
                    .Select((input, index) => new TransformInputEvidence(
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
                TransformCorrelationEvidence.FromSummary(snapshot.Result.Correlation),
                computationEvidence?.SourceSignature,
                computationEvidence?.PlanSignature,
                computationEvidence?.TraceSignature,
                computationEvidence?.ContractSignature,
                computationEvidence?.SourceSeriesSignatures ?? [],
                computationEvidence?.DerivedDatasetIds ?? [],
                computationEvidence?.Metadata ?? new Dictionary<string, string>(),
                computationEvidence?.FinalDatasetId,
                computationEvidence?.FinalDatasetLabel,
                computationEvidence?.FinalDatasetSourceSeriesSignatures ?? [],
                consumedInputIndexes,
                snapshot.Result.Rows.Count,
                snapshot.Result.Rows));
    }

    private static IReadOnlyList<int> ResolveConsumedInputIndexes(TransformEvidenceExportSnapshot snapshot)
    {
        var evidenceIndexes = snapshot.Result.ComputationEvidence?.ConsumedInputIndexes;
        if (evidenceIndexes is { Count: > 0 })
            return evidenceIndexes;

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

internal sealed record TransformEvidenceDiagnostics(
    string OperationTag,
    string OperationLabel,
    string? OperationKind,
    string? OperationId,
    string? OperationSignature,
    string ResolutionTableName,
    DateTime From,
    DateTime To,
    IReadOnlyList<TransformInputEvidence> Inputs,
    string ResultTitle,
    string Summary,
    string? Evidence,
    TransformCorrelationEvidence? Correlation,
    string? SourceSignature,
    string? PlanSignature,
    string? TraceSignature,
    string? ContractSignature,
    IReadOnlyList<string> SourceSeriesSignatures,
    IReadOnlyList<string> DerivedDatasetIds,
    IReadOnlyDictionary<string, string> ComputationEvidenceMetadata,
    string? FinalDatasetId,
    string? FinalDatasetLabel,
    IReadOnlyList<string> FinalDatasetSourceSeriesSignatures,
    IReadOnlyList<int> ConsumedInputIndexes,
    int ResultRowCount,
    IReadOnlyList<TransformResultGridRow> ResultRows);

internal sealed record TransformCorrelationEvidence(
    string Label,
    string SourceLabel,
    string TargetLabel,
    double Correlation,
    double ConfidenceLower,
    double ConfidenceUpper,
    int SampleCount)
{
    public static TransformCorrelationEvidence? FromSummary(TransformCorrelationSummary? summary) =>
        summary == null
            ? null
            : new TransformCorrelationEvidence(
                summary.Label,
                summary.SourceLabel,
                summary.TargetLabel,
                summary.Correlation,
                summary.ConfidenceLower,
                summary.ConfidenceUpper,
                summary.SampleCount);
}

internal sealed record TransformInputEvidence(
    string MetricType,
    string? Subtype,
    string? DisplayMetricType,
    string? DisplaySubtype,
    string DisplayName,
    string SignatureToken,
    bool IsConsumed);
