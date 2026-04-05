using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.Controls;

public sealed class ChartContextSelectionGuardTests
{
    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnTrue_WhenPrimaryAndSecondaryMatchLoadedContext()
    {
        var context = CreateContext(includeSecondary: true);
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "morning", "Weight", "Morning"),
            new("Weight", "evening", "Weight", "Evening")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections);

        Assert.True(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnTrue_WhenSubtypesChangeWithinSameMetricFamily()
    {
        var context = CreateContext(includeSecondary: true);
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "weekly_avg", "Weight", "Weekly Avg"),
            new("Weight", "monthly_avg", "Weight", "Monthly Avg")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections);

        Assert.True(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnTrue_WhenAdditionalSelectionsPreserveMetricFamily()
    {
        var context = CreateContext(includeSecondary: true);
        var selections = new List<MetricSeriesSelection>
        {
            new("Weight", "morning", "Weight", "Morning"),
            new("Weight", "evening", "Weight", "Evening"),
            new("Weight", "weekly_avg", "Weight", "Weekly Avg")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections);

        Assert.True(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenMetricTypeChanged()
    {
        var context = CreateContext(includeSecondary: true);
        var selections = new List<MetricSeriesSelection>
        {
            new("SkinTemperature", "max", "Skin Temperature", "Max"),
            new("SkinTemperature", "min", "Skin Temperature", "Min")
        };

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "SkinTemperature", selections);

        Assert.False(result);
    }

    [Fact]
    public void IsCompatibleWithCurrentSelection_ShouldReturnFalse_WhenNoSelectionsRemain()
    {
        var context = CreateContext(includeSecondary: true);
        var selections = new List<MetricSeriesSelection>();

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections);

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

        var result = ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(context, "Weight", selections);

        Assert.False(result);
    }

    private static ChartDataContext CreateContext(bool includeSecondary)
    {
        return new ChartDataContext
        {
            Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
            Data2 = includeSecondary ? [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }] : null,
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            SecondaryMetricType = includeSecondary ? "Weight" : null,
            PrimarySubtype = "morning",
            SecondarySubtype = includeSecondary ? "evening" : null
        };
    }
}
