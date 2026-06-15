using System.Globalization;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformOperationChainWorkbenchService
{
    private readonly MetricLoadSnapshotGateway _gateway;
    private readonly TransformOperationChainExecutionService _transformExecutionService;
    private readonly TransformInputDateRangeResolver? _dateRangeResolver;

    public TransformOperationChainWorkbenchService(MetricSelectionService metricSelectionService)
        : this(
            new MetricSelectionServiceSeriesLoader(metricSelectionService),
            new TransformInputDateRangeResolver((selection, tableName) =>
                metricSelectionService.LoadDateRangeAsync(selection.MetricType, selection.QuerySubtype, tableName)))
    {
    }

    internal TransformOperationChainWorkbenchService(
        IMetricSeriesLoader loader,
        TransformInputDateRangeResolver? dateRangeResolver = null)
    {
        ArgumentNullException.ThrowIfNull(loader);
        _gateway = new MetricLoadSnapshotGateway(loader);
        _transformExecutionService = new TransformOperationChainExecutionService(_gateway);
        _dateRangeResolver = dateRangeResolver;
    }

    public async Task<TransformInputGridLoadResult> LoadAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one operation-chain input series is required.", nameof(series));

        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);

        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        return TransformInputGridPresenter.Build(snapshot);
    }

    public async Task<TransformOperationChainComputationGridResult> ComputeAsync(
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
            return TransformInputGridPresenter.BuildComputationResult(
                await LoadAsync(series, from, to, resolutionTableName, cancellationToken));

        if (!TransformSeriesOperationRequestMapper.TryCreateOperationChainStep(operationTag, series, out var step) || step == null)
            throw new InvalidOperationException($"Operation Chain operation '{operationTag}' is not valid for {series.Count} selected input series.");

        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);

        if (TransformCorrelationMapper.IsCorrelationOperation(operationTag))
            return await ComputeCorrelationAsync(request, operationTag, cancellationToken);

        var result = await _transformExecutionService.ExecuteAsync(
            request,
            [step],
            step.Operation.Label,
            cancellationToken);

        return TransformOperationChainResultGridPresenter.Build(result);
    }

    public async Task<TransformOperationChainComputationGridResult> ComputeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        IReadOnlyList<OperationChainStep> steps,
        string title,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one operation-chain input series is required.", nameof(series));
        if (steps == null || steps.Count == 0)
            throw new ArgumentException("At least one operation-chain equation step is required.", nameof(steps));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Operation-chain equation title cannot be empty.", nameof(title));

        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);
        var result = await _transformExecutionService.ExecuteAsync(
            request,
            steps,
            title,
            cancellationToken);

        return TransformOperationChainResultGridPresenter.Build(result);
    }

    private async Task<TransformOperationChainComputationGridResult> ComputeCorrelationAsync(
        MetricSelectionRequest request,
        string operationTag,
        CancellationToken cancellationToken)
    {
        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        var aligned = new TimeSeriesAlignmentKernel().Align(snapshot);
        var summary = TransformCorrelationMapper.Compute(aligned, operationTag);
        return TransformOperationChainCorrelationGridPresenter.Build(summary, snapshot.Signature);
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

    public static TransformOperationChainComputationGridResult BuildComputationResult(TransformInputGridLoadResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var rows = result.Inputs
            .SelectMany(input => input.Rows.Select(row => new TransformResultGridRow(
                row.Timestamp,
                row.Value,
                string.Empty,
                input.Title)))
            .ToArray();

        return new TransformOperationChainComputationGridResult(
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

internal static class TransformOperationChainResultGridPresenter
{
    public static TransformOperationChainComputationGridResult Build(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var dataset = result.DerivedDatasets.LastOrDefault()
            ?? throw new InvalidOperationException("Operation Chain did not produce a derived dataset.");
        var computedSeries = ComputedSeriesResult.FromDerivedDataset(dataset);
        var rows = TransformGridRowProjector.BuildComputedRows(computedSeries);

        return new TransformOperationChainComputationGridResult(
            result,
            dataset.Label,
            rows,
            $"{dataset.Label}: {rows.Count} result points computed.")
        {
            Evidence = $"OperationChain trace: {result.Evidence.TraceSignature}; plan: {result.Evidence.PlanSignature}; contract: {result.Evidence.ContractSignature}"
        };
    }
}

internal sealed record TransformOperationChainComputationGridResult(
    OperationChainResult? Result,
    string Title,
    IReadOnlyList<TransformResultGridRow> Rows,
    string Summary)
{
    public string? Evidence { get; init; }
    public TransformCorrelationSummary? Correlation { get; init; }
    public IReadOnlyList<TransformInputGrid>? Inputs { get; init; }
    public MetricLoadSnapshot? InputSnapshot { get; init; }
}

internal sealed record TransformResultGridRow(
    string Timestamp,
    string Raw,
    string Smoothed,
    string Series = "");

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

internal static class TransformOperationChainCorrelationGridPresenter
{
    public static TransformOperationChainComputationGridResult Build(
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

        return new TransformOperationChainComputationGridResult(
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

