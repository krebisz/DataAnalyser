using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static string[] GetChartTitlesFromCombos(ComboBox TablesCombo, ComboBox SubtypeCombo, ComboBox SubtypeCombo2)
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
            if (SubtypeCombo2.IsEnabled && SubtypeCombo2.SelectedItem != null)
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

                Title = title, //$"{metricType} (Smoothed)",
                Values = new ChartValues<double>(),
                PointGeometrySize = pointSize, // 5,
                StrokeThickness = lineThickness, //2,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(colour), //Colors.Red,
                DataLabels = dataLabels
            };

            return smoothedSeries;
        }

        // Returns formatted values for every LineSeries in the chart at the given index.
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
                            var val = Convert.ToDouble(raw);
                            parts.Add($"{title}: {MathHelper.FormatToThreeSignificantDigits(val)}");
                        }
                        catch
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


        // Draw (or move) a thin black vertical line by using an AxisSection with zero width
        public static void UpdateVerticalLineForChart(ref CartesianChart chart, int index, ref AxisSection? sectionField)
        {
            if (chart == null) return;
            if (chart.AxisX == null || chart.AxisX.Count == 0) return;

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

        public static TextBlock SetHoverMainText()
        {
            TextBlock _hoverMainText = new TextBlock
            {
                Foreground = Brushes.White,
                Margin = new Thickness(6, 2, 6, 2)
            };

            return _hoverMainText;
        }

        public static TextBlock SetHoverDiffText()
        {
            TextBlock _hoverDiffText = new TextBlock
            {
                Foreground = Brushes.White,
                Margin = new Thickness(6, 2, 6, 2)
            };

            return _hoverDiffText;
        }

        public static TextBlock SetHoverRatioText()
        {
            TextBlock _hoverRatioText = new TextBlock
            {
                Foreground = Brushes.White,
                Margin = new Thickness(6, 2, 6, 2)
            };

            return _hoverRatioText;
        }


        public static TextBlock SetHoverText(bool isBold = false)
        {
            TextBlock _hoverTimestampText = new TextBlock
            {
                Foreground = Brushes.White,
                FontWeight = isBold? FontWeights.Bold : FontWeights.Normal,
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
    }
}   
