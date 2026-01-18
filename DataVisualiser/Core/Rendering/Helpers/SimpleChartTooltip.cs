using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

public sealed class SimpleChartTooltip : UserControl, IChartTooltip, INotifyPropertyChanged
{
    private readonly StackPanel _root;
    private TooltipSelectionMode? _selectionMode;

    public SimpleChartTooltip()
    {
        _root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(6)
        };

        Content = _root;
    }

    public TooltipData? Data
    {
        get => (TooltipData?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(TooltipData), typeof(SimpleChartTooltip),
                    new PropertyMetadata(null, OnDataChanged));

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
            var sharedText = data.XFormatter != null
                    ? data.XFormatter(data.SharedValue.Value)
                    : data.SharedValue.Value.ToString(CultureInfo.InvariantCulture);

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
            var formatted = data.YFormatter != null
                    ? data.YFormatter(yValue)
                    : double.IsNaN(yValue) ? "N/A" : yValue.ToString(CultureInfo.InvariantCulture);

            _root.Children.Add(CreateTextBlock($"{seriesTitle}: {formatted}", FontWeights.Normal));
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

    private void NotifyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
