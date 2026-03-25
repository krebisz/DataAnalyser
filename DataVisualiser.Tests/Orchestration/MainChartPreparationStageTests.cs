using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.MainChart;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Orchestration;

public sealed class MainChartPreparationStageTests
{
    [Fact]
    public async Task PrepareAsync_WithAdditionalSeries_BuildsMultiMetricWorkingContext()
    {
        var stage = new MainChartPreparationStage(metricSelectionService: null, connectionString: null);
        var cmsSeries = new List<ICanonicalMetricSeries>
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.a").WithDimension(MetricDimension.Mass).WithStartTime(new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero)).WithSampleCount(2).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.b").WithDimension(MetricDimension.Mass).WithStartTime(new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero)).WithSampleCount(2).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.c").WithDimension(MetricDimension.Mass).WithStartTime(new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero)).WithSampleCount(2).Build()
        };

        var context = new ChartDataContext
        {
            Data1 = TestDataBuilders.HealthMetricData().WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
            Data2 = TestDataBuilders.HealthMetricData().WithValue(20m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
            DisplayName1 = "MetricA:A",
            DisplayName2 = "MetricA:B",
            PrimaryCms = cmsSeries[0],
            SecondaryCms = cmsSeries[1],
            CmsSeries = cmsSeries,
            From = new DateTime(2024, 1, 1),
            To = new DateTime(2024, 1, 2),
            ActualSeriesCount = 2
        };

        var prepared = await stage.PrepareAsync(
            new MainChartRenderRequest(
                context,
                AdditionalSeries:
                [
                    TestDataBuilders.HealthMetricData().WithValue(30m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1))
                ],
                AdditionalLabels: ["MetricA:C"]));

        Assert.Equal(3, prepared.Series.Count);
        Assert.Equal(new[] { "MetricA:A", "MetricA:B", "MetricA:C" }, prepared.Labels);
        Assert.Equal(3, prepared.WorkingContext.ActualSeriesCount);
        Assert.NotNull(prepared.WorkingContext.CmsSeries);
        Assert.Equal(3, prepared.WorkingContext.CmsSeries!.Count);
    }
}
