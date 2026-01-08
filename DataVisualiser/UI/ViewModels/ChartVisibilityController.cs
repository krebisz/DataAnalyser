using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

public sealed class ChartVisibilityController
{
    private readonly ChartState _chartState;
    private readonly Func<bool> _isInitializing;
    private readonly Action<string?> _onPropertyChanged;

    public ChartVisibilityController(ChartState chartState, Func<bool> isInitializing, Action<string?> onPropertyChanged)
    {
        _chartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _onPropertyChanged = onPropertyChanged ?? throw new ArgumentNullException(nameof(onPropertyChanged));
    }

    public(ChartVisibilityChangedEventArgs VisibilityArgs, ChartUpdateRequestedEventArgs? UpdateArgs) ToggleChartVisibility(string chartName, Func<bool> getVisibility, Action<bool> setVisibility)
    {
        var newVisibility = !getVisibility();
        setVisibility(newVisibility);
        _onPropertyChanged(nameof(ChartState));

        var visibilityArgs = new ChartVisibilityChangedEventArgs
        {
                ChartName = chartName,
                IsVisible = newVisibility
        };

        var updateArgs = BuildChartUpdateRequest(true, chartName);

        return (visibilityArgs, updateArgs);
    }

    public ChartUpdateRequestedEventArgs? ToggleDiffRatioOperation()
    {
        _chartState.IsDiffRatioDifferenceMode = !_chartState.IsDiffRatioDifferenceMode;
        _onPropertyChanged(nameof(ChartState));

        // Re-render if visible
        if (_chartState.IsDiffRatioVisible && _chartState.LastContext != null)
            return BuildChartUpdateRequest();

        return null;
    }

    public void ToggleWeekdayTrendChartType()
    {
        _chartState.IsWeekdayTrendPolarMode = !_chartState.IsWeekdayTrendPolarMode;
        _onPropertyChanged(nameof(ChartState));
    }

    public ChartUpdateRequestedEventArgs? ToggleTransformPanel()
    {
        _chartState.IsTransformPanelVisible = !_chartState.IsTransformPanelVisible;
        _onPropertyChanged(nameof(ChartState));
        return BuildChartUpdateRequest(true, "Transform");
    }

    public ChartUpdateRequestedEventArgs? BuildChartUpdateRequest(bool isVisibilityOnlyToggle = false, string? toggledChartName = null)
    {
        if (_isInitializing())
            return null;

        return new ChartUpdateRequestedEventArgs
        {
                ShowMain = _chartState.IsMainVisible,
                ShowNormalized = _chartState.IsNormalizedVisible,
                ShowDiffRatio = _chartState.IsDiffRatioVisible,
                ShowWeekly = _chartState.IsWeeklyVisible,
                ShowHourly = _chartState.IsHourlyVisible,
                ShowWeeklyTrend = _chartState.IsWeeklyTrendVisible,
                ShowTransformPanel = _chartState.IsTransformPanelVisible,
                ShouldRenderCharts = _chartState.LastContext != null,
                IsVisibilityOnlyToggle = isVisibilityOnlyToggle,
                ToggledChartName = toggledChartName
        };
    }
}