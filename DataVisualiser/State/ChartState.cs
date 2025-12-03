using DataVisualiser.Class;
using LiveCharts.Wpf;

namespace DataVisualiser.State
{
    public class ChartState
    {
        // Track which charts are visible
        public bool IsNormalizedVisible { get; set; }
        public bool IsDifferenceVisible { get; set; }
        public bool IsRatioVisible { get; set; }
        public bool IsWeeklyVisible { get; set; }

        // Normalization mode
        public NormalizationMode SelectedNormalizationMode { get; set; }

        // Chart data from last load
        public ChartDataContext? LastContext { get; set; }

        // Current chart titles (left + right)
        public string LeftTitle { get; set; } = string.Empty;
        public string RightTitle { get; set; } = string.Empty;

        // Timestamps linked to each chart
        public Dictionary<CartesianChart, List<DateTime>> ChartTimestamps { get; } = new Dictionary<CartesianChart, List<DateTime>>();
    }
}
