using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Transforms;

public sealed class TransformDataHelperTests
{
    [Fact]
    public void CreateTransformResultData_ShouldCreateOneObjectPerInput()
    {
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = new DateTime(2024, 1, 1),
                        Value = 1m
                },
                new()
                {
                        NormalizedTimestamp = new DateTime(2024, 1, 2),
                        Value = 2m
                }
        };

        var results = new List<double>
        {
                10.123456,
                double.NaN
        };

        var output = TransformExpressionEvaluator.CreateTransformResultData(data, results);

        Assert.Equal(2, output.Count);
    }

    [Fact]
    public void CreateTransformResultData_ShouldFormatTimestampAndValue()
    {
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = new DateTime(2024, 1, 1, 13, 5, 9),
                        Value = 1m
                }
        };

        var results = new List<double>
        {
                10.123456
        };

        var output = TransformExpressionEvaluator.CreateTransformResultData(data, results);

        Assert.Single(output);

        var item = output[0]!;
        var timestamp = item.GetType().
                             GetProperty("Timestamp")!.GetValue(item) as string;
        var value = item.GetType().
                         GetProperty("Value")!.GetValue(item) as string;

        Assert.Equal("2024-01-01 13:05:09", timestamp);
        Assert.Equal("10.1235", value);
    }

    [Fact]
    public void CreateTransformResultData_ShouldRenderNaNAsStringNaN()
    {
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = new DateTime(2024, 1, 1),
                        Value = 1m
                }
        };

        var results = new List<double>
        {
                double.NaN
        };

        var output = TransformExpressionEvaluator.CreateTransformResultData(data, results);

        var item = output[0]!;
        var value = item.GetType().
                         GetProperty("Value")!.GetValue(item) as string;

        Assert.Equal("NaN", value);
    }
}