using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using Moq;

namespace DataVisualiser.Tests.Services;

/// <summary>
///     Unit tests for StrategySelectionService.
///     Tests strategy selection logic for single, combined, and multi-metric scenarios.
/// </summary>
public sealed class StrategySelectionServiceTests
{
    private const string ConnectionString = "TestConnectionString";
    private readonly Mock<IStrategyCutOverService> _mockCutOverService;
    private readonly StrategySelectionService _service;

    public StrategySelectionServiceTests()
    {
        _mockCutOverService = new Mock<IStrategyCutOverService>();
        _service = new StrategySelectionService(_mockCutOverService.Object, ConnectionString);
    }

    [Fact]
    public void SelectComputationStrategy_ShouldReturnSingleMetricStrategy_WhenOneSeries()
    {
        // Arrange
        var series = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().BuildSeries(5, TimeSpan.FromDays(1))
        };
        var labels = new List<string>
        {
                "Series1"
        };
        var ctx = new ChartDataContext
        {
                ActualSeriesCount = 1
        };
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        var mockStrategy = new Mock<IChartComputationStrategy>();
        _mockCutOverService.Setup(s => s.CreateStrategy(StrategyType.SingleMetric, It.IsAny<ChartDataContext>(), It.IsAny<StrategyCreationParameters>())).Returns(mockStrategy.Object);

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
        var series = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithValue(50m).BuildSeries(5, TimeSpan.FromDays(1))
        };
        var labels = new List<string>
        {
                "Series1",
                "Series2"
        };
        var ctx = new ChartDataContext
        {
                ActualSeriesCount = 2
        };
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        var mockStrategy = new Mock<IChartComputationStrategy>().Object;

        _mockCutOverService.Setup(s => s.CreateStrategy(StrategyType.CombinedMetric, It.IsAny<ChartDataContext>(), It.IsAny<StrategyCreationParameters>())).Returns(mockStrategy);
        // Act
        var (strategy, secondaryLabel) = _service.SelectComputationStrategy(series, labels, ctx, from, to);

        // Assert
        Assert.NotNull(strategy);
        Assert.Equal("Series2", secondaryLabel);
        Assert.Same(mockStrategy, strategy);
        _mockCutOverService.Verify(s => s.CreateStrategy(StrategyType.CombinedMetric, ctx, It.Is<StrategyCreationParameters>(p => p.LegacyData1 == series[0] && p.LegacyData2 == series[1] && p.Label1 == "Series1" && p.Label2 == "Series2" && p.From == from && p.To == to)), Times.Once);
    }

    [Fact]
    public void SelectComputationStrategy_ShouldReturnMultiMetricStrategy_WhenThreeOrMoreSeries()
    {
        // Arrange
        var series = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithValue(50m).BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithValue(75m).BuildSeries(5, TimeSpan.FromDays(1))
        };
        var labels = new List<string>
        {
                "Series1",
                "Series2",
                "Series3"
        };
        var ctx = new ChartDataContext
        {
                ActualSeriesCount = 3
        };
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        var mockStrategy = new Mock<IChartComputationStrategy>();
        _mockCutOverService.Setup(s => s.CreateStrategy(StrategyType.MultiMetric, It.IsAny<ChartDataContext>(), It.IsAny<StrategyCreationParameters>())).Returns(mockStrategy.Object);

        // Act
        var (strategy, secondaryLabel) = _service.SelectComputationStrategy(series, labels, ctx, from, to);

        // Assert
        Assert.NotNull(strategy);
        Assert.Null(secondaryLabel);
        _mockCutOverService.Verify(s => s.CreateStrategy(StrategyType.MultiMetric, ctx, It.Is<StrategyCreationParameters>(p => p.LegacySeries == series && p.Labels == labels && p.From == from && p.To == to)), Times.Once);
    }

    [Fact]
    public void SelectComputationStrategy_ShouldReturnMultiMetricStrategy_WhenFourSeries()
    {
        var series = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithValue(50m).BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithValue(75m).BuildSeries(5, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithValue(90m).BuildSeries(5, TimeSpan.FromDays(1))
        };
        var labels = new List<string>
        {
                "Series1",
                "Series2",
                "Series3",
                "Series4"
        };
        var ctx = new ChartDataContext
        {
                ActualSeriesCount = 4
        };
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow;

        var mockStrategy = new Mock<IChartComputationStrategy>();
        _mockCutOverService.Setup(s => s.CreateStrategy(StrategyType.MultiMetric, It.IsAny<ChartDataContext>(), It.IsAny<StrategyCreationParameters>())).Returns(mockStrategy.Object);

        var (strategy, secondaryLabel) = _service.SelectComputationStrategy(series, labels, ctx, from, to);

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
    public async Task LoadAdditionalSubtypesAsync_ShouldReturnEarly_WhenSubtypesCountIsTwoOrLess()
    {
        // Arrange
        var series = new List<IEnumerable<MetricData>>();
        var labels = new List<string>();
        var selectedSubtypes = new List<string?>
        {
                "subtype1",
                "subtype2"
        };

        // Act
        await _service.LoadAdditionalSubtypesAsync(series, labels, "Weight", "HealthMetrics", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, selectedSubtypes);

        // Assert
        Assert.Empty(series);
        Assert.Empty(labels);
    }

    [Fact]
    public async Task LoadAdditionalSubtypesAsync_ShouldReturnEarly_WhenMetricTypeIsNullOrEmpty()
    {
        // Arrange
        var series = new List<IEnumerable<MetricData>>();
        var labels = new List<string>();
        var selectedSubtypes = new List<string?>
        {
                "subtype1",
                "subtype2",
                "subtype3"
        };

        // Act
        await _service.LoadAdditionalSubtypesAsync(series, labels, null, "HealthMetrics", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, selectedSubtypes);

        await _service.LoadAdditionalSubtypesAsync(series, labels, string.Empty, "HealthMetrics", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, selectedSubtypes);

        // Assert
        Assert.Empty(series);
        Assert.Empty(labels);
    }

    [Fact]
    public async Task LoadAdditionalSubtypesAsync_ShouldApplySamplingStrategy_ToAdditionalSeriesLoads()
    {
        SetAppSetting("DataVisualiser:EnableSqlSampling", "true");
        SetAppSetting("DataVisualiser:SamplingThreshold", "1000");
        SetAppSetting("DataVisualiser:TargetSamplePoints", "200");
        SetAppSetting("DataVisualiser:EnableSqlResultLimiting", "false");

        var queries = new FakeMetricSelectionDataQueries
        {
            RecordCount = 5000,
            Data =
            [
                new MetricData
                {
                    NormalizedTimestamp = new DateTime(2024, 01, 01),
                    Value = 10m
                }
            ]
        };
        var service = new StrategySelectionService(_mockCutOverService.Object, queries);
        var series = new List<IEnumerable<MetricData>>();
        var labels = new List<string>();
        var selectedSubtypes = new List<string?>
        {
            "primary",
            "secondary",
            "third"
        };

        await service.LoadAdditionalSubtypesAsync(series, labels, "Weight", "HealthMetrics", new DateTime(2024, 01, 01), new DateTime(2024, 12, 31), selectedSubtypes);

        Assert.Single(series);
        Assert.Equal("Weight:third", Assert.Single(labels));
        var request = Assert.Single(queries.SeriesRequests);
        Assert.Equal("Weight", request.MetricType);
        Assert.Equal("third", request.MetricSubtype);
        Assert.Equal(SamplingMode.UniformOverTime, request.SamplingMode);
        Assert.Equal(200, request.TargetSamples);
        Assert.Null(request.MaxRecords);
    }

    private static void SetAppSetting(string key, string value)
    {
        var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        var setting = config.AppSettings.Settings[key];
        if (setting == null)
            config.AppSettings.Settings.Add(key, value);
        else
            setting.Value = value;

        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
        System.Configuration.ConfigurationManager.RefreshSection("appSettings");
    }

    private sealed class FakeMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public long RecordCount { get; set; }
        public IReadOnlyList<MetricData> Data { get; set; } = [];
        public List<(string MetricType, string? MetricSubtype, string TableName, int? MaxRecords, SamplingMode SamplingMode, int? TargetSamples)> SeriesRequests { get; } = [];

        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            return Task.FromResult(RecordCount);
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(
            string baseType,
            string? subtype,
            DateTime? from,
            DateTime? to,
            string tableName,
            int? maxRecords = null,
            SamplingMode samplingMode = SamplingMode.None,
            int? targetSamples = null)
        {
            SeriesRequests.Add((baseType, subtype, tableName, maxRecords, samplingMode, targetSamples));
            return Task.FromResult<IEnumerable<MetricData>>(Data);
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);

        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>([]);

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
