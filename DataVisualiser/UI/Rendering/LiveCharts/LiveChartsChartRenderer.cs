using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataVisualiser.UI.Defaults;
using LiveCharts;
using LiveCharts.Wpf;
using DataVisualiser.Shared.Helpers;

namespace DataVisualiser.UI.Rendering.LiveCharts;

public sealed class LiveChartsChartRenderer : IChartRenderer
{
    public void Apply(IChartSurface surface, ChartRenderModel model)
    {
        if (surface == null)
            throw new ArgumentNullException(nameof(surface));
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        surface.SetTitle(model.Title);
        surface.SetIsVisible(model.IsVisible);

        if (model.Facets.Count > 0)
        {
            surface.SetChartContent(BuildFacetPieContent(model));
            return;
        }

        surface.SetChartContent(BuildSingleChartContent(model));
    }

    private static UIElement BuildFacetPieContent(ChartRenderModel model)
    {
        var wrap = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = ChartUiDefaults.ChartContentMargin
        };

        foreach (var facet in model.Facets)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10)
            };

            if (!string.IsNullOrWhiteSpace(facet.Title))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = facet.Title,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 6)
                });
            }

            var pieChart = new PieChart
            {
                LegendLocation = GetLegendLocation(model.Legend),
                Hoverable = model.Interactions?.Hoverable ?? ChartUiDefaults.DefaultHoverable,
                MinHeight = ChartUiDefaults.ChartMinHeight,
                MinWidth = 250
            };

            foreach (var series in facet.Series)
            {
                var value = series.Values.FirstOrDefault();
                if (!value.HasValue)
                    continue;

                var sliceBrush = CreateBrush(series.Color);
                pieChart.Series.Add(new PieSeries
                {
                    Title = series.Name ?? string.Empty,
                    Values = new ChartValues<double> { value.Value },
                    DataLabels = true,
                    Fill = sliceBrush,
                    Stroke = sliceBrush,
                    LabelPoint = point => $"{series.Name}: {MathHelper.FormatDisplayedValue(point.Y)} ({point.Participation:P0})"
                });
            }

            panel.Children.Add(pieChart);
            wrap.Children.Add(panel);
        }

        return wrap;
    }

    private static UIElement BuildSingleChartContent(ChartRenderModel model)
    {
        if (model.Series.Count == 0)
            return new Grid();

        var chart = new CartesianChart
        {
            LegendLocation = GetLegendLocation(model.Legend),
            Zoom = GetZoomOptions(model.Interactions),
            Pan = GetPanOptions(model.Interactions),
            Hoverable = model.Interactions?.Hoverable ?? ChartUiDefaults.DefaultHoverable,
            Margin = ChartUiDefaults.ChartContentMargin,
            MinHeight = ChartUiDefaults.ChartMinHeight
        };

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
        {
            var axisModel = model.AxesX[0];
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
            chart.AxisX.Add(axis);
        }

        if (model.AxesY.Count > 0)
        {
            var axisModel = model.AxesY[0];
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
            chart.AxisY.Add(axis);
        }

        return chart;
    }

    private static LegendLocation GetLegendLocation(ChartLegendModel? legend)
    {
        if (legend == null || !legend.IsVisible)
            return LegendLocation.None;

        return legend.Placement switch
        {
            ChartLegendPlacement.Left => LegendLocation.Left,
            ChartLegendPlacement.Right => LegendLocation.Right,
            ChartLegendPlacement.Top => LegendLocation.Top,
            ChartLegendPlacement.Bottom => LegendLocation.Bottom,
            _ => LegendLocation.Right
        };
    }

    private static ZoomingOptions GetZoomOptions(ChartInteractionModel? interactions)
    {
        if (interactions == null)
            return ChartUiDefaults.DefaultZoom;

        if (interactions.EnableZoomX && interactions.EnableZoomY)
            return ZoomingOptions.Xy;
        if (interactions.EnableZoomY)
            return ZoomingOptions.Y;
        if (interactions.EnableZoomX)
            return ZoomingOptions.X;

        return ZoomingOptions.None;
    }

    private static PanningOptions GetPanOptions(ChartInteractionModel? interactions)
    {
        if (interactions == null)
            return ChartUiDefaults.DefaultPan;

        if (interactions.EnablePanX && interactions.EnablePanY)
            return PanningOptions.Xy;
        if (interactions.EnablePanY)
            return PanningOptions.Y;
        if (interactions.EnablePanX)
            return PanningOptions.X;

        return PanningOptions.None;
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
