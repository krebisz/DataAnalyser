using DataFileReader.Helper;
using System;
using System.Collections.Generic;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Stub mapper that defines the seam between HealthMetric records
    /// and Canonical Metric Series (CMS).
    ///
    /// IMPORTANT:
    /// - No grouping logic
    /// - No identity resolution
    /// - No CMS emission
    /// - No side effects
    ///
    /// This type exists only to anchor the mapping boundary.
    /// </summary>
    internal sealed class HealthMetricToCmsMapper
    {
        private readonly CanonicalMetricIdentityResolver _identityResolver;

        public HealthMetricToCmsMapper(
            CanonicalMetricIdentityResolver identityResolver)
        {
            _identityResolver = identityResolver;
        }

        /// <summary>
        /// Maps a set of HealthMetric records to canonical metric series.
        ///
        /// This method is intentionally unimplemented.
        /// It exists only to declare the mapping contract.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// Always thrown in this stub.
        /// </exception>
        public IReadOnlyList<ICanonicalMetricSeries> Map(
            IReadOnlyList<HealthMetric> records)
        {
            throw new NotImplementedException(
                "HealthMetric → CMS mapping is not yet implemented.");
        }
    }
}
