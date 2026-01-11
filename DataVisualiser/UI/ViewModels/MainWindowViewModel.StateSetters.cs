using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.ViewModels;

public partial class MainWindowViewModel
{
    // ======================
    // STATE SETTERS (UI -> VM)
    // ======================

    public void SetMainVisible(bool value)
    {
        ChartState.IsMainVisible = value;
        RequestChartUpdate();
    }

    public void SetNormalizedVisible(bool value)
    {
        ChartState.IsNormalizedVisible = value;
        RequestChartUpdate();
    }

    public void SetDiffRatioVisible(bool value)
    {
        ChartState.IsDiffRatioVisible = value;
        RequestChartUpdate();
    }

    public void SetDistributionVisible(bool value)
    {
        ChartState.IsDistributionVisible = value;
        RequestChartUpdate();
    }

    public void SetWeeklyTrendMondayVisible(bool value)
    {
        ChartState.ShowMonday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendTuesdayVisible(bool value)
    {
        ChartState.ShowTuesday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendWednesdayVisible(bool value)
    {
        ChartState.ShowWednesday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendThursdayVisible(bool value)
    {
        ChartState.ShowThursday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendFridayVisible(bool value)
    {
        ChartState.ShowFriday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendSaturdayVisible(bool value)
    {
        ChartState.ShowSaturday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendSundayVisible(bool value)
    {
        ChartState.ShowSunday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetNormalizationMode(NormalizationMode mode)
    {
        ChartState.SelectedNormalizationMode = mode;
    }

    public void SetDistributionMode(DistributionMode mode)
    {
        ChartState.SelectedDistributionMode = mode;
        RequestChartUpdate(false, "Distribution");
    }

    public void SetDistributionSeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedDistributionSeries = selection;
        RequestChartUpdate(false, "Distribution");
    }

    public void SetWeekdayTrendSeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedWeekdayTrendSeries = selection;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetTransformPrimarySeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedTransformPrimarySeries = selection;
    }

    public void SetTransformSecondarySeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedTransformSecondarySeries = selection;
    }

    public void SetDistributionFrequencyShading(DistributionMode mode, bool useFrequencyShading)
    {
        ChartState.GetDistributionSettings(mode).UseFrequencyShading = useFrequencyShading;
    }

    public void SetDistributionIntervalCount(DistributionMode mode, int intervalCount)
    {
        ChartState.GetDistributionSettings(mode).IntervalCount = intervalCount;
    }

    public void SetSelectedMetricType(string? metric)
    {
        MetricState.SelectedMetricType = metric;
    }

    public void SetSelectedSeries(IEnumerable<MetricSeriesSelection> selections)
    {
        MetricState.SetSeriesSelections(selections);
    }

    public void SetDateRange(DateTime? from, DateTime? to)
    {
        MetricState.FromDate = from;
        MetricState.ToDate = to;
    }

    public void SetLoadingMetricTypes(bool isLoading)
    {
        UiState.IsLoadingMetricTypes = isLoading;
    }

    public void SetLoadingSubtypes(bool isLoading)
    {
        UiState.IsLoadingSubtypes = isLoading;
    }

    public void SetLoadingData(bool isLoading)
    {
        UiState.IsLoadingData = isLoading;
    }
}
