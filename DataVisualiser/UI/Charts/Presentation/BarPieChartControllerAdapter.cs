using DataVisualiser.UI.Charts.Interfaces;
using System.Windows.Controls;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.BarPie;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.UI.Defaults;
using DataVisualiser.UI.Charts.Presentation.Rendering;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using UiChartRenderModel = DataVisualiser.UI.Charts.Presentation.Rendering.UiChartRenderModel;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class BarPieChartControllerAdapter : ChartControllerAdapterBase, IBarPieChartControllerExtras
{
    private readonly IBarPieChartController _controller;
    private readonly Func<bool> _isInitializing;
    private readonly IBarPieRenderingContract _barPieRenderingContract;
    private readonly IChartRendererResolver _rendererResolver;
    private readonly IChartSurfaceFactory _surfaceFactory;
    private readonly MainWindowViewModel _viewModel;
    private readonly BarPieRenderModelBuilder _renderModelBuilder;
    private IChartRenderer? _renderer;
    private IChartSurface? _surface;

    public BarPieChartControllerAdapter(
        IBarPieChartController controller,
        MainWindowViewModel viewModel,
        Func<bool> isInitializing,
        MetricSelectionService metricSelectionService,
        IBarPieRenderingContract barPieRenderingContract,
        IChartRendererResolver rendererResolver,
        IChartSurfaceFactory surfaceFactory)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _barPieRenderingContract = barPieRenderingContract ?? throw new ArgumentNullException(nameof(barPieRenderingContract));
        _rendererResolver = rendererResolver ?? throw new ArgumentNullException(nameof(rendererResolver));
        _surfaceFactory = surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory));
        _renderModelBuilder = new BarPieRenderModelBuilder(viewModel, metricSelectionService, controller);
    }

    public void InitializeControls()
    {
        _controller.BucketCountCombo.Items.Clear();
        for (var i = 1; i <= 20; i++)
            _controller.BucketCountCombo.Items.Add(new ComboBoxItem
            {
                    Content = i.ToString(),
                    Tag = i
            });

        SelectBarPieBucketCount(_viewModel.ChartState.BarPieBucketCount);
    }

    public Task RenderIfVisibleAsync()
    {
        if (!_viewModel.ChartState.IsBarPieVisible)
            return Task.CompletedTask;

        return RenderBarPieChartAsync();
    }

    public void SelectBucketCount(int bucketCount)
    {
        SelectBarPieBucketCount(bucketCount);
    }

    public string GetDisplayMode()
    {
        return _controller.PieModeRadio.IsChecked == true ? "Pie" : "Bar";
    }

    public override string Key => ChartControllerKeys.BarPie;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderBarPieChartAsync();
    }

    public override void Clear(ChartState state)
    {
        _ = _barPieRenderingContract.ClearAsync(CreateRenderHost());
    }

    public override void ResetZoom()
    {
        _barPieRenderingContract.ResetView(ResolveRenderingRoute(), CreateRenderHost());
    }

    public override bool HasSeries(ChartState state)
    {
        return _barPieRenderingContract.HasRenderableContent(ResolveRenderingRoute(), CreateRenderHost());
    }

    public override void UpdateSubtypeOptions()
    {
    }

    public override void ClearCache()
    {
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleBarPie();
    }

    public async void OnDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        await RerenderBarPieIfVisibleAsync();
    }

    public async void OnBucketCountChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        if (_controller.BucketCountCombo.SelectedItem is ComboBoxItem selectedItem && TryGetIntervalCount(selectedItem.Tag, out var bucketCount))
            _viewModel.SetBarPieBucketCount(bucketCount);

        await RerenderBarPieIfVisibleAsync();
    }

    private void EnsureSurfaceAndRenderer()
    {
        _surface ??= _surfaceFactory.Create(Key, _controller.Panel);
        _renderer ??= _rendererResolver.ResolveRenderer(Key);
    }

    private async Task RenderBarPieChartAsync()
    {
        if (!_viewModel.ChartState.IsBarPieVisible)
        {
            await _barPieRenderingContract.ClearAsync(CreateRenderHost());
            return;
        }

        var isPieMode = _controller.PieModeRadio.IsChecked == true;
        var model = await _renderModelBuilder.BuildAsync(isPieMode);
        await _barPieRenderingContract.RenderAsync(
            new BarPieChartRenderRequest(ResolveRenderingRoute(), model),
            CreateRenderHost());
    }

    private Task RerenderBarPieIfVisibleAsync()
    {
        return _viewModel.ChartState.IsBarPieVisible ? RenderBarPieChartAsync() : Task.CompletedTask;
    }

    private BarPieRenderingRoute ResolveRenderingRoute()
    {
        return BarPieRenderingRouteResolver.Resolve(_controller.PieModeRadio.IsChecked == true);
    }

    private BarPieChartRenderHost CreateRenderHost()
    {
        EnsureSurfaceAndRenderer();
        return new BarPieChartRenderHost(_surface!, _renderer!, _rendererResolver.ResolveKind(Key), _viewModel.ChartState.IsBarPieVisible);
    }

    private void SelectBarPieBucketCount(int bucketCount)
    {
        foreach (var item in _controller.BucketCountCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is int taggedInterval && taggedInterval == bucketCount)
            {
                _controller.BucketCountCombo.SelectedItem = item;
                return;
            }
    }

    private static bool TryGetIntervalCount(object? tag, out int intervalCount)
    {
        switch (tag)
        {
            case int direct:
                intervalCount = direct;
                return true;
            case string tagValue when int.TryParse(tagValue, out var parsed):
                intervalCount = parsed;
                return true;
            default:
                intervalCount = 0;
                return false;
        }
    }
}
