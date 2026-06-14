using System.Globalization;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.UI.OperationChain;

internal sealed class OperationChainInputGridLoadService
{
    private readonly MetricLoadSnapshotGateway _gateway;
    private readonly TransformInputDateRangeResolver? _dateRangeResolver;

    public OperationChainInputGridLoadService(MetricSelectionService metricSelectionService)
        : this(
            new MetricSelectionServiceSeriesLoader(metricSelectionService),
            new TransformInputDateRangeResolver((selection, tableName) =>
                metricSelectionService.LoadDateRangeAsync(selection.MetricType, selection.QuerySubtype, tableName)))
    {
    }

    internal OperationChainInputGridLoadService(
        IMetricSeriesLoader loader,
        TransformInputDateRangeResolver? dateRangeResolver = null)
    {
        ArgumentNullException.ThrowIfNull(loader);
        _gateway = new MetricLoadSnapshotGateway(loader);
        _dateRangeResolver = dateRangeResolver;
    }

    public async Task<OperationChainInputGridLoadResult> LoadAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one operation-chain input series is required.", nameof(series));

        var request = new MetricSelectionRequest(
            ResolveSelectionMetricType(series),
            series,
            from,
            to,
            resolutionTableName);

        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        return OperationChainInputGridPresenter.Build(snapshot);
    }

    public async Task<OperationChainComputationGridResult> ComputeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        string operationTag,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one operation-chain input series is required.", nameof(series));
        if (TransformSeriesOperationRequestMapper.IsPassThrough(operationTag))
            return OperationChainInputGridPresenter.BuildComputationResult(
                await LoadAsync(series, from, to, resolutionTableName, cancellationToken));

        if (!TransformSeriesOperationRequestMapper.TryCreateOperationChainStep(operationTag, series, out var step) || step == null)
            throw new InvalidOperationException($"Operation Chain operation '{operationTag}' is not valid for {series.Count} selected input series.");

        var request = new MetricSelectionRequest(
            ResolveSelectionMetricType(series),
            series,
            from,
            to,
            resolutionTableName);

        if (TransformCorrelationMapper.IsCorrelationOperation(operationTag))
            return await ComputeCorrelationAsync(request, operationTag, cancellationToken);

        var executor = new OperationChainExecutor(new TransformSnapshotReasoningEngine(_gateway));
        var result = await executor.ExecuteAsync(
            new OperationChainRequest(request, [step], title: step.Operation.Label),
            cancellationToken);

        return OperationChainResultGridPresenter.Build(result);
    }

    private async Task<OperationChainComputationGridResult> ComputeCorrelationAsync(
        MetricSelectionRequest request,
        string operationTag,
        CancellationToken cancellationToken)
    {
        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        var aligned = new TimeSeriesAlignmentKernel().Align(snapshot);
        var summary = TransformCorrelationMapper.Compute(aligned, operationTag);
        return OperationChainCorrelationGridPresenter.Build(summary, snapshot.Signature);
    }

    public Task<TransformInputDateRange?> ResolveDateRangeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (_dateRangeResolver == null)
            throw new InvalidOperationException("Operation Chain date-range resolution is not configured.");

        return _dateRangeResolver.ResolveAsync(series, resolutionTableName, cancellationToken);
    }

    private static string ResolveSelectionMetricType(IReadOnlyList<MetricSeriesRequest> series)
    {
        var distinct = series
            .Select(item => item.MetricType)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return distinct.Length == 1 ? distinct[0] : "Mixed";
    }
}

internal static class OperationChainInputGridPresenter
{
    public static OperationChainInputGridLoadResult Build(MetricLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var inputs = snapshot.Series
            .Select(series => new OperationChainInputGrid(
                series.Request.DisplayName,
                OperationChainGridRowProjector.BuildInputRows(series.RawData)))
            .ToArray();

        return new OperationChainInputGridLoadResult(
            snapshot,
            inputs,
            $"{inputs.Length} input series loaded for Operation Chain.");
    }

