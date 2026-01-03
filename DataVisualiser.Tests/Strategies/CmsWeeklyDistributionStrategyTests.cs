using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Charts.Strategies;

public class CmsWeeklyDistributionStrategyTests
{
    [Fact]
    public void Phase5_And_Phase6_AreObservable_AndCorrect_Via_ExtendedResult()
    {
        // Arrange: 2 samples on Monday+Tuesday, all other weekdays empty
        var start = new DateTimeOffset(2024, 01, 01, 0, 0, 0, TimeSpan.Zero); // Monday
        var cms = TestDataBuilders.CanonicalMetricSeries().
                                   WithMetricId("weight.test").
                                   WithStartTime(start).
                                   WithInterval(TimeSpan.FromDays(1)).
                                   WithValue(100m).
                                   WithSampleCount(2).
                                   Build();

        var from = new DateTime(2024, 01, 01);
        var to = new DateTime(2024, 01, 07);

        var strategy = new CmsWeeklyDistributionStrategy(cms, from, to, "test");

        // Act
        var chart = strategy.Compute();
        var ext = strategy.ExtendedResult;

        // Assert: legal surface exists
        Assert.NotNull(chart);
        Assert.NotNull(ext);

        // Assert Phase 5: counts + mins/maxs/ranges shape
        Assert.Equal(7, ext!.Counts.Count);
        Assert.Equal(7, ext.Mins.Count);
        Assert.Equal(7, ext.Maxs.Count);
        Assert.Equal(7, ext.Ranges.Count);

        Assert.Equal(1, ext.Counts[0]); // Monday
        Assert.Equal(1, ext.Counts[1]); // Tuesday
        for (var i = 2; i < 7; i++)
            Assert.Equal(0, ext.Counts[i]);

        Assert.Equal(100d, ext.Mins[0]);
        Assert.Equal(100d, ext.Maxs[0]);
        Assert.Equal(0d, ext.Ranges[0]);

        Assert.Equal(100d, ext.Mins[1]);
        Assert.Equal(100d, ext.Maxs[1]);
        Assert.Equal(0d, ext.Ranges[1]);

        for (var i = 2; i < 7; i++)
        {
            Assert.True(double.IsNaN(ext.Mins[i]));
            Assert.True(double.IsNaN(ext.Maxs[i]));
            Assert.True(double.IsNaN(ext.Ranges[i]));
        }

        // Assert Phase 6: global bounds derived only from non-empty days
        Assert.Equal(100d, ext.GlobalMin);
        Assert.Equal(100d, ext.GlobalMax);

        // Assert: chart mirrors mins+ranges (legacy convention)
        Assert.True(chart!.PrimaryRawValues.SequenceEqual(ext.Mins));
        Assert.True(chart.PrimarySmoothed.SequenceEqual(ext.Ranges));

        Assert.Equal("kg", ext.Unit);
        Assert.Equal("kg", chart.Unit);
    }
}