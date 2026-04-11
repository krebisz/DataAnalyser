using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.Controls;

public sealed class ChartContextSelectionGuardTests
{
    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnTrue_WhenPrimaryAndSecondaryMatchLoadedContext()
    {
        var context = CreateContext(includeSecondary: true, loadRequestSignature: "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening");
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "morning", "Weight", "Morning"),
            new("Weight", "evening", "Weight", "Evening")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        Assert.True(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenSubtypesChangeWithinSameMetricFamily()
    {
        var context = CreateContext(includeSecondary: true, loadRequestSignature: "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening");
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "weekly_avg", "Weight", "Weekly Avg"),
            new("Weight", "monthly_avg", "Weight", "Monthly Avg")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        Assert.False(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenAdditionalSelectionsExceedStampedLoadRequest()
    {
        var context = CreateContext(includeSecondary: true, loadRequestSignature: "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening");
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "morning", "Weight", "Morning"),
            new("Weight", "evening", "Weight", "Evening"),
            new("Weight", "weekly_avg", "Weight", "Weekly Avg")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        Assert.False(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenMetricTypeChanged()
    {
        var context = CreateContext(includeSecondary: true, loadRequestSignature: "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening");
        var selections = new List<MetricSeriesSelection>
        {
            new("SkinTemperature", "max", "Skin Temperature", "Max"),
            new("SkinTemperature", "min", "Skin Temperature", "Min")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "SkinTemperature", selections, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        Assert.False(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenNoSelectionsRemain()
    {
        var context = CreateContext(includeSecondary: true);
        var selections = new List<MetricSeriesSelection>();

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        Assert.False(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenSelectedSeriesContainDifferentMetricFamily()
    {
        var context = CreateContext(includeSecondary: true);
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "morning", "Weight", "Morning"),
            new("SkinTemperature", "max", "Skin Temperature", "Max")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        Assert.False(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenSingleSeriesContextIsUsedForTwoSeriesSelection()
    {
        var context = CreateContext(includeSecondary: false, loadRequestSignature: "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning");
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "morning", "Weight", "Morning"),
            new("Weight", "evening", "Weight", "Evening")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        Assert.False(result);
    }

    private static ChartDataContext CreateContext(bool includeSecondary, string? loadRequestSignature = null)
    {
        return new ChartDataContext
        {
            Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
            Data2 = includeSecondary ? [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }] : null,
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            SecondaryMetricType = includeSecondary ? "Weight" : null,
            PrimarySubtype = "morning",
            SecondarySubtype = includeSecondary ? "evening" : null,
            LoadRequestSignature = loadRequestSignature,
            ActualSeriesCount = includeSecondary ? 2 : 1,
            From = new DateTime(2024, 01, 01),
            To = new DateTime(2024, 01, 02)
        };
    }
}
