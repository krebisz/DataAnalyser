using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Helpers;
using DataVisualiser.Helper;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace DataVisualiser.Charts.Rendering
{
    public sealed class ChartRenderEngine
    {
        /// <summary>
        /// Renders chart series based on the provided model.
        /// IMPORTANT: Ensures consistent data ordering:
        /// - PrimaryRaw/PrimarySmoothed = first selected metric subtype
        /// - SecondaryRaw/SecondarySmoothed = second selected metric subtype (when applicable)
        /// This ordering is maintained across all charts and strategies.
        /// </summary>
        public void Render(CartesianChart targetChart, ChartRenderModel model, double minHeight = 400.0)
        {
            ValidateInputs(targetChart, model);
            ClearChart(targetChart);

            if (HasMultiSeriesMode(model))
            {
                RenderMultiSeriesMode(targetChart, model);
            }
            else
            {
                RenderLegacyMode(targetChart, model);
            }

            ConfigureXAxis(targetChart, model);
            ConfigureYAxis(targetChart);
        }

        /// <summary>
        /// Validates that required inputs are not null.
        /// </summary>
        private static void ValidateInputs(CartesianChart targetChart, ChartRenderModel model)
        {
            if (targetChart == null) throw new ArgumentNullException(nameof(targetChart));
            if (model == null) throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Clears all series from the target chart.
        /// </summary>
        private static void ClearChart(CartesianChart targetChart)
        {
            targetChart.Series.Clear();
        }

        /// <summary>
        /// Determines if the model uses multi-series mode (Series array is present and non-empty).
        /// </summary>
        private static bool HasMultiSeriesMode(ChartRenderModel model)
        {
            return model.Series != null && model.Series.Count > 0;
        }

        /// <summary>
        /// Renders chart in multi-series mode, where multiple series are rendered from the Series array.
        /// Each series is aligned to a main timeline and rendered according to the SeriesMode setting.
        /// </summary>
        private void RenderMultiSeriesMode(CartesianChart targetChart, ChartRenderModel model)
        {
            // Reset color palette for this chart to ensure consistent color assignment
            ColourPalette.Reset(targetChart);

            var mainTimeline = GetMainTimeline(model);

            if (model.Series != null)
            {
                foreach (var seriesResult in model.Series)
                {
                    RenderSingleSeries(targetChart, seriesResult, mainTimeline, model.SeriesMode);
                }
            }
        }

        /// <summary>
        /// Gets the main timeline for series alignment, using model.Timestamps or falling back to union of all series timestamps.
        /// </summary>
        private static List<DateTime> GetMainTimeline(ChartRenderModel model)
        {
            var mainTimeline = model.Timestamps;
            if (mainTimeline == null || mainTimeline.Count == 0)
            {
                // Fallback: use union of all series timestamps
                if (model.Series != null)
                {
                    mainTimeline = model.Series
                        .SelectMany(s => s.Timestamps)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToList();
                }
            }
            return mainTimeline ?? new List<DateTime>();
        }

        /// <summary>
        /// Renders a single series result, including both raw and smoothed series based on SeriesMode.
        /// </summary>
        private void RenderSingleSeries(CartesianChart targetChart, SeriesResult seriesResult, List<DateTime> mainTimeline, ChartSeriesMode seriesMode)
        {
            var seriesColor = ColourPalette.Next(targetChart);

            // Align series values to main timeline
            var alignedRaw = AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.RawValues, mainTimeline);
            var alignedSmoothed = seriesResult.Smoothed != null
                ? AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.Smoothed, mainTimeline)
                : null;

            // Render smoothed series if available and enabled
            if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.SmoothedOnly)
            {
                if (alignedSmoothed != null && alignedSmoothed.Count > 0)
                {
                    var smoothedSeries = CreateAndPopulateSeries(
                        $"{seriesResult.DisplayName} (smooth)",
                        5,
                        2,
                        seriesColor,
                        alignedSmoothed);
                    targetChart.Series.Add(smoothedSeries);
                }
            }

            // Render raw series if enabled
            if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.RawOnly)
            {
                var rawSeries = CreateAndPopulateSeries(
                    $"{seriesResult.DisplayName} (raw)",
                    3,
                    1,
                    Colors.DarkGray,
                    alignedRaw);
                targetChart.Series.Add(rawSeries);
            }
        }

        /// <summary>
        /// Creates a line series and populates it with the provided values.
        /// </summary>
        private static LineSeries CreateAndPopulateSeries(string title, int pointSize, int lineThickness, Color color, IList<double> values)
        {
            var series = ChartHelper.CreateLineSeries(title, pointSize, lineThickness, color);
            if (series == null)
            {
                throw new InvalidOperationException($"Failed to create line series: {title}");
            }

            foreach (var value in values)
                series.Values.Add(value);
            return series;
        }

        /// <summary>
        /// Renders chart in legacy mode, using PrimaryRaw/PrimarySmoothed and optionally SecondaryRaw/SecondarySmoothed.
        /// </summary>
        private void RenderLegacyMode(CartesianChart targetChart, ChartRenderModel model)
        {
            RenderPrimarySeries(targetChart, model);
            RenderSecondarySeries(targetChart, model);
        }

        /// <summary>
        /// Renders the primary series (both raw and smoothed) in legacy mode.
        /// </summary>
        private void RenderPrimarySeries(CartesianChart targetChart, ChartRenderModel model)
        {
            string primarySmoothedLabel = FormatSeriesLabel(model, isPrimary: true, isSmoothed: true);
            string primaryRawLabel = FormatSeriesLabel(model, isPrimary: true, isSmoothed: false);

            var smoothedPrimary = CreateAndPopulateSeries(
                primarySmoothedLabel,
                5,
                2,
                model.PrimaryColor,
                model.PrimarySmoothed);

            var rawPrimary = CreateAndPopulateSeries(
                primaryRawLabel,
                3,
                1,
                Colors.DarkGray,
                model.PrimaryRaw);

            targetChart.Series.Add(smoothedPrimary);
            targetChart.Series.Add(rawPrimary);
        }

        /// <summary>
        /// Renders the secondary series (both raw and smoothed) in legacy mode, if available.
        /// </summary>
        private void RenderSecondarySeries(CartesianChart targetChart, ChartRenderModel model)
        {
            if (model.SecondarySmoothed == null || model.SecondaryRaw == null)
                return;

            string secondarySmoothedLabel = FormatSeriesLabel(model, isPrimary: false, isSmoothed: true);
            string secondaryRawLabel = FormatSeriesLabel(model, isPrimary: false, isSmoothed: false);

            var smoothedSecondary = CreateAndPopulateSeries(
                secondarySmoothedLabel,
                5,
                2,
                model.SecondaryColor,
                model.SecondarySmoothed);

            var rawSecondary = CreateAndPopulateSeries(
                secondaryRawLabel,
                3,
                1,
                Colors.DarkGray,
                model.SecondaryRaw);

            targetChart.Series.Add(smoothedSecondary);
            targetChart.Series.Add(rawSecondary);
        }

        /// <summary>
        /// Configures the X-axis with uniform spacing, label formatting, and tick intervals.
        /// Forces every data point to live at x = 0,1,2,... so tick spacing is uniform.
        /// The formatter retrieves proper datetime labels from ChartRenderModel.
        /// </summary>
        private static void ConfigureXAxis(CartesianChart targetChart, ChartRenderModel model)
        {
            if (targetChart.AxisX.Count == 0)
                return;

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

        /// <summary>
        /// Configures the Y-axis by re-enabling labels when rendering data.
        /// Y-axis is already uniform by definition (numeric axis automatically spaces evenly).
        /// </summary>
        private static void ConfigureYAxis(CartesianChart targetChart)
        {
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

        /// <summary>
        /// Aligns a series' values to the main timeline by interpolating/mapping values.
        /// Uses forward-fill for missing values (carries last known value forward).
        /// </summary>
        private static List<double> AlignSeriesToTimeline(
            List<DateTime> seriesTimestamps,
            List<double> seriesValues,
            List<DateTime> mainTimeline)
        {
            if (seriesTimestamps.Count == 0 || seriesValues.Count == 0)
                return mainTimeline.Select(_ => double.NaN).ToList();

            if (seriesTimestamps.Count != seriesValues.Count)
                return mainTimeline.Select(_ => double.NaN).ToList();

            // Create a dictionary for quick lookup
            var valueMap = new Dictionary<DateTime, double>();
            for (int i = 0; i < seriesTimestamps.Count; i++)
            {
                var ts = seriesTimestamps[i];
                var val = seriesValues[i];
                // Use the latest value if there are duplicate timestamps
                valueMap[ts] = val;
            }

            var aligned = new List<double>(mainTimeline.Count);
            double lastValue = double.NaN;

            foreach (var timestamp in mainTimeline)
            {
                // Try exact match first
                if (valueMap.TryGetValue(timestamp, out var exactValue))
                {
                    aligned.Add(exactValue);
                    lastValue = exactValue;
                }
                else
                {
                    // Try to find nearest timestamp (within same day for simplicity)
                    var day = timestamp.Date;
                    var dayMatch = valueMap.Keys.FirstOrDefault(ts => ts.Date == day);

                    if (dayMatch != default(DateTime) && valueMap.TryGetValue(dayMatch, out var dayValue))
                    {
                        aligned.Add(dayValue);
                        lastValue = dayValue;
                    }
                    else if (!double.IsNaN(lastValue))
                    {
                        // Forward fill: use last known value
                        aligned.Add(lastValue);
                    }
                    else
                    {
                        // No value available
                        aligned.Add(double.NaN);
                    }
                }
            }

            return aligned;
        }
    }
}
