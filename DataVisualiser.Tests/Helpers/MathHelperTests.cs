using DataVisualiser.Core.Configuration.Defaults;
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
        Assert.All(baseline!, v => Assert.Equal(100.0, v));
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

    [Fact]
    public void CalculateOptimalMaxRecords_ShouldReturnNull_ForShortRanges()
    {
        var result = MathHelper.CalculateOptimalMaxRecords(new DateTime(2024, 01, 01), new DateTime(2024, 01, 01, 12, 0, 0));

        Assert.Null(result);
    }

    [Fact]
    public void CalculateOptimalMaxRecords_ShouldReturnConfiguredLimit_ForVeryLargeRanges()
    {
        var result = MathHelper.CalculateOptimalMaxRecords(new DateTime(2024, 01, 01), new DateTime(2026, 01, 05));

        Assert.Equal(ComputationDefaults.SqlLimitingMaxRecords, result);
    }

    [Fact]
    public void CalculateSeparatorStep_ShouldScaleToRequestedIntervalDensity()
    {
        var result = MathHelper.CalculateSeparatorStep(TickInterval.Day, 100, TimeSpan.FromDays(10));

        Assert.Equal(10.0, result);
    }

    [Fact]
    public void CreateSmoothedData_ShouldAggregateIntoAveragedPoint()
    {
        var from = new DateTime(2024, 01, 01);
        var to = from.AddDays(2);
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = from,
                        Value = 10m
                },
                new()
                {
                        NormalizedTimestamp = from.AddDays(1),
                        Value = 20m
                },
                new()
                {
                        NormalizedTimestamp = from.AddDays(2),
                        Value = 40m
                }
        };

        var result = MathHelper.CreateSmoothedData(data, from, to);

        var point = Assert.Single(result);
        Assert.Equal(23.333333333333332, point.Value, 10);
        Assert.Equal(from.AddDays(1), point.Timestamp);
    }

    [Fact]
    public void InterpolateSmoothedData_ShouldReturnNaNSeries_WhenNoSmoothedPointsExist()
    {
        var timestamps = new List<DateTime>
        {
                new(2024, 01, 01),
                new(2024, 01, 02)
        };

        var result = MathHelper.InterpolateSmoothedData(new List<SmoothedDataPoint>(), timestamps);

        Assert.Equal(2, result.Count);
        Assert.All(result, value => Assert.True(double.IsNaN(value)));
    }

    [Fact]
    public void ReturnValueDifferences_ShouldSubtractPairwise()
    {
        var result = MathHelper.ReturnValueDifferences(new List<double>
        {
                10,
                6
        }, new List<double>
        {
                3,
                2
        });

        Assert.Equal(new[]
        {
                7.0,
                4.0
        }, result);
    }

    [Fact]
    public void ReturnValueRatios_ShouldReturnNaN_WhenDividingByZero()
    {
        var result = MathHelper.ReturnValueRatios(new List<double>
        {
                10,
                6
        }, new List<double>
        {
                2,
                0
        });

        Assert.NotNull(result);
        Assert.Equal(5.0, result![0]);
        Assert.True(double.IsNaN(result[1]));
    }
}
