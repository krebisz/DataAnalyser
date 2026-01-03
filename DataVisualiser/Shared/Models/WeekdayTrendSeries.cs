namespace DataVisualiser.Shared.Models;

public sealed class WeekdayTrendSeries
{
    public DayOfWeek Day { get; set; }

    // Ordered by Date ascending
    public IReadOnlyList<WeekdayTrendPoint> Points { get; set; } = Array.Empty<WeekdayTrendPoint>();
}