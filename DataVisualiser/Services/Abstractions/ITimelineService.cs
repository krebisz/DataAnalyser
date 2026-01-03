using DataVisualiser.Models;

namespace DataVisualiser.Services.Abstractions;

/// <summary>
///     Unified timeline and interval generation service.
///     Eliminates duplication across all strategies.
/// </summary>
public interface ITimelineService
{
    /// <summary>
    ///     Generates unified timeline with intervals for a date range.
    /// </summary>
    TimelineResult GenerateTimeline(DateTime from, DateTime to, IReadOnlyList<DateTime>? dataTimestamps = null);

    /// <summary>
    ///     Maps timestamps to interval indices.
    /// </summary>
    IReadOnlyList<int> MapToIntervals(IReadOnlyList<DateTime> timestamps, TimelineResult timeline);

    /// <summary>
    ///     Clears cached timeline calculations.
    /// </summary>
    void ClearCache();
}

/// <summary>
///     Result of timeline generation.
/// </summary>
public class TimelineResult
{
    public TimeSpan DateRange { get; init; }
    public TickInterval TickInterval { get; init; }
    public IReadOnlyList<DateTime> NormalizedIntervals { get; init; } = Array.Empty<DateTime>();
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}