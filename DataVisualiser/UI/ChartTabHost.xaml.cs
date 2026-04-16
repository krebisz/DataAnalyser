using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DataVisualiser.UI;

[ContentProperty(nameof(ChartContent))]
public sealed class ChartTabHost : Control
{
    public static readonly DependencyProperty ChartContentProperty =
        DependencyProperty.Register(
            nameof(ChartContent),
            typeof(object),
            typeof(ChartTabHost),
            new PropertyMetadata(null));

    private MetricSelectionPanel? _selectionSurface;

    public object? ChartContent
    {
        get => GetValue(ChartContentProperty);
        set => SetValue(ChartContentProperty, value);
    }

    public MetricSelectionPanel SelectionSurface
    {
        get
        {
            ApplyTemplate();
            return _selectionSurface ?? throw new InvalidOperationException("ChartTabHost template does not define PART_SelectionPanel.");
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _selectionSurface = GetTemplateChild("PART_SelectionPanel") as MetricSelectionPanel;
    }
}
