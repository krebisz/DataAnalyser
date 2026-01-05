using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

/// <summary>
///     Custom tooltip manager for Hourly Distribution Chart that shows interval breakdown with percentages and counts.
///     Uses DataHover event to display custom popup tooltip.
/// </summary>
public class HourlyDistributionTooltip : BucketDistributionTooltip
{
    protected override int BucketCount => 24;

    protected override string[] BucketNames { get; } =
    {
            "12AM",
            "1AM",
            "2AM",
            "3AM",
            "4AM",
            "5AM",
            "6AM",
            "7AM",
            "8AM",
            "9AM",
            "10AM",
            "11AM",
            "12PM",
            "1PM",
            "2PM",
            "3PM",
            "4PM",
            "5PM",
            "6PM",
            "7PM",
            "8PM",
            "9PM",
            "10PM",
            "11PM"
    };

    public HourlyDistributionTooltip(CartesianChart chart, Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> bucketIntervalData)
        : base(chart, bucketIntervalData)
    {
    }
}