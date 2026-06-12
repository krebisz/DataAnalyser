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

        Assert.Equal("HeartRate_heart_rate", metric.MetricType);
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

    [Fact]
    public void SamsungHealthCsvParser_ShouldParseDirectHeaderCsvWithoutVendorIdentifier()
    {
        const string content = """
            create_time,update_time,total_body_water,datauuid,skeletal_muscle,height,deviceuuid,fat_free_mass,basal_metabolic_rate,skeletal_muscle_mass,body_fat_mass,body_fat,pkg_name,time_offset,fat_free,start_time,weight,comment,create_sh_ver,modify_sh_ver
            2023-08-15 16:05,2023-08-15 16:05,51.99932861328125,82089dc2-976a-4a69-a5c3-e21ab660ab63,35.64885330200195,183.0,jaUeRm+NVM,70.99873352050781,1903,37.787784576416016,35.00126647949219,33.02006149291992,com.sec.android.app.shealth,7200000,66.97994232177734,2023-08-15 16:05,106.0,,,
            """;

        var metrics = SamsungHealthCsvParser.Parse(@"C:\Health\Weight.csv", content);

        var weight = Assert.Single(metrics, metric => metric.MetricType == "Weight_weight");
        Assert.Equal(new DateTime(2023, 8, 15, 14, 5, 0, DateTimeKind.Utc), weight.NormalizedTimestamp);
        Assert.Equal("2023-08-15 16:05", weight.RawTimestamp);
        Assert.Equal(106.0m, weight.Value);
        Assert.Equal("kg", weight.Unit);
        Assert.DoesNotContain(metrics, metric => metric.MetricType.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase));
    }
}
