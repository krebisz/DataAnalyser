using System;
using System.Collections.Generic;
using System.Linq;
using DataFileReader.Normalization.Canonical;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Converts between internal normalization CMS types and consumer-facing CMS interface.
    /// 
    /// Architecture Note:
    /// - DataFileReader.Normalization.Canonical.CanonicalMetricSeries&lt;TValue&gt; is the internal
    ///   normalization pipeline type, used during RawRecord → CMS transformation.
    /// - DataFileReader.Canonical.ICanonicalMetricSeries is the consumer-facing interface,
    ///   used by downstream systems (e.g., DataVisualiser in Phase 4+).
    /// 
    /// This converter bridges the two representations, allowing the normalization pipeline
    /// to produce internal types while consumers receive the standardized interface.
    /// </summary>
    public static class CmsTypeConverter
    {
        /// <summary>
        /// Converts an internal CanonicalMetricSeries&lt;TValue&gt; to the consumer-facing ICanonicalMetricSeries.
        /// </summary>
        public static ICanonicalMetricSeries ToConsumerCms<TValue>(
            CanonicalMetricSeries<TValue> internalCms)
        {
            if (internalCms == null)
                throw new ArgumentNullException(nameof(internalCms));

            // Convert MetricIdentity to CanonicalMetricId
            var canonicalMetricId = new CanonicalMetricId(internalCms.Metric.Id);

            // Convert TimeAxis to TimeSemantics
            var timeSemantics = new TimeSemantics(
                internalCms.TimeAxis.IsIntervalBased
                    ? TimeRepresentation.Interval
                    : TimeRepresentation.Point,
                internalCms.Timestamps.Count > 0
                    ? internalCms.Timestamps[0]
                    : DateTimeOffset.MinValue,
                internalCms.Timestamps.Count > 0
                    ? internalCms.Timestamps[internalCms.Timestamps.Count - 1]
                    : (DateTimeOffset?)null
            );

            // Convert timestamps and values to MetricSamples
            var samples = new List<MetricSample>();
            for (int i = 0; i < internalCms.Timestamps.Count && i < internalCms.Values.Count; i++)
            {
                var timestamp = internalCms.Timestamps[i];
                var value = internalCms.Values[i];

                // Convert value to decimal?
                decimal? decimalValue = null;
                if (value != null)
                {
                    if (value is decimal d)
                    {
                        decimalValue = d;
                    }
                    else if (value is double db)
                    {
                        decimalValue = (decimal)db;
                    }
                    else if (value is float f)
                    {
                        decimalValue = (decimal)f;
                    }
                    else if (value is int iVal)
                    {
                        decimalValue = iVal;
                    }
                    else if (value is long l)
                    {
                        decimalValue = l;
                    }
                    else if (decimal.TryParse(value.ToString(), out var parsed))
                    {
                        decimalValue = parsed;
                    }
                }

                samples.Add(new MetricSample(timestamp, decimalValue));
            }

            // Determine dimension from DimensionSet
            var dimension = MetricDimension.Unknown;
            if (internalCms.Dimensions.Dimensions.TryGetValue("Dimension", out var dimStr))
            {
                dimension = Enum.TryParse<MetricDimension>(dimStr, ignoreCase: true, out var parsed)
                    ? parsed
                    : MetricDimension.Unknown;
            }

            // Extract unit from provenance or use default
            var unitSymbol = "unit";
            if (internalCms.Provenance.TryGetValue("Unit", out var unit))
            {
                unitSymbol = unit;
            }
            else if (internalCms.Metric.Id == "metric.body_weight")
            {
                unitSymbol = "kg";
            }
            else if (internalCms.Metric.Id == "metric.sleep")
            {
                unitSymbol = "hours";
            }

            // Build provenance
            var provenance = new MetricProvenance(
                internalCms.Provenance.TryGetValue("SourceProvider", out var provider)
                    ? provider
                    : "Unknown",
                internalCms.Provenance.TryGetValue("SourceDataset", out var dataset)
                    ? dataset
                    : "Unknown",
                internalCms.Provenance.TryGetValue("NormalizationVersion", out var version)
                    ? version
                    : "Unknown"
            );

            return new CanonicalMetricSeries
            {
                MetricId = canonicalMetricId,
                Time = timeSemantics,
                Samples = samples,
                Unit = new MetricUnit(unitSymbol, false),
                Dimension = dimension,
                Provenance = provenance,
                Quality = new MetricQuality(DataCompleteness.Unknown, ValidationStatus.Assumed)
            };
        }

        /// <summary>
        /// Converts multiple internal CMS instances to consumer-facing interface.
        /// </summary>
        public static IReadOnlyList<ICanonicalMetricSeries> ToConsumerCmsList<TValue>(
            IReadOnlyList<CanonicalMetricSeries<TValue>> internalCmsList)
        {
            if (internalCmsList == null)
                throw new ArgumentNullException(nameof(internalCmsList));

            return internalCmsList
                .Select(ToConsumerCms)
                .ToList();
        }
    }
}
