using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

/// <summary>
///     Custom tooltip manager for Weekly Distribution Chart that shows interval breakdown with percentages and counts.
///     Uses DataHover event to display custom popup tooltip.
/// </summary>
public class WeeklyDistributionTooltip : BucketDistributionTooltip
{
    public WeeklyDistributionTooltip(CartesianChart chart, Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> bucketIntervalData, Dictionary<int, double>? bucketAverages = null) : base(chart, bucketIntervalData, bucketAverages)
    {
    }

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
}
