using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DataVisualiser.Shared.Helpers;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

public sealed class SimpleChartTooltip : UserControl, IChartTooltip, INotifyPropertyChanged
{
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(TooltipData), typeof(SimpleChartTooltip), new PropertyMetadata(null, OnDataChanged));

    private readonly StackPanel _root;
    private TooltipSelectionMode? _selectionMode;

    public SimpleChartTooltip()
    {
        _root = new StackPanel
        {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(6),
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 230))
        };

        Content = _root;
    }

    public TooltipData? Data
    {
        get => (TooltipData?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public TooltipSelectionMode? SelectionMode
    {
        get => _selectionMode;
        set
        {
            if (_selectionMode == value)
                return;

            _selectionMode = value;
            NotifyChanged(nameof(SelectionMode));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SimpleChartTooltip tooltip)
        {
            tooltip.UpdateContent(e.NewValue as TooltipData);
            tooltip.NotifyChanged(nameof(Data));
        }
    }

    private void UpdateContent(TooltipData? data)
    {
        _root.Children.Clear();

        if (data == null)
            return;

        if (data.SharedValue.HasValue)
        {
            var sharedText = data.XFormatter != null ? data.XFormatter(data.SharedValue.Value) : MathHelper.FormatDisplayedValue(data.SharedValue.Value);

            _root.Children.Add(CreateTextBlock(sharedText, FontWeights.Bold));
        }

        if (data.Points == null || data.Points.Count == 0)
            return;

        foreach (var point in data.Points)
        {
            var seriesTitle = point?.Series?.Title;
            if (string.IsNullOrWhiteSpace(seriesTitle))
                seriesTitle = "Series";

            var yValue = point?.ChartPoint?.Y ?? double.NaN;
            var formatted = double.IsNaN(yValue) ? "N/A" : MathHelper.FormatDisplayedValue(yValue);

            var colorBrush = point?.Series?.Stroke ?? Brushes.Gray;
            _root.Children.Add(CreateLegendRow(colorBrush, $"{seriesTitle}: {formatted}"));
        }
    }

    private static TextBlock CreateTextBlock(string text, FontWeight weight)
    {
        return new TextBlock
        {
                Text = text,
                FontWeight = weight,
                Margin = new Thickness(0, 2, 0, 0)
        };
    }

    private static UIElement CreateLegendRow(Brush color, string text)
    {
        var row = new StackPanel
        {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 0)
        };

        var key = new Rectangle
        {
                Width = 10,
                Height = 10,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5,
                Margin = new Thickness(0, 2, 6, 0)
        };

        var textBlock = new TextBlock
        {
                Text = text,
                FontWeight = FontWeights.Normal
        };

        row.Children.Add(key);
        row.Children.Add(textBlock);

        return row;
    }

    private void NotifyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}