using DataVisualiser.Core.Services;
using DataVisualiser.Core.Computation.TimeSeries;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformWorkbenchService
{
    private readonly MetricLoadSnapshotGateway _gateway;
    private readonly TransformConstructionExecutionService _transformExecutionService;
    private readonly TransformInputDateRangeResolver? _dateRangeResolver;

    public TransformWorkbenchService(MetricSelectionService metricSelectionService)
        : this(
            new MetricSelectionServiceSeriesLoader(metricSelectionService),
            new TransformInputDateRangeResolver((selection, tableName) =>
                metricSelectionService.LoadDateRangeAsync(selection.MetricType, selection.QuerySubtype, tableName)))
    {
    }

    internal TransformWorkbenchService(
        IMetricSeriesLoader loader,
        TransformInputDateRangeResolver? dateRangeResolver = null)
    {
        ArgumentNullException.ThrowIfNull(loader);
        _gateway = new MetricLoadSnapshotGateway(loader);
        _transformExecutionService = new TransformConstructionExecutionService(_gateway);
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
            throw new ArgumentException("At least one transform input series is required.", nameof(series));

        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);

        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        return TransformInputGridPresenter.Build(snapshot);
    }

    public async Task<TransformComputationResult> ComputeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        string operationTag,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one transform input series is required.", nameof(series));
        if (TransformSeriesOperationRequestMapper.IsPassThrough(operationTag))
            return TransformInputGridPresenter.BuildComputationResult(
                await LoadAsync(series, from, to, resolutionTableName, cancellationToken));

        if (TryResolveOperationChainNormalizationMode(operationTag, out var normalizationMode))
            return await ComputeOperationChainNormalizationAsync(
                series,
                from,
                to,
                resolutionTableName,
                normalizationMode,
                operationTag,
                cancellationToken);

        if (!TransformSeriesOperationRequestMapper.TryCreateOperationChainStep(operationTag, series, out var step) || step == null)
            throw new InvalidOperationException($"Transform operation '{operationTag}' is not valid for {series.Count} selected input series.");

        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);

        if (TransformCorrelationMapper.IsCorrelationOperation(operationTag))
            return await ComputeCorrelationAsync(request, operationTag, cancellationToken);

        var result = await _transformExecutionService.ExecuteAsync(
            request,
            [step],
            step.Operation.Label,
            cancellationToken);

        return TransformComputationResultProjector.FromOperationChainResult(result);
    }

    public async Task<TransformComputationResult> ComputeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        IReadOnlyList<OperationChainStep> steps,
        string title,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one transform input series is required.", nameof(series));
        if (steps == null || steps.Count == 0)
            throw new ArgumentException("At least one transform equation step is required.", nameof(steps));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Transform equation title cannot be empty.", nameof(title));

        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);
        var result = await _transformExecutionService.ExecuteAsync(
            request,
            steps,
            title,
            cancellationToken);

        return TransformComputationResultProjector.FromOperationChainResult(result);
    }

    private async Task<TransformComputationResult> ComputeCorrelationAsync(
        MetricSelectionRequest request,
        string operationTag,
        CancellationToken cancellationToken)
    {
        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        var aligned = new TimeSeriesAlignmentKernel().Align(snapshot);
        var summary = TransformCorrelationMapper.Compute(aligned, operationTag);
        return TransformCorrelationGridPresenter.Build(summary, snapshot.Signature);
    }

    private async Task<TransformComputationResult> ComputeOperationChainNormalizationAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        NormalizationMode mode,
        string operationTag,
        CancellationToken cancellationToken)
    {
        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);
        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        var aligned = new TimeSeriesAlignmentKernel().Align(snapshot);
        var computedSeries = OperationChainNormalizationCalculator.Compute(aligned, mode);
        var computedSnapshot = BuildComputedSnapshot(snapshot, aligned.Timeline, computedSeries, operationTag);
        var inputs = TransformInputGridPresenter.Build(computedSnapshot).Inputs;
        var rows = inputs
            .SelectMany(input => input.Rows.Select(row => new TransformResultGridRow(
                row.Timestamp,
                row.Value,
                string.Empty,
                input.Title)))
            .ToArray();
        var title = ResolveNormalizationTitle(mode);

        return new TransformComputationResult(
            title,
            rows,
            $"{title}: {rows.Length} result points computed across {computedSeries.Count} series.")
        {
            Evidence = $"Operation Chain normalization source: {snapshot.Signature}; mode: {mode}",
            ComputedSnapshot = computedSnapshot,
            Selection = snapshot.Request
        };
    }

    private static MetricLoadSnapshot BuildComputedSnapshot(
        MetricLoadSnapshot source,
        IReadOnlyList<DateTime> timeline,
        IReadOnlyList<OperationChainNormalizedSeries> computedSeries,
        string operationTag)
    {
        var requests = computedSeries
            .Select(series => new MetricSeriesRequest(
                series.Id,
                null,
                series.Label,
                null))
            .ToArray();

        var selection = new MetricSelectionRequest(
            "Operation Chain",
            requests,
            source.Request.From,
            source.Request.To,
            source.Request.ResolutionTableName);

        var snapshots = computedSeries
            .Zip(requests, (series, request) => new MetricSeriesSnapshot(
                request,
                ProjectMetricData(timeline, series.RawValues, operationTag),
                CanonicalSeries: null))
            .ToArray();

        return new MetricLoadSnapshot(selection, snapshots, DateTime.UtcNow);
    }

    private static IReadOnlyList<MetricData> ProjectMetricData(
        IReadOnlyList<DateTime> timeline,
        IReadOnlyList<double> values,
        string operationTag)
    {
        var count = Math.Min(timeline.Count, values.Count);
        var data = new List<MetricData>(count);
        for (var index = 0; index < count; index++)
        {
            var value = values[index];
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            data.Add(new MetricData
            {
                NormalizedTimestamp = timeline[index],
                Value = Convert.ToDecimal(value),
                Unit = ResolveNormalizationUnit(operationTag),
                Provider = "Transform"
            });
        }

        return data;
    }

    private static bool TryResolveOperationChainNormalizationMode(
        string? operationTag,
        out NormalizationMode mode)
    {
        mode = operationTag switch
        {
            "NormalizeZeroToOne" => NormalizationMode.ZeroToOne,
            "NormalizePercentageOfMax" => NormalizationMode.PercentageOfMax,
            "NormalizeRelativeToMax" => NormalizationMode.RelativeToMax,
            _ => default
        };

        return operationTag is "NormalizeZeroToOne" or "NormalizePercentageOfMax" or "NormalizeRelativeToMax";
    }

    private static string ResolveNormalizationTitle(NormalizationMode mode)
    {
        return mode switch
        {
            NormalizationMode.ZeroToOne => "Zero-To-One",
            NormalizationMode.PercentageOfMax => "% of Max",
            NormalizationMode.RelativeToMax => "Relative to Max",
            _ => "Normalized"
        };
    }

    private static string ResolveNormalizationUnit(string operationTag) =>
        string.Equals(operationTag, "NormalizePercentageOfMax", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(operationTag, "NormalizeRelativeToMax", StringComparison.OrdinalIgnoreCase)
            ? "%"
            : string.Empty;

    public Task<TransformInputDateRange?> ResolveDateRangeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (_dateRangeResolver == null)
            throw new InvalidOperationException("Transform date-range resolution is not configured.");

        return _dateRangeResolver.ResolveAsync(series, resolutionTableName, cancellationToken);
    }
}
