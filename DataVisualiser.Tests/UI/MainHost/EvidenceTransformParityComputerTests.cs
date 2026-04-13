using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class EvidenceTransformParityComputerTests
{
    [Fact]
    public void ComputeUnary_WithMatchingLogResults_Passes()
    {
        var data = new[]
        {
            CreateMetricData(1m, 2026, 4, 1),
            CreateMetricData(10m, 2026, 4, 2)
        };

        var result = EvidenceTransformParityComputer.ComputeUnary(data, "Log");

        Assert.True(result.Result.Passed);
        Assert.True(result.ExpressionAvailable);
        Assert.Equal(2, result.LegacySamples);
        Assert.Equal(2, result.NewSamples);
    }

    [Fact]
    public void ComputeBinary_WithMatchingAddResults_Passes()
    {
        var data1 = new[]
        {
            CreateMetricData(1m, 2026, 4, 1),
            CreateMetricData(2m, 2026, 4, 2)
        };
        var data2 = new[]
        {
            CreateMetricData(3m, 2026, 4, 1),
            CreateMetricData(4m, 2026, 4, 2)
        };

        var result = EvidenceTransformParityComputer.ComputeBinary(data1, data2, "Add");

        Assert.True(result.Result.Passed);
        Assert.True(result.ExpressionAvailable);
        Assert.Equal(2, result.LegacySamples);
        Assert.Equal(2, result.NewSamples);
    }

    [Theory]
    [InlineData("Log", true)]
    [InlineData("Sqrt", true)]
    [InlineData("Add", false)]
    public void IsUnaryTransform_ShouldClassifyOperations(string operation, bool expected)
    {
        Assert.Equal(expected, EvidenceTransformParityComputer.IsUnaryTransform(operation));
    }

    private static MetricData CreateMetricData(decimal value, int year, int month, int day)
    {
        return new MetricData
        {
            Value = value,
            NormalizedTimestamp = new DateTime(year, month, day)
        };
    }
}
