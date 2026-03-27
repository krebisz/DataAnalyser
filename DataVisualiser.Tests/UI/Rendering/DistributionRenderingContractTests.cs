using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveChartsCore.SkiaSharpView.WPF;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class DistributionRenderingContractTests
{
    [Fact]
    public void GetBackendQualificationMatrix_IncludesQualifiedFallbackAndDormantDebtEntries()
    {
        var contract = CreateContract();

        var entries = contract.GetBackendQualificationMatrix();

        Assert.Contains(entries, entry => entry.BackendKey == DistributionBackendKey.LiveChartsWpfCartesian
            && entry.ActiveRoute == DistributionRenderingRoute.Cartesian
            && entry.Qualification == DistributionRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == DistributionBackendKey.LiveChartsWpfPolarFallbackProjection
            && entry.ActiveRoute == DistributionRenderingRoute.PolarFallback
            && entry.Qualification == DistributionRenderingQualification.TacticalFallback);
        Assert.Contains(entries, entry => entry.BackendKey == DistributionBackendKey.LiveChartsCorePolar
            && entry.ActiveRoute == null
            && entry.Qualification == DistributionRenderingQualification.UnqualifiedDebt
            && !entry.SupportsLifecycleSafety);
    }

    [Fact]
    public void GetCapabilities_ForCartesianRoute_ReportsQualifiedPath()
    {
        var contract = CreateContract();

        var capabilities = contract.GetCapabilities(DistributionRenderingRoute.Cartesian);

        Assert.Equal("Distribution.Cartesian.LiveChartsWpf", capabilities.PathKey);
        Assert.Equal(DistributionRenderingQualification.Qualified, capabilities.Qualification);
        Assert.True(capabilities.SupportsRender);
        Assert.True(capabilities.SupportsResetView);
        Assert.False(capabilities.Qualification == DistributionRenderingQualification.TacticalFallback);
    }

    [Fact]
    public void GetCapabilities_ForPolarFallbackRoute_ReportsTacticalFallbackPath()
    {
        var contract = CreateContract();

        var capabilities = contract.GetCapabilities(DistributionRenderingRoute.PolarFallback);

        Assert.Equal("Distribution.PolarFallback.LiveChartsWpfProjection", capabilities.PathKey);
        Assert.Equal(DistributionRenderingQualification.TacticalFallback, capabilities.Qualification);
        Assert.True(capabilities.SupportsRender);
        Assert.True(capabilities.SupportsResetView);
        Assert.True(capabilities.SupportsHoverTooltip);
    }

    [Fact]
    public void GetCapabilities_ForUnknownRoute_Throws()
    {
        var contract = CreateContract();

        Assert.Throws<ArgumentOutOfRangeException>(() => contract.GetCapabilities((DistributionRenderingRoute)999));
    }

    [Theory]
    [InlineData(false, DistributionRenderingRoute.Cartesian)]
    [InlineData(true, DistributionRenderingRoute.PolarFallback)]
    public void ResolveRoute_ReturnsExpectedActiveRoute(bool isPolarMode, DistributionRenderingRoute expectedRoute)
    {
        var route = DistributionRenderingRouteResolver.Resolve(isPolarMode);

        Assert.Equal(expectedRoute, route);
    }

    [Fact]
    public void HasRenderableContent_ForPolarFallback_UsesCartesianProjectionHost()
    {
        StaTestHelper.Run(() =>
        {
            var host = new DistributionChartRenderHost(new CartesianChart(), new PolarChart(), new ChartState(), () => null);
            host.CartesianChart.Series.Add(new LiveCharts.Wpf.LineSeries());
            var contract = CreateContract();

            Assert.True(contract.HasRenderableContent(DistributionRenderingRoute.PolarFallback, host));
        });
    }

    [Fact]
    public void QualificationMatrix_ShouldHaveUniquePathKeys_AndResolvableActiveRoutes()
    {
        var contract = CreateContract();

        var entries = contract.GetBackendQualificationMatrix();
        var activeEntries = entries.Where(entry => entry.ActiveRoute != null).ToList();

        Assert.Equal(entries.Count, entries.Select(entry => entry.PathKey).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(activeEntries.Count, activeEntries.Select(entry => entry.ActiveRoute).Distinct().Count());

        foreach (var entry in activeEntries)
        {
            var capabilities = contract.GetCapabilities(entry.ActiveRoute!.Value);
            Assert.Equal(entry.PathKey, capabilities.PathKey);
            Assert.Equal(entry.Qualification, capabilities.Qualification);
        }
    }

    private static DistributionRenderingContract CreateContract()
    {
        var distributionService = new StubDistributionService();
        return new DistributionRenderingContract(() => null, distributionService, distributionService, new DistributionPolarRenderingService());
    }

    private sealed class StubDistributionService : Core.Services.Abstractions.IDistributionService
    {
        public Task UpdateDistributionChartAsync(CartesianChart targetChart, IEnumerable<Shared.Models.MetricData> data, string displayName, DateTime from, DateTime to, double minHeight = 400, bool useFrequencyShading = true, int intervalCount = 10, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            return Task.CompletedTask;
        }

        public Task<Shared.Models.DistributionRangeResult?> ComputeSimpleRangeAsync(IEnumerable<Shared.Models.MetricData> data, string displayName, DateTime from, DateTime to, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            return Task.FromResult<Shared.Models.DistributionRangeResult?>(null);
        }

        public void SetShadingStrategy(Core.Rendering.Shading.IIntervalShadingStrategy strategy)
        {
        }
    }
}
