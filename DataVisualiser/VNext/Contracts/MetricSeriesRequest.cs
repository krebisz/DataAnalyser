using DataVisualiser.Shared.Models;

namespace DataVisualiser.VNext.Contracts;

public sealed record MetricSeriesRequest
{
    public MetricSeriesRequest(
        string metricType,
        string? subtype = null,
        string? displayMetricType = null,
        string? displaySubtype = null)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        MetricType = metricType;
        Subtype = subtype;
        DisplayMetricType = displayMetricType;
        DisplaySubtype = displaySubtype;
    }

    public string MetricType { get; }
    public string? Subtype { get; }
    public string? DisplayMetricType { get; }
    public string? DisplaySubtype { get; }

    public string? QuerySubtype =>
        string.IsNullOrWhiteSpace(Subtype) || string.Equals(Subtype, "(All)", StringComparison.OrdinalIgnoreCase)
            ? null
            : Subtype;

    public string DisplayName
    {
        get
        {
            var metricDisplay = string.IsNullOrWhiteSpace(DisplayMetricType) ? MetricType : DisplayMetricType;
            var subtypeDisplay = string.IsNullOrWhiteSpace(DisplaySubtype) ? Subtype : DisplaySubtype;

            if (string.IsNullOrWhiteSpace(subtypeDisplay) || string.Equals(subtypeDisplay, "(All)", StringComparison.OrdinalIgnoreCase))
                return metricDisplay;

            return $"{metricDisplay} - {subtypeDisplay}";
        }
    }

    public string SignatureToken => $"{MetricType}:{QuerySubtype ?? "<none>"}";

    public MetricSeriesSelection ToLegacySelection()
    {
        return new MetricSeriesSelection(MetricType, Subtype, DisplayMetricType, DisplaySubtype);
    }

    public static MetricSeriesRequest FromLegacy(MetricSeriesSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new MetricSeriesRequest(selection.MetricType, selection.Subtype, selection.DisplayMetricType, selection.DisplaySubtype);
    }
}
