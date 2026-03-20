using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Helpers;

public sealed class TransformChartAxisCalculatorTests
{
    [Fact]
    public void TryCreateYAxisLayout_ShouldReturnInvalidLayout_WhenNoUsableValuesExist()
    {
        var success = TransformChartAxisCalculator.TryCreateYAxisLayout(new List<MetricData>(), new List<double>(), out var layout);

        Assert.False(success);
        Assert.False(layout.ShowLabels);
        Assert.True(double.IsNaN(layout.MinValue));
        Assert.True(double.IsNaN(layout.MaxValue));
        Assert.Null(layout.Step);
    }

    [Fact]
    public void TryCreateYAxisLayout_ShouldClampMinimumToZero_WhenAllDataIsPositive()
    {
        var rawData = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = new DateTime(2024, 01, 01),
                        Value = 10m
                },
                new()
                {
                        NormalizedTimestamp = new DateTime(2024, 01, 02),
                        Value = 12m
                }
        };
        var smoothedValues = new List<double> { 11, 13 };

        var success = TransformChartAxisCalculator.TryCreateYAxisLayout(rawData, smoothedValues, out var layout);

        Assert.True(success);
        Assert.True(layout.ShowLabels);
        Assert.True(layout.MinValue >= 0);
        Assert.True(layout.MaxValue > layout.MinValue);
        Assert.True(layout.Step.HasValue);
        Assert.True(layout.Step.Value > 0);
    }

    [Fact]
    public void CalculateChartHeight_ShouldRespectMinimumHeight_WhenAxisValuesAreInvalid()
    {
        var height = TransformChartAxisCalculator.CalculateChartHeight(double.NaN, 10, 1, 400);

        Assert.Equal(400, height);
    }

    [Fact]
    public void CalculateChartHeight_ShouldExpandWithinConfiguredBounds_WhenTicksIncrease()
    {
        var height = TransformChartAxisCalculator.CalculateChartHeight(0, 100, 10, 200);

        Assert.True(height > 200);
        Assert.True(height <= 2000);
    }
}
