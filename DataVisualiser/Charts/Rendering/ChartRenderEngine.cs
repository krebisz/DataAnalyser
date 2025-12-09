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

            // === Primary series ===
            var smoothedPrimary = ChartHelper.CreateLineSeries(
                $"{model.PrimarySeriesName} (Smoothed)",
                5,
                2,
                model.PrimaryColor);

            foreach (var v in model.PrimarySmoothed)
                smoothedPrimary.Values.Add(v);

            // RAW is always grey for visual separation
            var rawPrimary = ChartHelper.CreateLineSeries(
                $"{model.PrimarySeriesName} (Raw)",
                3,
                1,
                Colors.DarkGray);

            foreach (var v in model.PrimaryRaw)
                rawPrimary.Values.Add(v);

            targetChart.Series.Add(smoothedPrimary);
            targetChart.Series.Add(rawPrimary);

            // === Secondary series (if present) ===
            if (model.SecondarySmoothed != null && model.SecondaryRaw != null)
            {
                var smoothedSecondary = ChartHelper.CreateLineSeries(
                    $"{model.SecondarySeriesName} (Smoothed)",
                    5,
                    2,
                    model.SecondaryColor);

                foreach (var v in model.SecondarySmoothed)
                    smoothedSecondary.Values.Add(v);

                // RAW is also grey here
                var rawSecondary = ChartHelper.CreateLineSeries(
                    $"{model.SecondarySeriesName} (Raw)",
                    3,
                    1,
                    Colors.DarkGray);

                foreach (var v in model.SecondaryRaw)
                    rawSecondary.Values.Add(v);

                targetChart.Series.Add(smoothedSecondary);
                targetChart.Series.Add(rawSecondary);
            }

            // X-axis configuration (unchanged)
            if (targetChart.AxisX.Count > 0)
            {
                var xAxis = targetChart.AxisX[0];
                xAxis.Title = "Time";

                var intervalIndices = model.IntervalIndices;
                var normalizedIntervals = model.NormalizedIntervals;
                var tickInterval = model.TickInterval;

                var labelDataPointIndices = new HashSet<int>();
                var seenIntervals = new HashSet<int>();

                for (int i = 0; i < intervalIndices.Count; i++)
                {
                    int intervalIndex = intervalIndices[i];
                    if (!seenIntervals.Contains(intervalIndex))
                    {
                        labelDataPointIndices.Add(i);
                        seenIntervals.Add(intervalIndex);
                    }
                }

                xAxis.LabelFormatter = value =>
                {
                    try
                    {
                        int index = (int)value;
                        if (index < 0 || index >= intervalIndices.Count)
                            return string.Empty;

                        int intervalIndex = intervalIndices[index];
                        if (intervalIndex >= 0 && intervalIndex < normalizedIntervals.Count)
                        {
                            if (labelDataPointIndices.Contains(index))
                                return ChartHelper.FormatDateTimeLabel(normalizedIntervals[intervalIndex], tickInterval);
                        }

                        return string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                };

                var intervalsToShow = tickInterval switch
                {
                    TickInterval.Month => Math.Max(6, Math.Min(12, normalizedIntervals.Count)),
                    TickInterval.Week => Math.Max(6, Math.Min(26, normalizedIntervals.Count)),
                    _ => Math.Max(6, Math.Min(10, normalizedIntervals.Count)),
                };

                double step = Math.Max(1, normalizedIntervals.Count / (double)intervalsToShow);
                step = MathHelper.RoundToThreeSignificantDigits(step);

                xAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
                xAxis.Labels = null;
                xAxis.MinValue = double.NaN;
                xAxis.MaxValue = double.NaN;
            }

            // Y-axis normalization is handled by ChartUpdateCoordinator after Render.
        }

    }
}
