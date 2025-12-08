using DataVisualiser.Class;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DataVisualiser.Helper
{
    public static class ChartHelper
    {
        /// <summary>
        /// Formats DateTime label based on tick interval.
        /// </summary>
        public static string FormatDateTimeLabel(DateTime dateTime, TickInterval interval)
        {
            return interval switch
            {
                TickInterval.Month => dateTime.ToString("MMM yyyy"),
                TickInterval.Week => dateTime.ToString("MMM dd"),
                TickInterval.Day => dateTime.ToString("MM/dd"),
                TickInterval.Hour => dateTime.ToString("MM/dd HH:mm"),
                _ => dateTime.ToString("MM/dd HH:mm")
            };
        }

        /// <summary>
        /// Gets the table name based on the selected resolution.
        /// </summary>
        public static string GetTableNameFromResolution(ComboBox ResolutionCombo)
        {
            var selectedResolution = ResolutionCombo.SelectedItem?.ToString() ?? "All";
            return selectedResolution switch
            {
                "Hourly" => "HealthMetricsHour",
                "Daily" => "HealthMetricsDay",
                "Weekly" => "HealthMetricsWeek",
                "Monthly" => "HealthMetricsMonth",
                "Yearly" => "HealthMetricsYear",
                _ => "HealthMetrics" // Default to "All" which uses HealthMetrics
            };
        }

        public static string[] GetChartTitlesFromCombos(ComboBox TablesCombo, ComboBox SubtypeCombo, ComboBox? SubtypeCombo2)
        {
            var baseMetric = TablesCombo.SelectedItem?.ToString() ?? string.Empty;

            string display1;
            if (SubtypeCombo.IsEnabled && SubtypeCombo.SelectedItem != null)
            {
                var s = SubtypeCombo.SelectedItem.ToString();
                display1 = !string.IsNullOrEmpty(s) && s != "(All)" ? s : baseMetric;
            }
            else
            {
                display1 = baseMetric;
            }

            string display2;
            if (SubtypeCombo2 != null && SubtypeCombo2.IsEnabled && SubtypeCombo2.SelectedItem != null)
            {
                var s2 = SubtypeCombo2.SelectedItem.ToString();
                display2 = !string.IsNullOrEmpty(s2) && s2 != "(All)" ? s2 : baseMetric;
            }
            else
            {
                display2 = baseMetric;
            }

            return new string[] { display1, display2 };
        }

        public static LineSeries? CreateLineSeries(string title, int pointSize, int lineThickness, Color colour, bool dataLabels = false)
        {
            var smoothedSeries = new LineSeries
            {
                Title = title,
                Values = new ChartValues<double>(),
                PointGeometrySize = pointSize,
                StrokeThickness = lineThickness,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(colour),
                DataLabels = dataLabels
            };

            return smoothedSeries;
        }

        /// <summary>
        /// Returns formatted values for every LineSeries in the chart at the given index.
        /// </summary>
        public static string GetChartValuesAtIndex(CartesianChart chart, int index)
        {
            if (chart == null) return "N/A";
            if (chart.Series == null || chart.Series.Count == 0) return "No series";

            var parts = new List<string>();

            foreach (var s in chart.Series)
            {
                if (s is LineSeries lineSeries)
                {
                    var title = string.IsNullOrEmpty(lineSeries.Title) ? "Series" : lineSeries.Title;

                    if (lineSeries.Values == null)
                    {
                        parts.Add($"{title}: N/A");
                        continue;
                    }

                    if (index >= 0 && index < lineSeries.Values.Count)
                    {
                        try
                        {
                            var raw = lineSeries.Values[index];
                            if (raw == null)
                            {
                                parts.Add($"{title}: N/A");
                            }
                            else
                            {
                                var val = Convert.ToDouble(raw);
                                parts.Add($"{title}: {MathHelper.FormatToThreeSignificantDigits(val)}");
                            }
                        }
                        catch (Exception)
                        {
                            parts.Add($"{title}: N/A");
                        }
                    }
                    else
                    {
                        parts.Add($"{title}: N/A");
                    }
                }
                else
                {
                    parts.Add($"{s.Title ?? "Series"}: N/A");
                }
            }

            return string.Join(" | ", parts);
        }

        /// <summary>
        /// Returns formatted values in the order: Primary Smoothed, Secondary Smoothed, Primary Raw, Secondary Raw.
        /// Format: "{metric subtype} Smoothed: {value}" or "{metric subtype} Raw: {value}"
        /// </summary>
        public static string GetChartValuesFormattedAtIndex(CartesianChart chart, int index)
        {
            if (chart == null) return "N/A";
            if (chart.Series == null || chart.Series.Count == 0) return "No series";

            // Store values by series type
            string? primarySmoothedValue = null;
            string? primaryRawValue = null;
            string? secondarySmoothedValue = null;
            string? secondaryRawValue = null;
            string? primarySeriesName = null;
            string? secondarySeriesName = null;

            // First pass: identify primary and secondary series names
            var seenBaseNames = new HashSet<string>();
            foreach (var s in chart.Series)
            {
                if (s is LineSeries lineSeries)
                {
                    var title = string.IsNullOrEmpty(lineSeries.Title) ? "Series" : lineSeries.Title;
                    string baseName = title;

                    if (title.EndsWith(" (Raw)"))
                    {
                        baseName = title.Substring(0, title.Length - 6); // Remove " (Raw)"
                    }
                    else if (title.EndsWith(" (Smoothed)"))
                    {
                        baseName = title.Substring(0, title.Length - 11); // Remove " (Smoothed)"
                    }

                    if (!seenBaseNames.Contains(baseName))
                    {
                        seenBaseNames.Add(baseName);
                        if (primarySeriesName == null)
                        {
                            primarySeriesName = baseName;
                        }
                        else if (secondarySeriesName == null && baseName != primarySeriesName)
                        {
                            secondarySeriesName = baseName;
                        }
                    }
                }
            }

            // Second pass: extract values
            foreach (var s in chart.Series)
            {
                if (s is LineSeries lineSeries)
                {
                    var title = string.IsNullOrEmpty(lineSeries.Title) ? "Series" : lineSeries.Title;

                    // Extract value
                    string? value = null;
                    if (lineSeries.Values != null && index >= 0 && index < lineSeries.Values.Count)
                    {
                        try
                        {
                            var raw = lineSeries.Values[index];
                            if (raw == null)
                            {
                                value = "N/A";
                            }
                            else
                            {
                                var val = Convert.ToDouble(raw);
                                value = MathHelper.FormatToThreeSignificantDigits(val);
                            }
                        }
                        catch (Exception)
                        {
                            value = "N/A";
                        }
                    }
                    else
                    {
                        value = "N/A";
                    }

                    // Parse title to determine if it's Primary/Secondary and Raw/Smoothed
                    bool isRaw = false;
                    bool isSmoothed = false;
                    string baseName = title;

                    if (title.EndsWith(" (Raw)"))
                    {
                        baseName = title.Substring(0, title.Length - 6); // Remove " (Raw)"
                        isRaw = true;
                    }
                    else if (title.EndsWith(" (Smoothed)"))
                    {
                        baseName = title.Substring(0, title.Length - 11); // Remove " (Smoothed)"
                        isSmoothed = true;
                    }

                    // Determine if this is primary or secondary
                    bool isPrimary = (baseName == primarySeriesName);
                    bool isSecondary = (baseName == secondarySeriesName);

                    // Store the value in the appropriate slot
                    if (isPrimary && isSmoothed)
                    {
                        primarySmoothedValue = value;
                    }
                    else if (isPrimary && isRaw)
                    {
                        primaryRawValue = value;
                    }
                    else if (isSecondary && isSmoothed)
                    {
                        secondarySmoothedValue = value;
                    }
                    else if (isSecondary && isRaw)
                    {
                        secondaryRawValue = value;
                    }
                }
            }

            // Build the formatted string in the specified order
            var parts = new List<string>();

            // 1. Primary Smoothed
            if (primarySeriesName != null && primarySmoothedValue != null)
            {
                parts.Add($"{primarySeriesName} Smoothed: {primarySmoothedValue}");
            }

            // 2. Secondary Smoothed
            if (secondarySeriesName != null && secondarySmoothedValue != null)
            {
                parts.Add($"{secondarySeriesName} Smoothed: {secondarySmoothedValue}");
            }

            // 3. Primary Raw
            if (primarySeriesName != null && primaryRawValue != null)
            {
                parts.Add($"{primarySeriesName} Raw: {primaryRawValue}");
            }

            // 4. Secondary Raw
            if (secondarySeriesName != null && secondaryRawValue != null)
            {
                parts.Add($"{secondarySeriesName} Raw: {secondaryRawValue}");
            }

            return parts.Count > 0 ? string.Join("; ", parts) : "N/A";
        }

        /// <summary>
        /// Draws (or moves) a thin black vertical line by using an AxisSection with zero width.
        /// </summary>
        public static void UpdateVerticalLineForChart(ref CartesianChart chart, int index, ref AxisSection? sectionField)
        {
            if (chart == null) return;
            if (chart.AxisX == null || chart.AxisX.Count == 0) return;
            if (index < 0) return; // Validate index is non-negative

            // Remove existing - get fresh references right before the operation
            if (sectionField != null)
            {
                try
                {
                    // Re-fetch axis and Sections from chart to ensure we have valid references
                    if (chart.AxisX != null && chart.AxisX.Count > 0)
                    {
                        var axis = chart.AxisX[0];
                        if (axis != null && axis.Sections != null)
                        {
                            // Try to remove the section - Remove is safe even if item not in collection
                            // but may throw if sectionField is in an invalid/disposed state
                            axis.Sections.Remove(sectionField);
                        }
                    }
                }
                catch (Exception)
                {
                    // Section or axis may have been disposed/invalidated by LiveCharts internally
                    // Ignore and continue - we'll create a new one below
                }
                sectionField = null;
            }

            // Create new section
            var line = new AxisSection
            {
                Value = index,
                SectionWidth = 0,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };

            // Add new section - get fresh references right before the operation
            try
            {
                // Re-fetch axis and Sections from chart to ensure we have valid references
                if (chart.AxisX != null && chart.AxisX.Count > 0)
                {
                    var axis = chart.AxisX[0];
                    if (axis != null && axis.Sections != null)
                    {
                        axis.Sections.Add(line);
                        sectionField = line;
                    }
                }
            }
            catch (Exception)
            {
                // Axis or Sections may have been disposed/invalidated by LiveCharts internally
                // Ignore and leave sectionField as null
            }
        }

        public static Border CreateBorder(StackPanel stack)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Padding = new Thickness(4),
                Child = stack,
                MaxWidth = 600
            };

            return border;
        }

        public static TextBlock SetHoverTimeStampText()
        {
            TextBlock _hoverTimestampText = new TextBlock
            {
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(6, 2, 6, 4)
            };

            return _hoverTimestampText;
        }

        public static TextBlock SetHoverText(bool isBold = false)
        {
            TextBlock _hoverTimestampText = new TextBlock
            {
                Foreground = Brushes.White,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(6, 2, 6, 4)
            };

            return _hoverTimestampText;
        }

        public static void RemoveAxisSection(ref CartesianChart? chart, AxisSection? axisSection)
        {
            if (chart?.AxisX != null && chart.AxisX.Count > 0 && axisSection != null)
            {
                var axis = chart.AxisX[0];

                if (axis?.Sections != null)
                {
                    try
                    {
                        // Re-check axis and Sections before attempting removal
                        // as they may have been disposed/invalidated between checks
                        if (axis != null && axis.Sections != null)
                        {
                            // Try to remove the section - Remove is safe even if item not in collection
                            // but may throw if sectionField is in an invalid/disposed state
                            axis.Sections.Remove(axisSection);
                        }
                    }
                    catch (Exception)
                    {
                        // Section or axis may have been disposed/invalidated by LiveCharts internally
                        // Ignore and continue
                    }
                }
                axisSection = null;
            }
        }

        public static string? GetSubMetricType(ComboBox subMetricTypeCombo)
        {
            string? selectedSubtype = null;
            if (subMetricTypeCombo.IsEnabled && subMetricTypeCombo.SelectedItem != null)
            {
                var subtypeValue = subMetricTypeCombo.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(subtypeValue) && subtypeValue != "(All)")
                {
                    selectedSubtype = subtypeValue;
                }
            }

            return selectedSubtype;
        }

        public static void ResetZoom(ref CartesianChart chart)
        {
            if (chart != null && chart.AxisX != null && chart.AxisX.Count > 0)
            {
                var axis = chart.AxisX[0];
                if (axis != null)
                {
                    axis.MinValue = double.NaN;
                    axis.MaxValue = double.NaN;
                }
            }
        }

        /// <summary>
        /// Initializes common chart behavior settings (zoom and pan options).
        /// </summary>
        public static void InitializeChartBehavior(CartesianChart chart)
        {
            if (chart == null) return;
            chart.Zoom = ZoomingOptions.X;
            chart.Pan = PanningOptions.X;
        }

        /// <summary>
        /// Clears all series from a chart and removes it from the timestamps dictionary.
        /// </summary>
        public static void ClearChart(CartesianChart? chart, Dictionary<CartesianChart, List<DateTime>>? chartTimestamps = null)
        {
            if (chart == null) return;
            chart.Series.Clear();
            chartTimestamps?.Remove(chart);
        }

        /// <summary>
        /// Initializes the default tooltip for a chart if it doesn't already have one.
        /// </summary>
        public static void InitializeChartTooltip(CartesianChart chart)
        {
            if (chart == null) return;
            if (chart.DataTooltip == null)
                chart.DataTooltip = new DefaultTooltip();
        }

        /// <summary>
        /// Gets the timestamp text for a given index from a chart's timestamp dictionary.
        /// </summary>
        public static string GetTimestampTextForIndex(int index, Dictionary<CartesianChart, List<DateTime>> chartTimestamps, params CartesianChart[] charts)
        {
            foreach (var chart in charts)
            {
                if (chartTimestamps.TryGetValue(chart, out var list) && index >= 0 && index < list.Count)
                {
                    return list[index].ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            return "Timestamp: N/A";
        }

        /// <summary>
        /// Positions a hover popup with standard offsets.
        /// </summary>
        public static void PositionHoverPopup(Popup popup, double horizontalOffset = 10, double verticalOffset = 10)
        {
            if (popup == null) return;
            if (!popup.IsOpen) popup.IsOpen = true;
            popup.HorizontalOffset = 0;
            popup.VerticalOffset = 0;
            popup.HorizontalOffset = horizontalOffset;
            popup.VerticalOffset = verticalOffset;
        }

        /// <summary>
        /// Normalizes Y-axis ticks to show uniform intervals (~10 ticks) with rounded bounds.
        /// </summary>
        public static void NormalizeYAxis(Axis yAxis, List<HealthMetricData> rawData, List<double> smoothedValues)
        {
            var allValues = new List<double>();

            foreach (var point in rawData)
            {
                if (point.Value.HasValue)
                {
                    allValues.Add((double)point.Value.Value);
                }
            }

            foreach (var value in smoothedValues)
            {
                if (!double.IsNaN(value) && !double.IsInfinity(value))
                {
                    allValues.Add(value);
                }
            }

            if (!allValues.Any())
            {
                yAxis.MinValue = double.NaN;
                yAxis.MaxValue = double.NaN;
                yAxis.Separator = new LiveCharts.Wpf.Separator();
                yAxis.ShowLabels = false;
                return;
            }

            double dataMin = allValues.Min();
            double dataMax = allValues.Max();

            if (double.IsNaN(dataMin) || double.IsNaN(dataMax) || double.IsInfinity(dataMin) || double.IsInfinity(dataMax))
            {
                yAxis.MinValue = double.NaN;
                yAxis.MaxValue = double.NaN;
                yAxis.Separator = new LiveCharts.Wpf.Separator();
                yAxis.ShowLabels = false;
                return;
            }

            double minValue = dataMin;
            double maxValue = dataMax;
            double range = maxValue - minValue;

            if (range <= double.Epsilon)
            {
                double padding = Math.Max(Math.Abs(minValue) * 0.1, 1e-3);

                if (Math.Abs(minValue) < 1e-6)
                {
                    minValue = -padding;
                    maxValue = padding;
                }
                else
                {
                    minValue = minValue - padding;
                    maxValue = maxValue + padding;
                    if (dataMin >= 0)
                    {
                        minValue = Math.Max(0, minValue);
                    }
                }

                range = maxValue - minValue;
            }
            else
            {
                double padding = range * 0.05;
                minValue -= padding;
                maxValue += padding;

                if (dataMin >= 0)
                {
                    minValue = Math.Max(0, minValue);
                }

                range = maxValue - minValue;
            }

            const double targetTicks = 10.0;
            double rawTickInterval = range / targetTicks;
            if (rawTickInterval <= 0 || double.IsNaN(rawTickInterval) || double.IsInfinity(rawTickInterval))
            {
                yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(minValue);
                yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(maxValue);
                var fallbackStep = MathHelper.RoundToThreeSignificantDigits((maxValue - minValue) / targetTicks);
                yAxis.Separator = fallbackStep > 0 ? new LiveCharts.Wpf.Separator { Step = fallbackStep } : new LiveCharts.Wpf.Separator();
                yAxis.ShowLabels = true;
                yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                return;
            }

            double magnitude;
            try
            {
                var logValue = Math.Log10(Math.Abs(rawTickInterval));
                magnitude = Math.Pow(10, Math.Floor(logValue));
            }
            catch
            {
                magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(1e-6, rawTickInterval))));
            }

            var normalizedInterval = rawTickInterval / magnitude;
            double niceInterval = normalizedInterval switch
            {
                <= 1 => 1 * magnitude,
                <= 2 => 2 * magnitude,
                <= 5 => 5 * magnitude,
                _ => 10 * magnitude
            };

            niceInterval = MathHelper.RoundToThreeSignificantDigits(niceInterval);
            if (niceInterval <= 0 || double.IsNaN(niceInterval) || double.IsInfinity(niceInterval))
                niceInterval = rawTickInterval;

            var niceMin = Math.Floor(minValue / niceInterval) * niceInterval;
            var niceMax = Math.Ceiling(maxValue / niceInterval) * niceInterval;

            niceMin -= niceInterval * 0.0;
            niceMax += niceInterval * 0.0;

            if (dataMin >= 0 && niceMin < 0)
            {
                niceMin = 0;
            }

            yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(niceMin);
            yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(niceMax);

            double step = MathHelper.RoundToThreeSignificantDigits(niceInterval);
            if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            {
                step = MathHelper.RoundToThreeSignificantDigits((yAxis.MaxValue - yAxis.MinValue) / targetTicks);
            }

            yAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
            yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
            yAxis.ShowLabels = true;
            yAxis.Labels = null;
        }

        /// <summary>
        /// Adjusts chart control Height based on Y-axis tick count to ensure ticks are spaced 20-40px apart.
        /// Charts live inside a ScrollViewer; if total chart heights exceed window height a scrollbar will appear.
        /// </summary>
        public static void AdjustChartHeightBasedOnYAxis(CartesianChart chart, double minHeight)
        {
            if (chart == null || chart.AxisY.Count == 0) return;

            var yAxis = chart.AxisY[0];

            if (double.IsNaN(yAxis.MinValue) || double.IsNaN(yAxis.MaxValue) ||
                double.IsInfinity(yAxis.MinValue) || double.IsInfinity(yAxis.MaxValue))
            {
                chart.Height = minHeight;
                return;
            }

            double minValue = yAxis.MinValue;
            double maxValue = yAxis.MaxValue;
            double step = yAxis.Separator?.Step ?? 0;

            if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            {
                step = (maxValue - minValue) / 10.0;
                if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
                {
                    chart.Height = minHeight;
                    return;
                }
            }

            double range = maxValue - minValue;
            int tickCount = (int)Math.Ceiling(range / step) + 1;

            tickCount = Math.Max(2, tickCount);

            const double tickSpacingPx = 30.0;
            const double paddingPx = 100.0;

            double calculatedHeight = (tickCount * tickSpacingPx) + paddingPx;

            calculatedHeight = Math.Max(minHeight, calculatedHeight);

            const double maxHeight = 2000.0;
            calculatedHeight = Math.Min(maxHeight, calculatedHeight);

            chart.Height = calculatedHeight;
        }
    }
}
