using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using DataVisualiser.Class;

namespace DataVisualiser
{
    // Partial MainWindow contains weekly-distribution chart logic.
    public partial class MainWindow : Window
    {
        // Public method you can call from your existing flow to update weekly distribution.
        // Accepts an IEnumerable<HealthMetricData> and updates ChartWeekly.
        // title parameter kept for symmetry with other chart update calls.
        public async Task UpdateWeeklyDistributionChartAsync(
            CartesianChart chart,
            IEnumerable<HealthMetricData> metrics,
            string title,
            double minHeight = 400.0)
        {
            if (chart == null) return;

            // Defensive: ensure we run UI updates through Dispatcher when necessary.
            try
            {
                var metricsList = metrics?.Where(m => m != null && m.Value.HasValue).ToList() ?? new List<HealthMetricData>();

                if (!metricsList.Any())
                {
                    // Clear chart
                    await Dispatcher.InvokeAsync(() =>
                    {
                        chart.Series = new SeriesCollection();
                    });
                    return;
                }

                // Convert values to double for math
                var dayIndexedValues = metricsList
                    .GroupBy(m => m.NormalizedTimestamp.DayOfWeek)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => (double)x.Value!.Value).ToList()
                    );

                // Make sure all days exist (Mon..Sun): use Monday-first order for labels / presentation
                var dayOrder = new[] {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Saturday,
                    DayOfWeek.Sunday
                };

                foreach (var dow in dayOrder)
                {
                    if (!dayIndexedValues.ContainsKey(dow))
                        dayIndexedValues[dow] = new List<double>();
                }

