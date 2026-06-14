using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformChartEmptyStateHelper
{
    public static void Apply(CartesianChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        chart.AxisX.Clear();
        chart.AxisY.Clear();
        chart.AxisX.Add(new Axis
        {
            Title = ChartUiDefaults.AxisTitleTime,
            MinValue = 0,
            MaxValue = 1,
            Separator = new Separator { Step = 1 },
            ShowLabels = false
        });
        chart.AxisY.Add(new Axis
        {
            Title = ChartUiDefaults.AxisTitleValue,
            MinValue = -0.2,
            MaxValue = 0.2,
            Separator = new Separator { Step = 0.1 },
            LabelFormatter = value => MathHelper.FormatDisplayedValue(value),
            ShowLabels = true
        });
    }
}
