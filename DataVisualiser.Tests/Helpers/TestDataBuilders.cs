namespace DataVisualiser.Tests.Helpers;

/// <summary>
///     Builders for creating test data for parity validation.
/// </summary>
public static class TestDataBuilders
{
    public static HealthMetricDataBuilder HealthMetricData()
    {
        return new HealthMetricDataBuilder();
    }

    public static MockCmsBuilder CanonicalMetricSeries()
    {
        return new MockCmsBuilder();
    }
}