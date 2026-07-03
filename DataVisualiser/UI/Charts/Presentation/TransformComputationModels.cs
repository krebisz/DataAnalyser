using System.Globalization;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed record TransformInputGridLoadResult(
    MetricLoadSnapshot Snapshot,
    IReadOnlyList<TransformInputGrid> Inputs,
    string Summary);

internal sealed record TransformInputGrid(
    string Title,
    IReadOnlyList<TransformInputGridRow> Rows);

internal sealed record TransformInputGridRow(
    string Timestamp,
    string Value);

internal sealed record TransformResultGridRow(
    string Timestamp,
    string Raw,
    string Smoothed,
    string Series = "");

internal sealed record TransformComputationEvidence(
    string? SourceSignature,
    string? PlanSignature,
    string? TraceSignature,
    string? ContractSignature,
    IReadOnlyList<string> SourceSeriesSignatures,
    IReadOnlyList<string> DerivedDatasetIds,
    IReadOnlyDictionary<string, string> Metadata,
    string? FinalDatasetId,
    string? FinalDatasetLabel,
    IReadOnlyList<string> FinalDatasetSourceSeriesSignatures,
    SeriesOperationKind? OperationKind,
    string? OperationId,
    string? OperationSignature,
    IReadOnlyList<int> ConsumedInputIndexes)
{
    public static TransformComputationEvidence FromOperationChain(
        OperationChainResult result,
        DerivedDataset? finalDataset,
        OperationChainTraceEntry? finalTraceEntry)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new TransformComputationEvidence(
            result.Evidence.SourceSignature,
            result.Evidence.PlanSignature,
            result.Evidence.TraceSignature,
            result.Evidence.ContractSignature,
            result.Evidence.SourceSeriesSignatures,
            result.Evidence.DerivedDatasetIds,
            result.Evidence.Metadata,
            finalDataset?.Id,
            finalDataset?.Label,
            finalDataset?.SourceSeriesSignatures ?? [],
            finalTraceEntry?.OperationKind,
            finalTraceEntry?.Metadata.TryGetValue(ConstructionMetadataKeys.OperationId, out var operationId) == true ? operationId : null,
            finalDataset?.OperationSignature,
            finalTraceEntry?.InputIndexes ?? []);
    }
}

internal sealed record TransformComputationResult(
    string Title,
    IReadOnlyList<TransformResultGridRow> Rows,
    string Summary)
{
    public string? Evidence { get; init; }
    public TransformCorrelationSummary? Correlation { get; init; }
    public IReadOnlyList<TransformInputGrid>? Inputs { get; init; }
    public MetricLoadSnapshot? InputSnapshot { get; init; }
    public MetricSelectionRequest? Selection { get; init; }
    public DerivedDataset? DerivedDataset { get; init; }
    public MetricLoadSnapshot? ComputedSnapshot { get; init; }
    public TransformComputationEvidence? ComputationEvidence { get; init; }
}

internal static class TransformInputGridPresenter
{
    public static TransformInputGridLoadResult Build(MetricLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var inputs = snapshot.Series
            .Select(series => new TransformInputGrid(
                series.Request.DisplayName,
                TransformGridRowProjector.BuildInputRows(series.RawData)))
            .ToArray();

        return new TransformInputGridLoadResult(
            snapshot,
            inputs,
            $"{inputs.Length} input series loaded for Operation Chain.");
    }

    public static TransformComputationResult BuildComputationResult(TransformInputGridLoadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var rows = result.Inputs
            .SelectMany(input => input.Rows.Select(row => new TransformResultGridRow(
                row.Timestamp,
                row.Value,
                string.Empty,
                input.Title)))
            .ToArray();

        return new TransformComputationResult(
            "Input Series",
            rows,
            result.Summary)
        {
            Evidence = $"Input source: {result.Snapshot.Signature}",
            Inputs = result.Inputs,
            InputSnapshot = result.Snapshot,
            Selection = result.Snapshot.Request
        };
    }
}

internal static class TransformComputationResultProjector
{
    public static TransformComputationResult FromOperationChainResult(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (ShouldProjectAllDatasets(result))
            return FromOperationChainDatasets(result);

        var dataset = result.DerivedDatasets.LastOrDefault()
            ?? throw new InvalidOperationException("Operation Chain did not produce a derived dataset.");
        var finalTraceEntry = result.Trace.Entries.LastOrDefault();
        var computedSeries = ComputedSeriesResult.FromDerivedDataset(dataset);
        var rows = TransformGridRowProjector.BuildComputedRows(computedSeries);

        return new TransformComputationResult(
            dataset.Label,
            rows,
            $"{dataset.Label}: {rows.Count} result points computed.")
        {
            Evidence = $"OperationChain trace: {result.Evidence.TraceSignature}; plan: {result.Evidence.PlanSignature}; contract: {result.Evidence.ContractSignature}",
            Selection = result.Request.Selection,
            DerivedDataset = dataset,
            ComputationEvidence = TransformComputationEvidence.FromOperationChain(result, dataset, finalTraceEntry)
        };
    }

