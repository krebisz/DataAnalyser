using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration;

public sealed record MetricLoadRequest
{
    public MetricLoadRequest(
        string metricType,
        IReadOnlyList<MetricSeriesSelection> selectedSeries,
        DateTime from,
        DateTime to,
        string resolutionTableName)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        if (string.IsNullOrWhiteSpace(resolutionTableName))
            throw new ArgumentException("Resolution table name cannot be null or empty.", nameof(resolutionTableName));

        MetricType = metricType;
        SelectedSeries = selectedSeries?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(selectedSeries));
        From = from;
        To = to;
        ResolutionTableName = resolutionTableName;
    }

    public string MetricType { get; }
    public IReadOnlyList<MetricSeriesSelection> SelectedSeries { get; }
    public DateTime From { get; }
    public DateTime To { get; }
    public string ResolutionTableName { get; }

    public MetricSeriesSelection? PrimarySelection => SelectedSeries.Count > 0 ? SelectedSeries[0] : null;
    public MetricSeriesSelection? SecondarySelection => SelectedSeries.Count > 1 ? SelectedSeries[1] : null;

    public string Signature =>
        $"{MetricType}::{ResolutionTableName}::{From:O}->{To:O}::{string.Join("|", SelectedSeries.Select(series => $"{series.MetricType}:{series.QuerySubtype ?? "<none>"}"))}";
}
