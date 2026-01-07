namespace DataVisualiser.Core.Rendering.Shading;

/// <summary>
///     Renders interval series for hourly distribution charts.
///     Extracted from HourlyDistributionService to improve testability and maintainability.
/// </summary>
public sealed class HourlyIntervalRenderer : BucketIntervalRenderer
{
    protected override double MaxColumnWidth => 40.0;

    protected override int BucketCount => 24;
}