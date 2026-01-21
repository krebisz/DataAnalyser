using DataVisualiser.Core.Services;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Services;

public sealed class DataPreparationServiceTests
{
    [Fact]
    public void PrepareCmsData_ShouldFilterUsingLocalTimestamps()
    {
        var start = new DateTimeOffset(new DateTime(2024, 01, 01), TimeSpan.Zero);
        var cms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(start).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(2).Build();

        var sample = cms.Samples.First();
        var from = sample.Timestamp.LocalDateTime;
        var to = sample.Timestamp.LocalDateTime;

        var service = new DataPreparationService();
        var result = service.PrepareCmsData(cms, from, to);

        Assert.Single(result);
        Assert.Equal(sample.Timestamp.LocalDateTime, result[0].Timestamp.LocalDateTime);
    }
}