    public static OperationChainComputationGridResult BuildComputationResult(OperationChainInputGridLoadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var rows = result.Inputs
            .SelectMany(input => input.Rows.Select(row => new OperationChainResultGridRow(
                row.Timestamp,
                row.Value,
                string.Empty,
                input.Title)))
            .ToArray();

        return new OperationChainComputationGridResult(
            null,
            "Input Series",
            rows,
            result.Summary)
        {
            Evidence = $"Input source: {result.Snapshot.Signature}",
            Inputs = result.Inputs,
            InputSnapshot = result.Snapshot
        };
    }
}

internal sealed record OperationChainInputGridLoadResult(
    MetricLoadSnapshot Snapshot,
    IReadOnlyList<OperationChainInputGrid> Inputs,
    string Summary);

internal sealed record OperationChainInputGrid(
    string Title,
    IReadOnlyList<OperationChainInputGridRow> Rows);

internal sealed record OperationChainInputGridRow(
    string Timestamp,
    string Value);

internal static class OperationChainResultGridPresenter
{
    public static OperationChainComputationGridResult Build(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var dataset = result.DerivedDatasets.LastOrDefault()
            ?? throw new InvalidOperationException("Operation Chain did not produce a derived dataset.");
        var computedSeries = ComputedSeriesResult.FromDerivedDataset(dataset);
        var rows = OperationChainGridRowProjector.BuildComputedRows(computedSeries);

        return new OperationChainComputationGridResult(
            result,
            dataset.Label,
            rows,
            $"{dataset.Label}: {rows.Count} result points computed.")
        {
            Evidence = $"OperationChain trace: {result.Evidence.TraceSignature}; plan: {result.Evidence.PlanSignature}; contract: {result.Evidence.ContractSignature}"
        };
    }
}

internal sealed record OperationChainComputationGridResult(
    OperationChainResult? Result,
    string Title,
    IReadOnlyList<OperationChainResultGridRow> Rows,
    string Summary)
{
    public string? Evidence { get; init; }
    public TransformCorrelationSummary? Correlation { get; init; }
    public IReadOnlyList<OperationChainInputGrid>? Inputs { get; init; }
    public MetricLoadSnapshot? InputSnapshot { get; init; }
}

internal sealed record OperationChainResultGridRow(
    string Timestamp,
    string Raw,
    string Smoothed,
    string Series = "");

internal static class OperationChainGridRowProjector
{
    public static IReadOnlyList<OperationChainInputGridRow> BuildInputRows(IReadOnlyList<MetricData> data) =>
        TransformGridDataProjector.ProjectMetricRows(data)
            .Select(row => new OperationChainInputGridRow(row.Timestamp, row.Value))
            .ToArray();

    public static IReadOnlyList<OperationChainResultGridRow> BuildComputedRows(ComputedSeriesResult result, string series = "")
    {
        ArgumentNullException.ThrowIfNull(result);

        return TransformGridDataProjector.ProjectComputedRows(result)
            .Select(row => new OperationChainResultGridRow(
                row.Timestamp,
                row.Raw,
                row.Smoothed,
                series))
            .ToArray();
    }
}

internal static class OperationChainCorrelationGridPresenter
{
    public static OperationChainComputationGridResult Build(
        TransformCorrelationSummary summary,
        string sourceSignature)
    {
        var rows = new[]
        {
            new OperationChainResultGridRow("Correlation", FormatValue(summary.Correlation), string.Empty),
            new OperationChainResultGridRow("95% CI lower", FormatValue(summary.ConfidenceLower), string.Empty),
            new OperationChainResultGridRow("95% CI upper", FormatValue(summary.ConfidenceUpper), string.Empty),
            new OperationChainResultGridRow("Sample count", summary.SampleCount.ToString(CultureInfo.InvariantCulture), string.Empty)
        };

        return new OperationChainComputationGridResult(
            null,
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

