using DataVisualiser.Core.Orchestration.Builders;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Orchestration;

public sealed class ChartDataContextBuilderTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 03);

    [Fact]
    public void Build_ShouldAlignAndComputeDerivedValues()
    {
        var data1 = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10m
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(2),
                        Value = 30m
                }
        };

        var data2 = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 5m
                }
        };

        var builder = new ChartDataContextBuilder();

        var ctx = builder.Build("Weight", "A", "B", data1, data2, From, To);

        Assert.Equal(new[]
                {
                        From.Date,
                        From.AddDays(1).Date,
                        From.AddDays(2).Date
                },
                ctx.Timestamps);
        Assert.Equal(new[]
                {
                        10.0,
                        10.0,
                        30.0
                },
                ctx.RawValues1);
        Assert.Equal(new[]
                {
                        0.0,
                        5.0,
                        5.0
                },
                ctx.RawValues2);
        Assert.Equal(new[]
                {
                        10.0,
                        5.0,
                        25.0
                },
                ctx.DifferenceValues);
        Assert.Equal(new[]
                {
                        0.0,
                        2.0,
                        6.0
                },
                ctx.RatioValues);
        Assert.Equal(new[]
                {
                        10.0 / 30.0,
                        10.0 / 30.0,
                        1.0
                },
                ctx.NormalizedValues1);
        Assert.Equal(new[]
                {
                        0.0,
                        1.0,
                        1.0
                },
                ctx.NormalizedValues2);

        AssertInRange(ctx.SmoothedValues1![0], 16.6, 16.7);
        AssertInRange(ctx.SmoothedValues2![0], 3.3, 3.4);
    }

    [Fact]
    public void Build_ShouldSetDisplayNamesAndSemanticCount()
    {
        var data = TestDataBuilders.HealthMetricData().WithTimestamp(From).BuildSeries(1, TimeSpan.FromDays(1));

        var builder = new ChartDataContextBuilder();

        var ctx = builder.Build("Weight", "(All)", null, data, null, From, To);

        Assert.Equal("Weight", ctx.DisplayName1);
        Assert.Equal("Weight", ctx.DisplayName2);
        Assert.Equal(1, ctx.SemanticMetricCount);
    }

    [Fact]
    public void Build_WithCms_ShouldStoreCmsReferences()
    {
        var data = TestDataBuilders.HealthMetricData().WithTimestamp(From).BuildSeries(1, TimeSpan.FromDays(1));

        var cms1 = TestDataBuilders.CanonicalMetricSeries().Build();
        var cms2 = TestDataBuilders.CanonicalMetricSeries().Build();

        var builder = new ChartDataContextBuilder();

        var ctx = builder.Build("Weight", "A", "B", data, data, From, To, cms1, cms2);

        Assert.Same(cms1, ctx.PrimaryCms);
        Assert.Same(cms2, ctx.SecondaryCms);
    }

    private static void AssertInRange(double value, double minInclusive, double maxInclusive)
    {
        Assert.True(value >= minInclusive && value <= maxInclusive, $"Expected {value} to be in range [{minInclusive},{maxInclusive}].");
    }
}