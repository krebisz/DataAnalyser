using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed class WeekdayTrendRenderingContract : IWeekdayTrendRenderingContract
{
    private readonly ChartRenderPlanAdapterDispatcher<WeekdayTrendRenderSurface> _dispatcher;
    private static readonly IReadOnlyList<WeekdayTrendBackendQualification> QualificationMatrix =
    [
        new WeekdayTrendBackendQualification(
            WeekdayTrendBackendKey.LiveChartsWpfCartesian,
            "WeekdayTrend.Cartesian.LiveChartsWpf",
            WeekdayTrendRenderingQualification.Qualified,
            WeekdayTrendRenderingRoute.Cartesian,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new WeekdayTrendBackendQualification(
            WeekdayTrendBackendKey.LiveChartsWpfPolar,
            "WeekdayTrend.Polar.LiveChartsWpf",
            WeekdayTrendRenderingQualification.Qualified,
            WeekdayTrendRenderingRoute.Polar,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true),
        new WeekdayTrendBackendQualification(
            WeekdayTrendBackendKey.LiveChartsWpfScatter,
            "WeekdayTrend.Scatter.LiveChartsWpf",
            WeekdayTrendRenderingQualification.Qualified,
            WeekdayTrendRenderingRoute.Scatter,
            SupportsRender: true,
            SupportsUpdate: true,
            SupportsResetView: true,
            SupportsClear: true,
            SupportsLifecycleSafety: true)
    ];

    private readonly WeekdayTrendChartUpdateCoordinator _updateCoordinator;

    public WeekdayTrendRenderingContract(WeekdayTrendChartUpdateCoordinator updateCoordinator)
    {
        _updateCoordinator = updateCoordinator ?? throw new ArgumentNullException(nameof(updateCoordinator));
        _dispatcher = new ChartRenderPlanAdapterDispatcher<WeekdayTrendRenderSurface>(
            [new WeekdayTrendRenderPlanAdapter(RenderCore)]);
    }

    public IReadOnlyList<WeekdayTrendBackendQualification> GetBackendQualificationMatrix()
    {
        return QualificationMatrix;
    }

    public WeekdayTrendRenderingCapabilities GetCapabilities(WeekdayTrendRenderingRoute route)
    {
        var qualification = QualificationMatrix.FirstOrDefault(entry => entry.ActiveRoute == route);
        if (qualification == null)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown weekday trend rendering route.");

        return new WeekdayTrendRenderingCapabilities(
            qualification.PathKey,
            qualification.Qualification,
            qualification.SupportsRender,
            qualification.SupportsUpdate,
            qualification.SupportsResetView,
            qualification.SupportsClear,
            qualification.SupportsLifecycleSafety);
    }

    public ChartRenderAdapterResult Render(WeekdayTrendChartRenderRequest request, WeekdayTrendChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        var plan = WeekdayTrendRenderPlanBuilder.Build(request);
        var consumptionContract = request.ConsumptionContract
            ?? WeekdayTrendVNextConsumptionContractBuilder.Build(request, plan);
        var qualifiedPlan = WeekdayTrendVNextConsumptionContractBuilder.AttachMetadata(plan, consumptionContract);
        return _dispatcher.ApplyAsync(
            new WeekdayTrendRenderSurface(request with { ConsumptionContract = consumptionContract }, host),
            qualifiedPlan).AsTask().GetAwaiter().GetResult();
    }

    private void RenderCore(WeekdayTrendChartRenderRequest request, WeekdayTrendChartRenderHost host)
    {
        _updateCoordinator.UpdateChart(request.Result, request.ChartState, host.CartesianChart, host.PolarChart);
    }

    public void Clear(WeekdayTrendChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        ChartSurfaceHelper.ClearCartesian(host.CartesianChart, host.ChartState);
        ChartSurfaceHelper.ClearCartesian(host.PolarChart, host.ChartState);
    }

    public void ResetView(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        if (_updateCoordinator.TryRefitActiveChart())
            return;

        switch (route)
        {
            case WeekdayTrendRenderingRoute.Polar:
                ChartSurfaceHelper.ResetZoom(host.PolarChart);
                return;
            case WeekdayTrendRenderingRoute.Cartesian:
            case WeekdayTrendRenderingRoute.Scatter:
                ChartSurfaceHelper.ResetZoom(host.CartesianChart);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown weekday trend rendering route.");
        }
    }

    public bool HasRenderableContent(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return route switch
        {
            WeekdayTrendRenderingRoute.Polar => ChartSurfaceHelper.HasSeries(host.PolarChart),
            WeekdayTrendRenderingRoute.Cartesian => ChartSurfaceHelper.HasSeries(host.CartesianChart),
            WeekdayTrendRenderingRoute.Scatter => ChartSurfaceHelper.HasSeries(host.CartesianChart),
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown weekday trend rendering route.")
        };
    }
}

public static class WeekdayTrendBackendKey
{
    public const string LiveChartsWpfCartesian = "LiveChartsWpf.Cartesian";
    public const string LiveChartsWpfPolar = "LiveChartsWpf.PolarProjection";
    public const string LiveChartsWpfScatter = "LiveChartsWpf.Scatter";
}

public sealed record WeekdayTrendBackendQualification(
    string BackendKey,
    string PathKey,
    WeekdayTrendRenderingQualification Qualification,
    WeekdayTrendRenderingRoute ActiveRoute,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);

public sealed record WeekdayTrendRenderingCapabilities(
    string PathKey,
    WeekdayTrendRenderingQualification Qualification,
    bool SupportsRender,
    bool SupportsUpdate,
    bool SupportsResetView,
    bool SupportsClear,
    bool SupportsLifecycleSafety);

public enum WeekdayTrendRenderingQualification
{
    Qualified = 0
}

public enum WeekdayTrendRenderingRoute
{
    Cartesian = 0,
    Polar = 1,
    Scatter = 2
}

public static class WeekdayTrendRenderingRouteResolver
{
    public static WeekdayTrendRenderingRoute Resolve(WeekdayTrendChartMode mode)
    {
        return mode switch
        {
            WeekdayTrendChartMode.Polar => WeekdayTrendRenderingRoute.Polar,
            WeekdayTrendChartMode.Scatter => WeekdayTrendRenderingRoute.Scatter,
            _ => WeekdayTrendRenderingRoute.Cartesian
        };
    }
}

public sealed record WeekdayTrendChartRenderRequest(
    WeekdayTrendRenderingRoute Route,
    WeekdayTrendResult Result,
    ChartState ChartState,
    string SelectionDisplayKey = "<none>",
    WeekdayTrendCapabilityContract? CapabilityContract = null,
    VNextUiConsumptionContract? ConsumptionContract = null);

public sealed record WeekdayTrendCapabilityContract : IAnalyticalCapabilityContract
{
    public WeekdayTrendCapabilityContract(
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery)
    {
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(delivery);

        if (programRequest.Kind != ChartProgramKind.WeekdayTrend)
            throw new ArgumentException("WeekdayTrend capability contracts must use a WeekdayTrend program request.", nameof(programRequest));
        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("WeekdayTrend delivery contract must target the WeekdayTrend program kind.", nameof(delivery));

        ProgramRequest = programRequest;
        Capability = capability;
        Delivery = delivery;
    }

    public ChartProgramRequest ProgramRequest { get; }
    public CapabilityRequest Capability { get; }
    public ConsumerDeliveryContract Delivery { get; }

    public static WeekdayTrendCapabilityContract Create()
    {
        var programRequest = ChartProgramRequest.WeekdayTrend();
        return new WeekdayTrendCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ChartProgramDeliveryTargetResolver.CreateDelivery(programRequest.Kind, "WeekdayTrendChart"));
    }
}

