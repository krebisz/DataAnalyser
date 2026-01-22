using System.Diagnostics;
using System.Globalization;
using System.Windows.Media;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Engines;

/// <summary>
///     Handles rendering of weekday trend charts (Cartesian, Polar, and Scatter modes).
///     Extracts complex weekday trend rendering logic from MainWindow.
/// </summary>
public sealed class WeekdayTrendRenderingService
{
    public const int BucketCount = WeekdayTrendDefaults.BucketCount;

    /// <summary>
    ///     Renders a weekday trend chart based on the chart state mode (Cartesian, Polar, or Scatter).
    ///     Note: Polar mode uses a CartesianChart with special axis configuration.
    /// </summary>
    public void RenderWeekdayTrendChart(WeekdayTrendResult result, ChartState chartState, CartesianChart chartCartesian, CartesianChart chartPolar)
    {
        switch (chartState.WeekdayTrendChartMode)
        {
            case WeekdayTrendChartMode.Polar:
                RenderPolarChart(result, chartState, chartPolar);
                break;
            case WeekdayTrendChartMode.Scatter:
                RenderScatterChart(result, chartState, chartCartesian);
                break;
            default:
                RenderCartesianChart(result, chartState, chartCartesian);
                break;
        }
    }

    private void RenderCartesianChart(WeekdayTrendResult result, ChartState chartState, CartesianChart chart)
    {
        ChartHelper.ClearChart(chart, chartState.ChartTimestamps);
        chart.AxisX.Clear();
        chart.AxisY.Clear();

        if (result == null || result.SeriesByDay.Count == 0)
            return;

        chart.AxisX.Add(new Axis
        {
                Title = ChartRenderDefaults.AxisTitleTime,
                MinValue = result.From.Ticks,
                MaxValue = result.To.Ticks,
                LabelFormatter = v => new DateTime((long)v).ToString(ChartRenderDefaults.WeekdayDateLabelFormat, CultureInfo.InvariantCulture)
        });

        chart.AxisY.Add(new Axis
        {
                Title = result.Unit ?? ChartRenderDefaults.AxisTitleValue,
                MinValue = result.GlobalMin,
                MaxValue = result.GlobalMax
        });

        for (var dayIndex = 0; dayIndex <= BucketCount - 1; dayIndex++)
        {
            if (!result.SeriesByDay.TryGetValue(dayIndex, out var series))
                continue;

            if (!IsDayEnabled(dayIndex, chartState))
                continue;

            Debug.WriteLine($"[WeekdayTrend] Day={series.Day}, Points={series.Points.Count}");
            var values = new ChartValues<ObservablePoint>();
            foreach (var point in series.Points)
                values.Add(new ObservablePoint(point.Date.Ticks, point.Value));

            var showPoints = values.Count <= 1;

            var title = series.Day.ToString();
            chart.Series.Add(new LineSeries
            {
                    Title = title,
                    Values = values,
                    PointGeometry = showPoints ? DefaultGeometries.Circle : null,
                    PointGeometrySize = showPoints ? ChartRenderDefaults.WeekdayPointSize : 0,
                    LineSmoothness = ChartRenderDefaults.WeekdayLineSmoothness,
                    Fill = Brushes.Transparent,
                    StrokeThickness = ChartRenderDefaults.WeekdayLineStrokeThickness,
                    Stroke = WeekdayTrendDefaults.WeekdayStrokes[dayIndex],
                    LabelPoint = point => $"{title}: {MathHelper.FormatDisplayedValue(point.Y)}"
            });
        }

        if (chartState.ShowAverage)
            AddAverageLineSeries(result, chartState, chart);
    }

