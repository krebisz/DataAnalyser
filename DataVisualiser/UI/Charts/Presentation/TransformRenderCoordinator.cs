using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformRenderCoordinator
{
    private readonly ITransformDataPanelController _controller;
    private readonly ChartState _chartState;
    private readonly ITransformRenderingContract _transformRenderingContract;

    public TransformRenderCoordinator(
        ITransformDataPanelController controller,
        ChartState chartState,
        ITransformRenderingContract transformRenderingContract)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _chartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        _transformRenderingContract = transformRenderingContract ?? throw new ArgumentNullException(nameof(transformRenderingContract));
    }

    public void PopulateInputGrids(ChartDataContext context, bool hasAvailableSecondaryInput, bool resetResults, Action<bool> setBinaryTransformOperationsEnabled)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(setBinaryTransformOperationsEnabled);

        TransformGridPresentationCoordinator.PopulateInputGrids(
            _controller,
            context,
            hasAvailableSecondaryInput,
            setBinaryTransformOperationsEnabled,
            resetResults);
    }

    public async Task RenderResultsAsync(TransformExecutionResult execution, TransformResolutionResult resolution)
    {
        ArgumentNullException.ThrowIfNull(execution);
        ArgumentNullException.ThrowIfNull(resolution);

        var transformContext = resolution.Context;
        var resultData = DataVisualiser.Core.Transforms.TransformExpressionEvaluator.CreateTransformResultData(execution.DataList, execution.Results);
        TransformGridPresentationCoordinator.PopulateResultGrid(_controller, resultData);

        if (resultData.Count == 0)
            return;

        TransformGridPresentationCoordinator.ShowResultPanels(_controller);
        await TransformChartPresentationCoordinator.RenderResultsAsync(
            _controller,
            _transformRenderingContract,
            CreateRenderHost(),
            execution.DataList,
            execution.Results,
            execution.OperationTag,
            execution.Metrics,
            transformContext,
            execution.OverrideLabel);

        if (_controller is TransformDataPanelControllerV2 v2)
            v2.UpdateMinMaxLines();
    }

    public void Clear()
    {
        TransformGridPresentationCoordinator.ClearAllGrids(_controller);
        _transformRenderingContract.Clear(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public void ResetZoom()
    {
        _transformRenderingContract.ResetView(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public bool HasRenderableContent()
    {
        return _transformRenderingContract.HasRenderableContent(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public static bool ShouldRenderCharts(ChartDataContext? context)
    {
        return TransformGridPresentationCoordinator.ShouldRenderCharts(context);
    }

    private TransformChartRenderHost CreateRenderHost()
    {
        return new TransformChartRenderHost(
            _controller.ChartTransformResult,
            _chartState,
            ResetTransformAuxiliaryVisuals);
    }

    private void ResetTransformAuxiliaryVisuals()
    {
        if (_controller is TransformDataPanelControllerV2 v2)
            v2.ResetMinMaxLines();
    }
}
