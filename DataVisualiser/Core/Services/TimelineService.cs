using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Helpers;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Implementation of ITimelineService.
///     Provides unified timeline and interval generation with caching.
/// </summary>
public sealed class TimelineService : ITimelineService
{
    private readonly Dictionary<string, TimelineResult> _cache = new();

    public TimelineResult GenerateTimeline(DateTime from, DateTime to, IReadOnlyList<DateTime>? dataTimestamps = null)
    {
        // Create cache key based on date range
        var cacheKey = $"{from:yyyyMMddHHmmss}_{to:yyyyMMddHHmmss}";

        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var dateRange = to - from;
        var tickInterval = MathHelper.DetermineTickInterval(dateRange);
        var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(from, to, tickInterval);

        var result = new TimelineResult
        {
                DateRange = dateRange,
                TickInterval = tickInterval,
                NormalizedIntervals = normalizedIntervals,
                From = from,
                To = to
        };

        _cache[cacheKey] = result;
        return result;
    }

    public IReadOnlyList<int> MapToIntervals(IReadOnlyList<DateTime> timestamps, TimelineResult timeline)
    {
        if (timestamps == null || timestamps.Count == 0)
            return Array.Empty<int>();

        var intervalsList = timeline.NormalizedIntervals is List<DateTime> list ? list : timeline.NormalizedIntervals.ToList();

        return timestamps.Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, intervalsList, timeline.TickInterval)).ToList();
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
}