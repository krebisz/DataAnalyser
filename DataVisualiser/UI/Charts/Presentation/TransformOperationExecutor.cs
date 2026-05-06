using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Transforms;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformOperationExecutor
{
    private readonly OperationKernel _operationKernel;
    private readonly TransformComputationService _transformComputationService;

    public TransformOperationExecutor(
        TransformComputationService transformComputationService,
        OperationKernel? operationKernel = null)
    {
        _transformComputationService = transformComputationService ?? throw new ArgumentNullException(nameof(transformComputationService));
        _operationKernel = operationKernel ?? new OperationKernel();
    }

    public bool CanExecute(ChartDataContext context, TransformSelectionResolution selection, string? operationTag)
    {
        if (string.IsNullOrWhiteSpace(operationTag))
            return TransformDataResolver.CanRenderPrimarySelection(context);

        var operation = TransformOperationRegistry.GetOperation(operationTag);
        if (operation == null)
            return false;

        return operation.Arity switch
        {
            1 => TransformDataResolver.CanRenderPrimarySelection(context),
            2 => selection.HasAvailableSecondaryInput,
            _ => false
        };
    }

    public TransformExecutionResult? Execute(TransformResolutionResult resolution, string? operationTag)
    {
        if (resolution.PrimaryData == null)
            return null;

        if (string.IsNullOrWhiteSpace(operationTag))
            return BuildPrimaryProjection(resolution);

        var operation = TransformOperationRegistry.GetOperation(operationTag);
        if (operation == null)
            return null;

        var vnextExecution = TryExecuteWithVNext(resolution, operationTag, operation.Arity);
        if (vnextExecution != null)
            return vnextExecution;

        return operation.Arity switch
        {
            1 => BuildUnaryExecution(resolution.PrimaryData, operationTag),
            2 => resolution.SecondaryData != null && resolution.SecondaryData.Any() ? BuildBinaryExecution(resolution.PrimaryData, resolution.SecondaryData, operationTag) : null,
            _ => null
        };
    }

    private TransformExecutionResult? TryExecuteWithVNext(
        TransformResolutionResult resolution,
        string operationTag,
        int operationArity)
    {
        var primaryLabel = resolution.Context.DisplayName1 ?? resolution.Context.PrimarySubtype ?? "Primary";
        var secondaryLabel = resolution.Context.DisplayName2 ?? resolution.Context.SecondarySubtype ?? "Secondary";
        if (!TransformSeriesOperationRequestMapper.TryCreateOperationChainStep(operationTag, primaryLabel, secondaryLabel, out var step) || step == null)
            return null;

        return operationArity switch
        {
            1 => TryExecuteUnaryWithVNext(resolution, step.Operation, operationTag),
            2 => TryExecuteBinaryWithVNext(resolution, step.Operation, operationTag),
            _ => null
        };
    }

    private TransformExecutionResult? TryExecuteUnaryWithVNext(
        TransformResolutionResult resolution,
        SeriesOperationRequest request,
        string operationTag)
    {
        var dataList = PrepareMetricData(resolution.PrimaryData);
        if (dataList.Count == 0)
            return null;

        var program = _operationKernel.BuildSeries(
            CreateBundle([dataList], resolution.Context.DisplayName1 ?? "Primary"),
            request);

        return new TransformExecutionResult(
            dataList,
            program.RawValues.ToList(),
            operationTag,
            1,
            [dataList],
            program.Label);
    }

    private TransformExecutionResult? TryExecuteBinaryWithVNext(
        TransformResolutionResult resolution,
        SeriesOperationRequest request,
        string operationTag)
    {
        var primary = PrepareMetricData(resolution.PrimaryData);
        var secondary = PrepareMetricData(resolution.SecondaryData);
        if (primary.Count == 0 || secondary.Count == 0)
            return null;

        var (alignedPrimary, alignedSecondary) = DataVisualiser.Core.Transforms.TransformExpressionEvaluator.AlignMetricsByTimestamp(primary, secondary);
        if (alignedPrimary.Count == 0 || alignedSecondary.Count == 0)
            return null;

        var program = _operationKernel.BuildSeries(
            CreateBundle(
                [alignedPrimary, alignedSecondary],
                resolution.Context.DisplayName1 ?? "Primary",
                resolution.Context.DisplayName2 ?? "Secondary"),
            request);

        return new TransformExecutionResult(
            alignedPrimary,
            program.RawValues.ToList(),
            operationTag,
            2,
            [alignedPrimary, alignedSecondary],
            program.Label);
    }

    private TransformExecutionResult? BuildUnaryExecution(IEnumerable<MetricData> data, string operationTag)
    {
        var computation = _transformComputationService.ComputeUnaryTransform(data, operationTag);
        if (!computation.IsSuccess || computation.DataList.Count == 0)
            return null;

        return new TransformExecutionResult(
            computation.DataList,
            computation.ComputedResults,
            operationTag,
            1,
            computation.MetricsList,
            null);
    }

    private TransformExecutionResult? BuildBinaryExecution(IEnumerable<MetricData> primaryData, IEnumerable<MetricData> secondaryData, string operationTag)
    {
        var computation = _transformComputationService.ComputeBinaryTransform(primaryData, secondaryData, operationTag);
        if (!computation.IsSuccess || computation.DataList.Count == 0)
            return null;

        return new TransformExecutionResult(
            computation.DataList,
            computation.ComputedResults,
            operationTag,
            2,
            computation.MetricsList,
            null);
    }

    private static TransformExecutionResult? BuildPrimaryProjection(TransformResolutionResult resolution)
    {
        var dataList = resolution.PrimaryData!
            .Where(d => d.Value.HasValue)
            .OrderBy(d => d.NormalizedTimestamp)
            .ToList();

        if (dataList.Count == 0)
            return null;

        var results = dataList.Select(d => (double)d.Value!.Value).ToList();
        var metrics = new List<IReadOnlyList<MetricData>>
        {
            dataList
        };
        var label = string.IsNullOrWhiteSpace(resolution.Context.DisplayName1) ? "Primary Data" : resolution.Context.DisplayName1;

        return new TransformExecutionResult(
            dataList,
            results,
            string.Empty,
            1,
            metrics,
            label);
    }

    private static List<MetricData> PrepareMetricData(IEnumerable<MetricData>? data)
    {
        return data?
            .Where(point => point.Value.HasValue)
            .OrderBy(point => point.NormalizedTimestamp)
            .ToList() ?? [];
    }

    private static AlignedSeriesBundle CreateBundle(
        IReadOnlyList<IReadOnlyList<MetricData>> seriesData,
        params string[] labels)
    {
        var timeline = seriesData[0].Select(point => point.NormalizedTimestamp).ToList();
        var series = seriesData
            .Select((data, index) => new AlignedMetricSeries(
                new MetricSeriesRequest("Transform", $"input-{index}", "Transform", index < labels.Length ? labels[index] : $"Input {index + 1}"),
                data.Select(point => Convert.ToDouble(point.Value!.Value)).ToList(),
                data.Select(point => Convert.ToDouble(point.Value!.Value)).ToList()))
            .ToList();

        return new AlignedSeriesBundle(timeline, series);
    }
}
