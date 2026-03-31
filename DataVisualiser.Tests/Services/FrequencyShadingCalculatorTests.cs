using System.Windows.Media;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services;
using Moq;

namespace DataVisualiser.Tests.Services;

/// <summary>
///     Unit tests for FrequencyShadingCalculator.
///     Tests interval creation and frequency counting logic.
/// </summary>
public sealed class FrequencyShadingCalculatorTests
{
    private readonly FrequencyShadingCalculator _calculator;
    private readonly Mock<IIntervalShadingStrategy> _mockShadingStrategy;

    public FrequencyShadingCalculatorTests()
    {
        _mockShadingStrategy = new Mock<IIntervalShadingStrategy>();
        _calculator = new FrequencyShadingCalculator(_mockShadingStrategy.Object, 7);
    }

    [Fact]
    public void BuildFrequencyShadingData_ShouldCallShadingStrategy()
    {
        // Arrange
        var dayValues = new Dictionary<int, List<double>>
        {
                [0] = new()
                {
                        10.0,
                        20.0,
                        30.0
                }
        };
        var globalMin = 0.0;
        var globalMax = 100.0;
        var intervalCount = 10;

        var mockColorMap = new Dictionary<int, Dictionary<int, Color>>
        {
                [0] = new()
                {
                        [0] = Colors.Blue
                }
        };

        _mockShadingStrategy.Setup(s => s.CalculateColorMap(It.IsAny<IntervalShadingContext>())).Returns(mockColorMap);

        // Act
        var result = _calculator.BuildFrequencyShadingData(dayValues, globalMin, globalMax, intervalCount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(intervalCount, result.Intervals.Count);
        Assert.True(result.FrequenciesPerBucket.ContainsKey(0));
        _mockShadingStrategy.Verify(s => s.CalculateColorMap(It.Is<IntervalShadingContext>(ctx => ctx.GlobalMin == globalMin && ctx.GlobalMax == globalMax && ctx.BucketValues == dayValues)), Times.Once);
    }

    [Fact]
    public void BuildFrequencyShadingData_ShouldReturnCorrectStructure()
    {
        // Arrange
        var dayValues = new Dictionary<int, List<double>>
        {
                [0] = new()
                {
                        10.0,
                        20.0
                }
        };
        var globalMin = 0.0;
        var globalMax = 100.0;
        var intervalCount = 10;

        var mockColorMap = new Dictionary<int, Dictionary<int, Color>>
        {
                [0] = new()
                {
                        [0] = Colors.Blue
                }
        };

        _mockShadingStrategy.Setup(s => s.CalculateColorMap(It.IsAny<IntervalShadingContext>())).Returns(mockColorMap);

        // Act
        var result = _calculator.BuildFrequencyShadingData(dayValues, globalMin, globalMax, intervalCount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(intervalCount, result.Intervals.Count);
        Assert.NotNull(result.FrequenciesPerBucket);
        Assert.NotNull(result.ColorMap);
        Assert.Equal(dayValues, result.BucketValues);
    }
}
