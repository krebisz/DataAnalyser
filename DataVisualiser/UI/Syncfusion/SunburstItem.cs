namespace DataVisualiser.UI.Syncfusion;

public sealed class SunburstItem
{
    public SunburstItem(string bucket, string submetric, double value)
    {
        Bucket = bucket;
        Submetric = submetric;
        Value = value;
    }

    public string Bucket { get; }
    public string Submetric { get; }
    public double Value { get; }
    public double BucketTotal { get; set; }
    public string PercentText { get; set; } = "Percent: n/a";
}
