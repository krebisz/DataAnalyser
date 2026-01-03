using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Result of timeline generation.
/// </summary>
public class TimelineResult
{
    public TimeSpan                DateRange           { get; init; }
    public TickInterval            TickInterval        { get; init; }
    public IReadOnlyList<DateTime> NormalizedIntervals { get; init; } = Array.Empty<DateTime>();
    public DateTime                From                { get; init; }
    public DateTime                To                  { get; init; }
}