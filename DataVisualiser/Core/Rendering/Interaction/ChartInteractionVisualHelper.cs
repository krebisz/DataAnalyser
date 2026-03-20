using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DataVisualiser.Core.Configuration.Defaults;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Interaction;

public static class ChartInteractionVisualHelper
{
    public static Border CreateBorder(StackPanel stack)
    {
        return new Border
        {
                Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Padding = new Thickness(4),
                Child = stack,
                MaxWidth = 600
        };
    }

    public static TextBlock CreateHoverText(bool isBold = false)
    {
        return new TextBlock
        {
                Foreground = Brushes.White,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(6, 2, 6, 4)
        };
    }

    public static void PositionHoverPopup(Popup popup, double horizontalOffset = RenderingDefaults.HoverPopupOffsetPx, double verticalOffset = RenderingDefaults.HoverPopupOffsetPx)
    {
        if (popup == null)
            return;

        if (!popup.IsOpen)
            popup.IsOpen = true;

        popup.HorizontalOffset = 0;
        popup.VerticalOffset = 0;
        popup.HorizontalOffset = horizontalOffset;
        popup.VerticalOffset = verticalOffset;
    }

    public static void UpdateVerticalLineForChart(ref CartesianChart chart, int index, ref AxisSection? sectionField)
    {
        if (chart == null || index < 0)
            return;

        var axis = GetXAxisSafely(chart);
        if (axis?.Sections == null)
            return;

        TryRemoveAxisSection(axis, sectionField);

        try
        {
            var line = new AxisSection
            {
                    Value = index,
                    SectionWidth = 0,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
            };

            axis.Sections.Add(line);
            sectionField = line;
        }
        catch
        {
            sectionField = null;
        }
    }

    public static void UpdateHorizontalLineForChart(ref CartesianChart chart, double value, ref AxisSection? sectionField, Brush? stroke = null, double strokeThickness = 1, DoubleCollection? dashArray = null, string? label = null)
    {
        if (chart == null || double.IsNaN(value) || double.IsInfinity(value))
            return;

        var axis = GetYAxisSafely(chart);
        if (axis == null)
            return;

        axis.Sections ??= new SectionsCollection();
        TryRemoveAxisSection(axis, sectionField);

        try
        {
            var line = new AxisSection
            {
                    Value = value,
                    SectionWidth = 0,
                    Stroke = stroke ?? Brushes.Black,
                    StrokeThickness = strokeThickness,
                    StrokeDashArray = dashArray,
                    Fill = Brushes.Transparent,
                    Label = label
            };

            axis.Sections.Add(line);
            sectionField = line;
        }
        catch
        {
            sectionField = null;
        }
    }

    public static void RemoveAxisSection(CartesianChart? chart, AxisSection? axisSection)
    {
        if (axisSection == null)
            return;

        var axis = chart != null ? GetXAxisSafely(chart) : null;
        if (axis?.Sections == null)
            return;

        try
        {
            axis.Sections.Remove(axisSection);
        }
        catch
        {
        }
    }

    public static void RemoveAxisSectionFromYAxis(CartesianChart? chart, AxisSection? axisSection)
    {
        if (axisSection == null)
            return;

        var axis = GetYAxisSafely(chart);
        if (axis?.Sections == null)
            return;

        try
        {
            axis.Sections.Remove(axisSection);
        }
        catch
        {
        }
    }

    private static Axis? GetXAxisSafely(CartesianChart? chart)
    {
        if (chart == null)
            return null;

        try
        {
            if (chart.AxisX != null && chart.AxisX.Count > 0)
                return chart.AxisX[0];
        }
        catch
        {
        }

        return null;
    }

    private static Axis? GetYAxisSafely(CartesianChart? chart)
    {
        if (chart == null)
            return null;

        try
        {
            if (chart.AxisY != null && chart.AxisY.Count > 0)
                return chart.AxisY[0];
        }
        catch
        {
        }

        return null;
    }

    private static void TryRemoveAxisSection(Axis axis, AxisSection? section)
    {
        if (section == null)
            return;

        try
        {
            axis.Sections.Remove(section);
        }
        catch
        {
        }
    }
}
