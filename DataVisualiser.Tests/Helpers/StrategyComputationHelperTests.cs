using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Helpers;

public sealed class StrategyComputationHelperTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 05);

    [Fact]
    public void PrepareDataForComputation_ShouldReturnNull_WhenBothInputsEmpty()
    {
        var result = StrategyComputationHelper.PrepareDataForComputation(Enumerable.Empty<MetricData>(), Enumerable.Empty<MetricData>(), From, To);

        Assert.Null(result);
    }

    [Fact]
    public void PrepareDataForComputation_ShouldOrderAndFilterInputs()
    {
        var left = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From.AddDays(2),
                        Value = 30
                },
                new()
                {
                        NormalizedTimestamp = From,
                        Value = null
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 20
                }
        };

        var right = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 5
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 10
                }
        };

        var result = StrategyComputationHelper.PrepareDataForComputation(left, right, From, To);

        Assert.NotNull(result);

        var ordered1 = result!.Value.Item1;
        var ordered2 = result.Value.Item2;

        Assert.Equal(2, ordered1.Count);
        Assert.Equal(2, ordered2.Count);
        Assert.True(ordered1.Select(x => x.NormalizedTimestamp).SequenceEqual(ordered1.Select(x => x.NormalizedTimestamp).OrderBy(t => t)));
    }

    [Fact]
    public void CombineTimestamps_ShouldUnionAndOrder_FromHealthMetricData()
    {
        var left = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1)
                }
        };

        var right = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From.AddDays(1)
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(2)
                }
        };

        var combined = StrategyComputationHelper.CombineTimestamps(left, right);

        Assert.Equal(3, combined.Count);
        Assert.True(combined.SequenceEqual(combined.OrderBy(t => t)));
    }

    [Fact]
    public void ExtractAlignedRawValues_ShouldReturnPairedLists_WithNaNForMissing()
    {
        var timestamps = new List<DateTime>
        {
                From,
                From.AddDays(1)
        };

        var dict1 = new Dictionary<DateTime, double>
        {
                { From, 10 }
        };

        var dict2 = new Dictionary<DateTime, double>
        {
                { From.AddDays(1), 20 }
        };

        var (raw1, raw2) = StrategyComputationHelper.ExtractAlignedRawValues(timestamps, dict1, dict2);

        Assert.Equal(2, raw1.Count);
        Assert.True(double.IsNaN(raw1[1]));
        Assert.True(double.IsNaN(raw2[0]));
    }

    [Fact]
    public void ProcessSmoothedData_ShouldReturnPairedSmoothedLists()
    {
        var left = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 20
                }
        };

        var right = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 5
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 15
                }
        };

        var timestamps = StrategyComputationHelper.CombineTimestamps(left, right);

        var (s1, s2) = StrategyComputationHelper.ProcessSmoothedData(left, right, timestamps, From, To);

        Assert.Equal(2, s1.Count);
        Assert.Equal(2, s2.Count);
    }

    [Fact]
    public void PrepareOrderedData_ShouldFilterNullValues_AndSortAscending()
    {
        var source = new[]
        {
                new MetricData
                {
                        NormalizedTimestamp = From.AddDays(2),
                        Value = 30
                },
                new MetricData
                {
                        NormalizedTimestamp = From,
                        Value = null
                },
                new MetricData
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 20
                }
        };

        var result = StrategyComputationHelper.PrepareOrderedData(source);

        Assert.Equal(2, result.Count);
        Assert.Equal(From.AddDays(1), result[0].NormalizedTimestamp);
        Assert.Equal(From.AddDays(2), result[1].NormalizedTimestamp);
    }

    [Fact]
    public void FilterAndOrderByRange_ShouldApplyInclusiveBounds()
    {
        var source = new[]
        {
                new MetricData
                {
                        NormalizedTimestamp = From.AddDays(-1),
                        Value = 1
                },
                new MetricData
                {
                        NormalizedTimestamp = From,
                        Value = 2
                },
                new MetricData
                {
                        NormalizedTimestamp = To,
                        Value = 3
                },
                new MetricData
                {
                        NormalizedTimestamp = To.AddDays(1),
                        Value = 4
                }
        };

        var result = StrategyComputationHelper.FilterAndOrderByRange(source, From, To);

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { From, To }, result.Select(item => item.NormalizedTimestamp));
    }

    [Fact]
    public void CreateTimestampValueDictionaries_ShouldKeepFirstValue_ForDuplicateTimestamps()
    {
        var timestamp = From;
        var ordered = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = timestamp,
                        Value = 10
                },
                new()
                {
                        NormalizedTimestamp = timestamp,
                        Value = 20
                }
        };

        var (dict1, dict2) = StrategyComputationHelper.CreateTimestampValueDictionaries(ordered, new List<MetricData>());

        Assert.Equal(10.0, dict1[timestamp]);
        Assert.Empty(dict2);
    }

    [Fact]
    public void GetUnit_ShouldPreferPrimarySeries_AndFallbackToSecondary()
    {
        var primary = new List<MetricData>
        {
                new()
                {
                        Unit = "kg"
                }
        };
        var secondary = new List<MetricData>
        {
                new()
                {
                        Unit = "lbs"
                }
        };

        Assert.Equal("kg", StrategyComputationHelper.GetUnit(primary, secondary));
        Assert.Equal("lbs", StrategyComputationHelper.GetUnit(new List<MetricData>(), secondary));
    }

    [Fact]
    public void AlignByIndex_ShouldProjectMetricDataSeries()
    {
        var left = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = null
                }
        };
        var right = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 5
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 15
                }
        };

        var (timestamps, primary, secondary) = StrategyComputationHelper.AlignByIndex(left, right, 2);

        Assert.Equal(new[] { From, From.AddDays(1) }, timestamps);
        Assert.Equal(10.0, primary[0]);
        Assert.True(double.IsNaN(primary[1]));
        Assert.Equal(15.0, secondary[1]);
    }

    [Fact]
    public void AlignByIndex_ShouldProjectCmsSeries()
    {
        var left = new List<(DateTime Timestamp, decimal? ValueDecimal)>
        {
                (From, 10m),
                (From.AddDays(1), null)
        };
        var right = new List<(DateTime Timestamp, decimal? ValueDecimal)>
        {
                (From, 5m),
                (From.AddDays(1), 15m)
        };

        var (timestamps, primary, secondary) = StrategyComputationHelper.AlignByIndex(left, right, 2);

        Assert.Equal(new[] { From, From.AddDays(1) }, timestamps);
        Assert.Equal(10.0, primary[0]);
        Assert.True(double.IsNaN(primary[1]));
        Assert.Equal(15.0, secondary[1]);
    }
}
