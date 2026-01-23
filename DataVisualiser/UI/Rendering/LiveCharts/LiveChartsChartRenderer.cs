using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.UI.Controls;
using DataVisualiser.UI.Defaults;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Rendering.LiveCharts;

public sealed class LiveChartsChartRenderer : IChartRenderer
{
    public void Apply(IChartSurface surface, UiChartRenderModel model)
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

    private static UIElement BuildFacetPieContent(UiChartRenderModel model)
    {
        var columnCount = Math.Min(5, Math.Max(1, model.Facets.Count));
        var pieSize = GetFacetPieSize(columnCount);

        var grid = new Grid
        {
                Margin = ChartUiDefaults.ChartContentMargin
        };

        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
                Width = new GridLength(1, GridUnitType.Star)
        });

        if (model.Legend?.IsVisible == true)
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                    Width = GridLength.Auto
            });

        var uniform = new UniformGrid
        {
                Columns = columnCount
        };

        foreach (var facet in model.Facets)
        {
            var panel = new StackPanel
            {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center
            };

            if (!string.IsNullOrWhiteSpace(facet.Title))
                panel.Children.Add(new TextBlock
                {
                        Text = facet.Title,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 6)
                });

            var pieChart = new PieChart
            {
                    LegendLocation = LegendLocation.None,
                    Hoverable = model.Interactions?.Hoverable ?? ChartUiDefaults.DefaultHoverable,
                    Width = pieSize,
                    Height = pieSize
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
                        Values = new ChartValues<double>
                        {
                                value.Value
                        },
                        DataLabels = true,
                        Fill = sliceBrush,
                        Stroke = sliceBrush,
                        LabelPoint = point => $"{series.Name}: {MathHelper.FormatDisplayedValue(point.Y)} ({point.Participation:P0})"
                });
            }

            panel.Children.Add(pieChart);
            uniform.Children.Add(panel);
        }

        Grid.SetColumn(uniform, 0);
        grid.Children.Add(uniform);

        if (model.Legend?.IsVisible == true)
        {
            var legendItems = BuildFacetLegendItems(model);
            if (legendItems != null)
            {
                var legendContainer = LegendToggleManager.CreateLegendContainer(legendItems);
                Grid.SetColumn(legendContainer, 1);
                grid.Children.Add(legendContainer);
            }
        }

        return grid;
    }

    private static UIElement BuildSingleChartContent(UiChartRenderModel model)
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

        if (model.Legend?.IsVisible == true)
        {
            var legendItems = BuildSeriesLegendItemsControl(chart.Series);
            var legendContainer = LegendToggleManager.CreateLegendContainer(legendItems);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                    Width = new GridLength(1, GridUnitType.Star)
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                    Width = GridLength.Auto
            });
            Grid.SetColumn(chart, 0);
            Grid.SetColumn(legendContainer, 1);
            grid.Children.Add(chart);
            grid.Children.Add(legendContainer);
            return grid;
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

    private static double GetFacetPieSize(int columnCount)
    {
        return columnCount switch
        {
                <= 3 => 240,
                4 => 200,
                _ => 160
        };
    }

    private static ItemsControl? BuildFacetLegendItems(UiChartRenderModel model)
    {
        var firstFacet = model.Facets.FirstOrDefault();
        if (firstFacet == null || firstFacet.Series.Count == 0)
            return null;

        var entries = firstFacet.Series.Select(series => new LegendEntry(series.Name ?? "Series", CreateBrush(series.Color) ?? Brushes.Gray)).ToList();

        var itemsControl = new ItemsControl
        {
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = entries
        };

        itemsControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));

        var stackFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);

        var rectFactory = new FrameworkElementFactory(typeof(Rectangle));
        rectFactory.SetValue(FrameworkElement.WidthProperty, 12.0);
        rectFactory.SetValue(FrameworkElement.HeightProperty, 12.0);
        rectFactory.SetBinding(Shape.FillProperty, new Binding(nameof(LegendEntry.Stroke)));
        rectFactory.SetValue(Shape.StrokeProperty, Brushes.White);
        rectFactory.SetValue(Shape.StrokeThicknessProperty, 0.5);
        rectFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 6, 0));

        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(LegendEntry.Title)));
        textFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        textFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);

        stackFactory.AppendChild(rectFactory);
        stackFactory.AppendChild(textFactory);

        itemsControl.ItemTemplate = new DataTemplate
        {
                VisualTree = stackFactory
        };

        return itemsControl;
    }

    private static ItemsControl BuildSeriesLegendItemsControl(SeriesCollection seriesCollection)
    {
        var entries = seriesCollection.OfType<Series>().Select(series => new LegendEntry(string.IsNullOrWhiteSpace(series.Title) ? "Series" : series.Title, series.Stroke ?? Brushes.Gray)).ToList();

        var itemsControl = new ItemsControl
        {
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = entries
        };

        itemsControl.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(StackPanel)));

        var stackFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);

        var rectFactory = new FrameworkElementFactory(typeof(Rectangle));
        rectFactory.SetValue(FrameworkElement.WidthProperty, 12.0);
        rectFactory.SetValue(FrameworkElement.HeightProperty, 12.0);
        rectFactory.SetBinding(Shape.FillProperty, new Binding(nameof(LegendEntry.Stroke)));
        rectFactory.SetValue(Shape.StrokeProperty, Brushes.White);
        rectFactory.SetValue(Shape.StrokeThicknessProperty, 0.5);
        rectFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 6, 0));

        var textFactory = new FrameworkElementFactory(typeof(TextBlock));
        textFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(LegendEntry.Title)));
        textFactory.SetValue(TextBlock.ForegroundProperty, Brushes.White);
        textFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);

        stackFactory.AppendChild(rectFactory);
        stackFactory.AppendChild(textFactory);

        itemsControl.ItemTemplate = new DataTemplate
        {
                VisualTree = stackFactory
        };

        return itemsControl;
    }

    private static Brush? CreateBrush(Color? color)
    {
        if (!color.HasValue)
            return null;

        var brush = new SolidColorBrush(color.Value);
        brush.Freeze();
        return brush;
    }

    private sealed record LegendEntry(string Title, Brush Stroke);
}