    private void RenderPolarChart(WeekdayTrendResult result, ChartState chartState, CartesianChart chart)
    {
        ChartHelper.ClearChart(chart, chartState.ChartTimestamps);
        chart.AxisX.Clear();
        chart.AxisY.Clear();

        if (result == null || result.SeriesByDay.Count == 0)
            return;

        // Configure axes for polar-like display
        chart.AxisX.Add(new Axis
        {
                Title = ChartRenderDefaults.AxisTitleDayOfWeek,
                MinValue = ChartRenderDefaults.PolarAxisMinValue,
                MaxValue = ChartRenderDefaults.PolarAxisMaxValue,
                LabelFormatter = v =>
                {
                    // Convert angle (0-360) to day name
                    var dayIndex = (int)Math.Round(v / (360.0 / BucketCount)) % BucketCount;
                    return dayIndex switch
                    {
                            0 => "Mon",
                            1 => "Tue",
                            2 => "Wed",
                            3 => "Thu",
                            4 => "Fri",
                            5 => "Sat",
                            6 => "Sun",
                            _ => ""
                    };
                }
        });

        chart.AxisY.Add(new Axis
        {
                Title = result.Unit ?? ChartRenderDefaults.AxisTitleValue,
                MinValue = result.GlobalMin,
                MaxValue = result.GlobalMax
        });

        // Convert each day's data to polar-like coordinates
        // X (angle): 0° = Monday, 51.43° = Tuesday, ... 308.57° = Sunday (360/7 per day)
        // Y (radius): value
        for (var dayIndex = 0; dayIndex <= BucketCount - 1; dayIndex++)
        {
            if (!result.SeriesByDay.TryGetValue(dayIndex, out var series))
                continue;

            if (!IsDayEnabled(dayIndex, chartState))
                continue;

            var values = new ChartValues<ObservablePoint>();
            // Base angle for this day (in degrees)
            var baseAngleDegrees = dayIndex * ChartRenderDefaults.PolarAxisMaxValue / BucketCount;

            // For each time point in the series, plot at the day's angle with the value as radius
            foreach (var point in series.Points)
                values.Add(new ObservablePoint(baseAngleDegrees, point.Value));

            var title = series.Day.ToString();
            chart.Series.Add(new LineSeries
            {
                    Title = title,
                    Values = values,
                    LineSmoothness = ChartRenderDefaults.WeekdayLineSmoothness,
                    StrokeThickness = ChartRenderDefaults.WeekdayLineStrokeThickness,
                    Stroke = WeekdayTrendDefaults.WeekdayStrokes[dayIndex],
                    Fill = Brushes.Transparent,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = ChartRenderDefaults.WeekdayPointSize,
                    LabelPoint = point => $"{title}: {MathHelper.FormatDisplayedValue(point.Y)}"
            });
        }
    }

    private void RenderScatterChart(WeekdayTrendResult result, ChartState chartState, CartesianChart chart)
    {
        ChartHelper.ClearChart(chart, chartState.ChartTimestamps);
        chart.AxisX.Clear();
        chart.AxisY.Clear();

        if (result == null || result.SeriesByDay.Count == 0)
            return;

        chart.AxisX.Add(new Axis
        {
                Title = ChartRenderDefaults.AxisTitleTime,
                MinValue = result.From.Ticks,
                MaxValue = result.To.Ticks,
                LabelFormatter = v => new DateTime((long)v).ToString(ChartRenderDefaults.WeekdayDateLabelFormat, CultureInfo.InvariantCulture)
        });

        chart.AxisY.Add(new Axis
        {
                Title = result.Unit ?? ChartRenderDefaults.AxisTitleValue,
                MinValue = result.GlobalMin,
                MaxValue = result.GlobalMax
        });

        for (var dayIndex = 0; dayIndex <= BucketCount - 1; dayIndex++)
        {
            if (!result.SeriesByDay.TryGetValue(dayIndex, out var series))
                continue;

            if (!IsDayEnabled(dayIndex, chartState))
                continue;

            var values = new ChartValues<ScatterPoint>();
            foreach (var point in series.Points)
                values.Add(new ScatterPoint(point.Date.Ticks, point.Value, 1));

            var title = series.Day.ToString();
            chart.Series.Add(new ScatterSeries
            {
                    Title = title,
                    Values = values,
                    PointGeometry = DefaultGeometries.Circle,
                    MinPointShapeDiameter = ChartRenderDefaults.WeekdayPointSize,
                    MaxPointShapeDiameter = ChartRenderDefaults.WeekdayPointSize,
                    StrokeThickness = ChartRenderDefaults.WeekdayLineStrokeThickness,
                    Stroke = WeekdayTrendDefaults.WeekdayStrokes[dayIndex],
                    Fill = Brushes.Transparent,
                    LabelPoint = point => $"{title}: {MathHelper.FormatDisplayedValue(point.Y)}"
            });
        }

        if (chartState.ShowAverage)
            AddAverageScatterSeries(result, chartState, chart);
    }