public sealed record WeekdayTrendChartRenderHost(
    CartesianChart CartesianChart,
    CartesianChart PolarChart,
    ChartState ChartState);

public sealed record WeekdayTrendRenderSurface(
    WeekdayTrendChartRenderRequest Request,
    WeekdayTrendChartRenderHost Host);

public static class WeekdayTrendVNextConsumptionContractBuilder
{
    public const string ConsumptionContractSignatureKey = "ConsumptionContractSignature";
    public const string SurfaceKindKey = "SurfaceKind";
    public const string SurfaceIdKey = "SurfaceId";

    public static VNextUiConsumptionContract Build(
        WeekdayTrendChartRenderRequest request,
        ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(plan);

        var capabilityContract = request.CapabilityContract ?? WeekdayTrendCapabilityContract.Create();
        var provider = ConsumerProviderContracts.LiveChartsWpf;

        return new VNextUiConsumptionContract(
            capabilityContract.ProgramRequest.Kind,
            capabilityContract.Capability.CapabilityKind,
            capabilityContract.Capability.CompositionKind,
            capabilityContract.Delivery,
            provider,
            plan.SourceSignature,
            ReadRequiredMetadata(plan, ChartRenderPlanMetadataKeys.IntentSignature),
            ReadRequiredMetadata(plan, ChartRenderPlanMetadataKeys.ProvenanceSignature),
            ConsumerSurfaceModel.FromRenderPlan(plan),
            metadata: new Dictionary<string, string>
            {
                ["WeekdayTrend.Route"] = request.Route.ToString(),
                ["WeekdayTrend.Mode"] = request.ChartState.WeekdayTrendChartMode.ToString(),
                ["WeekdayTrend.Selection"] = request.SelectionDisplayKey
            });
    }

