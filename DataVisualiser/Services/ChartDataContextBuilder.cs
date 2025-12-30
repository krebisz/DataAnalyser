using DataVisualiser.Charts;
using DataVisualiser.Models;
using DataFileReader.Canonical;

namespace DataVisualiser.Services
{
    /// <summary>
    /// Builds a fully aligned, fully derived ChartDataContext from raw
    /// HealthMetricData series. This includes unified timeline construction,
    /// numeric extraction, smoothing, ratio/diff/normalization calculations.
    /// </summary>
    public sealed class ChartDataContextBuilder
    {
        // Configurable smoothing radius for moving average
        private const int SmoothWindow = 3;

        /// <summary>
        /// Builds a ChartDataContext from metric data.
        /// IMPORTANT: Ensures consistent ordering:
        /// - data1 = first selected metric subtype (primary)
        /// - data2 = second selected metric subtype (secondary)
        /// - primarySubtype = first selected subtype
        /// - secondarySubtype = second selected subtype
        /// </summary>
        public ChartDataContext Build(
            string metricType,
            string? primarySubtype,      // First selected subtype
            string? secondarySubtype,     // Second selected subtype
            IEnumerable<HealthMetricData> data1,  // First selected subtype data (primary)
            IEnumerable<HealthMetricData>? data2, // Second selected subtype data (secondary)
            DateTime from,
            DateTime to)
        {
            // Normalize inputs - maintain ordering: list1 = first selected, list2 = second selected
            var list1 = data1?.ToList() ?? new List<HealthMetricData>(); // First selected subtype
            var list2 = data2?.ToList() ?? new List<HealthMetricData>(); // Second selected subtype

            // STEP 1 — Unified timeline
            var timestamps = BuildUnifiedTimeline(list1, list2);

            // STEP 2 — Extract aligned numeric arrays
            var raw1 = AlignValues(list1, timestamps);
            var raw2 = AlignValues(list2, timestamps);

            // STEP 3 — Smoothing
            var smooth1 = Smooth(raw1, SmoothWindow);
            var smooth2 = Smooth(raw2, SmoothWindow);

            // STEP 4 — Derived series
            var diff = ComputeDifference(raw1, raw2);
            var ratio = ComputeRatio(raw1, raw2);
            var norm1 = Normalize(raw1);
            var norm2 = Normalize(raw2);

            // STEP 5 — Display labels
            var (display1, display2) = BuildDisplayNames(metricType, primarySubtype, secondarySubtype);

            // Construct full context
            return new ChartDataContext
            {
                Data1 = list1,
                Data2 = list2,

                Timestamps = timestamps,

                RawValues1 = raw1,
                RawValues2 = raw2,
                SmoothedValues1 = smooth1,
                SmoothedValues2 = smooth2,

                DifferenceValues = diff,
                RatioValues = ratio,
                NormalizedValues1 = norm1,
                NormalizedValues2 = norm2,

                DisplayName1 = display1,
                DisplayName2 = display2,

                MetricType = metricType,
                PrimarySubtype = primarySubtype,
                SecondarySubtype = secondarySubtype,

                From = from,
                To = to,

                SemanticMetricCount = secondarySubtype == null ? 1 : 2
            };
        }


        public ChartDataContext Build(
            string metricType,
            string? primarySubtype,
            string? secondarySubtype,
            IEnumerable<HealthMetricData> data1,
            IEnumerable<HealthMetricData>? data2,
            DateTime from,
            DateTime to,
            DataFileReader.Canonical.ICanonicalMetricSeries? primaryCms,
            DataFileReader.Canonical.ICanonicalMetricSeries? secondaryCms)
        {
            // CRITICAL FIX: Don't convert CMS to legacy here.
            // Strategies should receive CMS directly via StrategyCutOverService.
            // Only convert if we need legacy data for derived calculations (diff/ratio/norm).
            // For now, use legacy data as-is and let strategies handle CMS directly.
            
            var ctx = Build(
                metricType,
                primarySubtype,
                secondarySubtype,
                data1,
                data2,
                from,
                to);

            // Store CMS in context for strategies to use directly
            ctx.PrimaryCms = primaryCms;
            ctx.SecondaryCms = secondaryCms;

            return ctx;
        }


