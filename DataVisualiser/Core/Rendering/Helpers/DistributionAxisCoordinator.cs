using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Helpers;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class DistributionAxisCoordinator
{
    public static void ConfigureYAxis(CartesianChart targetChart, IList<double> mins, IList<double> ranges, int bucketCount)
    {
        if (targetChart.AxisY.Count == 0)
            targetChart.AxisY.Add(new Axis());

        var allValues = new List<double>();
        for (var i = 0; i < bucketCount; i++)
        {
            if (!double.IsNaN(mins[i]))
                allValues.Add(mins[i]);

            if (!double.IsNaN(mins[i]) && !double.IsNaN(ranges[i]))
                allValues.Add(mins[i] + ranges[i]);
        }

        if (allValues.Count == 0)
        {
            var defaultYAxis = targetChart.AxisY[0];
            defaultYAxis.MinValue = 0;
            defaultYAxis.MaxValue = 100;
            defaultYAxis.ShowLabels = true;
            return;
        }

        var min = Math.Floor(allValues.Min() / DistributionDefaults.YAxisRoundingStep) * DistributionDefaults.YAxisRoundingStep;
        var max = Math.Ceiling(allValues.Max() / DistributionDefaults.YAxisRoundingStep) * DistributionDefaults.YAxisRoundingStep;

        var rawRange = max - min;
        var pad = Math.Max(DistributionDefaults.MinYAxisPadding, rawRange * DistributionDefaults.YAxisPaddingPercentage);
        var yMin = Math.Max(0, min - pad);
        var yMax = max + pad;

        var yAxis = targetChart.AxisY[0];
        yAxis.MinValue = yMin;
        yAxis.MaxValue = yMax;

        var step = MathHelper.RoundToThreeSignificantDigits((yMax - yMin) / 8.0);
        if (step > 0 && !double.IsNaN(step) && !double.IsInfinity(step))
            yAxis.Separator = new Separator
            {
                Step = step
            };

        yAxis.LabelFormatter = value => MathHelper.FormatDisplayedValue(value);
        yAxis.ShowLabels = true;
        yAxis.Title = "Value";
    }

    public static void ConfigureXAxis(CartesianChart chart, IReadOnlyList<string> bucketLabels, string xAxisTitle)
    {
        if (chart.AxisX.Count == 0)
            chart.AxisX.Add(new Axis());

        var axis = chart.AxisX[0];
        axis.MinValue = double.NaN;
        axis.MaxValue = double.NaN;
        axis.Labels = bucketLabels.ToList();
        axis.Title = xAxisTitle;
        axis.ShowLabels = true;
        axis.Separator = new Separator
        {
            Step = 1,
            IsEnabled = false
        };
    }
}
