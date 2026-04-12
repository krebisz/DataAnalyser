using DataVisualiser.UI.Charts.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Transforms;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class TransformDataPanelControllerAdapter : CartesianChartControllerAdapterBase<ITransformDataPanelController>, ITransformPanelControllerExtras
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly ITransformDataPanelController _controller;
    private readonly Func<bool> _isInitializing;
    private readonly TransformDataResolutionCoordinator _transformDataResolutionCoordinator;
    private readonly TransformOperationExecutionCoordinator _transformOperationExecutionCoordinator;
    private readonly TransformOperationStateCoordinator _transformOperationStateCoordinator;
    private readonly TransformSessionMilestoneRecorder _transformSessionMilestoneRecorder;
    private readonly ITransformRenderingContract _transformRenderingContract;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly MainWindowViewModel _viewModel;
    private bool _isTransformSelectionPendingLoad;
    private bool _isUpdatingTransformSubtypeCombos;

    public TransformDataPanelControllerAdapter(ITransformDataPanelController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, ITransformRenderingContract transformRenderingContract, TransformComputationService? transformComputationService = null)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _transformRenderingContract = transformRenderingContract ?? throw new ArgumentNullException(nameof(transformRenderingContract));
        var computationService = transformComputationService ?? new TransformComputationService();
        _transformDataResolutionCoordinator = new TransformDataResolutionCoordinator(_controller, _viewModel, metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService)), _selectionCache);
        _transformOperationExecutionCoordinator = new TransformOperationExecutionCoordinator(computationService);
        _transformOperationStateCoordinator = new TransformOperationStateCoordinator();
        _transformSessionMilestoneRecorder = new TransformSessionMilestoneRecorder(_viewModel);
    }

    public override void ClearCache()
    {
        _selectionCache.Clear();
    }

    public override string Key => ChartControllerKeys.Transform;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override async Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsTransformPanelVisible)
            return;

        UpdateTransformSubtypeOptions();
        var resolution = await _transformDataResolutionCoordinator.ResolveAsync(context, _isTransformSelectionPendingLoad);
        PopulateTransformGrids(resolution);
        UpdateTransformComputeButtonState();
    }

    public override void Clear(ChartState state)
    {
        ClearTransformGrids(state);
        _transformRenderingContract.Clear(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public override void ResetZoom()
    {
        _transformRenderingContract.ResetView(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public override bool HasSeries(ChartState state)
    {
        return _transformRenderingContract.HasRenderableContent(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public override void UpdateSubtypeOptions()
    {
        UpdateTransformSubtypeOptions();
    }

    public void CompleteSelectionsPendingLoad()
    {
        _isTransformSelectionPendingLoad = false;
    }

    public void ResetSelectionsPendingLoad()
    {
        _isTransformSelectionPendingLoad = true;
        _viewModel.ChartState.SelectedTransformPrimarySeries = null;
        _viewModel.ChartState.SelectedTransformSecondarySeries = null;

        if (_controller.TransformPrimarySubtypeCombo == null || _controller.TransformSecondarySubtypeCombo == null)
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            TransformSubtypeSelectionCoordinator.ResetSelectionControls(_controller);
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    public void HandleVisibilityOnlyToggle(ChartDataContext? context)
    {
        if (!_viewModel.ChartState.IsTransformPanelVisible)
            return;

        if (context != null && ShouldRenderCharts(context))
            PopulateTransformGrids(context, false);

        UpdateTransformSubtypeOptions();
    }

    public void UpdateTransformSubtypeOptions()
    {
        if (!TransformSubtypeSelectionCoordinator.CanUpdateSubtypeOptions(_controller, _isTransformSelectionPendingLoad))
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            TransformSubtypeSelectionCoordinator.ApplySubtypeOptions(
                _controller,
                _viewModel.ChartState,
                _viewModel.MetricState.SelectedSeries,
                SetBinaryTransformOperationsEnabled);
            UpdateTransformComputeButtonState();
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    public void UpdateTransformComputeButtonState()
    {
        _transformOperationStateCoordinator.UpdateComputeButtonState(
            _controller,
            _viewModel.ChartState.LastContext,
            _isTransformSelectionPendingLoad,
            ctx => _transformDataResolutionCoordinator.ResolveSelections(ctx, _isTransformSelectionPendingLoad),
            _transformOperationExecutionCoordinator);
    }

    public string? GetSelectedOperationTag()
    {
        return _transformOperationStateCoordinator.GetSelectedOperationTag(_controller);
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleTransformPanel();
        _transformSessionMilestoneRecorder.RecordToggle();
    }

    public void OnOperationChanged(object? sender, EventArgs e)
    {
        UpdateTransformComputeButtonState();
    }

    public async void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformPrimarySubtypeCombo);
        _viewModel.SetTransformPrimarySeries(selection);

        UpdateTransformComputeButtonState();
        await RefreshTransformGridsFromSelectionAsync();
    }

    public async void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformSecondarySubtypeCombo);
        _viewModel.SetTransformSecondarySeries(selection);

        UpdateTransformComputeButtonState();
        await RefreshTransformGridsFromSelectionAsync();
    }

    public async void OnComputeRequested(object? sender, EventArgs e)
    {
        if (_isTransformSelectionPendingLoad)
            return;

        if (_viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        using var _ = _beginUiBusyScope();
        var operationTag = _transformOperationStateCoordinator.GetSelectedOperationTag(_controller);
        await ExecuteTransformOperation(ctx, operationTag);
    }

    private void PopulateTransformGrids(ChartDataContext ctx, bool resetResults = true)
    {
        var selection = _transformDataResolutionCoordinator.ResolveSelections(ctx, _isTransformSelectionPendingLoad);
        PopulateTransformGrids(ctx, selection.HasAvailableSecondaryInput, resetResults);
    }

    private void PopulateTransformGrids(TransformResolutionResult resolution, bool resetResults = true)
    {
        PopulateTransformGrids(resolution.Context, resolution.Selection.HasAvailableSecondaryInput, resetResults);
    }

    private void PopulateTransformGrids(ChartDataContext context, bool hasAvailableSecondaryInput, bool resetResults)
    {
        TransformGridPresentationCoordinator.PopulateInputGrids(
            _controller,
            context,
            hasAvailableSecondaryInput,
            SetBinaryTransformOperationsEnabled,
            resetResults);
    }

    private void SetBinaryTransformOperationsEnabled(bool enabled)
    {
        TransformGridPresentationCoordinator.SetBinaryTransformOperationsEnabled(_controller, enabled);
    }

    private async Task ExecuteTransformOperation(ChartDataContext ctx, string? operationTag)
    {
        var resolution = await _transformDataResolutionCoordinator.ResolveAsync(ctx, _isTransformSelectionPendingLoad);
        var execution = _transformOperationExecutionCoordinator.Execute(resolution, operationTag);
        if (execution == null)
            return;

        await RenderTransformResults(execution, resolution);
    }

    private async Task RenderTransformResults(TransformExecutionResult execution, TransformResolutionResult resolution)
    {
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

        if (_controller is DataVisualiser.UI.Charts.Controllers.TransformDataPanelControllerV2 v2)
            v2.UpdateMinMaxLines();

        _transformSessionMilestoneRecorder.RecordExecution(execution, resolution);
    }

    private async Task RenderPrimarySelectionAsResult(ChartDataContext ctx)
    {
        var resolution = await _transformDataResolutionCoordinator.ResolveAsync(ctx, _isTransformSelectionPendingLoad);
        var execution = _transformOperationExecutionCoordinator.Execute(resolution, null);
        if (execution == null)
            return;

        await RenderTransformResults(execution, resolution);
    }

    private async Task RefreshTransformGridsFromSelectionAsync()
    {
        var ctx = _viewModel.ChartState.LastContext;
        if (ctx == null)
            return;

        var resolution = await _transformDataResolutionCoordinator.ResolveAsync(ctx, _isTransformSelectionPendingLoad);
        PopulateTransformGrids(resolution);
        UpdateTransformComputeButtonState();
    }

    private void ClearTransformGrids(ChartState state)
    {
        TransformGridPresentationCoordinator.ClearAllGrids(_controller);
    }

    private static bool ShouldRenderCharts(ChartDataContext? ctx)
    {
        return TransformGridPresentationCoordinator.ShouldRenderCharts(ctx);
    }

    private TransformChartRenderHost CreateRenderHost()
    {
        return new TransformChartRenderHost(
            _controller.ChartTransformResult,
            _viewModel.ChartState,
            ResetTransformAuxiliaryVisuals);
    }

    private void ResetTransformAuxiliaryVisuals()
    {
        if (_controller is DataVisualiser.UI.Charts.Controllers.TransformDataPanelControllerV2 v2)
            v2.ResetMinMaxLines();
    }

}
