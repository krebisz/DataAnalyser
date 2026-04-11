namespace DataVisualiser.VNext.Contracts;

public sealed record MetricLoadSnapshot(
    MetricSelectionRequest Request,
    IReadOnlyList<MetricSeriesSnapshot> Series,
    DateTime CreatedUtc)
{
    public string Signature => Request.Signature;
    public int SeriesCount => Series.Count;
}
