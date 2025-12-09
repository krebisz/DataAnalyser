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
            string primarySmoothedLabel = FormatSeriesLabel(model, isPrimary: true, isSmoothed: true);
            string primaryRawLabel = FormatSeriesLabel(model, isPrimary: true, isSmoothed: false);

            var smoothedPrimary = ChartHelper.CreateLineSeries(
                primarySmoothedLabel,
                5,
                2,
                model.PrimaryColor);

            foreach (var value in model.PrimarySmoothed)
                smoothedPrimary.Values.Add(value);

            var rawPrimary = ChartHelper.CreateLineSeries(
                primaryRawLabel,
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
                string secondarySmoothedLabel = FormatSeriesLabel(model, isPrimary: false, isSmoothed: true);
                string secondaryRawLabel = FormatSeriesLabel(model, isPrimary: false, isSmoothed: false);

                var smoothedSecondary = ChartHelper.CreateLineSeries(
                    secondarySmoothedLabel,
                    5,
                    2,
                    model.SecondaryColor);

                foreach (var value in model.SecondarySmoothed)
                    smoothedSecondary.Values.Add(value);

                var rawSecondary = ChartHelper.CreateLineSeries(
                    secondaryRawLabel,
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
                xAxis.ShowLabels = true; // Re-enable labels when rendering data

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
            // Re-enable Y-axis labels when rendering data
            if (targetChart.AxisY.Count > 0)
            {
                targetChart.AxisY[0].ShowLabels = true;
            }
        }

        /// <summary>
        /// Formats series labels according to the new requirements:
        /// - For operation charts: "MetricType:subtype1 (operation) MetricType:subtype2 (smooth/raw)"
        /// - For independent charts: "MetricType:subtype (smooth/raw)"
        /// </summary>
        private static string FormatSeriesLabel(ChartRenderModel model, bool isPrimary, bool isSmoothed)
        {
            string smoothRaw = isSmoothed ? "smooth" : "raw";

            // If we have metric type and subtype information, use the new format
            if (!string.IsNullOrEmpty(model.MetricType))
            {
                if (model.IsOperationChart && !string.IsNullOrEmpty(model.OperationType))
                {
                    // Operation chart format: "Weight:fat_free_mass (-) Weight:body_fat_mass (smooth)"
                    string primarySubtype = model.PrimarySubtype ?? string.Empty;
                    string secondarySubtype = model.SecondarySubtype ?? string.Empty;
                    string operation = model.OperationType;

                    if (isPrimary)
                    {
                        // Primary series shows: "Weight:fat_free_mass (-) Weight:body_fat_mass (smooth/raw)"
                        return $"{model.MetricType}:{primarySubtype} ({operation}) {model.MetricType}:{secondarySubtype} ({smoothRaw})";
                    }
                    else
                    {
                        // Secondary series (shouldn't happen for operation charts, but handle it)
                        return $"{model.MetricType}:{secondarySubtype} ({smoothRaw})";
                    }
                }
                else
                {
                    // Independent chart format: "Weight:fat_free_mass (smooth/raw)"
                    string subtype = isPrimary ? (model.PrimarySubtype ?? string.Empty) : (model.SecondarySubtype ?? string.Empty);

                    if (!string.IsNullOrEmpty(subtype))
                    {
                        return $"{model.MetricType}:{subtype} ({smoothRaw})";
                    }
                    else
                    {
                        // Fallback if no subtype
                        return $"{model.MetricType} ({smoothRaw})";
                    }
                }
            }

            // Fallback to old format if metric type info is not available
            string seriesName = isPrimary ? model.PrimarySeriesName : model.SecondarySeriesName;
            return $"{seriesName} ({smoothRaw})";
        }
    }
}
