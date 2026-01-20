using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Controls;

public sealed class BarPieChartControllerAdapter : IChartController
{
    private readonly BarPieChartController _controller;
    private readonly MainWindowViewModel _viewModel;
    private readonly Func<bool> _isInitializing;
    private readonly Func<bool> _getIsVisible;
    private readonly Action<bool> _setIsVisible;
    private readonly Func<Task> _renderAsync;
    private readonly Action _clear;

    public BarPieChartControllerAdapter(
        BarPieChartController controller,
        MainWindowViewModel viewModel,
        Func<bool> isInitializing,
        Func<bool> getIsVisible,
        Action<bool> setIsVisible,
        Func<Task> renderAsync,
        Action clear)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _getIsVisible = getIsVisible ?? throw new ArgumentNullException(nameof(getIsVisible));
        _setIsVisible = setIsVisible ?? throw new ArgumentNullException(nameof(setIsVisible));
        _renderAsync = renderAsync ?? throw new ArgumentNullException(nameof(renderAsync));
        _clear = clear ?? throw new ArgumentNullException(nameof(clear));
    }

    public string Key => ChartControllerKeys.BarPie;
    public bool RequiresPrimaryData => true;
    public bool RequiresSecondaryData => false;
    public ChartPanelController Panel => _controller.Panel;
    public ButtonBase ToggleButton => _controller.ToggleButton;

    public void Initialize()
    {
    }

    public Task RenderAsync(ChartDataContext context)
    {
        if (!_getIsVisible())
            return Task.CompletedTask;

        return _renderAsync();
    }

    public void Clear(ChartState state)
    {
        _clear();
    }

    public void ResetZoom()
    {
    }

    public async void OnToggleRequested(object? sender, EventArgs e)
    {
        var isVisible = !_getIsVisible();
        _setIsVisible(isVisible);
        await _renderAsync();
    }

    public async void OnDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        if (_getIsVisible())
            await _renderAsync();
    }

    public async void OnBucketCountChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        if (_controller.BucketCountCombo.SelectedItem is ComboBoxItem selectedItem && TryGetIntervalCount(selectedItem.Tag, out var bucketCount))
            _viewModel.ChartState.BarPieBucketCount = bucketCount;

        if (_getIsVisible())
            await _renderAsync();
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
