using DataVisualiser.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVisualiser
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
