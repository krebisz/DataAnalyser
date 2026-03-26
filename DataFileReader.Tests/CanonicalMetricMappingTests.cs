using DataFileReader.Canonical;

namespace DataFileReader.Tests;

public sealed class CanonicalMetricMappingTests : IDisposable
{
    public CanonicalMetricMappingTests()
    {
        CanonicalMetricMappingStore.ResetForTesting();
    }

    [Fact]
    public void FromLegacyFields_NormalizesCaseWhitespaceAndPeriods()
    {
        CanonicalMetricMappingStore.InitializeForTesting(
        [
            new CanonicalMetricMappingRecord("weight", "body_fat_mass", "weight.body_fat_mass")
        ]);

        var canonicalId = CanonicalMetricMapping.FromLegacyFields(" Weight ", "Body Fat.Mass");

        Assert.Equal("weight.body_fat_mass", canonicalId);
    }

    [Fact]
    public void ToLegacyFields_ReturnsNullSubtypeForAllSubtypeToken()
    {
        CanonicalMetricMappingStore.InitializeForTesting(
        [
            new CanonicalMetricMappingRecord("sleep", CanonicalMetricMappingStore.AllSubtypeToken, "sleep.(all)")
        ]);

        var legacy = CanonicalMetricMapping.ToLegacyFields("sleep.(all)");

        Assert.Equal("sleep", legacy.MetricType);
        Assert.Null(legacy.Subtype);
    }

    [Fact]
    public void FromLegacyFields_ReturnsNullForUnknownMapping()
    {
        CanonicalMetricMappingStore.InitializeForTesting(
        [
            new CanonicalMetricMappingRecord("weight", "skeletal_mass", "weight.skeletal_mass")
        ]);

        var canonicalId = CanonicalMetricMapping.FromLegacyFields("weight", "total_body_water");

        Assert.Null(canonicalId);
    }

    public void Dispose()
    {
        CanonicalMetricMappingStore.ResetForTesting();
    }
}
