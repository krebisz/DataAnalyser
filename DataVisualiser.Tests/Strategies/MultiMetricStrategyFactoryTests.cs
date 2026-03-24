using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Factories;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Strategies;

public sealed class MultiMetricStrategyFactoryTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 10);

    [Fact]
    public void CreateCmsStrategy_ShouldUseCmsSeries()
    {
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build()
        };

        var ctx = new ChartDataContext
        {
            CmsSeries = cmsSeries,
            From = From,
            To = To
        };

        var parameters = new Core.Strategies.Abstractions.StrategyCreationParameters
        {
            CmsSeries = cmsSeries,
            Labels = ["A", "B", "C"],
            From = From,
            To = To
        };

        var factory = new MultiMetricStrategyFactory();
        var strategy = factory.CreateCmsStrategy(ctx, parameters);
        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.Series);
        Assert.Equal(3, result.Series!.Count);
        Assert.Equal(5, result.Series[0].RawValues.Count);
    }

    [Fact]
    public void CreateCmsStrategy_ShouldAllowDifferentMetricIds_WhenDimensionsMatch()
    {
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_fat_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.total_body_water").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.skeletal_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build()
        };

        var ctx = new ChartDataContext
        {
            CmsSeries = cmsSeries,
            From = From,
            To = To
        };

        var parameters = new Core.Strategies.Abstractions.StrategyCreationParameters
        {
            CmsSeries = cmsSeries,
            Labels = ["Fat", "Water", "Skeletal"],
            From = From,
            To = To
        };

        var factory = new MultiMetricStrategyFactory();
        var strategy = factory.CreateCmsStrategy(ctx, parameters);
        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(3, result!.Series!.Count);
    }
}
