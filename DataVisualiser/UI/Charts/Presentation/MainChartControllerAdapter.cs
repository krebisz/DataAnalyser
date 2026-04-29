using System.Linq;
using System.Windows;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class MainChartControllerAdapter : CartesianChartControllerAdapterBase<IMainChartController>, IMainChartControllerExtras
{
    private const CartesianMetricChartRoute RenderingRoute = CartesianMetricChartRoute.Main;
    private readonly IMainChartController _controller;
    private readonly ICartesianMetricChartRenderingContract _renderingContract;
    private readonly CartesianMetricOverlaySeriesBuilder _overlayBuilder;
    private readonly Func<bool> _isInitializing;
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombo;

    public MainChartControllerAdapter(IMainChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, MetricSelectionService metricSelectionService, ICartesianMetricChartRenderingContract renderingContract)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _renderingContract = renderingContract ?? throw new ArgumentNullException(nameof(renderingContract));
        _overlayBuilder = new CartesianMetricOverlaySeriesBuilder(viewModel, metricSelectionService);
    }

    public override string Key => ChartControllerKeys.Main;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsMainVisible)
            return Task.CompletedTask;

        return RenderMainChartAsync(context);
    }

    public override void UpdateSubtypeOptions()
    {
        if (_controller.OverlaySubtypeCombo == null)
            return;

        var selections = GetStackedSelections();
        _isUpdatingSubtypeCombo = true;
        try
        {
            if (selections.Count == 0)
            {
                ChartSubtypeComboHelper.DisableCombo(_controller.OverlaySubtypeCombo);
                _viewModel.ChartState.SelectedStackedOverlaySeries = null;
                UpdateOverlayControlsVisibility(selections);
                return;
            }

            ChartSubtypeComboHelper.PopulateCombo(_controller.OverlaySubtypeCombo, selections);
            var selection = ChartSubtypeComboHelper.ResolveSelection(selections, _viewModel.ChartState.SelectedStackedOverlaySeries) ?? selections[0];
            ChartSubtypeComboHelper.SelectComboItem(_controller.OverlaySubtypeCombo, selection);

            if (_isInitializing())
                _viewModel.ChartState.SelectedStackedOverlaySeries = selection;
            else
                _viewModel.SetStackedOverlaySeries(selection);
        }
        finally
        {
            _isUpdatingSubtypeCombo = false;
        }

        UpdateOverlayControlsVisibility(selections);
    }

    public override void ClearCache()
    {
    }

    public override void Clear(ChartState state)
    {
        RenderingHostLifecycleAdapterHelper.Clear(RenderingRoute, CreateRenderHost, _renderingContract.Clear);
    }

    public override void ResetZoom()
    {
        RenderingHostLifecycleAdapterHelper.ResetView(RenderingRoute, CreateRenderHost, _renderingContract.ResetView);
    }

    public override bool HasSeries(ChartState state)
    {
        return RenderingHostLifecycleAdapterHelper.HasRenderableContent(RenderingRoute, CreateRenderHost, _renderingContract.HasRenderableContent);
    }

    public void SyncDisplayModeSelection()
    {
        var mode = _viewModel.ChartState.MainChartDisplayMode;
        _controller.DisplayRegularRadio.IsChecked = mode == MainChartDisplayMode.Regular;
        _controller.DisplaySummedRadio.IsChecked = mode == MainChartDisplayMode.Summed;
        _controller.DisplayStackedRadio.IsChecked = mode == MainChartDisplayMode.Stacked;
        UpdateOverlayControlsVisibility(GetStackedSelections());
    }

    public void SetStackedAvailability(bool canStack)
    {
        _controller.DisplayStackedRadio.IsEnabled = canStack;

        if (!canStack && _controller.DisplayStackedRadio.IsChecked == true)
        {
            _controller.DisplayRegularRadio.IsChecked = true;
            _viewModel.SetMainChartDisplayMode(MainChartDisplayMode.Regular);
        }

        UpdateOverlayControlsVisibility(GetStackedSelections());
    }


    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleMain();
    }

    public void OnDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        var previousMode = _viewModel.ChartState.MainChartDisplayMode;
        var mode = _controller.DisplayStackedRadio.IsChecked == true ? MainChartDisplayMode.Stacked : _controller.DisplaySummedRadio.IsChecked == true ? MainChartDisplayMode.Summed : MainChartDisplayMode.Regular;

        _viewModel.SetMainChartDisplayMode(mode);
        if (previousMode != mode)
            RecordSessionMilestone(
                "MainChartDisplayModeChanged",
                "Success",
                mode.ToString(),
                $"Main chart display mode changed from {previousMode} to {mode}.");

        UpdateSubtypeOptions();
        UpdateOverlayControlsVisibility(GetStackedSelections());
    }

    public void OnOverlaySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombo)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.OverlaySubtypeCombo);
        _viewModel.SetStackedOverlaySeries(selection);
        RecordSessionMilestone(
            "MainChartOverlaySelectionChanged",
            selection == null ? "Warning" : "Success",
            selection?.DisplayKey,
            selection == null
                ? "Main chart stacked overlay selection cleared."
                : $"Main chart stacked overlay selection changed to {selection.DisplayName}.");
    }

    private async Task RenderMainChartAsync(ChartDataContext ctx)
    {
        if (ctx.Data1 == null)
            return;

        var mode = _viewModel.ChartState.MainChartDisplayMode;
        var selections = GetStackedSelections();
        var canStack = mode == MainChartDisplayMode.Stacked && selections.Count >= 2;
        var isCumulative = mode == MainChartDisplayMode.Summed;
        var overlaySeries = canStack ? await _overlayBuilder.BuildAsync(ctx, selections) : null;

        await _renderingContract.RenderAsync(
            new CartesianMetricChartRenderRequest(
                RenderingRoute,
                ctx,
                _viewModel.MetricState.SelectedSeries,
                _viewModel.MetricState.ResolutionTableName,
                canStack,
                isCumulative,
                overlaySeries,
                CapabilityContract: CartesianMetricCapabilityContract.Create(ChartProgramKind.Main)),
            CreateRenderHost());
    }

    private List<MetricSeriesSelection> GetStackedSelections()
    {
        return _viewModel.MetricState.SelectedSeries
            .Where(selection => selection.QuerySubtype != null)
            .ToList();
    }

    private void UpdateOverlayControlsVisibility(IReadOnlyList<MetricSeriesSelection> selections)
    {
        if (_controller.OverlaySubtypePanel == null || _controller.OverlaySubtypeCombo == null)
            return;

        var isStacked = _viewModel.ChartState.MainChartDisplayMode == MainChartDisplayMode.Stacked;
        var canStack = isStacked && selections.Count >= 2;
        _controller.OverlaySubtypePanel.Visibility = canStack ? Visibility.Visible : Visibility.Collapsed;
        _controller.OverlaySubtypeCombo.IsEnabled = canStack;
    }

    private CartesianMetricChartRenderHost CreateRenderHost()
    {
        return new CartesianMetricChartRenderHost(_controller.Chart, _viewModel.ChartState);
    }

    private void RecordSessionMilestone(string kind, string outcome, string? operation, string note)
    {
        _viewModel.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = kind,
            Outcome = outcome,
            MetricType = _viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = _viewModel.MetricState.SelectedSeries.Count,
            SelectedDisplayKeys = _viewModel.MetricState.SelectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = _viewModel.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = _viewModel.ChartState.LastContext?.ActualSeriesCount ?? 0,
            ContextSignature = EvidenceDiagnosticsBuilder.BuildContextSignature(_viewModel.ChartState.LastContext),
            Operation = operation,
            Note = note
        });
    }
}
