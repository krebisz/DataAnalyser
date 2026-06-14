using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformInputDateRangeResolver
{
    private readonly Func<MetricSeriesSelection, string, Task<(DateTime MinDate, DateTime MaxDate)?>> _loadDateRange;

    public TransformInputDateRangeResolver(
        Func<MetricSeriesSelection, string, Task<(DateTime MinDate, DateTime MaxDate)?>> loadDateRange)
    {
        _loadDateRange = loadDateRange ?? throw new ArgumentNullException(nameof(loadDateRange));
    }

    public async Task<TransformInputDateRange?> ResolveAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            return null;

        var ranges = new List<(DateTime MinDate, DateTime MaxDate)>();
        foreach (var request in series)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var range = await _loadDateRange(request.ToLegacySelection(), resolutionTableName);
            if (range.HasValue)
                ranges.Add(range.Value);
        }

        return ranges.Count == 0
            ? null
            : new TransformInputDateRange(
                ranges.Min(item => item.MinDate),
                ranges.Max(item => item.MaxDate));
    }
}

internal sealed record TransformInputDateRange(DateTime From, DateTime To);
