namespace DataVisualiser.Shared.Models;

public sealed class WeekdayTrendResult
{
    // Key: 0 = Monday … 6 = Sunday
    public Dictionary<int, WeekdayTrendSeries> SeriesByDay { get; } = new();

    public DateTime From { get; set; }
    public DateTime To { get; set; }

    // For uniform Y-axis scaling
    public double GlobalMin { get; set; }
    public double GlobalMax { get; set; }

    public string? Unit { get; set; }
}
