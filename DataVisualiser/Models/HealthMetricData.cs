namespace DataVisualiser.Models
{
    /// <summary>
    /// Data class for health metric records
    /// </summary>
    public class HealthMetricData
    {
        public DateTime NormalizedTimestamp { get; set; }
        public decimal? Value { get; set; }
        public string? Unit { get; set; }
        public string? Provider { get; set; }
    }
}

