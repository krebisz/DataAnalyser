using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.OperationChain;

internal sealed class OperationChainTransformOutputRenderer
{
    private readonly TransformOperationChainOutputRenderer _renderer;

    public OperationChainTransformOutputRenderer()
        : this(new TransformOperationChainOutputRenderer())
    {
    }

    private OperationChainTransformOutputRenderer(TransformOperationChainOutputRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    internal OperationChainTransformOutputRenderer(
        ChartState chartState,
        ITransformRenderingContract? renderingContract = null)
        : this(new TransformOperationChainOutputRenderer(chartState, renderingContract))
    {
    }

    public Task RenderAsync(CartesianChart chart, OperationChainResult result) =>
        _renderer.RenderAsync(chart, result);

    public Task RenderAsync(CartesianChart chart, OperationChainComputationGridResult result, IReadOnlyList<bool>? includedRows)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Correlation != null)
            return _renderer.RenderCorrelationAsync(chart, result.Correlation);

        if (result.InputSnapshot != null)
            return _renderer.RenderInputSnapshotAsync(chart, result.InputSnapshot, includedRows);

        return result.Result == null
            ? _renderer.ClearAsync(chart)
            : _renderer.RenderAsync(chart, result.Result, includedRows);
    }

    public Task RenderAsync(CartesianChart chart, OperationChainComputationGridResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Correlation != null)
            return _renderer.RenderCorrelationAsync(chart, result.Correlation);

        if (result.InputSnapshot != null)
            return _renderer.RenderInputSnapshotAsync(chart, result.InputSnapshot);

        return result.Result == null
            ? _renderer.ClearAsync(chart)
            : _renderer.RenderAsync(chart, result.Result);
    }

    public Task ClearAsync(CartesianChart chart) =>
        _renderer.ClearAsync(chart);
}