    private static TransformComputationResult FromOperationChainDatasets(OperationChainResult result)
    {
        var snapshot = BuildComputedSnapshot(result);
        var inputs = TransformInputGridPresenter.Build(snapshot).Inputs;
        var rows = inputs
            .SelectMany(input => input.Rows.Select(row => new TransformResultGridRow(
                row.Timestamp,
                row.Value,
                string.Empty,
                input.Title)))
            .ToArray();
        var finalDataset = result.DerivedDatasets.LastOrDefault();
        var finalTraceEntry = result.Trace.Entries.LastOrDefault();
        var mode = finalDataset?.Metadata.TryGetValue("NormalizationMode", out var normalizationMode) == true
            ? normalizationMode
            : null;

        return new TransformComputationResult(
            result.Request.Title,
            rows,
            $"{result.Request.Title}: {rows.Length} result points computed across {result.DerivedDatasets.Count} series.")
        {
            Evidence = $"OperationChain trace: {result.Evidence.TraceSignature}; plan: {result.Evidence.PlanSignature}; contract: {result.Evidence.ContractSignature}; mode: {mode}",
            Selection = result.Request.Selection,
            ComputedSnapshot = snapshot,
            ComputationEvidence = TransformComputationEvidence.FromOperationChain(result, finalDataset, finalTraceEntry)
        };
    }

    private static bool ShouldProjectAllDatasets(OperationChainResult result) =>
        result.DerivedDatasets.Count > 0 &&
        result.Trace.Entries.Count == result.DerivedDatasets.Count &&
        result.Trace.Entries.All(entry => entry.OperationKind == SeriesOperationKind.Normalize) &&
        result.DerivedDatasets.All(dataset => dataset.Metadata.ContainsKey("NormalizationMode"));

    private static MetricLoadSnapshot BuildComputedSnapshot(OperationChainResult result)
    {
        var requests = result.DerivedDatasets
            .Select(dataset => new MetricSeriesRequest(
                dataset.Label,
                null,
                dataset.Label,
                null))
            .ToArray();
        var selection = new MetricSelectionRequest(
            "Operation Chain",
            requests,
            result.Request.Selection.From,
            result.Request.Selection.To,
            result.Request.Selection.ResolutionTableName);
        var snapshots = result.DerivedDatasets
            .Zip(requests, (dataset, request) => new MetricSeriesSnapshot(
                request,
                ProjectMetricData(dataset),
                CanonicalSeries: null))
            .ToArray();

        return new MetricLoadSnapshot(selection, snapshots, DateTime.UtcNow);
    }

    private static IReadOnlyList<MetricData> ProjectMetricData(DerivedDataset dataset)
    {
        var unit = dataset.Metadata.TryGetValue("NormalizationMode", out var mode) &&
                   (string.Equals(mode, "PercentageOfMax", StringComparison.Ordinal) ||
                    string.Equals(mode, "RelativeToMax", StringComparison.Ordinal))
            ? "%"
            : string.Empty;
        var count = Math.Min(dataset.Timeline.Count, dataset.RawValues.Count);
        var data = new List<MetricData>(count);
        for (var index = 0; index < count; index++)
        {
            var value = dataset.RawValues[index];
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            data.Add(new MetricData
            {
                NormalizedTimestamp = dataset.Timeline[index],
                Value = Convert.ToDecimal(value),
                Unit = unit,
                Provider = "Transform"
            });
        }

        return data;
    }
}

internal static class TransformGridRowProjector
{
    public static IReadOnlyList<TransformInputGridRow> BuildInputRows(IReadOnlyList<MetricData> data) =>
        TransformGridDataProjector.ProjectMetricRows(data)
            .Select(row => new TransformInputGridRow(row.Timestamp, row.Value))
            .ToArray();

    public static IReadOnlyList<TransformResultGridRow> BuildComputedRows(ComputedSeriesResult result, string series = "")
    {
        ArgumentNullException.ThrowIfNull(result);

        return TransformGridDataProjector.ProjectComputedRows(result)
            .Select(row => new TransformResultGridRow(
                row.Timestamp,
                row.Raw,
                row.Smoothed,
                series))
            .ToArray();
    }
}

internal static class TransformCorrelationGridPresenter
{
    public static TransformComputationResult Build(
        TransformCorrelationSummary summary,
        string sourceSignature)
    {
        var rows = new[]
        {
            new TransformResultGridRow("Correlation", FormatValue(summary.Correlation), string.Empty),
            new TransformResultGridRow("95% CI lower", FormatValue(summary.ConfidenceLower), string.Empty),
            new TransformResultGridRow("95% CI upper", FormatValue(summary.ConfidenceUpper), string.Empty),
            new TransformResultGridRow("Sample count", summary.SampleCount.ToString(CultureInfo.InvariantCulture), string.Empty)
        };

        return new TransformComputationResult(
            summary.Label,
            rows,
            $"{summary.Label}: r={FormatValue(summary.Correlation)}, 95% CI [{FormatValue(summary.ConfidenceLower)}, {FormatValue(summary.ConfidenceUpper)}], n={summary.SampleCount}.")
        {
            Evidence = $"Correlation source: {sourceSignature}; {summary.SourceLabel} -> {summary.TargetLabel}",
            Correlation = summary
        };
    }

    private static string FormatValue(double value) =>
        double.IsNaN(value) || double.IsInfinity(value)
            ? string.Empty
            : value.ToString("F4", CultureInfo.InvariantCulture);
}

internal static class TransformWorkbenchPresenter
{
    public static TransformWorkbenchPresentation Build(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new TransformWorkbenchPresentation(
            $"{result.DerivedDatasets.Count} derived dataset(s) from {result.Plan.SourceSeriesSignatures.Count} source series.",
            result.Trace.Entries
                .Select(entry => new TransformTraceRow(
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

internal sealed record TransformWorkbenchPresentation(
    string Summary,
    IReadOnlyList<TransformTraceRow> TraceRows,
    IReadOnlyList<TransformResultGridRow> DatasetRows,
    string Evidence);

internal sealed record TransformTraceRow(string StepLabel, string Detail);
