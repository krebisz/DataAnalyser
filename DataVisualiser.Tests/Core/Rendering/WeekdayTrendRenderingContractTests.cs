using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Core.Rendering;

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

    [Fact]
    public void WeekdayTrendRenderPlanBuilder_ShouldUseRuntimeCapabilityContract()
    {
        var programRequest = ChartProgramRequest.WeekdayTrend();
        var request = new WeekdayTrendChartRenderRequest(
            WeekdayTrendRenderingRoute.Scatter,
            CreateResult(),
            new ChartState { WeekdayTrendChartMode = WeekdayTrendChartMode.Scatter },
            "Weight:body_fat_mass",
            new WeekdayTrendCapabilityContract(
                programRequest,
                CapabilityRequest.FromProgramRequest(programRequest),
                ConsumerDeliveryContract.Chart(ChartProgramKind.WeekdayTrend, "WeekdayTrendDiagnosticSurface")));

        var plan = WeekdayTrendRenderPlanBuilder.Build(request);

        Assert.Equal(ChartProgramKind.WeekdayTrend, plan.ProgramKind);
        Assert.Equal(WeekdayTrendBackendKey.LiveChartsWpfScatter, plan.Metadata[ChartRenderPlanMetadataKeys.BackendKey]);
        Assert.Equal("Scatter", plan.Metadata["Route"]);
        Assert.Equal("WeekdayTrendDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal(AnalyticalCapabilityKind.TemporalTrend.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        Assert.Equal(CompositionKind.SingleSeries.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
        Assert.Contains("Chart:WeekdayTrend:WeekdayTrendDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature], StringComparison.Ordinal);
    }

    [Fact]
    public void WeekdayTrendVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata()
    {
        var request = new WeekdayTrendChartRenderRequest(
            WeekdayTrendRenderingRoute.Scatter,
            CreateResult(),
            new ChartState { WeekdayTrendChartMode = WeekdayTrendChartMode.Scatter },
            "Weight:body_fat_mass",
            WeekdayTrendCapabilityContract.Create());
        var plan = WeekdayTrendRenderPlanBuilder.Build(request);

        var contract = WeekdayTrendVNextConsumptionContractBuilder.Build(request, plan);
        var qualifiedPlan = WeekdayTrendVNextConsumptionContractBuilder.AttachMetadata(plan, contract);

        Assert.Equal(ChartProgramKind.WeekdayTrend, contract.ProgramKind);
        Assert.Equal(AnalyticalCapabilityKind.TemporalTrend, contract.CapabilityKind);
        Assert.Equal(CompositionKind.SingleSeries, contract.CompositionKind);
        Assert.Equal(ConsumerKind.Chart, contract.Delivery.ConsumerKind);
        Assert.Equal("WeekdayTrendChart", contract.Delivery.DeliveryTarget);
        Assert.Equal(ConsumerSurfaceModelKind.ChartRenderPlan, contract.SurfaceModel.Kind);
        Assert.Equal(plan.Id, contract.SurfaceModel.SurfaceId);
        Assert.Equal("Scatter", contract.Metadata["WeekdayTrend.Route"]);
        Assert.Equal("Scatter", contract.Metadata["WeekdayTrend.Mode"]);
        Assert.Equal("Weight:body_fat_mass", contract.Metadata["WeekdayTrend.Selection"]);
        Assert.Equal(contract.Signature, qualifiedPlan.Metadata[WeekdayTrendVNextConsumptionContractBuilder.ConsumptionContractSignatureKey]);
        Assert.Equal("ChartRenderPlan", qualifiedPlan.Metadata[WeekdayTrendVNextConsumptionContractBuilder.SurfaceKindKey]);
        Assert.Equal(plan.Id, qualifiedPlan.Metadata[WeekdayTrendVNextConsumptionContractBuilder.SurfaceIdKey]);
    }

    [Fact]
    public void Render_ShouldAttachVNextConsumptionMetadata()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState { WeekdayTrendChartMode = WeekdayTrendChartMode.Cartesian };
            var contract = CreateContract();
            var host = CreateHost(chartState);

            var result = contract.Render(
                new WeekdayTrendChartRenderRequest(
                    WeekdayTrendRenderingRoute.Cartesian,
                    CreateResult(),
                    chartState,
                    "Weight:body_fat_mass",
                    WeekdayTrendCapabilityContract.Create()),
                host);

            Assert.Equal(WeekdayTrendBackendKey.LiveChartsWpfCartesian, result.BackendKey);
            Assert.Equal("ChartRenderPlan", result.Metadata[WeekdayTrendVNextConsumptionContractBuilder.SurfaceKindKey]);
            Assert.True(result.Metadata.ContainsKey(WeekdayTrendVNextConsumptionContractBuilder.ConsumptionContractSignatureKey));
            Assert.Equal("WeekdayTrendChart", result.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
            Assert.Equal("TemporalTrend", result.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        });
    }

    [Fact]
    public void WeekdayTrendCapabilityContract_ShouldRejectProgramKindDrift()
    {
        var programRequest = ChartProgramRequest.Distribution();

        Assert.Throws<ArgumentException>(() => new WeekdayTrendCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.Chart(ChartProgramKind.WeekdayTrend, "WeekdayTrendChart")));
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

    private static WeekdayTrendResult CreateResult()
    {
        var result = new WeekdayTrendResult
        {
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7),
            GlobalMin = 1d,
            GlobalMax = 5d,
            Unit = "kg"
        };

        result.SeriesByDay[0] = new WeekdayTrendSeries
        {
            Day = DayOfWeek.Monday,
            Points =
            [
                new WeekdayTrendPoint
                {
                    Date = new DateTime(2026, 1, 5),
                    Value = 2d
                }
            ]
        };

        return result;
    }
}
