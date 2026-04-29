using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.UI.Charts.Syncfusion;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class SyncfusionSunburstChartControllerAdapter : ChartControllerAdapterBase
{
    private readonly ISyncfusionSunburstChartController _controller;
    private readonly ISyncfusionSunburstRenderingContract _renderingContract;
    private readonly SyncfusionSunburstRenderModelBuilder _renderModelBuilder;
    private readonly MainWindowViewModel _viewModel;

    public SyncfusionSunburstChartControllerAdapter(
        ISyncfusionSunburstChartController controller,
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService,
        ISyncfusionSunburstRenderingContract? renderingContract = null)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _renderingContract = renderingContract ?? new SyncfusionSunburstRenderingContract();
        _renderModelBuilder = new SyncfusionSunburstRenderModelBuilder(viewModel, metricSelectionService);
    }

    public override string Key => ChartControllerKeys.SyncfusionSunburst;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;

    public override Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsSyncfusionSunburstVisible)
        {
            _renderingContract.Clear(CreateRenderHost());
            return Task.CompletedTask;
        }

        if (context == null)
        {
            _renderingContract.Clear(CreateRenderHost());
            return Task.CompletedTask;
        }

        return RenderSunburstAsync();
    }

    public override void Clear(ChartState state)
    {
        _renderingContract.Clear(CreateRenderHost());
    }

    public override void ResetZoom()
    {
        _renderingContract.ResetView(ResolveRenderingRoute(), CreateRenderHost());
    }

    public override bool HasSeries(ChartState state)
    {
        return _renderingContract.HasRenderableContent(ResolveRenderingRoute(), CreateRenderHost());
    }

    public override void UpdateSubtypeOptions()
    {
    }

    private async Task RenderSunburstAsync()
    {
        var model = await _renderModelBuilder.BuildAsync();
        var route = ResolveRenderingRoute();
        var request = new SyncfusionSunburstChartRenderRequest(
            route,
            model.Items,
            model.BucketCount,
            model.SelectionCount,
            model.From,
            model.To,
            SyncfusionSunburstCapabilityContract.Create());

        var renderResult = await _renderingContract.RenderAsync(request, CreateRenderHost());
        _viewModel.ChartState.SetRenderPlanDiagnostics(
            ChartProgramKind.SyncfusionSunburst,
            renderResult);
    }

    private static SyncfusionSunburstRenderingRoute ResolveRenderingRoute()
    {
        return SyncfusionSunburstRenderingRoute.Hierarchy;
    }

    private SyncfusionSunburstChartRenderHost CreateRenderHost()
    {
        return new SyncfusionSunburstChartRenderHost(_controller, _viewModel.ChartState.IsSyncfusionSunburstVisible);
    }
}
