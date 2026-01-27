using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataVisualiser.UI.Rendering.ECharts;

/// <summary>
///     Minimal stub renderer that keeps all decision-making in .NET while
///     making the renderer seam ready for a future WebView2-based ECharts host.
/// </summary>
public sealed class EChartsChartRenderer : IChartRenderer
{
    public Task ApplyAsync(IChartSurface surface, UiChartRenderModel model, CancellationToken cancellationToken = default)
    {
        if (surface == null)
            throw new ArgumentNullException(nameof(surface));
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        surface.SetTitle(model.Title);
        surface.SetIsVisible(model.IsVisible);

        surface.SetChartContent(BuildPlaceholder(model.Title));
        return Task.CompletedTask;
    }

    private static UIElement BuildPlaceholder(string? title)
    {
        var text = string.IsNullOrWhiteSpace(title)
            ? "ECharts renderer seam is ready (placeholder)."
            : $"ECharts renderer seam is ready for '{title}' (placeholder).";

        return new Border
        {
            BorderBrush = Brushes.DimGray,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12),
            Margin = new Thickness(6),
            Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.DimGray,
                TextWrapping = TextWrapping.Wrap
            }
        };
    }
}
