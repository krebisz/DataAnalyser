namespace DataVisualiser.Shared.Models;

public sealed record MetricSeriesSelection(string MetricType, string? Subtype)
{
    public string? QuerySubtype => string.IsNullOrWhiteSpace(Subtype) || Subtype == "(All)" ? null : Subtype;

    public string DisplayName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Subtype) || Subtype == "(All)")
                return MetricType;

            return $"{MetricType} - {Subtype}";
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