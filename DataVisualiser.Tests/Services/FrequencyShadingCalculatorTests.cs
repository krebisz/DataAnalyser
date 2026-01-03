using DataVisualiser.Shared.Models;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services;
using DataVisualiser.Tests.Helpers;
using Moq;
using System.Windows.Media;
using Xunit;

namespace DataVisualiser.Tests.Services
{
    /// <summary>
    /// Unit tests for FrequencyShadingCalculator.
    /// Tests interval creation and frequency counting logic.
    /// </summary>
    public sealed class FrequencyShadingCalculatorTests
    {
        private readonly Mock<IIntervalShadingStrategy> _mockShadingStrategy;
        private readonly FrequencyShadingCalculator _calculator;

        public FrequencyShadingCalculatorTests()
        {
            _mockShadingStrategy = new Mock<IIntervalShadingStrategy>();
            _calculator = new FrequencyShadingCalculator(_mockShadingStrategy.Object);
        }

        [Fact]
        public void CreateUniformIntervals_ShouldReturnSingleInterval_WhenMaxLessThanOrEqualMin()
        {
            // Arrange
            var globalMin = 100.0;
            var globalMax = 50.0; // Invalid: max < min
            var intervalCount = 10;

            // Act
            var intervals = _calculator.CreateUniformIntervals(globalMin, globalMax, intervalCount);

            // Assert
            Assert.Single(intervals);
            Assert.Equal((globalMin, globalMax), intervals[0]);
        }

        [Fact]
        public void CreateUniformIntervals_ShouldReturnSingleInterval_WhenIntervalCountIsZero()
        {
            // Arrange
            var globalMin = 0.0;
            var globalMax = 100.0;
            var intervalCount = 0;

            // Act
            var intervals = _calculator.CreateUniformIntervals(globalMin, globalMax, intervalCount);

            // Assert
            Assert.Single(intervals);
            Assert.Equal((globalMin, globalMax), intervals[0]);
        }

        [Fact]
        public void CreateUniformIntervals_ShouldCreateCorrectNumberOfIntervals()
        {
            // Arrange
            var globalMin = 0.0;
            var globalMax = 100.0;
            var intervalCount = 10;

            // Act
            var intervals = _calculator.CreateUniformIntervals(globalMin, globalMax, intervalCount);

            // Assert
            Assert.Equal(intervalCount, intervals.Count);
        }

        [Fact]
        public void CreateUniformIntervals_ShouldCoverFullRange()
        {
            // Arrange
            var globalMin = 0.0;
            var globalMax = 100.0;
            var intervalCount = 10;

            // Act
            var intervals = _calculator.CreateUniformIntervals(globalMin, globalMax, intervalCount);

            // Assert
            Assert.Equal(globalMin, intervals[0].Min);
            Assert.Equal(globalMax, intervals[^1].Max);
        }

        [Fact]
        public void CreateUniformIntervals_ShouldCreateNonOverlappingIntervals()
        {
            // Arrange
            var globalMin = 0.0;
            var globalMax = 100.0;
            var intervalCount = 10;

            // Act
            var intervals = _calculator.CreateUniformIntervals(globalMin, globalMax, intervalCount);

            // Assert
            for (int i = 0; i < intervals.Count - 1; i++)
            {
                Assert.Equal(intervals[i].Max, intervals[i + 1].Min);
            }
        }

        [Fact]
        public void CreateUniformIntervals_ShouldMakeLastIntervalInclusive()
        {
            // Arrange
            var globalMin = 0.0;
            var globalMax = 100.0;
            var intervalCount = 10;

            // Act
            var intervals = _calculator.CreateUniformIntervals(globalMin, globalMax, intervalCount);

            // Assert
            var lastInterval = intervals[^1];
            Assert.Equal(globalMax, lastInterval.Max);
        }

        [Fact]
        public void CountFrequenciesPerInterval_ShouldReturnFrequenciesForAllDays_EvenWhenDayValuesIsEmpty()
        {
            // Arrange
            var dayValues = new Dictionary<int, List<double>>();
            var intervals = _calculator.CreateUniformIntervals(0.0, 100.0, 10);

            // Act
            var frequencies = _calculator.CountFrequenciesPerInterval(dayValues, intervals);

            // Assert
            // The implementation always creates entries for all 7 days (0-6) with zero frequencies
            Assert.Equal(7, frequencies.Count);
            for (int i = 0; i < 7; i++)
            {
                Assert.True(frequencies.ContainsKey(i));
                Assert.All(frequencies[i].Values, v => Assert.Equal(0, v));
            }
        }

