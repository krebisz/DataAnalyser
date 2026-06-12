using DataFileReader.Ingestion;
using DataFileReader.Helper;

namespace DataFileReader.Tests;

public sealed class ShadowValidateSamsungHealthCsvTests
{
    [Fact]
    public void GetComparableActualValue_TreatsMissingMetricSubtypeAsEmpty()
    {
        var record = new RawRecord(
            "SamsungHealthCsv",
            new Dictionary<string, object?>
            {
                ["MetricType"] = "advanced_glycation_endproduct_com"
            });

        var actual = ShadowValidate_SamsungHealthCsv.GetComparableActualValue(record, "MetricSubtype");

        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetComparableActualValue_KeepsOtherMissingFieldsExplicit()
    {
        var record = new RawRecord(
            "SamsungHealthCsv",
            new Dictionary<string, object?>
            {
                ["MetricType"] = "advanced_glycation_endproduct_com"
            });

        var actual = ShadowValidate_SamsungHealthCsv.GetComparableActualValue(record, "Unit");

        Assert.Equal("<missing>", actual);
    }

    [Fact]
    public void SamsungHealthCsvParser_ShouldNotUseStructuredValueAsRawTimestamp()
    {
        const string blob = "[{start_time:1692108120000,end_time:1692108179000,heart_rate:71}]";
        var content = $"""
            com.samsung.health.heart_rate
            start_time,end_time,heart_rate
            "{blob}",1692108180000,71
            """;

        var metric = Assert.Single(SamsungHealthCsvParser.Parse("Heart Rate.csv", content));

        Assert.Equal("1692108180000", metric.RawTimestamp);
        Assert.Equal(blob, metric.AdditionalFields["start_time"]);
        Assert.Equal(new DateTime(2023, 8, 15, 14, 3, 0, DateTimeKind.Utc), metric.NormalizedTimestamp);
    }

    [Fact]
    public void SamsungHealthCsvParser_ShouldLeaveRawTimestampEmptyWhenOnlyStructuredTimestampCandidateExists()
    {
        const string blob = "[{start_time:1692108120000,end_time:1692108179000,heart_rate:71}]";
        var content = $"""
            com.samsung.health.heart_rate
            start_time,heart_rate
            "{blob}",71
            """;

        var metric = Assert.Single(SamsungHealthCsvParser.Parse("Heart Rate.csv", content));

        Assert.Equal(string.Empty, metric.RawTimestamp);
        Assert.Null(metric.NormalizedTimestamp);
        Assert.Equal(blob, metric.AdditionalFields["start_time"]);
    }
}
