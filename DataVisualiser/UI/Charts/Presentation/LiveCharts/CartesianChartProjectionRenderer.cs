using System.Windows.Media;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Helpers;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation.LiveCharts;

internal static class CartesianChartProjectionRenderer
{
    public static void Apply(CartesianChart chart, UiChartRenderModel model)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(model);

        chart.Series.Clear();
        chart.AxisX.Clear();
        chart.AxisY.Clear();

        foreach (var series in model.Series)
        {
            var values = new ChartValues<double>(series.Values.Select(value => value ?? 0d));
            if (values.Count == 0)
                continue;

            var seriesBrush = CreateBrush(series.Color);
            switch (series.SeriesType)
            {
                case ChartSeriesType.Column:
                    chart.Series.Add(new ColumnSeries
                    {
                        Title = series.Name ?? string.Empty,
                        Values = values,
                        Fill = seriesBrush,
                        Stroke = seriesBrush,
                        LabelPoint = point => MathHelper.FormatDisplayedValue(point.Y)
                    });
                    break;
                case ChartSeriesType.Line:
                default:
                    chart.Series.Add(new LineSeries
                    {
                        Title = series.Name ?? string.Empty,
                        Values = values,
                        Stroke = seriesBrush ?? Brushes.Gray,
                        Fill = Brushes.Transparent
                    });
                    break;
            }
        }

        if (model.AxesX.Count > 0)
            chart.AxisX.Add(BuildAxis(model.AxesX[0]));

        if (model.AxesY.Count > 0)
            chart.AxisY.Add(BuildAxis(model.AxesY[0]));

        ChartThemeStylingHelper.ApplyCartesianChartTheme(chart);
    }

    public static void Clear(CartesianChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        chart.Series.Clear();
        chart.AxisX.Clear();
        chart.AxisY.Clear();
    }

    private static Axis BuildAxis(ChartAxisModel axisModel)
    {
        var axis = new Axis
        {
            Title = axisModel.Title ?? string.Empty
        };

        if (axisModel.Labels != null)
            axis.Labels = axisModel.Labels.ToArray();
        if (axisModel.Min.HasValue)
            axis.MinValue = axisModel.Min.Value;
        if (axisModel.Max.HasValue)
            axis.MaxValue = axisModel.Max.Value;
        if (axisModel.Step.HasValue)
            axis.Separator = new Separator { Step = axisModel.Step.Value };
        if (axisModel.ShowLabels.HasValue)
            axis.ShowLabels = axisModel.ShowLabels.Value;
        if (axisModel.UseDisplayValueFormatter)
        {
            axis.LabelFormatter = value => MathHelper.FormatDisplayedValue(value);
            axis.Labels = null;
        }

        return axis;
    }

    private static Brush? CreateBrush(Color? color)
    {
        if (!color.HasValue)
            return null;

        var brush = new SolidColorBrush(color.Value);
        brush.Freeze();
        return brush;
    }
}
