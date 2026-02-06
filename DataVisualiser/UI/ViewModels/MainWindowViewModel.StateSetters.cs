using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

public partial class MainWindowViewModel
{
    // ======================
    // STATE SETTERS (UI -> VM)
    // ======================

    public void SetMainVisible(bool value)
    {
        ChartState.IsMainVisible = value;
        RequestChartUpdate(true, "Main");
    }

    public void SetNormalizedVisible(bool value)
    {
        ChartState.IsNormalizedVisible = value;
        RequestChartUpdate(true, "Norm");
    }

    public void SetDiffRatioVisible(bool value)
    {
        ChartState.IsDiffRatioVisible = value;
        RequestChartUpdate(true, "DiffRatio");
    }

    public void SetMainChartDisplayMode(MainChartDisplayMode mode)
    {
        if (ChartState.MainChartDisplayMode == mode)
            return;

        ChartState.MainChartDisplayMode = mode;
        RequestChartUpdate(false, "Main");
    }

    public void SetDistributionVisible(bool value)
    {
        ChartState.IsDistributionVisible = value;
        RequestChartUpdate(true, "Distribution");
    }

    public void SetBarPieVisible(bool value)
    {
        ChartState.IsBarPieVisible = value;
        RequestChartUpdate(true, "BarPie");
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

    public void SetWeeklyTrendAverageVisible(bool value)
    {
        ChartState.ShowAverage = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendAverageWindow(WeekdayTrendAverageWindow window)
    {
        ChartState.WeekdayTrendAverageWindow = window;
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

    public void SetStackedOverlaySeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedStackedOverlaySeries = selection;
        RequestChartUpdate(false, "Main");
    }

    public void SetNormalizedPrimarySeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedNormalizedPrimarySeries = selection;
    }

    public void SetNormalizedSecondarySeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedNormalizedSecondarySeries = selection;
    }

    public void SetDiffRatioPrimarySeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedDiffRatioPrimarySeries = selection;
    }

    public void SetDiffRatioSecondarySeries(MetricSeriesSelection? selection)
    {
        ChartState.SelectedDiffRatioSecondarySeries = selection;
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

    public void SetBarPieBucketCount(int bucketCount)
    {
        if (ChartState.BarPieBucketCount == bucketCount)
            return;

        ChartState.BarPieBucketCount = bucketCount;
        RaiseSelectionStateChanged();
    }

    public void SetSelectedMetricType(string? metric)
    {
        MetricState.SelectedMetricType = metric;
        RaiseSelectionStateChanged();
    }

    public void SetSelectedSeries(IEnumerable<MetricSeriesSelection> selections)
    {
        MetricState.SetSeriesSelections(selections);
        RaiseSelectionStateChanged();
    }

    public void SetDateRange(DateTime? from, DateTime? to)
    {
        MetricState.FromDate = from;
        MetricState.ToDate = to;
        RaiseSelectionStateChanged();
    }

    public void SetResolutionTableName(string? tableName)
    {
        MetricState.ResolutionTableName = tableName;
        RaiseSelectionStateChanged();
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
