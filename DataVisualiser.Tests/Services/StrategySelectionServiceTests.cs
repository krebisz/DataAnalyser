using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Selection;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Validation;
using Moq;

namespace DataVisualiser.Tests.Services;

/// <summary>
///     Unit tests for StrategySelectionService.
///     Tests strategy selection logic for single, combined, and multi-metric scenarios.
/// </summary>
public sealed class StrategySelectionServiceTests
{
    private const    string                        ConnectionString = "TestConnectionString";
    private readonly Mock<IStrategyCutOverService> _mockCutOverService;
    private readonly Mock<ParityValidationService> _mockParityService;
    private readonly StrategySelectionService      _service;

    public StrategySelectionServiceTests()
    {
        _mockCutOverService = new Mock<IStrategyCutOverService>();
        _mockParityService = new Mock<ParityValidationService>();
        _service = new StrategySelectionService(_mockCutOverService.Object, _mockParityService.Object, ConnectionString);
    }

    [Fact]
    public void SelectComputationStrategy_ShouldReturnSingleMetricStrategy_WhenOneSeries()
    {
        // Arrange
        var series = new List<IEnumerable<HealthMetricData>>
        {
                TestDataBuilders.HealthMetricData().
                                 BuildSeries(5, TimeSpan.FromDays(1))
        };
        var labels = new List<string>
        {
                "Series1"
        };
        var ctx = new ChartDataContext
        {
                SemanticMetricCount = 1
        };
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        var mockStrategy = new Mock<IChartComputationStrategy>();
        _mockCutOverService.Setup(s => s.CreateStrategy(StrategyType.SingleMetric, It.IsAny<ChartDataContext>(), It.IsAny<StrategyCreationParameters>())).
                            Returns(mockStrategy.Object);

        // Act
        var (strategy, secondaryLabel) = _service.SelectComputationStrategy(series, labels, ctx, from, to);

        // Assert
        Assert.NotNull(strategy);
        Assert.Null(secondaryLabel);
        _mockCutOverService.Verify(s => s.CreateStrategy(StrategyType.SingleMetric, ctx, It.Is<StrategyCreationParameters>(p => p.LegacyData1 == series[0] && p.Label1 == "Series1" && p.From == from && p.To == to)), Times.Once);
    }

    [Fact]
    public void SelectComputationStrategy_ShouldReturnCombinedMetricStrategy_WhenTwoSeries()
    {
        // Arrange
        var series = new List<IEnumerable<HealthMetricData>>
        {
                TestDataBuilders.HealthMetricData().
                                 BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().
                                 WithValue(50m).
                                 BuildSeries(5, TimeSpan.FromDays(1))
        };
        var labels = new List<string>
        {
                "Series1",
                "Series2"
        };
        var ctx = new ChartDataContext
        {
                SemanticMetricCount = 2
        };
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        // Act
        var (strategy, secondaryLabel) = _service.SelectComputationStrategy(series, labels, ctx, from, to);

        // Assert
        Assert.NotNull(strategy);
        Assert.Equal("Series2", secondaryLabel);
        // Verify it's a CombinedMetricStrategy (created via parity service)
        Assert.IsType<CombinedMetricStrategy>(strategy);
    }

    [Fact]
    public void SelectComputationStrategy_ShouldReturnMultiMetricStrategy_WhenThreeOrMoreSeries()
    {
        // Arrange
        var series = new List<IEnumerable<HealthMetricData>>
        {
                TestDataBuilders.HealthMetricData().
                                 BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().
                                 WithValue(50m).
                                 BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().
                                 WithValue(75m).
                                 BuildSeries(5, TimeSpan.FromDays(1))
        };
        var labels = new List<string>
        {
                "Series1",
                "Series2",
                "Series3"
        };
        var ctx = new ChartDataContext
        {
                SemanticMetricCount = 3
        };
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        var mockStrategy = new Mock<IChartComputationStrategy>();
        _mockCutOverService.Setup(s => s.CreateStrategy(StrategyType.MultiMetric, It.IsAny<ChartDataContext>(), It.IsAny<StrategyCreationParameters>())).
                            Returns(mockStrategy.Object);

        // Act
        var (strategy, secondaryLabel) = _service.SelectComputationStrategy(series, labels, ctx, from, to);

        // Assert
        Assert.NotNull(strategy);
        Assert.Null(secondaryLabel);
        _mockCutOverService.Verify(s => s.CreateStrategy(StrategyType.MultiMetric, ctx, It.Is<StrategyCreationParameters>(p => p.LegacySeries == series && p.Labels == labels && p.From == from && p.To == to)), Times.Once);
    }

    [Fact]
    public void SelectComputationStrategy_ShouldThrowNullReferenceException_WhenSeriesIsNull()
    {
        // Arrange
        var labels = new List<string>
        {
                "Series1"
        };
        var ctx = new ChartDataContext();
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        // Act & Assert
        // Note: The method doesn't explicitly check for null, so it throws NullReferenceException
        Assert.Throws<NullReferenceException>(() => _service.SelectComputationStrategy(null!, labels, ctx, from, to));
    }

    [Fact]
    public void LoadAdditionalSubtypesAsync_ShouldReturnEarly_WhenSubtypesCountIsTwoOrLess()
    {
        // Arrange
        var series = new List<IEnumerable<HealthMetricData>>();
        var labels = new List<string>();
        var selectedSubtypes = new List<string?>
        {
                "subtype1",
                "subtype2"
        };

        // Act
        var task = _service.LoadAdditionalSubtypesAsync(series, labels, "Weight", "HealthMetrics", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, selectedSubtypes);
        task.Wait();

        // Assert
        Assert.Equal(0, series.Count);
        Assert.Equal(0, labels.Count);
    }

    [Fact]
    public void LoadAdditionalSubtypesAsync_ShouldReturnEarly_WhenMetricTypeIsNullOrEmpty()
    {
        // Arrange
        var series = new List<IEnumerable<HealthMetricData>>();
        var labels = new List<string>();
        var selectedSubtypes = new List<string?>
        {
                "subtype1",
                "subtype2",
                "subtype3"
        };

        // Act
        var task1 = _service.LoadAdditionalSubtypesAsync(series, labels, null, "HealthMetrics", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, selectedSubtypes);
        task1.Wait();

        var task2 = _service.LoadAdditionalSubtypesAsync(series, labels, string.Empty, "HealthMetrics", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, selectedSubtypes);
        task2.Wait();

        // Assert
        Assert.Equal(0, series.Count);
        Assert.Equal(0, labels.Count);
    }
}