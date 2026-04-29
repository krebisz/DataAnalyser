using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost.Evidence;

internal static class EvidenceExportConsumerContractBuilder
{
    internal static IReadOnlyList<ConsumerDeliveryEvidenceSnapshot> Build(
        ChartState chartState,
        MetricState metricState)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(metricState);

        if (!TryCreateSelection(metricState, out var selection))
            return Array.Empty<ConsumerDeliveryEvidenceSnapshot>();

        return ResolveProgramKinds(chartState)
            .Select(kind => ConsumerDeliveryEvidence.ForExport(selection, kind))
            .Select(ToSnapshot)
            .ToArray();
    }

    private static bool TryCreateSelection(MetricState metricState, out MetricSelectionRequest selection)
    {
        selection = null!;

        if (string.IsNullOrWhiteSpace(metricState.SelectedMetricType) ||
            string.IsNullOrWhiteSpace(metricState.ResolutionTableName) ||
            metricState.FromDate == null ||
            metricState.ToDate == null)
        {
            return false;
        }

        selection = new MetricSelectionRequest(
            metricState.SelectedMetricType,
            metricState.SelectedSeries
                .Select(MetricSeriesRequest.FromLegacy)
                .ToArray(),
            metricState.FromDate.Value,
            metricState.ToDate.Value,
            metricState.ResolutionTableName);
        return true;
    }

    private static IReadOnlyList<ChartProgramKind> ResolveProgramKinds(ChartState chartState)
    {
        var kinds = new List<ChartProgramKind>();

        if (chartState.LastLoadRuntime?.ProgramKind != null)
            kinds.Add(chartState.LastLoadRuntime.ProgramKind.Value);

        kinds.AddRange(chartState.FamilyLoadRuntimes.Keys);
        kinds.AddRange(chartState.RenderPlanDiagnostics.Keys);

        if (kinds.Count == 0)
            kinds.Add(ChartProgramKind.Main);

        return kinds
            .Distinct()
            .OrderBy(kind => kind)
            .ToArray();
    }

    private static ConsumerDeliveryEvidenceSnapshot ToSnapshot(ConsumerDeliveryEvidence evidence)
    {
        return new ConsumerDeliveryEvidenceSnapshot
        {
            ProgramKind = evidence.ProgramKind.ToString(),
            SourceSignature = evidence.SourceSignature,
            IntentSignature = evidence.IntentSignature,
            ConsumerKind = evidence.ConsumerKind.ToString(),
            DeliveryTarget = evidence.DeliveryTarget,
            RequiresRenderPlan = evidence.RequiresRenderPlan,
            CapabilityKind = evidence.CapabilityKind.ToString(),
            CompositionKind = evidence.CompositionKind.ToString(),
            ProvenanceSignature = evidence.ProvenanceSignature,
            ProviderKey = evidence.ProviderKey,
            ProviderDisplayName = evidence.ProviderDisplayName,
            ProviderSignature = evidence.ProviderSignature,
            Metadata = evidence.Metadata
        };
    }
}
