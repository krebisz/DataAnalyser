using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Core.Configuration.Defaults;
using LiveCharts.Defaults;
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

        var chart = data.Points.FirstOrDefault()?.ChartPoint?.ChartView as CartesianChart;
        var allowDelta = chart != null && RenderingDefaults.DeltaTooltipChartNames.Contains(chart.Name);

        var minValue = double.PositiveInfinity;
        var maxValue = double.NegativeInfinity;
        var validCount = 0;

        foreach (var point in data.Points)
        {
            var seriesTitle = point?.Series?.Title;
            if (string.IsNullOrWhiteSpace(seriesTitle))
                seriesTitle = "Series";

            var yValue = point?.ChartPoint?.Y ?? double.NaN;
            var formatted = double.IsNaN(yValue) ? "N/A" : MathHelper.FormatDisplayedValue(yValue);

            var colorBrush = point?.Series?.Stroke ?? Brushes.Gray;
            _root.Children.Add(CreateLegendRow(colorBrush, $"{seriesTitle}: {formatted}"));

            if (!double.IsNaN(yValue) && !double.IsInfinity(yValue))
            {
                validCount++;
                if (yValue < minValue)
                    minValue = yValue;
                if (yValue > maxValue)
                    maxValue = yValue;
            }
        }

        if (!allowDelta)
            return;

        if (chart != null && string.Equals(chart.Name, RenderingDefaults.TransformChartName, StringComparison.OrdinalIgnoreCase))
        {
            if (TryGetGlobalMinMax(chart, out var globalMin, out var globalMax))
            {
                var delta = globalMax - globalMin;
                var formattedDelta = MathHelper.FormatDisplayedValue(delta);
                _root.Children.Add(CreateTextBlock($"Δ: {formattedDelta}", FontWeights.Normal));
            }

            return;
        }

        if (validCount >= 2 && minValue <= maxValue)
        {
            var delta = maxValue - minValue;
            var formattedDelta = MathHelper.FormatDisplayedValue(delta);
            _root.Children.Add(CreateTextBlock($"Δ: {formattedDelta}", FontWeights.Normal));
        }
    }

    private static bool TryGetGlobalMinMax(CartesianChart chart, out double min, out double max)
    {
        min = double.PositiveInfinity;
        max = double.NegativeInfinity;
        var validCount = 0;

        foreach (var series in chart.Series.OfType<Series>())
        {
            if (series.Visibility == Visibility.Collapsed || series.Values == null)
                continue;

            foreach (var value in series.Values)
            {
                if (!TryExtractNumeric(value, out var numeric))
                    continue;
                if (double.IsNaN(numeric) || double.IsInfinity(numeric))
                    continue;

                validCount++;
                if (numeric < min)
                    min = numeric;
                if (numeric > max)
                    max = numeric;
            }
        }

        return validCount > 0 && min <= max;
    }

    private static bool TryExtractNumeric(object value, out double numeric)
    {
        switch (value)
        {
            case double d:
                numeric = d;
                return true;
            case float f:
                numeric = f;
                return true;
            case decimal m:
                numeric = (double)m;
                return true;
            case ObservablePoint point:
                numeric = point.Y;
                return true;
            default:
                numeric = 0;
                return false;
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
