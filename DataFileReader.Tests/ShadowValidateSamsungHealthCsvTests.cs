using DataFileReader.Ingestion;

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
}
