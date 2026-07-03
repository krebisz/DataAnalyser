using DataVisualiser.Core.Services;
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

        var request = TransformMetricSelectionRequestFactory.Create(series, from, to, resolutionTableName);

        if (TransformCorrelationMapper.IsCorrelationOperation(operationTag))
            return await ComputeCorrelationAsync(request, operationTag, cancellationToken);

        if (!TransformSeriesOperationRequestMapper.TryCreateOperationChainSteps(operationTag, series, out var steps, out var title))
            throw new InvalidOperationException($"Transform operation '{operationTag}' is not valid for {series.Count} selected input series.");

        var result = await _transformExecutionService.ExecuteAsync(
            request,
            steps,
            title,
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
