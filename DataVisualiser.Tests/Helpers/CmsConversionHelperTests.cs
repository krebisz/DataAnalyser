using DataVisualiser.Shared.Helpers;

namespace DataVisualiser.Tests.Helpers;

public sealed class CmsConversionHelperTests
{
    private static readonly DateTimeOffset Start = new(new DateTime(2024, 01, 01), TimeSpan.Zero);

    [Fact]
    public void ConvertSamplesToHealthMetricData_ShouldFilterByDateRange()
    {
        var cms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(Start).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(3).WithUnit("kg").Build();

        var from = Start.LocalDateTime.AddDays(1);
        var to = Start.LocalDateTime.AddDays(1);

        var result = CmsConversionHelper.ConvertSamplesToHealthMetricData(cms, from, to).ToList();

        Assert.Single(result);
        Assert.Equal(from, result[0].NormalizedTimestamp);
        Assert.Equal("kg", result[0].Unit);
    }

    [Fact]
    public void ConvertSamplesToHealthMetricData_ShouldUseLocalTimestamps()
    {
        var cms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(Start).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(1).Build();

        var sample = cms.Samples.First();
        var from = sample.Timestamp.LocalDateTime;
        var to = sample.Timestamp.LocalDateTime;

        var result = CmsConversionHelper.ConvertSamplesToHealthMetricData(cms, from, to).ToList();

        Assert.Single(result);
        Assert.Equal(sample.Timestamp.LocalDateTime, result[0].NormalizedTimestamp);
    }

    [Fact]
    public void ConvertMultipleCmsToHealthMetricData_ShouldMergeAndOrder()
    {
        var cms1 = TestDataBuilders.CanonicalMetricSeries().WithStartTime(Start).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(2).Build();

        var cms2 = TestDataBuilders.CanonicalMetricSeries().WithStartTime(Start.AddDays(3)).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(2).Build();

        var result = CmsConversionHelper.ConvertMultipleCmsToHealthMetricData(new[]
                                        {
                                                cms2,
                                                cms1
                                        })
                                        .ToList();

        Assert.Equal(4, result.Count);
        Assert.True(result.SequenceEqual(result.OrderBy(r => r.NormalizedTimestamp)));
    }
}