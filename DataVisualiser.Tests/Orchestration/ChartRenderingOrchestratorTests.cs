using System.Reflection;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Orchestration;

public sealed class ChartRenderingOrchestratorTests
{
    [Fact]
    public void ShouldRenderCharts_ShouldReturnFalse_WhenNoPrimaryData()
    {
        var method = GetPrivateStaticMethod("ShouldRenderCharts");

        var result = (bool)method.Invoke(null,
                new object?[]
                {
                        null
                })!;

        Assert.False(result);
    }

    [Fact]
    public void HasSecondaryData_ShouldReturnTrue_WhenSecondaryPresent()
    {
        var method = GetPrivateStaticMethod("HasSecondaryData");

        var ctx = new ChartDataContext
        {
                Data2 = new List<MetricData>
                {
                        TestDataBuilders.HealthMetricData().Build()
                }
        };

        var result = (bool)method.Invoke(null,
                new object?[]
                {
                        ctx
                })!;

        Assert.True(result);
    }

    [Fact]
    public void BuildInitialSeriesList_ShouldIncludeSecondaryWhenPresent()
    {
        var method = GetPrivateStaticMethod("BuildInitialSeriesList");

        var data1 = TestDataBuilders.HealthMetricData().BuildSeries(1, TimeSpan.FromDays(1));
        var data2 = TestDataBuilders.HealthMetricData().BuildSeries(1, TimeSpan.FromDays(1));

        var result = method.Invoke(null,
                new object?[]
                {
                        data1,
                        data2,
                        "A",
                        "B"
                })!;
        var typed = ((List<IEnumerable<MetricData>> series, List<string> labels))result;

        Assert.Equal(2, typed.series.Count);
        Assert.Equal(new[]
                {
                        "A",
                        "B"
                },
                typed.labels);
    }

    [Fact]
    public void BuildSeriesAndLabels_ShouldSkipEmptyAdditionalSeries()
    {
        var method = GetPrivateStaticMethod("BuildSeriesAndLabels");

        var ctx = new ChartDataContext
        {
                Data1 = TestDataBuilders.HealthMetricData().BuildSeries(1, TimeSpan.FromDays(1)),
                DisplayName1 = "A"
        };

        var additionalSeries = new List<IEnumerable<MetricData>>
        {
                new List<MetricData>()
        };
        var additionalLabels = new List<string>
        {
                "C"
        };

        var result = method.Invoke(null,
                new object?[]
                {
                        ctx,
                        additionalSeries,
                        additionalLabels
                })!;
        var typed = ((List<IEnumerable<MetricData>> series, List<string> labels))result;

        Assert.Single(typed.series);
        Assert.Equal(new[]
                {
                        "A"
                },
                typed.labels);
    }

    private static MethodInfo GetPrivateStaticMethod(string name)
    {
        var method = typeof(ChartRenderingOrchestrator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return method!;
    }
}