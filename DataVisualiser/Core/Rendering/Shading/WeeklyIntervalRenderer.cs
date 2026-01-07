namespace DataVisualiser.Core.Rendering.Shading;

/// <summary>
///     Renders interval series for weekly distribution charts.
///     Extracted from WeeklyDistributionService to improve testability and maintainability.
/// </summary>
public sealed class WeeklyIntervalRenderer : BucketIntervalRenderer
{
    protected override double MaxColumnWidth => 40.0;

    protected override int BucketCount => 7;
}