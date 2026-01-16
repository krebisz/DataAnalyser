namespace DataVisualiser.Shared.Models;

public sealed record MetricNameOption(string Value, string Display)
{
    public override string ToString() => Display;
}