    public static ChartRenderPlan AttachMetadata(
        ChartRenderPlan plan,
        VNextUiConsumptionContract consumptionContract)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(consumptionContract);

        var metadata = new Dictionary<string, string>(plan.Metadata)
        {
            [ConsumptionContractSignatureKey] = consumptionContract.Signature,
            [SurfaceKindKey] = consumptionContract.SurfaceModel.Kind.ToString(),
            [SurfaceIdKey] = consumptionContract.SurfaceModel.SurfaceId
        };

        return plan with { Metadata = metadata };
    }

    private static string ReadRequiredMetadata(ChartRenderPlan plan, string key)
    {
        if (plan.Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        throw new InvalidOperationException($"WeekdayTrend render plan is missing required metadata '{key}'.");
    }
}

public static class WeekdayTrendRenderPlanBuilder
{
    public static ChartRenderPlan Build(WeekdayTrendChartRenderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var capabilityContract = request.CapabilityContract ?? WeekdayTrendCapabilityContract.Create();
        var backendKey = ResolveBackendKey(request.Route);
        var sourcePointCount = request.Result.SeriesByDay.Values.Sum(series => series.Points.Count);
        var sourceSignature = $"{request.SelectionDisplayKey}:{request.Route}:{sourcePointCount}:{request.Result.Unit}";
        var metadata = new Dictionary<string, string>
        {
            ["Adapter"] = nameof(WeekdayTrendRenderPlanAdapter),
            [ChartRenderPlanMetadataKeys.BackendKey] = backendKey,
            ["ProgramKind"] = capabilityContract.ProgramRequest.Kind.ToString(),
            ["Route"] = request.Route.ToString(),
            ["Mode"] = request.ChartState.WeekdayTrendChartMode.ToString(),
            ["Selection"] = request.SelectionDisplayKey
        };
        ChartRenderPlanVocabularyMetadata.AddTo(
            metadata,
            capabilityContract.ProgramRequest,
            capabilityContract.Capability,
            capabilityContract.Delivery,
            sourceSignature);

        return new ChartRenderPlan(
            $"{backendKey}:{request.SelectionDisplayKey}:{request.Route}:{request.Result.From:O}:{request.Result.To:O}",
            capabilityContract.ProgramRequest.Kind,
            ChartRenderPlanKind.Cartesian,
            ChartDisplayMode.Regular,
            "Weekday Trend",
            request.Result.From,
            request.Result.To,
            sourceSignature,
            Array.Empty<ChartSeriesPlan>(),
            Array.Empty<ChartHierarchyNodePlan>(),
            new RenderDensityPlan(
                ChartRenderDensityMode.FullFidelity,
                sourcePointCount,
                sourcePointCount,
                request.Result.SeriesByDay.Count),
            new ChartInteractionPlan(
                SupportsZoom: true,
                SupportsPan: true,
                SupportsTooltips: true,
                SupportsSelection: true,
                SupportsViewportRefinement: false),
            metadata);
    }

    private static string ResolveBackendKey(WeekdayTrendRenderingRoute route)
    {
        return route switch
        {
            WeekdayTrendRenderingRoute.Polar => WeekdayTrendBackendKey.LiveChartsWpfPolar,
            WeekdayTrendRenderingRoute.Scatter => WeekdayTrendBackendKey.LiveChartsWpfScatter,
            _ => WeekdayTrendBackendKey.LiveChartsWpfCartesian
        };
    }
}
