namespace DataVisualiser.Shared.Models;

public sealed class WeekdayTrendPoint
{
    public DateTime Date        { get; set; }
    public double   Value       { get; set; }
    public int      SampleCount { get; set; }
}