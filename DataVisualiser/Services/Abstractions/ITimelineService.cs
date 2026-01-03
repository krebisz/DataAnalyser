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