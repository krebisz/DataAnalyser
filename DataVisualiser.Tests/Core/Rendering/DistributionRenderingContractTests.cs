using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveChartsCore.SkiaSharpView.WPF;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
namespace DataVisualiser.Tests.Core.Rendering;

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

    [Theory]
    [InlineData(DistributionRenderingRoute.Cartesian, DistributionMode.Weekly, DistributionBackendKey.LiveChartsWpfCartesian)]
    [InlineData(DistributionRenderingRoute.PolarFallback, DistributionMode.Hourly, DistributionBackendKey.LiveChartsWpfPolarFallbackProjection)]
    public void DistributionRenderPlanBuilder_ShouldPreserveCapabilityContractAndDeliveryMetadata(
        DistributionRenderingRoute route,
        DistributionMode mode,
        string expectedBackendKey)
    {
        var request = CreateRequest(route, mode);

        var plan = DistributionRenderPlanBuilder.Build(request);

        Assert.Equal(ChartProgramKind.Distribution, plan.ProgramKind);
        Assert.Equal(ChartRenderPlanKind.Cartesian, plan.PlanKind);
        Assert.Equal(ChartRenderDensityMode.FullFidelity, plan.Density.Mode);
        Assert.Equal(2, plan.Density.SourcePointCount);
        Assert.Equal(2, plan.Density.RenderedPointCount);
        Assert.Equal(request.Settings.IntervalCount, plan.Density.BucketCount);
        Assert.Equal(expectedBackendKey, plan.Metadata[ChartRenderPlanMetadataKeys.BackendKey]);
        Assert.Equal(nameof(DistributionRenderPlanAdapter), plan.Metadata["Adapter"]);
        Assert.Equal(ChartProgramKind.Distribution.ToString(), plan.Metadata["ProgramKind"]);
        Assert.Equal(route.ToString(), plan.Metadata["Route"]);
        Assert.Equal(mode.ToString(), plan.Metadata["Mode"]);
        Assert.Equal("Weight - Morning", plan.Metadata["Selection"]);
        Assert.Equal(ConsumerKind.Chart.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.ConsumerKind]);
        Assert.Equal("DistributionChart", plan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal(AnalyticalCapabilityKind.Distribution.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        Assert.Equal(CompositionKind.SingleSeries.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
        Assert.Equal("LiveChartsWpf", plan.Metadata[ChartRenderPlanMetadataKeys.ProviderKey]);
        Assert.Contains("Distribution:SingleSeries", plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature], StringComparison.Ordinal);
        Assert.Contains(plan.SourceSignature, plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature], StringComparison.Ordinal);
        Assert.Contains(plan.SourceSignature, plan.Metadata[ChartRenderPlanMetadataKeys.ProvenanceSignature], StringComparison.Ordinal);
        Assert.True(plan.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.ProviderSignature));
    }

    [Fact]
    public void DistributionRenderPlanBuilder_ShouldUseRuntimeCapabilityContract()
    {
        var programRequest = ChartProgramRequest.Distribution();
        var request = CreateRequest(DistributionRenderingRoute.Cartesian, DistributionMode.Weekly) with
        {
            CapabilityContract = new DistributionCapabilityContract(
                programRequest,
                CapabilityRequest.FromProgramRequest(programRequest),
                ConsumerDeliveryContract.Chart(ChartProgramKind.Distribution, "DistributionDiagnosticSurface"))
        };

        var plan = DistributionRenderPlanBuilder.Build(request);

        Assert.Equal("DistributionDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Contains("Chart:Distribution:DistributionDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature], StringComparison.Ordinal);
    }

    [Fact]
    public void DistributionCapabilityContract_ShouldRejectProgramKindDrift()
    {
        var programRequest = ChartProgramRequest.MainProgram();

        Assert.Throws<ArgumentException>(() => new DistributionCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Distribution, "DistributionChart")));
    }

    private static DistributionRenderingContract CreateContract()
    {
        var distributionService = new StubDistributionService();
        return new DistributionRenderingContract(() => null, distributionService, distributionService, new DistributionPolarRenderingService());
    }

    private static DistributionChartRenderRequest CreateRequest(DistributionRenderingRoute route, DistributionMode mode)
    {
        return new DistributionChartRenderRequest(
            route,
            mode,
            new DistributionModeSettings(true, 7),
            [
                new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m },
                new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 2), Value = 2m }
            ],
            "Weight",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31),
            null,
            new ChartDataContext(),
            new ChartState(),
            "Weight - Morning");
    }

    private sealed class StubDistributionService : DataVisualiser.Core.Services.Abstractions.IDistributionService
    {
        public Task UpdateDistributionChartAsync(CartesianChart targetChart, IEnumerable<Shared.Models.MetricData> data, string displayName, DateTime from, DateTime to, double minHeight = 400, bool useFrequencyShading = true, int intervalCount = 10, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            return Task.CompletedTask;
        }

        public Task<Shared.Models.DistributionRangeResult?> ComputeSimpleRangeAsync(IEnumerable<Shared.Models.MetricData> data, string displayName, DateTime from, DateTime to, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            return Task.FromResult<Shared.Models.DistributionRangeResult?>(null);
        }

        public void SetShadingStrategy(IIntervalShadingStrategy strategy)
        {
        }
    }
}
