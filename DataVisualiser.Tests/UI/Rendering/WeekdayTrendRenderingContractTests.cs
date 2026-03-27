using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class WeekdayTrendRenderingContractTests
{
    [Fact]
    public void GetBackendQualificationMatrix_IncludesAllActiveRoutes()
    {
        var contract = CreateContract();

        var entries = contract.GetBackendQualificationMatrix();

        Assert.Contains(entries, entry => entry.BackendKey == WeekdayTrendBackendKey.LiveChartsWpfCartesian
            && entry.ActiveRoute == WeekdayTrendRenderingRoute.Cartesian
            && entry.Qualification == WeekdayTrendRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == WeekdayTrendBackendKey.LiveChartsWpfPolar
            && entry.ActiveRoute == WeekdayTrendRenderingRoute.Polar
            && entry.Qualification == WeekdayTrendRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == WeekdayTrendBackendKey.LiveChartsWpfScatter
            && entry.ActiveRoute == WeekdayTrendRenderingRoute.Scatter
            && entry.Qualification == WeekdayTrendRenderingQualification.Qualified);
    }

    [Fact]
    public void GetCapabilities_ForPolarRoute_ReportsQualifiedPath()
    {
        var contract = CreateContract();

        var capabilities = contract.GetCapabilities(WeekdayTrendRenderingRoute.Polar);

        Assert.Equal("WeekdayTrend.Polar.LiveChartsWpf", capabilities.PathKey);
        Assert.Equal(WeekdayTrendRenderingQualification.Qualified, capabilities.Qualification);
        Assert.True(capabilities.SupportsRender);
        Assert.True(capabilities.SupportsResetView);
    }

    [Fact]
    public void GetCapabilities_ForUnknownRoute_Throws()
    {
        var contract = CreateContract();

        Assert.Throws<ArgumentOutOfRangeException>(() => contract.GetCapabilities((WeekdayTrendRenderingRoute)999));
    }

    [Theory]
    [InlineData(WeekdayTrendChartMode.Cartesian, WeekdayTrendRenderingRoute.Cartesian)]
    [InlineData(WeekdayTrendChartMode.Polar, WeekdayTrendRenderingRoute.Polar)]
    [InlineData(WeekdayTrendChartMode.Scatter, WeekdayTrendRenderingRoute.Scatter)]
    public void ResolveRoute_ReturnsExpectedRoute(WeekdayTrendChartMode mode, WeekdayTrendRenderingRoute expectedRoute)
    {
        var route = WeekdayTrendRenderingRouteResolver.Resolve(mode);

        Assert.Equal(expectedRoute, route);
    }

    [Fact]
    public void ResetView_WithoutCachedResult_ResetsActiveCartesianChart()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                WeekdayTrendChartMode = WeekdayTrendChartMode.Cartesian
            };
            var host = CreateHost(chartState);
            host.CartesianChart.AxisX.Add(new Axis
            {
                MinValue = 1,
                MaxValue = 2
            });

            var contract = CreateContract();

            contract.ResetView(WeekdayTrendRenderingRoute.Cartesian, host);

            Assert.True(double.IsNaN(host.CartesianChart.AxisX[0].MinValue));
            Assert.True(double.IsNaN(host.CartesianChart.AxisX[0].MaxValue));
        });
    }

    [Fact]
    public void HasRenderableContent_ForPolarRoute_UsesPolarChart()
    {
        StaTestHelper.Run(() =>
        {
            var host = CreateHost(new ChartState
            {
                WeekdayTrendChartMode = WeekdayTrendChartMode.Polar
            });
            host.PolarChart.Series.Add(new LineSeries());

            var contract = CreateContract();

            Assert.True(contract.HasRenderableContent(WeekdayTrendRenderingRoute.Polar, host));
            Assert.False(contract.HasRenderableContent(WeekdayTrendRenderingRoute.Cartesian, host));
        });
    }

    [Fact]
    public void QualificationMatrix_ShouldHaveUniquePathKeys_AndResolvableActiveRoutes()
    {
        var contract = CreateContract();

        var entries = contract.GetBackendQualificationMatrix();

        Assert.Equal(entries.Count, entries.Select(entry => entry.PathKey).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(entries.Count, entries.Select(entry => entry.ActiveRoute).Distinct().Count());

        foreach (var entry in entries)
        {
            var capabilities = contract.GetCapabilities(entry.ActiveRoute);
            Assert.Equal(entry.PathKey, capabilities.PathKey);
            Assert.Equal(entry.Qualification, capabilities.Qualification);
        }
    }

    private static WeekdayTrendRenderingContract CreateContract()
    {
        return new WeekdayTrendRenderingContract(
            new WeekdayTrendChartUpdateCoordinator(
                new WeekdayTrendRenderingService(),
                new Dictionary<CartesianChart, List<DateTime>>()));
    }

    private static WeekdayTrendChartRenderHost CreateHost(ChartState chartState)
    {
        return new WeekdayTrendChartRenderHost(new CartesianChart(), new CartesianChart(), chartState);
    }
}