                // Flatten all values to get global min/max
                var allValues = dayIndexedValues.SelectMany(k => k.Value).Where(v => !double.IsNaN(v)).ToList();
                if (!allValues.Any())
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        chart.Series = new SeriesCollection();
                    });
                    return;
                }

                double globalMin = Math.Floor(allValues.Min() / 5.0) * 5.0;
                double globalMax = Math.Ceiling(allValues.Max() / 5.0) * 5.0;
                if (globalMax == globalMin)
                {
                    // Small fudge so range isn't zero
                    globalMax = globalMin + 1;
                }

                double range = globalMax - globalMin;
                // adaptive bucket count
                int bucketCount = range < 20 ? 5 : range < 50 ? 10 : 15;
                bucketCount = Math.Min(30, Math.Max(4, bucketCount)); // clamp to reasonable limits

                double bucketSize = range / bucketCount;

                // Build buckets
                var buckets = new List<(double Min, double Max)>();
                for (int i = 0; i < bucketCount; i++)
                {
                    double bmin = globalMin + i * bucketSize;
                    double bmax = (i == bucketCount - 1) ? globalMax : bmin + bucketSize;
                    buckets.Add((bmin, bmax));
                }

                // Frequency per day per bucket
                var freq = new Dictionary<DayOfWeek, int[]>();
                foreach (var d in dayOrder)
                {
                    var counts = new int[bucketCount];
                    foreach (var v in dayIndexedValues[d])
                    {
                        int idx = (int)Math.Floor((v - globalMin) / bucketSize);
                        if (idx < 0) idx = 0;
                        if (idx >= bucketCount) idx = bucketCount - 1;
                        counts[idx]++;
                    }
                    freq[d] = counts;
                }

                // maximum frequency across all buckets/days, used to normalize shading
                int maxFreq = freq.Values.SelectMany(a => a).DefaultIfEmpty(0).Max();
                if (maxFreq == 0) maxFreq = 1;

                // Build series collection using stacked column series.
                // Each bucket will produce *two* stacked series entries per bucket:
                //  1) a transparent baseline series that positions the bucket correctly
                //  2) a colored series representing the bucket block (height = bucketSize)
                var seriesCollection = new SeriesCollection();

                // We'll accumulate lower-bucket heights per day to compute the baseline values.
                var cumulativeLower = new Dictionary<DayOfWeek, double>();
                foreach (var d in dayOrder) cumulativeLower[d] = 0.0;

                // Important: use StackedColumnSeries so the values stack
                for (int bIndex = 0; bIndex < bucketCount; bIndex++)
                {
                    var baselineValues = new ChartValues<double>();
                    var bucketHeights = new ChartValues<double>();

                    // For shading intensity we compute per-bucket (across days) intensity as default.
                    // This produces consistent palette across days for the same bucket.
                    int bucketMaxAcrossDays = dayOrder.Select(d => freq[d][bIndex]).DefaultIfEmpty(0).Max();
                    double bucketIntensityNormalized = (double)bucketMaxAcrossDays / maxFreq;

                    for (int i = 0; i < dayOrder.Length; i++)
                    {
                        var dow = dayOrder[i];
                        double baseline = cumulativeLower[dow];
                        baselineValues.Add(baseline);

                        bool present = freq[dow][bIndex] > 0;
                        double height = present ? bucketSize : 0.0;
                        bucketHeights.Add(height);

                        if (present)
                        {
                            cumulativeLower[dow] = baseline + height;
                        }
                    }

                    // Baseline: transparent stacked column to offset the actual colored block
                    var baselineSeries = new StackedColumnSeries
                    {
                        Title = null,
                        Values = baselineValues,
                        Fill = Brushes.Transparent,
                        Stroke = Brushes.Transparent,
                        MaxColumnWidth = 80,
                        DataLabels = false
                    };

                    // Bucket block: colored according to intensity
                    // Use a blue ramp -> near-black at high intensity.
                    var bucketBrush = CreateBucketBrush(bucketIntensityNormalized);

                    var bucketSeries = new StackedColumnSeries
                    {
                        Title = null,
                        Values = bucketHeights,
                        Fill = bucketBrush,
                        Stroke = bucketBrush,
                        MaxColumnWidth = 80,
                        DataLabels = false
                    };

                    // Add baseline first (so stacking offset is applied), then colored series
                    seriesCollection.Add(baselineSeries);
                    seriesCollection.Add(bucketSeries);
                }

                // Day labels (Monday..Sunday)
                var labels = dayOrder.Select(d => d.ToString()).ToArray();

                // Apply to chart on UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    chart.MinHeight = minHeight;
                    chart.Series = seriesCollection;
                    chart.LegendLocation = LegendLocation.None;

                    // X Axis
                    chart.AxisX.Clear();
                    chart.AxisX.Add(new Axis
                    {
                        Title = "Day of Week",
                        Labels = labels,
                        Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
                    });

                    // Y Axis
                    chart.AxisY.Clear();
                    chart.AxisY.Add(new Axis
                    {
                        Title = "Value",
                        MinValue = globalMin,
                        MaxValue = globalMax,
                        LabelFormatter = value => FormatDoubleLabel(value)
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateWeeklyDistributionChartAsync error: {ex.Message}");
            }
        }

        // Toggle handler for the weekly chart visibility (bound to ChartWeeklyToggleButton)
        private void OnChartWeeklyToggle(object sender, RoutedEventArgs e)
        {
            if (ChartWeeklyPanel == null || ChartWeekly == null) return;

            if (ChartWeeklyPanel.Visibility == Visibility.Collapsed)
            {
                ChartWeeklyPanel.Visibility = Visibility.Visible;
                ChartWeeklyToggleButton.Content = "Hide";
            }
            else
            {
                ChartWeeklyPanel.Visibility = Visibility.Collapsed;
                ChartWeeklyToggleButton.Content = "Show";
            }
        }

        #region Helpers

        // Create a SolidColorBrush from normalized intensity [0..1] that maps light blue -> dark blue/near-black.
        private SolidColorBrush CreateBucketBrush(double normalizedIntensity)
        {
            normalizedIntensity = Math.Max(0.0, Math.Min(1.0, normalizedIntensity));

            // Start color (light blue)
            byte r0 = 180, g0 = 215, b0 = 255;
            // End color (near-black/very dark blue)
            byte r1 = 8, g1 = 10, b1 = 25;

            byte r = (byte)Math.Round(r0 + (r1 - r0) * normalizedIntensity);
            byte g = (byte)Math.Round(g0 + (g1 - g0) * normalizedIntensity);
            byte b = (byte)Math.Round(b0 + (b1 - b0) * normalizedIntensity);

            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private static string FormatDoubleLabel(double value)
        {
            // If MathHelper exists in your project, you can replace this with MathHelper.FormatToThreeSignificantDigits(value)
            // Otherwise use a simple formatting fallback:
            if (double.IsNaN(value) || double.IsInfinity(value)) return string.Empty;
            return Math.Round(value, 2).ToString();
        }

        #endregion
    }
}
