namespace DataVisualiser.VNext.Contracts;

public sealed record MetricSelectionRequest
{
    public MetricSelectionRequest(
        string metricType,
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        IReadOnlyList<DataViewKind>? requestedViews = null)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));
        if (string.IsNullOrWhiteSpace(resolutionTableName))
            throw new ArgumentException("Resolution table name cannot be null or empty.", nameof(resolutionTableName));
        if (from > to)
            throw new ArgumentException("From date must be earlier than or equal to To date.");

        MetricType = metricType;
        Series = series?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(series));
        From = from;
        To = to;
        ResolutionTableName = resolutionTableName;
        RequestedViews = (requestedViews?.Count ?? 0) == 0
            ? new[] { DataViewKind.Raw, DataViewKind.Canonical }
            : requestedViews!.ToArray();
    }

    public string MetricType { get; }
    public IReadOnlyList<MetricSeriesRequest> Series { get; }
    public DateTime From { get; }
    public DateTime To { get; }
    public string ResolutionTableName { get; }
    public IReadOnlyList<DataViewKind> RequestedViews { get; }

    public string Signature =>
        $"{MetricType}::{ResolutionTableName}::{From:O}->{To:O}::{string.Join("|", Series.Select(series => series.SignatureToken))}";
}
