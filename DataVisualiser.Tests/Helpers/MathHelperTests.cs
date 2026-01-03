using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Helpers;

public sealed class MathHelperTests
{
    [Fact]
    public void DetermineTickInterval_ShouldReturnMonth_ForLargeRanges()
    {
        var interval = MathHelper.DetermineTickInterval(TimeSpan.FromDays(900));
        Assert.Equal(TickInterval.Month, interval);
    }

    [Fact]
    public void DetermineTickInterval_ShouldReturnDay_ForMediumRanges()
    {
        var interval = MathHelper.DetermineTickInterval(TimeSpan.FromDays(40));
        Assert.Equal(TickInterval.Day, interval);
    }

    [Fact]
    public void GenerateNormalizedIntervals_ShouldReturnOrderedIntervals()
    {
        var from = new DateTime(2024, 01, 01);
        var to = new DateTime(2024, 01, 05);

        var intervals = MathHelper.GenerateNormalizedIntervals(from, to, TickInterval.Day);

        Assert.NotEmpty(intervals);
        Assert.True(intervals.SequenceEqual(intervals.OrderBy(d => d)));
    }

    [Fact]
    public void MapTimestampToIntervalIndex_ShouldReturnValidIndex()
    {
        var from = new DateTime(2024, 01, 01);
        var to = new DateTime(2024, 01, 10);
        var intervals = MathHelper.GenerateNormalizedIntervals(from, to, TickInterval.Day);

        var index = MathHelper.MapTimestampToIntervalIndex(new DateTime(2024, 01, 05), intervals, TickInterval.Day);

        Assert.InRange(index, 0, intervals.Count - 1);
    }

    [Fact]
    public void ReturnValueNormalized_ZeroToOne_ShouldNormalizeValues()
    {
        var values = new List<double>
        {
                10,
                20,
                30
        };

        var normalized = MathHelper.ReturnValueNormalized(values, NormalizationMode.ZeroToOne);

        Assert.NotNull(normalized);
        Assert.Equal(0.0, normalized![0], 5);
        Assert.Equal(1.0, normalized[^1], 5);
    }

    [Fact]
    public void ReturnValueNormalized_PercentageOfMax_ShouldNormalizeTo100()
    {
        var values = new List<double>
        {
                10,
                20,
                40
        };

        var normalized = MathHelper.ReturnValueNormalized(values, NormalizationMode.PercentageOfMax);

        Assert.NotNull(normalized);
        Assert.Equal(100.0, normalized![^1], 5);
    }

    [Fact]
    public void ReturnValueNormalized_RelativeToMax_ShouldPropagateNaN_WhenFirstSeriesHasNoRange()
    {
        var first = new List<double>
        {
                50,
                100
        };
        var second = new List<double>
        {
                100,
                100
        };

        var (relative, baseline) = MathHelper.ReturnValueNormalized(first, second, NormalizationMode.RelativeToMax);

        Assert.NotNull(relative);
        Assert.NotNull(baseline);

        // Baseline is always a flat 100%
        Assert.All(baseline!, v => Assert.Equal(100.0, v));

        // First series has no usable range → NaN propagates
        Assert.All(relative!, v => Assert.True(double.IsNaN(v)));
    }


    [Fact]
    public void ApplyBinaryOperation_ShouldReturnNaN_WhenInvalid()
    {
        var a = new List<double>
        {
                1.0
        };
        var b = new List<double>
        {
                0.0
        };

        var result = MathHelper.ApplyBinaryOperation(a, b, (x, y) => x / y);

        Assert.True(double.IsNaN(result[0]));
    }

    [Fact]
    public void ApplyUnaryOperation_ShouldPreserveNaN()
    {
        var values = new List<double>
        {
                double.NaN,
                10.0
        };

        var result = MathHelper.ApplyUnaryOperation(values, v => v * 2);

        Assert.True(double.IsNaN(result[0]));
        Assert.Equal(20.0, result[1]);
    }

    [Fact]
    public void DetermineRecordToDayRatio_ShouldReturnCorrectEnum()
    {
        var ratio = MathHelper.DetermineRecordToDayRatio(0.5m);
        Assert.Equal(RecordToDayRatio.Day, ratio);
    }
}