using System.Diagnostics;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.ViewModels;

internal sealed class VNextMetricLoadRouter
{
    private readonly ChartState _chartState;
    private readonly VNextMainChartIntegrationCoordinator _vnextMainChartIntegrationCoordinator;

    public VNextMetricLoadRouter(
        ChartState chartState,
        VNextMainChartIntegrationCoordinator vnextMainChartIntegrationCoordinator)
    {
        _chartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        _vnextMainChartIntegrationCoordinator = vnextMainChartIntegrationCoordinator ?? throw new ArgumentNullException(nameof(vnextMainChartIntegrationCoordinator));
    }

    public async Task<VNextMetricLoadRoutingResult> TryLoadAsync(
        MetricLoadRequest request,
        Stopwatch totalStopwatch)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(totalStopwatch);

        if (!VNextChartRoutePolicy.ShouldUseMainFamilyPath(_chartState))
            return VNextMetricLoadRoutingResult.NotRouted();

        var vnextStopwatch = Stopwatch.StartNew();
        var vnextResults = await _vnextMainChartIntegrationCoordinator.LoadProgramsAsync(
            request,
            VNextChartProgramRequestPlanner.BuildVisibleChartFamilyRequests(_chartState));
        vnextStopwatch.Stop();

        var vnextResult = vnextResults.FirstOrDefault(result => result.ProgramKind == ChartProgramKind.Main) ??
                          vnextResults.First();
        _chartState.RecordPerformanceTiming(
            "MetricLoad",
            "VNextMainFamilyLoad",
            vnextStopwatch.ElapsedMilliseconds,
            EvidenceRuntimePath.VNextMain,
            vnextResult.Success ? null : vnextResult.FailureReason);

        if (vnextResult.Success && vnextResult.Program != null && vnextResult.Snapshot != null && vnextResult.ProjectedContext != null)
        {
            RecordVNextFamilyRuntimes(vnextResults, request.Signature);
            var supportsOnlyMainChart = VNextChartRoutePolicy.SupportsOnlyMainChart(_chartState);
            // Compatibility assignment remains until main-chart consumers no longer require ChartDataContext.
            _chartState.LastContext = vnextResult.ProjectedContext;
            _chartState.LastLoadRuntime = new LoadRuntimeState(
                EvidenceRuntimePath.VNextMain,
                vnextResult.RequestSignature ?? request.Signature,
                vnextResult.SnapshotSignature,
                vnextResult.ProgramKind,
                vnextResult.ProgramSourceSignature,
                vnextResult.ProjectedContextSignature,
                null,
                supportsOnlyMainChart);
            totalStopwatch.Stop();
            _chartState.RecordPerformanceTiming(
                "MetricLoad",
                "Total",
                totalStopwatch.ElapsedMilliseconds,
                EvidenceRuntimePath.VNextMain,
                "VNext success");
            return VNextMetricLoadRoutingResult.Loaded();
        }

        _chartState.LastLoadRuntime = new LoadRuntimeState(
            EvidenceRuntimePath.VNextMain,
            vnextResult.RequestSignature ?? request.Signature,
            vnextResult.SnapshotSignature,
            vnextResult.ProgramKind,
            vnextResult.ProgramSourceSignature,
            vnextResult.ProjectedContextSignature,
            vnextResult.FailureReason,
            false);

        return VNextMetricLoadRoutingResult.FallbackRequired();
    }

    private void RecordVNextFamilyRuntimes(
        IReadOnlyList<VNextMainChartLoadResult> results,
        string fallbackRequestSignature)
    {
        foreach (var result in results.Where(result => result.Success && result.ProgramKind.HasValue))
        {
            var programKind = result.ProgramKind!.Value;
            if (programKind == ChartProgramKind.Main)
                continue;

            _chartState.SetFamilyRuntime(
                programKind,
                new LoadRuntimeState(
                    VNextChartProgramRequestPlanner.ResolveRuntimePath(programKind),
                    result.RequestSignature ?? fallbackRequestSignature,
                    result.SnapshotSignature,
                    result.ProgramKind,
                    result.ProgramSourceSignature,
                    result.ProjectedContextSignature,
                    null,
                    false));
        }
    }
}

internal sealed record VNextMetricLoadRoutingResult(bool WasRouted, bool Succeeded)
{
    public static VNextMetricLoadRoutingResult NotRouted() => new(false, false);

    public static VNextMetricLoadRoutingResult Loaded() => new(true, true);

    public static VNextMetricLoadRoutingResult FallbackRequired() => new(true, false);
}
