namespace DataFileReader.Normalization.Canonical
{
    public sealed class MetricIdentity
    {
        public string Id { get; }

        public MetricIdentity(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Metric identity must be non-empty.", nameof(id));

            Id = id;
        }

        public override string ToString() => Id;

        // ---------------------------------
        // Canonical Metric Identities
        // ---------------------------------

        /// <summary>
        /// Canonical identity for body weight.
        /// Dimension: Mass
        /// </summary>
        public static readonly MetricIdentity BodyWeight =
            new MetricIdentity("metric.body_weight");

        /// <summary>
        /// Canonical identity for sleep.
        /// Dimension: Duration
        /// </summary>
        public static readonly MetricIdentity Sleep =
            new MetricIdentity("metric.sleep");
    }
}
