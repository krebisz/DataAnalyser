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

    [Fact]
    public async Task PrepareAsync_WithSelectedSeriesSubset_UsesCurrentSelectionsInsteadOfStaleContextSeries()
    {
        var stage = new MainChartPreparationStage(metricSelectionService: null, connectionString: null);
        var context = new ChartDataContext
        {
            Data1 = TestDataBuilders.HealthMetricData().WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
            Data2 = TestDataBuilders.HealthMetricData().WithValue(20m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
            DisplayName1 = "Old Primary",
            DisplayName2 = "Old Secondary",
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            SecondaryMetricType = "Weight",
            PrimarySubtype = "morning",
            SecondarySubtype = "evening",
            From = new DateTime(2024, 1, 1),
            To = new DateTime(2024, 1, 2),
            ActualSeriesCount = 2
        };

        var prepared = await stage.PrepareAsync(
            new MainChartRenderRequest(
                context,
                SelectedSeries:
                [
                    new MetricSeriesSelection("Weight", "morning", "Weight", "Morning")
                ]));

        Assert.Single(prepared.Series);
        Assert.Equal(["Weight - Morning"], prepared.Labels);
        Assert.Equal(1, prepared.WorkingContext.ActualSeriesCount);
        Assert.Equal("Weight", prepared.WorkingContext.PrimaryMetricType);
        Assert.Equal("morning", prepared.WorkingContext.PrimarySubtype);
        Assert.Null(prepared.WorkingContext.SecondarySubtype);
        Assert.Equal("Weight - Morning", prepared.WorkingContext.DisplayName1);
    }
}