        [Fact]
        public void CountFrequenciesPerInterval_ShouldCountValuesInCorrectIntervals()
        {
            // Arrange
            var dayValues = new Dictionary<int, List<double>>
            {
                [0] = new List<double> { 10.0, 20.0, 30.0, 50.0, 90.0 }
            };
            var intervals = _calculator.CreateUniformIntervals(0.0, 100.0, 10);

            // Act
            var frequencies = _calculator.CountFrequenciesPerInterval(dayValues, intervals);

            // Assert
            Assert.True(frequencies.ContainsKey(0));
            var day0Freqs = frequencies[0];
            Assert.True(day0Freqs.ContainsKey(0)); // 10.0 should be in interval 0
            Assert.True(day0Freqs.ContainsKey(1)); // 20.0 should be in interval 1
            Assert.True(day0Freqs.ContainsKey(2)); // 30.0 should be in interval 2
            Assert.True(day0Freqs.ContainsKey(4)); // 50.0 should be in interval 5
            Assert.True(day0Freqs.ContainsKey(8)); // 90.0 should be in interval 9
        }

        [Fact]
        public void CountFrequenciesPerInterval_ShouldHandleAllSevenDays()
        {
            // Arrange
            var dayValues = new Dictionary<int, List<double>>();
            for (int i = 0; i < 7; i++)
            {
                dayValues[i] = new List<double> { 10.0 + i * 10.0 };
            }
            var intervals = _calculator.CreateUniformIntervals(0.0, 100.0, 10);

            // Act
            var frequencies = _calculator.CountFrequenciesPerInterval(dayValues, intervals);

            // Assert
            Assert.Equal(7, frequencies.Count);
            for (int i = 0; i < 7; i++)
            {
                Assert.True(frequencies.ContainsKey(i));
            }
        }

        [Fact]
        public void CountFrequenciesPerInterval_ShouldIgnoreNaNValues()
        {
            // Arrange
            var dayValues = new Dictionary<int, List<double>>
            {
                [0] = new List<double> { 10.0, double.NaN, 20.0, double.PositiveInfinity }
            };
            var intervals = _calculator.CreateUniformIntervals(0.0, 100.0, 10);

            // Act
            var frequencies = _calculator.CountFrequenciesPerInterval(dayValues, intervals);

            // Assert
            Assert.True(frequencies.ContainsKey(0));
            var day0Freqs = frequencies[0];
            // Should only count valid values (10.0 and 20.0)
            var totalCount = day0Freqs.Values.Sum();
            Assert.Equal(2, totalCount);
        }

        [Fact]
        public void BuildFrequencyShadingData_ShouldCallShadingStrategy()
        {
            // Arrange
            var dayValues = new Dictionary<int, List<double>>
            {
                [0] = new List<double> { 10.0, 20.0, 30.0 }
            };
            var globalMin = 0.0;
            var globalMax = 100.0;
            var intervalCount = 10;

            var mockColorMap = new Dictionary<int, Dictionary<int, Color>>
            {
                [0] = new Dictionary<int, Color> { [0] = Colors.Blue }
            };

            _mockShadingStrategy
                .Setup(s => s.CalculateColorMap(It.IsAny<IntervalShadingContext>()))
                .Returns(mockColorMap);

            // Act
            var result = _calculator.BuildFrequencyShadingData(dayValues, globalMin, globalMax, intervalCount);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(intervalCount, result.Intervals.Count);
            Assert.True(result.FrequenciesPerDay.ContainsKey(0));
            _mockShadingStrategy.Verify(
                s => s.CalculateColorMap(It.Is<IntervalShadingContext>(ctx =>
                    ctx.GlobalMin == globalMin &&
                    ctx.GlobalMax == globalMax &&
                    ctx.DayValues == dayValues)),
                Times.Once);
        }

        [Fact]
        public void BuildFrequencyShadingData_ShouldReturnCorrectStructure()
        {
            // Arrange
            var dayValues = new Dictionary<int, List<double>>
            {
                [0] = new List<double> { 10.0, 20.0 }
            };
            var globalMin = 0.0;
            var globalMax = 100.0;
            var intervalCount = 10;

            var mockColorMap = new Dictionary<int, Dictionary<int, Color>>
            {
                [0] = new Dictionary<int, Color> { [0] = Colors.Blue }
            };

            _mockShadingStrategy
                .Setup(s => s.CalculateColorMap(It.IsAny<IntervalShadingContext>()))
                .Returns(mockColorMap);

            // Act
            var result = _calculator.BuildFrequencyShadingData(dayValues, globalMin, globalMax, intervalCount);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(intervalCount, result.Intervals.Count);
            Assert.NotNull(result.FrequenciesPerDay);
            Assert.NotNull(result.ColorMap);
            Assert.Equal(dayValues, result.DayValues);
        }
    }
}

