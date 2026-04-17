using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DataVisualiser.UI;

[ContentProperty(nameof(BodyContent))]
public sealed class WorkspaceTabHost : Control
{
    public static readonly DependencyProperty HeaderContentProperty =
        DependencyProperty.Register(
            nameof(HeaderContent),
            typeof(object),
            typeof(WorkspaceTabHost),
            new PropertyMetadata(null));

    public static readonly DependencyProperty BodyContentProperty =
        DependencyProperty.Register(
            nameof(BodyContent),
            typeof(object),
            typeof(WorkspaceTabHost),
            new PropertyMetadata(null));

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public object? BodyContent
    {
        get => GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }
}
