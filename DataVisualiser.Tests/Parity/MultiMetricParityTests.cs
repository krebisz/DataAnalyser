using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Parity;

public sealed class MultiMetricParityTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 10);

    [Fact]
    public void Parity_ShouldPass_WithThreeMetrics()
    {
        var legacySeries = CreateLegacySeries(3, 10);
        AssertParity(legacySeries,
                new[]
                {
                        "A",
                        "B",
                        "C"
                });
    }

    [Fact]
    public void Parity_ShouldPass_WithFourMetrics()
    {
        var legacySeries = CreateLegacySeries(4, 10);
        AssertParity(legacySeries,
                new[]
                {
                        "A",
                        "B",
                        "C",
                        "D"
                });
    }

    [Fact]
    public void Parity_ShouldPass_WithEmptyData()
    {
        var legacySeries = new List<IEnumerable<MetricData>>
        {
                Enumerable.Empty<MetricData>(),
                Enumerable.Empty<MetricData>()
        };

        AssertParity(legacySeries,
                new[]
                {
                        "A",
                        "B"
                });
    }

    [Fact]
    public void Parity_ShouldPass_WithMismatchedCounts()
    {
        var legacySeries = new List<IEnumerable<MetricData>>
        {
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(8, TimeSpan.FromDays(1)),

                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(6, TimeSpan.FromDays(1))
        };

        AssertParity(legacySeries,
                new[]
                {
                        "A",
                        "B"
                });
    }

    [Fact]
    public void Parity_ShouldPass_WithDifferentMetricIds_WhenDimensionsMatch()
    {
        var from = From.AddDays(-1);
        var to = To.AddDays(5);
        var legacySeries = CreateLegacySeries(4, 10);
        var labels = new[]
        {
                "Fat Mass",
                "Water Mass",
                "Skeletal Mass",
                "Muscle Mass"
        };

        var legacyStrategy = new MultiMetricStrategy(legacySeries, labels, from, to);
        var legacyResult = legacyStrategy.Compute();

        var cmsSeries = new[]
        {
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_fat_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(10).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.total_body_water").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(10).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.skeletal_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(10).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.muscle_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(10).Build()
        };

        var cmsStrategy = new MultiMetricStrategy(cmsSeries, labels, from, to);
        var cmsResult = cmsStrategy.Compute();

        Assert.NotNull(legacyResult);
        Assert.NotNull(cmsResult);
        Assert.NotNull(legacyResult!.Series);
        Assert.NotNull(cmsResult!.Series);
        Assert.Equal(legacyResult.Series.Count, cmsResult.Series.Count);
    }

    private static List<IEnumerable<MetricData>> CreateLegacySeries(int seriesCount, int pointsPerSeries)
    {
        var result = new List<IEnumerable<MetricData>>();

        for (var i = 0; i < seriesCount; i++)
            result.Add(TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").BuildSeries(pointsPerSeries, TimeSpan.FromDays(1)));

        return result;
    }

    private static void AssertParity(IReadOnlyList<IEnumerable<MetricData>> legacySeries, IReadOnlyList<string> labels)
    {
        var legacyStrategy = new MultiMetricStrategy(legacySeries, labels, From, To);

        var legacyResult = legacyStrategy.Compute();

        const string sharedMetricId = "metric.test.multi";

        var start = new DateTimeOffset(From, TimeZoneInfo.Local.GetUtcOffset(From));
        var cmsSeries = legacySeries.Select(series => TestDataBuilders.CanonicalMetricSeries().WithMetricId(sharedMetricId).WithUnit("kg").WithStartTime(start).WithSampleCount(series.Count()).Build()).ToList();

        var cmsStrategy = new MultiMetricStrategy(cmsSeries, labels, From, To);

        var cmsResult = cmsStrategy.Compute();

        if (legacySeries.All(series => !series.Any()))
        {
            Assert.Null(legacyResult);
            Assert.Null(cmsResult);
            return;
        }

        Assert.NotNull(legacyResult);
        Assert.NotNull(cmsResult);

        Assert.NotNull(legacyResult.Series);
        Assert.NotNull(cmsResult.Series);

        Assert.Equal(legacyResult.Series.Count, cmsResult.Series.Count);

        for (var i = 0; i < legacyResult.Series.Count; i++)
        {
            var legacySeriesResult = legacyResult.Series[i];
            var cmsSeriesResult = cmsResult.Series[i];

            Assert.Equal(legacySeriesResult.RawValues, cmsSeriesResult.RawValues);
            Assert.Equal(legacySeriesResult.Smoothed, cmsSeriesResult.Smoothed);
            Assert.Equal(legacySeriesResult.Timestamps.Count, cmsSeriesResult.Timestamps.Count);
        }
    }
}
