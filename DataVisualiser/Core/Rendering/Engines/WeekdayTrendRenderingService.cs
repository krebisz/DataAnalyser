using System.Diagnostics;
using System.Globalization;
using System.Windows.Media;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Engines;

/// <summary>
///     Handles rendering of weekday trend charts (both Cartesian and Polar modes).
///     Extracts complex weekday trend rendering logic from MainWindow.
/// </summary>
public sealed class WeekdayTrendRenderingService
{
    public const int BucketCount = 7;

    private static readonly Brush[] WeekdayStrokes =
    {
            Brushes.SteelBlue,
            Brushes.CadetBlue,
            Brushes.SeaGreen,
            Brushes.OliveDrab,
            Brushes.Goldenrod,
            Brushes.OrangeRed,
            Brushes.IndianRed
    };

    /// <summary>
    ///     Renders a weekday trend chart based on the chart state mode (Cartesian or Polar).
    ///     Note: Polar mode uses a CartesianChart with special axis configuration.
    /// </summary>
    public void RenderWeekdayTrendChart(WeekdayTrendResult result, ChartState chartState, CartesianChart chartCartesian, CartesianChart chartPolar)
    {
        if (chartState.IsWeekdayTrendPolarMode)
            RenderPolarChart(result, chartState, chartPolar);
        else
            RenderCartesianChart(result, chartState, chartCartesian);
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
                Title = "Time",
                MinValue = result.From.Ticks,
                MaxValue = result.To.Ticks,
                LabelFormatter = v => new DateTime((long)v).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        });

        chart.AxisY.Add(new Axis
        {
                Title = result.Unit ?? "Value",
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

            chart.Series.Add(new LineSeries
            {
                    Title = series.Day.ToString(),
                    Values = values,
                    PointGeometry = showPoints ? DefaultGeometries.Circle : null,
                    PointGeometrySize = showPoints ? 6 : 0,
                    LineSmoothness = 0.3,
                    Fill = Brushes.Transparent,
                    StrokeThickness = 2,
                    Stroke = WeekdayStrokes[dayIndex]
            });
        }
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
                Title = "Day of Week",
                MinValue = 0,
                MaxValue = 360,
                LabelFormatter = v =>
                {
                    // Convert angle (0-360) to day name
                    var dayIndex = (int)Math.Round(v / (360.0 / 7.0)) % BucketCount;
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
                Title = result.Unit ?? "Value",
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
            var baseAngleDegrees = dayIndex * 360.0 / 7.0;

            // For each time point in the series, plot at the day's angle with the value as radius
            foreach (var point in series.Points)
                values.Add(new ObservablePoint(baseAngleDegrees, point.Value));

            chart.Series.Add(new LineSeries
            {
                    Title = series.Day.ToString(),
                    Values = values,
                    LineSmoothness = 0.3,
                    StrokeThickness = 2,
                    Stroke = WeekdayStrokes[dayIndex],
                    Fill = Brushes.Transparent,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 6
            });
        }
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