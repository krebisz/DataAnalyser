using System.Configuration;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Services;

public sealed class MetricSelectionServiceTests
{
    [Fact]
    public void ResolveDataLoadStrategy_ActivatesSampling_WhenRecordCountExceedsThreshold()
    {
        // Arrange
        SetAppSetting("DataVisualiser:EnableSqlSampling", "true");
        SetAppSetting("DataVisualiser:SamplingThreshold", "1000");
        SetAppSetting("DataVisualiser:TargetSamplePoints", "200");
        SetAppSetting("DataVisualiser:EnableSqlResultLimiting", "false");

        var from = new DateTime(2024, 01, 01);
        var to = new DateTime(2024, 12, 31);
        var recordCount = 5000; // > threshold

        // Act
        var result = MetricDataLoadStrategyResolver.Resolve(from, to, recordCount);

        // Assert
        Assert.Equal(SamplingMode.UniformOverTime, result.Mode);
        Assert.Equal(200, result.TargetSamples);
        Assert.Null(result.MaxRecords);
    }

    private static void SetAppSetting(string key, string value)
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var setting = config.AppSettings.Settings[key];
        if (setting == null)
            config.AppSettings.Settings.Add(key, value);
        else
            setting.Value = value;

        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }

    [Fact]
    public void ResolveDataLoadStrategy_DoesNotActivateSampling_WhenRecordCountIsBelowThreshold()
    {
        // Arrange
        SetAppSetting("DataVisualiser:EnableSqlSampling", "true");
        SetAppSetting("DataVisualiser:SamplingThreshold", "1000");
        SetAppSetting("DataVisualiser:TargetSamplePoints", "200");
        SetAppSetting("DataVisualiser:EnableSqlResultLimiting", "false");

        var from = new DateTime(2024, 01, 01);
        var to = new DateTime(2024, 12, 31);
        var recordCount = 500; // < threshold

        // Act
        var result = MetricDataLoadStrategyResolver.Resolve(from, to, recordCount);

        // Assert
        Assert.Equal(SamplingMode.None, result.Mode);
        Assert.Null(result.TargetSamples);
        Assert.Null(result.MaxRecords);
    }

    [Fact]
    public async Task LoadMetricDataAsync_UsesInjectedQueries_InsteadOfConstructingDataFetcherInternally()
    {
        var queries = new FakeMetricSelectionDataQueries
        {
            RecordCount = 12,
            Data = new List<MetricData>
            {
                new()
                {
                    NormalizedTimestamp = new DateTime(2024, 01, 01),
                    Value = 1m
                }
            }
        };
        var service = new MetricSelectionService(queries, "FakeConnectionString");

        var (primary, secondary) = await service.LoadMetricDataAsync("MetricA", "SubA", null, new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), "HealthMetrics");

        var primaryList = primary.ToList();
        Assert.Single(primaryList);
        Assert.Empty(secondary);
        Assert.Equal(("MetricA", "SubA"), Assert.Single(queries.RecordCountRequests));
        Assert.Single(queries.SeriesRequests);
        Assert.Equal("MetricA", queries.SeriesRequests[0].MetricType);
        Assert.Equal("SubA", queries.SeriesRequests[0].MetricSubtype);
        Assert.Equal("HealthMetrics", queries.SeriesRequests[0].TableName);
    }

    [Fact]
    public async Task LoadMetricDataAsync_PassesSamplingStrategy_ToInjectedQueries_WhenRecordCountExceedsThreshold()
    {
        SetAppSetting("DataVisualiser:EnableSqlSampling", "true");
        SetAppSetting("DataVisualiser:SamplingThreshold", "1000");
        SetAppSetting("DataVisualiser:TargetSamplePoints", "200");
        SetAppSetting("DataVisualiser:EnableSqlResultLimiting", "false");

        var queries = new FakeMetricSelectionDataQueries
        {
            RecordCount = 5000,
            Data = new List<MetricData>
            {
                new()
                {
                    NormalizedTimestamp = new DateTime(2024, 01, 01),
                    Value = 1m
                }
            }
        };
        var service = new MetricSelectionService(queries, "FakeConnectionString");

        await service.LoadMetricDataAsync("MetricA", null, null, new DateTime(2024, 01, 01), new DateTime(2024, 12, 31), "HealthMetrics");

        var request = Assert.Single(queries.SeriesRequests);
        Assert.Equal("MetricA", request.MetricType);
        Assert.Null(request.MetricSubtype);
        Assert.Equal(SamplingMode.UniformOverTime, request.SamplingMode);
        Assert.Equal(200, request.TargetSamples);
        Assert.Null(request.MaxRecords);
    }

    private sealed class FakeMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public long RecordCount { get; set; }
        public IReadOnlyList<MetricData> Data { get; set; } = Array.Empty<MetricData>();
        public List<(string MetricType, string? MetricSubtype)> RecordCountRequests { get; } = new();
        public List<(string MetricType, string? MetricSubtype, string TableName, int? MaxRecords, SamplingMode SamplingMode, int? TargetSamples)> SeriesRequests { get; } = new();

        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            RecordCountRequests.Add((metricType, metricSubtype));
            return Task.FromResult(RecordCount);
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
        {
            SeriesRequests.Add((baseType, subtype, tableName, maxRecords, samplingMode, targetSamples));
            return Task.FromResult<IEnumerable<MetricData>>(Data);
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }
    }
}
