using System;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Stub resolver for Canonical Metric Identity.
    ///
    /// This type defines the seam where declarative identity
    /// resolution rules will be applied.
    ///
    /// IMPORTANT:
    /// - No inference
    /// - No defaults
    /// - No fallback logic
    /// - No rules implemented yet
    /// </summary>
    internal sealed class CanonicalMetricIdentityResolver
    {
        /// <summary>
        /// Resolves a canonical metric identity from descriptive metadata.
        ///
        /// This method is intentionally unimplemented.
        /// It exists only to anchor the identity resolution boundary.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// Always thrown in this stub.
        /// </exception>
        public CanonicalMetricId Resolve(
            string provider,
            string metricType,
            string metricSubtype)
        {
            throw new NotImplementedException(
                "Canonical metric identity resolution is not yet implemented.");
        }
    }
}
