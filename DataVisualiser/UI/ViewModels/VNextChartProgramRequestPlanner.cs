using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.ViewModels;

internal static class VNextChartProgramRequestPlanner
{
    public static IReadOnlyList<ChartProgramRequest> BuildMainFamilyRequests(ChartState chartState)
    {
        ArgumentNullException.ThrowIfNull(chartState);

        var requests = new List<ChartProgramRequest>
        {
            ChartProgramRequest.MainProgram(
                VNextMainChartIntegrationCoordinator.TranslateDisplayMode(chartState.MainChartDisplayMode))
        };

        if (chartState.IsNormalizedVisible)
            requests.Add(ChartProgramRequest.Normalized());

        if (chartState.IsDiffRatioVisible)
        {
            requests.Add(chartState.IsDiffRatioDifferenceMode
                ? ChartProgramRequest.Difference()
                : ChartProgramRequest.Ratio());
        }

        return requests;
    }

    public static IReadOnlyList<ChartProgramRequest> BuildVisibleChartFamilyRequests(
        ChartState chartState,
        IReadOnlyList<SeriesOperationRequest>? transformOperations = null,
        string transformTitle = "Transform")
    {
        ArgumentNullException.ThrowIfNull(chartState);

        var requests = BuildMainFamilyRequests(chartState).ToList();

        if (chartState.IsDistributionVisible)
            requests.Add(ChartProgramRequest.Distribution());

        if (chartState.IsWeeklyTrendVisible)
            requests.Add(ChartProgramRequest.WeekdayTrend());

        if (chartState.IsTransformPanelVisible)
            requests.Add(ChartProgramRequest.Transform(transformTitle, transformOperations ?? []));

        if (chartState.IsBarPieVisible)
            requests.Add(ChartProgramRequest.BarPie());

        return requests;
    }

    public static EvidenceRuntimePath ResolveRuntimePath(ChartProgramKind programKind)
    {
        return programKind switch
        {
            ChartProgramKind.Normalized => EvidenceRuntimePath.VNextNormalized,
            ChartProgramKind.Difference or ChartProgramKind.Ratio => EvidenceRuntimePath.VNextDiffRatio,
            ChartProgramKind.Distribution => EvidenceRuntimePath.VNextDistribution,
            ChartProgramKind.WeekdayTrend => EvidenceRuntimePath.VNextWeekdayTrend,
            ChartProgramKind.Transform => EvidenceRuntimePath.VNextTransform,
            ChartProgramKind.BarPie => EvidenceRuntimePath.VNextBarPie,
            _ => EvidenceRuntimePath.VNextMain
        };
    }
}
