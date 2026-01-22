namespace DataVisualiser.Shared.Models;

public sealed record MetricSeriesSelection
{
    public MetricSeriesSelection(string metricType, string? subtype, string? displayMetricType = null, string? displaySubtype = null)
    {
        MetricType = metricType;
        Subtype = subtype;
        DisplayMetricType = displayMetricType;
        DisplaySubtype = displaySubtype;
    }

    public string MetricType { get; }
    public string? Subtype { get; }
    public string? DisplayMetricType { get; }
    public string? DisplaySubtype { get; }

    public string? QuerySubtype => string.IsNullOrWhiteSpace(Subtype) || Subtype == "(All)" ? null : Subtype;

    public string DisplayName
    {
        get
        {
            var metricDisplay = string.IsNullOrWhiteSpace(DisplayMetricType) ? MetricType : DisplayMetricType;
            var subtypeDisplay = string.IsNullOrWhiteSpace(DisplaySubtype) ? Subtype : DisplaySubtype;

            if (string.IsNullOrWhiteSpace(subtypeDisplay) || subtypeDisplay == "(All)")
                return metricDisplay;

            return $"{metricDisplay} - {subtypeDisplay}";
        }
    }

    public string DisplayKey
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Subtype) || Subtype == "(All)")
                return MetricType;

            return $"{MetricType}:{Subtype}";
        }
    }
}