        // ---------------------------------------------------------
        // TIMELINE CONSTRUCTION
        // ---------------------------------------------------------
        private static IReadOnlyList<DateTime> BuildUnifiedTimeline(
            List<HealthMetricData> list1,
            List<HealthMetricData> list2)
        {
            return list1
                .Select(d => d.NormalizedTimestamp.Date)
                .Concat(list2.Select(d => d.NormalizedTimestamp.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        // ---------------------------------------------------------
        // VALUE ALIGNMENT
        // ---------------------------------------------------------
        private static IReadOnlyList<double> AlignValues(
            List<HealthMetricData> source,
            IReadOnlyList<DateTime> timeline)
        {
            // Map: date -> numeric value
            var dict = source
                .GroupBy(d => d.NormalizedTimestamp.Date)
                .ToDictionary(
                    g => g.Key,
                    g => Convert.ToDouble(g.First().Value ?? 0m));

            double lastValue = 0.0;

            var aligned = new List<double>(timeline.Count);

            foreach (var day in timeline)
            {
                if (dict.TryGetValue(day, out var v))
                {
                    aligned.Add(v);
                    lastValue = v;
                }
                else
                {
                    // Forward fill missing values
                    aligned.Add(lastValue);
                }
            }

            return aligned;
        }

        // ---------------------------------------------------------
        // SMOOTHING FUNCTION
        // ---------------------------------------------------------
        private static IReadOnlyList<double> Smooth(IReadOnlyList<double> values, int window)
        {
            if (window <= 1)
                return values;

            var result = new double[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                int start = Math.Max(0, i - window);
                int end = Math.Min(values.Count - 1, i + window);
                int count = end - start + 1;

                double sum = 0;
                for (int j = start; j <= end; j++)
                    sum += values[j];

                result[i] = sum / count;
            }

            return result.ToList();
        }

        // ---------------------------------------------------------
        // DERIVED NUMERIC SERIES
        // ---------------------------------------------------------
        private static IReadOnlyList<double> ComputeDifference(
            IReadOnlyList<double> a,
            IReadOnlyList<double> b)
        {
            var result = new double[a.Count];
            for (int i = 0; i < a.Count; i++)
                result[i] = a[i] - b[i];
            return result.ToList();
        }

        private static IReadOnlyList<double> ComputeRatio(
            IReadOnlyList<double> a,
            IReadOnlyList<double> b)
        {
            var result = new double[a.Count];
            for (int i = 0; i < a.Count; i++)
                result[i] = b[i] == 0 ? 0 : a[i] / b[i];
            return result.ToList();
        }

        private static IReadOnlyList<double> Normalize(IReadOnlyList<double> values)
        {
            double max = values.Max();
            if (max <= 0)
                return values.ToList();
            return values.Select(v => v / max).ToList();
        }

        // ---------------------------------------------------------
        // LABEL GENERATION
        // ---------------------------------------------------------
        private static (string DisplayName1, string DisplayName2) BuildDisplayNames(
            string metricType,
            string? primarySubtype,
            string? secondarySubtype)
        {
            string display1 = !string.IsNullOrWhiteSpace(primarySubtype) &&
                              primarySubtype != "(All)"
                ? $"{metricType} - {primarySubtype}"
                : metricType;

            string display2 = !string.IsNullOrWhiteSpace(secondarySubtype) &&
                              secondarySubtype != "(All)"
                ? $"{metricType} - {secondarySubtype}"
                : metricType;

            return (display1, display2);
        }

        private static IEnumerable<HealthMetricData> ConvertCmsToHealthMetricData(
    ICanonicalMetricSeries cms,
    DateTime from,
    DateTime to)
        {
            // Minimal, explicit adapter for Phase 4.
            // This preserves CMS authority while keeping the existing numeric pipeline unchanged.

            return cms.Samples
                .Where(s => s.Value.HasValue)
                .Where(s =>
                {
                    var ts = s.Timestamp.UtcDateTime;
                    return ts >= from && ts <= to;
                })
                .Select(s => new HealthMetricData
                {
                    NormalizedTimestamp = s.Timestamp.UtcDateTime,
                    Value = s.Value.Value,
                    Unit = cms.Unit.Symbol
                })
                .OrderBy(d => d.NormalizedTimestamp)
                .ToList();
        }

    }
}
