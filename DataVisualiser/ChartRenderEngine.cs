using DataVisualiser.Class;
using DataVisualiser.Helper;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace DataVisualiser.Charts
{
    /// <summary>
    /// Responsible for rendering LiveCharts objects from a ChartRenderModel.
    /// To stay decoupled from MainWindow helper methods (NormalizeYAxis / AdjustChartHeight), accept delegates.
    /// </summary>
    public sealed class ChartRenderEngine
    {
        private readonly Action<Axis, System.Collections.Generic.List<HealthMetricData>, System.Collections.Generic.List<double>> _normalizeYAxis;
        private readonly Action<CartesianChart, double> _adjustHeight;

        public ChartRenderEngine(
            Action<Axis, System.Collections.Generic.List<HealthMetricData>, System.Collections.Generic.List<double>> normalizeYAxisDelegate,
            Action<CartesianChart, double> adjustHeightDelegate)
        {
            _normalizeYAxis = normalizeYAxisDelegate ?? throw new ArgumentNullException(nameof(normalizeYAxisDelegate));
            _adjustHeight = adjustHeightDelegate ?? throw new ArgumentNullException(nameof(adjustHeightDelegate));
        }

        public void Render(CartesianChart targetChart, ChartRenderModel model, double minHeight = 400.0)
        {
            if (targetChart == null) throw new ArgumentNullException(nameof(targetChart));
            if (model == null) throw new ArgumentNullException(nameof(model));

            targetChart.Series.Clear();

            // create smoothed and raw series for primary
            var smoothedPrimary = ChartHelper.CreateLineSeries($"{model.PrimarySeriesName} (Smoothed)", 5, 2, model.PrimaryColor);
            foreach (var v in model.PrimarySmoothed) smoothedPrimary.Values.Add(v);

            var rawPrimary = ChartHelper.CreateLineSeries($"{model.PrimarySeriesName} (Raw)", 3, 1, Colors.DarkGray);
            foreach (var v in model.PrimaryRaw) rawPrimary.Values.Add(v);

            targetChart.Series.Add(smoothedPrimary);
            targetChart.Series.Add(rawPrimary);

            // optional secondary
            if (model.SecondaryRaw != null && model.SecondarySmoothed != null)
            {
                var smoothedSecondary = ChartHelper.CreateLineSeries($"{model.SecondarySeriesName} (Smoothed)", 5, 2, model.SecondaryColor);
                foreach (var v in model.SecondarySmoothed) smoothedSecondary.Values.Add(v);

                var rawSecondary = ChartHelper.CreateLineSeries($"{model.SecondarySeriesName} (Raw)", 3, 1, Colors.DarkGray);
                foreach (var v in model.SecondaryRaw) rawSecondary.Values.Add(v);

                targetChart.Series.Add(smoothedSecondary);
                targetChart.Series.Add(rawSecondary);
            }

            // store timestamps mapping on chart (index -> DateTime)
            // caller should provide access to the dictionary; we'll use a simple attach via Tag if needed
            // But in your project you have a dictionary _chartTimestamps in MainWindow — the MainWindow will set that after Render.

            // Configure X axis label formatter & separator (same logic used before)
            if (targetChart.AxisX.Count > 0)
            {
                var xAxis = targetChart.AxisX[0];
                xAxis.Title = "Time";

                var intervalIndices = model.IntervalIndices;
                var normalizedIntervals = model.NormalizedIntervals;
                var tickInterval = model.TickInterval;

                // compute first-occurrence indices to display labels
                var labelDataPointIndices = new System.Collections.Generic.HashSet<int>();
                var seenIntervals = new System.Collections.Generic.HashSet<int>();
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
                        double roundedValue = MathHelper.RoundToThreeSignificantDigits(value);
                        int dataPointIndex = (int)Math.Round(roundedValue);
                        if (dataPointIndex >= 0 &&
                            dataPointIndex < intervalIndices.Count &&
                            labelDataPointIndices.Contains(dataPointIndex))
                        {
                            int intervalIndex = intervalIndices[dataPointIndex];
                            if (intervalIndex >= 0 && intervalIndex < normalizedIntervals.Count)
                            {
                                return ChartHelper.FormatDateTimeLabel(normalizedIntervals[intervalIndex], tickInterval);
                            }
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
                    TickInterval.Week => Math.Max(4, Math.Min(8, normalizedIntervals.Count)),
                    TickInterval.Day => Math.Max(7, Math.Min(14, normalizedIntervals.Count)),
                    TickInterval.Hour => Math.Max(12, Math.Min(24, normalizedIntervals.Count)),
                    _ => Math.Min(10, normalizedIntervals.Count)
                };

                var sortedLabelIndices = labelDataPointIndices.OrderBy(x => x).ToList();
                double step = 1.0;

                if (sortedLabelIndices.Count > intervalsToShow && sortedLabelIndices.Count > 1)
                {
                    var totalSpacing = sortedLabelIndices.Last() - sortedLabelIndices.First();
                    var averageSpacing = totalSpacing / (double)(sortedLabelIndices.Count - 1);
                    step = Math.Max(1.0, Math.Ceiling(averageSpacing * (sortedLabelIndices.Count / (double)intervalsToShow)));
                }
                else if (sortedLabelIndices.Count > 0)
                {
                    if (sortedLabelIndices.Count > 1)
                    {
                        var minSpacing = sortedLabelIndices
                            .Zip(sortedLabelIndices.Skip(1), (a, b) => b - a)
                            .Where(s => s > 0)
                            .DefaultIfEmpty(1)
                            .Min();
                        step = Math.Max(1.0, minSpacing);
                    }
                }

                step = MathHelper.RoundToThreeSignificantDigits(step);
                xAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
                xAxis.Labels = null;
                xAxis.MinValue = double.NaN;
                xAxis.MaxValue = double.NaN;
            }

            // Y axis normalization: the render engine doesn't know how to call your NormalizeYAxis (it needs rawData list).
            // So caller can call the delegate it passed to the ChartRenderEngine, or we can expect it to call NormalizeYAxis itself.
            // Here we don't change Y axis — caller (MainWindow) will call NormalizeYAxis with the synthetic data afterwards.
        }
    }
}
