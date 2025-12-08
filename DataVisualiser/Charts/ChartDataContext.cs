using DataVisualiser.Models;

namespace DataVisualiser.Charts
{
    public class ChartDataContext
    {
        public IEnumerable<HealthMetricData>? Data1 { get; set; }
        public IEnumerable<HealthMetricData>? Data2 { get; set; }
        public string DisplayName1 { get; set; } = string.Empty;
        public string DisplayName2 { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}

