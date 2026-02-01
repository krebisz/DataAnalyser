namespace DataVisualiser.UI.ViewModels;

public partial class MainWindowViewModel
{
    // ======================
    // CHART VISIBILITY TOGGLES
    // ======================

    private void ToggleChartVisibility(string chartName, Func<bool> getVisibility, Action<bool> setVisibility)
    {
        var (visibilityArgs, updateArgs) = _chartVisibilityController.ToggleChartVisibility(chartName, getVisibility, setVisibility);
        ChartVisibilityChanged?.Invoke(this, visibilityArgs);
        if (updateArgs != null)
            ChartUpdateRequested?.Invoke(this, updateArgs);
    }

    public void ToggleMain()
    {
        ToggleChartVisibility("Main", () => ChartState.IsMainVisible, v => ChartState.IsMainVisible = v);
    }

    public void ToggleStacked()
    {
        ToggleChartVisibility("Stacked", () => ChartState.IsStackedVisible, v => ChartState.IsStackedVisible = v);
    }

    public void ToggleNorm()
    {
        ToggleChartVisibility("Norm", () => ChartState.IsNormalizedVisible, v => ChartState.IsNormalizedVisible = v);
    }

    public void ToggleDiffRatio()
    {
        ToggleChartVisibility("DiffRatio", () => ChartState.IsDiffRatioVisible, v => ChartState.IsDiffRatioVisible = v);
    }

    public void ToggleDiffRatioOperation()
    {
        var updateArgs = _chartVisibilityController.ToggleDiffRatioOperation();
        if (updateArgs != null)
            ChartUpdateRequested?.Invoke(this, updateArgs);
    }

    public void ToggleDistribution()
    {
        ToggleChartVisibility("Distribution", () => ChartState.IsDistributionVisible, v => ChartState.IsDistributionVisible = v);
    }

    public void ToggleWeeklyTrend()
    {
        ToggleChartVisibility("WeeklyTrend", () => ChartState.IsWeeklyTrendVisible, v => ChartState.IsWeeklyTrendVisible = v);
    }

    public void ToggleWeekdayTrendChartType()
    {
        _chartVisibilityController.ToggleWeekdayTrendChartType();
    }

    public void ToggleDistributionChartType()
    {
        _chartVisibilityController.ToggleDistributionChartType();
    }

    public void ToggleTransformPanel()
    {
        var updateArgs = _chartVisibilityController.ToggleTransformPanel();
        if (updateArgs != null)
            ChartUpdateRequested?.Invoke(this, updateArgs);
    }

    public void ToggleBarPie()
    {
        ToggleChartVisibility("BarPie", () => ChartState.IsBarPieVisible, v => ChartState.IsBarPieVisible = v);
    }

    public void RequestChartUpdate(bool isVisibilityOnlyToggle = false, string? toggledChartName = null)
    {
        var updateArgs = _chartVisibilityController.BuildChartUpdateRequest(isVisibilityOnlyToggle, toggledChartName);
        if (updateArgs != null)
            ChartUpdateRequested?.Invoke(this, updateArgs);
    }

    public void CompleteInitialization()
    {
        _isInitializing = false;
        RequestChartUpdate();
    }
}
