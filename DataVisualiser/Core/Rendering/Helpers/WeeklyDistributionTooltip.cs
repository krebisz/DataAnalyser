using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

/// <summary>
///     Custom tooltip manager for Weekly Distribution Chart that shows interval breakdown with percentages and counts.
///     Uses DataHover event to display custom popup tooltip.
/// </summary>
public class WeeklyDistributionTooltip : BucketDistributionTooltip
{
    protected override int BucketCount => 7;

    protected override string[] BucketNames { get; } =
    {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"
    };

    public WeeklyDistributionTooltip(CartesianChart chart, Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> bucketIntervalData)
        : base(chart, bucketIntervalData)
    {
    }
}