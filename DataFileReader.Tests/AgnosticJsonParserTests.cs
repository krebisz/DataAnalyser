using DataFileReader.Parsers;
using System.Data;

namespace DataFileReader.Tests;

public sealed class AgnosticJsonParserTests
{
    [Fact]
    public void CanParse_ShouldAcceptJsonWithoutVendorPathInspection()
    {
        var parser = new AgnosticJsonParser();

        Assert.True(parser.CanParse(new FileInfo("data_flow_active_kcal.json")));
        Assert.True(parser.CanParse(new FileInfo("anything.json")));
    }

    [Fact]
    public void Parse_ShouldExtractTimestampedNumericValuesFromGenericJson()
    {
        const string content = """
            [
              {
                "title": "Active kcal",
                "slug": "dis_calories_burned",
                "data": [
                  { "x_value_iso": "2026-06-05", "y_value": 123, "x_value": ["06.05"] },
                  { "x_value_iso": "2026-06-06", "y_value": null, "x_value": ["06.06"] },
                  { "x_value_iso": "2026-06-07", "y_value": 42.5, "x_value": ["06.07"] }
                ]
              }
            ]
            """;
        var parser = new AgnosticJsonParser();

        var metrics = parser.Parse("agent_export.json", content);

        Assert.Equal(2, metrics.Count);
        Assert.All(metrics, metric => Assert.Equal("AgnosticJson", metric.Provider));
        Assert.All(metrics, metric => Assert.Equal("DisCaloriesBurned", metric.MetricType));
        Assert.All(metrics, metric => Assert.Equal("Active kcal", metric.MetricSubtype));
        Assert.Equal(new DateTime(2026, 6, 5), metrics[0].NormalizedTimestamp);
        Assert.Equal(123m, metrics[0].Value);
        Assert.Equal(new DateTime(2026, 6, 7), metrics[1].NormalizedTimestamp);
        Assert.Equal(42.5m, metrics[1].Value);
    }

    [Fact]
    public void Parse_ShouldPreserveFlatTimestampedNumericJsonExtraction()
    {
        const string content = """
            [
              {
                "start_time": "2026-06-05T08:15:00",
                "end_time": "2026-06-05T08:16:00",
                "heart_rate": 72,
                "confidence": 98,
                "comment": "resting"
              }
            ]
            """;
        var parser = new AgnosticJsonParser();

        var metrics = parser.Parse("heart_rate.json", content);

        Assert.Equal(2, metrics.Count);
        Assert.All(metrics, metric => Assert.Equal("AgnosticJson", metric.Provider));
        Assert.Equal("HeartRate_heart_rate", metrics[0].MetricType);
        Assert.Equal("HeartRate_confidence", metrics[1].MetricType);
        Assert.Equal(new DateTime(2026, 6, 5, 8, 15, 0), metrics[0].NormalizedTimestamp);
        Assert.Equal("2026-06-05T08:15:00", metrics[0].RawTimestamp);
        Assert.Equal(72m, metrics[0].Value);
        Assert.Equal(98m, metrics[1].Value);
        Assert.Equal("resting", metrics[0].AdditionalFields["comment"]);
    }

    [Fact]
    public void Parse_ShouldPreserveTimestampedMetadataRecordsWhenNoNumericMetricExists()
    {
        const string content = """
            {
              "start_time": "2026-06-05T08:15:00",
              "state": "asleep",
              "source": "watch"
            }
            """;
        var parser = new AgnosticJsonParser();

        var metric = Assert.Single(parser.Parse("sleep_session.json", content));

        Assert.Equal("AgnosticJson", metric.Provider);
        Assert.Equal("SleepSession", metric.MetricType);
        Assert.Equal("SleepSession", metric.MetricSubtype);
        Assert.Equal(new DateTime(2026, 6, 5, 8, 15, 0), metric.NormalizedTimestamp);
        Assert.Null(metric.Value);
        Assert.Equal("TimestampedMetadata", metric.AdditionalFields["RecordKind"]);
        Assert.Equal("asleep", metric.AdditionalFields["state"]);
        Assert.Equal("watch", metric.AdditionalFields["source"]);
    }

    [Fact]
    public void Parse_WhenHierarchyProcessingDisabled_ShouldNotBuildFlattenedData()
    {
        const string content = """
            {
              "start_time": "2026-06-05T08:15:00",
              "heart_rate": 72
            }
            """;
        var existingFlattenedData = new DataTable("sentinel");
        AgnosticJsonParser.flattenedData = existingFlattenedData;
        var parser = new AgnosticJsonParser(processHierarchy: false);

        var metric = Assert.Single(parser.Parse("heart_rate.json", content));

        Assert.Equal("HeartRate_heart_rate", metric.MetricType);
        Assert.Same(existingFlattenedData, AgnosticJsonParser.flattenedData);
    }

    [Fact]
    public void ProcessHierarchy_ShouldBuildFlattenedDataWithoutMetricExtraction()
    {
        const string content = """
            {
              "start_time": "2026-06-05T08:15:00",
              "heart_rate": 72
            }
            """;
        AgnosticJsonParser.flattenedData = new DataTable();
        var parser = new AgnosticJsonParser(processHierarchy: false);

        parser.ProcessHierarchy("heart_rate.json", content);

        Assert.Contains("start_time", AgnosticJsonParser.flattenedData.Columns.Cast<DataColumn>().Select(column => column.ColumnName));
        Assert.Contains("heart_rate", AgnosticJsonParser.flattenedData.Columns.Cast<DataColumn>().Select(column => column.ColumnName));
    }
}
