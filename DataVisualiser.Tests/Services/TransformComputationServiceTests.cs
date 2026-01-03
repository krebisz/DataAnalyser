using DataVisualiser.Core.Transforms;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Services;

/// <summary>
///     Unit tests for TransformComputationService.
///     Tests unary and binary transform operations.
/// </summary>
public sealed class TransformComputationServiceTests
{
    private static readonly DateTime                    BaseDate = new(2024, 1, 1);
    private readonly        TransformComputationService _service;

    public TransformComputationServiceTests()
    {
        _service = new TransformComputationService();
    }

    [Fact]
    public void ComputeUnaryTransform_ShouldReturnEmpty_WhenDataIsEmpty()
    {
        // Arrange
        var data = Enumerable.Empty<MetricData>();

        // Act
        var result = _service.ComputeUnaryTransform(data, "Log");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Empty(result.DataList);
        Assert.Empty(result.ComputedResults);
    }

    [Fact]
    public void ComputeUnaryTransform_ShouldReturnEmpty_WhenAllValuesAreNull()
    {
        // Arrange
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = BaseDate,
                        Value = null
                },
                new()
                {
                        NormalizedTimestamp = BaseDate.AddDays(1),
                        Value = null
                }
        };

        // Act
        var result = _service.ComputeUnaryTransform(data, "Log");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ComputeUnaryTransform_ShouldComputeLog_WhenOperationIsLog()
    {
        // Arrange
        var data = TestDataBuilders.HealthMetricData().
                                    WithTimestamp(BaseDate).
                                    WithValue(100m).
                                    BuildSeries(5, TimeSpan.FromDays(1));

        // Act
        var result = _service.ComputeUnaryTransform(data, "Log");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.DataList.Count);
        Assert.Equal(5, result.ComputedResults.Count);
        Assert.Equal("Log", result.Operation);
        Assert.All(result.ComputedResults, r => Assert.True(r > 0)); // Log of positive values
    }

    [Fact]
    public void ComputeUnaryTransform_ShouldComputeSqrt_WhenOperationIsSqrt()
    {
        // Arrange
        var data = TestDataBuilders.HealthMetricData().
                                    WithTimestamp(BaseDate).
                                    WithValue(100m).
                                    BuildSeries(5, TimeSpan.FromDays(1));

        // Act
        var result = _service.ComputeUnaryTransform(data, "Sqrt");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.DataList.Count);
        Assert.Equal(5, result.ComputedResults.Count);
        Assert.Equal("Sqrt", result.Operation);
        Assert.All(result.ComputedResults, r => Assert.True(r > 0)); // Sqrt of positive values
    }

    [Fact]
    public void ComputeUnaryTransform_ShouldFilterNullValues()
    {
        // Arrange
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = BaseDate,
                        Value = 10m
                },
                new()
                {
                        NormalizedTimestamp = BaseDate.AddDays(1),
                        Value = null
                },
                new()
                {
                        NormalizedTimestamp = BaseDate.AddDays(2),
                        Value = 20m
                }
        };

        // Act
        var result = _service.ComputeUnaryTransform(data, "Log");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.DataList.Count); // Only non-null values
        Assert.Equal(2, result.ComputedResults.Count);
    }

    [Fact]
    public void ComputeBinaryTransform_ShouldReturnEmpty_WhenData1IsEmpty()
    {
        // Arrange
        var data1 = Enumerable.Empty<MetricData>();
        var data2 = TestDataBuilders.HealthMetricData().
                                     BuildSeries(5, TimeSpan.FromDays(1));

        // Act
        var result = _service.ComputeBinaryTransform(data1, data2, "Add");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ComputeBinaryTransform_ShouldReturnEmpty_WhenData2IsEmpty()
    {
        // Arrange
        var data1 = TestDataBuilders.HealthMetricData().
                                     BuildSeries(5, TimeSpan.FromDays(1));
        var data2 = Enumerable.Empty<MetricData>();

        // Act
        var result = _service.ComputeBinaryTransform(data1, data2, "Add");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ComputeBinaryTransform_ShouldComputeAdd_WhenOperationIsAdd()
    {
        // Arrange
        var data1 = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(BaseDate).
                                     WithValue(10m).
                                     BuildSeries(5, TimeSpan.FromDays(1));

        var data2 = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(BaseDate).
                                     WithValue(20m).
                                     BuildSeries(5, TimeSpan.FromDays(1));

        // Act
        var result = _service.ComputeBinaryTransform(data1, data2, "Add");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Add", result.Operation);
        Assert.True(result.ComputedResults.Count > 0);
        // First result should be approximately 30 (10 + 20)
        Assert.True(result.ComputedResults[0] >= 29.9 && result.ComputedResults[0] <= 30.1);
    }

    [Fact]
    public void ComputeBinaryTransform_ShouldComputeSubtract_WhenOperationIsSubtract()
    {
        // Arrange
        var data1 = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(BaseDate).
                                     WithValue(20m).
                                     BuildSeries(5, TimeSpan.FromDays(1));

        var data2 = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(BaseDate).
                                     WithValue(10m).
                                     BuildSeries(5, TimeSpan.FromDays(1));

        // Act
        var result = _service.ComputeBinaryTransform(data1, data2, "Subtract");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("Subtract", result.Operation);
        Assert.True(result.ComputedResults.Count > 0);
        // First result should be approximately 10 (20 - 10)
        Assert.True(result.ComputedResults[0] >= 9.9 && result.ComputedResults[0] <= 10.1);
    }

    [Fact]
    public void ComputeBinaryTransform_ShouldAlignDataByTimestamp()
    {
        // Arrange
        var data1 = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = BaseDate,
                        Value = 10m
                },
                new()
                {
                        NormalizedTimestamp = BaseDate.AddDays(2),
                        Value = 20m
                }
        };

        var data2 = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = BaseDate.AddDays(1),
                        Value = 5m
                },
                new()
                {
                        NormalizedTimestamp = BaseDate.AddDays(2),
                        Value = 15m
                }
        };

        // Act
        var result = _service.ComputeBinaryTransform(data1, data2, "Add");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        // Should align and only include overlapping timestamps
        Assert.True(result.ComputedResults.Count > 0);
    }

    [Fact]
    public void ComputeBinaryTransform_ShouldReturnEmpty_WhenNoOverlappingTimestamps()
    {
        // Arrange
        var data1 = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = BaseDate,
                        Value = 10m
                }
        };

        var data2 = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = BaseDate.AddDays(10),
                        Value = 5m
                }
        };

        // Act
        var result = _service.ComputeBinaryTransform(data1, data2, "Add");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }
}