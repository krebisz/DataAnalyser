using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class BinaryMetricChartContextHelperTests
{
    [Fact]
    public void BuildContext_ShouldMapCommonBinarySeriesFields()
    {
        var source = new ChartDataContext
        {
            MetricType = "FallbackMetric",
            PrimaryMetricType = "PrimaryFallback",
            SecondaryMetricType = "SecondaryFallback",
            DisplayPrimaryMetricType = "Primary Display Fallback",
            DisplaySecondaryMetricType = "Secondary Display Fallback",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 2)
        };
        var primaryData = new List<MetricData>();
        var secondaryData = new List<MetricData>();
        var primary = new MetricSeriesSelection("Weight", "fat", "Weight", "Fat");
        var secondary = new MetricSeriesSelection("Weight", "lean", "Weight", "Lean");

        var context = BinaryMetricChartContextHelper.BuildContext(
            source,
            primary,
            secondary,
            primaryData,
            secondaryData,
            null,
            null,
            "Left",
            "Right");

        Assert.Same(primaryData, context.Data1);
        Assert.Same(secondaryData, context.Data2);
        Assert.Equal("Left", context.DisplayName1);
        Assert.Equal("Right", context.DisplayName2);
        Assert.Equal("Weight", context.MetricType);
        Assert.Equal("Weight", context.PrimaryMetricType);
        Assert.Equal("fat", context.PrimarySubtype);
        Assert.Equal("Weight", context.SecondaryMetricType);
        Assert.Equal("lean", context.SecondarySubtype);
        Assert.Equal("Fat", context.DisplayPrimarySubtype);
        Assert.Equal("Lean", context.DisplaySecondarySubtype);
        Assert.Equal(2, context.ActualSeriesCount);
        Assert.Equal(source.From, context.From);
        Assert.Equal(source.To, context.To);
    }

    [Fact]
    public async Task RerenderIfVisibleAsync_ShouldRenderOnlyWhenVisibleAndContextExists()
    {
        var context = new ChartDataContext();
        var renderCount = 0;

        await BinaryMetricChartContextHelper.RerenderIfVisibleAsync(false, context, _ =>
        {
            renderCount++;
            return Task.CompletedTask;
        });
        await BinaryMetricChartContextHelper.RerenderIfVisibleAsync(true, null, _ =>
        {
            renderCount++;
            return Task.CompletedTask;
        });
        await BinaryMetricChartContextHelper.RerenderIfVisibleAsync(true, context, _ =>
        {
            renderCount++;
            return Task.CompletedTask;
        });

        Assert.Equal(1, renderCount);
    }
}