    private static void AddAverageLineSeries(WeekdayTrendResult result, ChartState chartState, CartesianChart chart)
    {
        var values = BuildRunningMeanObservablePoints(result, chartState, ResolveAverageWindow(chartState));
        if (values.Count == 0)
            return;

        chart.Series.Add(new LineSeries
        {
                Title = "Ave",
                Values = values,
                PointGeometry = null,
                PointGeometrySize = 0,
                LineSmoothness = ChartRenderDefaults.WeekdayLineSmoothness,
                Fill = Brushes.Transparent,
                StrokeThickness = ChartRenderDefaults.WeekdayLineStrokeThickness,
                Stroke = Brushes.Black,
                LabelPoint = point => $"Ave: {MathHelper.FormatDisplayedValue(point.Y)}"
        });
    }

    private static void AddAverageScatterSeries(WeekdayTrendResult result, ChartState chartState, CartesianChart chart)
    {
        var values = BuildRunningMeanScatterPoints(result, chartState, ResolveAverageWindow(chartState));
        if (values.Count == 0)
            return;

        chart.Series.Add(new ScatterSeries
        {
                Title = "Ave",
                Values = values,
                PointGeometry = DefaultGeometries.Circle,
                MinPointShapeDiameter = ChartRenderDefaults.WeekdayPointSize,
                MaxPointShapeDiameter = ChartRenderDefaults.WeekdayPointSize,
                StrokeThickness = ChartRenderDefaults.WeekdayLineStrokeThickness,
                Stroke = Brushes.Black,
                Fill = Brushes.Transparent,
                LabelPoint = point => $"Ave: {MathHelper.FormatDisplayedValue(point.Y)}"
        });
    }

    private static ChartValues<ObservablePoint> BuildRunningMeanObservablePoints(WeekdayTrendResult result, ChartState chartState, TimeSpan? window)
    {
        var values = new ChartValues<ObservablePoint>();
        foreach (var point in BuildRunningMeanPoints(result, chartState, window))
            values.Add(new ObservablePoint(point.Ticks, point.Value));

        return values;
    }

    private static ChartValues<ScatterPoint> BuildRunningMeanScatterPoints(WeekdayTrendResult result, ChartState chartState, TimeSpan? window)
    {
        var values = new ChartValues<ScatterPoint>();
        foreach (var point in BuildRunningMeanPoints(result, chartState, window))
            values.Add(new ScatterPoint(point.Ticks, point.Value, 1));

        return values;
    }

    private static IEnumerable<(long Ticks, double Value)> BuildRunningMeanPoints(WeekdayTrendResult result, ChartState chartState, TimeSpan? window)
    {
        var points = new List<(long Ticks, double Value)>();
        foreach (var entry in result.SeriesByDay)
        {
            if (!IsDayEnabled(entry.Key, chartState))
                continue;

            foreach (var point in entry.Value.Points)
                points.Add((point.Date.Ticks, point.Value));
        }

        if (points.Count == 0)
            yield break;

        points.Sort((a, b) => a.Ticks.CompareTo(b.Ticks));

        if (window == null)
        {
            var runningSum = 0.0;
            var runningCount = 0;
            foreach (var point in points)
            {
                runningSum += point.Value;
                runningCount++;
                yield return (point.Ticks, runningSum / runningCount);
            }

            yield break;
        }

        var windowQueue = new Queue<(long Ticks, double Value)>();
        var windowSum = 0.0;
        var windowTicks = window.Value.Ticks;

        foreach (var point in points)
        {
            windowQueue.Enqueue(point);
            windowSum += point.Value;

            while (windowQueue.Count > 0 && point.Ticks - windowQueue.Peek().Ticks > windowTicks)
            {
                var removed = windowQueue.Dequeue();
                windowSum -= removed.Value;
            }

            yield return (point.Ticks, windowSum / windowQueue.Count);
        }
    }

    private static TimeSpan? ResolveAverageWindow(ChartState chartState)
    {
        return chartState.WeekdayTrendAverageWindow switch
        {
                WeekdayTrendAverageWindow.Weekly => TimeSpan.FromDays(7),
                WeekdayTrendAverageWindow.Monthly => TimeSpan.FromDays(30),
                _ => null
        };
    }

    private static bool IsDayEnabled(int dayIndex, ChartState chartState)
    {
        return dayIndex switch
        {
                0 => chartState.ShowMonday,
                1 => chartState.ShowTuesday,
                2 => chartState.ShowWednesday,
                3 => chartState.ShowThursday,
                4 => chartState.ShowFriday,
                5 => chartState.ShowSaturday,
                6 => chartState.ShowSunday,
                _ => false
        };
    }
}