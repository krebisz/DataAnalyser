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

    public void SetWeeklyVisible(bool value)
    {
        ChartState.IsWeeklyVisible = value;
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

    public void SetWeeklyFrequencyShading(bool useFrequencyShading)
    {
        ChartState.UseWeeklyFrequencyShading = useFrequencyShading;
    }

    public void SetHourlyFrequencyShading(bool useFrequencyShading)
    {
        ChartState.UseHourlyFrequencyShading = useFrequencyShading;
    }

    public void SetWeeklyIntervalCount(int intervalCount)
    {
        ChartState.WeeklyIntervalCount = intervalCount;
    }

    public void SetHourlyIntervalCount(int intervalCount)
    {
        ChartState.HourlyIntervalCount = intervalCount;
    }

    public void SetSelectedMetricType(string? metric)
    {
        MetricState.SelectedMetricType = metric;
    }

    public void SetSelectedSubtypes(IEnumerable<string?> subtypes)
    {
        MetricState.SetSubtypes(subtypes);
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