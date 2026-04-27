namespace DataVisualiser.Core.Strategies.Abstractions;

public class IntervalShadingContext
{
    public List<(double Min, double Max)> Intervals { get; set; } = new();
    public Dictionary<int, Dictionary<int, int>> FrequenciesPerBucket { get; set; } = new();
    public Dictionary<int, List<double>> BucketValues { get; set; } = new();
    public double GlobalMin { get; set; }
    public double GlobalMax { get; set; }
}
