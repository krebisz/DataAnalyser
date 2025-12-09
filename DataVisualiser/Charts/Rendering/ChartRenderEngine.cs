using DataVisualiser.Charts.Helpers;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;
using System.Windows.Media;

namespace DataVisualiser.Charts.Rendering
{
    public sealed class ChartRenderEngine
    {
        public void Render(CartesianChart targetChart, ChartRenderModel model, double minHeight = 400.0)
        {
            if (targetChart == null) throw new ArgumentNullException(nameof(targetChart));
            if (model == null) throw new ArgumentNullException(nameof(model));

            targetChart.Series.Clear();

            // ============================
            //  PRIMARY SERIES
            // ============================
            var smoothedPrimary = ChartHelper.CreateLineSeries(
                $"{model.PrimarySeriesName} (Smoothed)",
                5,
                2,
                model.PrimaryColor);

            foreach (var value in model.PrimarySmoothed)
                smoothedPrimary.Values.Add(value);

            var rawPrimary = ChartHelper.CreateLineSeries(
                $"{model.PrimarySeriesName} (Raw)",
                3,
                1,
                Colors.DarkGray);

            foreach (var value in model.PrimaryRaw)
                rawPrimary.Values.Add(value);

            targetChart.Series.Add(smoothedPrimary);
            targetChart.Series.Add(rawPrimary);

            // ============================
            //  SECONDARY SERIES (IF ANY)
            // ============================
            if (model.SecondarySmoothed != null && model.SecondaryRaw != null)
            {
                var smoothedSecondary = ChartHelper.CreateLineSeries(
                    $"{model.SecondarySeriesName} (Smoothed)",
                    5,
                    2,
                    model.SecondaryColor);

                foreach (var value in model.SecondarySmoothed)
                    smoothedSecondary.Values.Add(value);

                var rawSecondary = ChartHelper.CreateLineSeries(
                    $"{model.SecondarySeriesName} (Raw)",
                    3,
                    1,
                    Colors.DarkGray);

                foreach (var value in model.SecondaryRaw)
                    rawSecondary.Values.Add(value);

                targetChart.Series.Add(smoothedSecondary);
                targetChart.Series.Add(rawSecondary);
            }

            // ============================
            //  UNIFORM X-AXIS RESTORATION
            // ============================
            //
            // We force every data point to live at x = 0,1,2,... so tick spacing is uniform.
            // The formatter retrieves proper datetime labels from ChartRenderModel.
            //
            // ============================
            if (targetChart.AxisX.Count > 0)
            {
                var xAxis = targetChart.AxisX[0];
                xAxis.Title = "Time";

                var timestamps = model.NormalizedIntervals;
                int total = timestamps.Count;

                // === LABEL FORMATTER ===
                xAxis.LabelFormatter = indexAsDouble =>
                {
                    int idx = (int)indexAsDouble;
                    if (idx < 0 || idx >= total)
                        return string.Empty;

                    return ChartHelper.FormatDateTimeLabel(
                        timestamps[idx],
                        model.TickInterval);
                };

                // === UNIFORM STEP ===
                // Roughly 10 visible ticks, auto-adjusted
                int desiredTicks = 10;
                double step = Math.Max(1, total / (double)desiredTicks);

                // Cleaner numbers
                step = MathHelper.RoundToThreeSignificantDigits(step);

                xAxis.Separator = new Separator
                {
                    Step = step
                };

                // These three settings ensure LiveCharts does NOT compress or stretch based on time
                xAxis.MinValue = 0;
                xAxis.MaxValue = total - 1;
                xAxis.Labels = null;   // Force formatter rather than static label array
            }

            // Y-axis is already uniform by definition (numeric axis automatically spaces evenly).
        }
    }
}
