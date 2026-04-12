using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Transforms;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformOperationExecutionCoordinator
{
    private readonly TransformComputationService _transformComputationService;

    public TransformOperationExecutionCoordinator(TransformComputationService transformComputationService)
    {
        _transformComputationService = transformComputationService ?? throw new ArgumentNullException(nameof(transformComputationService));
    }

    public bool CanExecute(ChartDataContext context, TransformSelectionResolution selection, string? operationTag)
    {
        if (string.IsNullOrWhiteSpace(operationTag))
            return TransformDataResolutionCoordinator.CanRenderPrimarySelection(context);

        var operation = TransformOperationRegistry.GetOperation(operationTag);
        if (operation == null)
            return false;

        return operation.Arity switch
        {
            1 => TransformDataResolutionCoordinator.CanRenderPrimarySelection(context),
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

        return operation.Arity switch
        {
            1 => BuildUnaryExecution(resolution.PrimaryData, operationTag),
            2 => resolution.SecondaryData != null && resolution.SecondaryData.Any() ? BuildBinaryExecution(resolution.PrimaryData, resolution.SecondaryData, operationTag) : null,
            _ => null
        };
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
}
