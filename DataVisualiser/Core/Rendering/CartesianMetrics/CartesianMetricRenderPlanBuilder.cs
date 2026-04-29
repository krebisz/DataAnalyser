using System.Linq;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Adapters;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public static class CartesianMetricRenderPlanBuilder
{
    public static ChartRenderPlan Build(
        ChartRenderModel model,
        string? title,
        bool isCumulative,
        ChartProgramKind programKind,
        ChartRenderPlanProjector renderPlanProjector,
        ChartProgramRequest? programRequest = null,
        CapabilityRequest? capability = null,
        ConsumerDeliveryContract? delivery = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(renderPlanProjector);

        var timeline = ResolveTimeline(model);
        var sourceSignature = BuildSignature(model, timeline);
        var resolvedProgramRequest = programRequest ?? new ChartProgramRequest(programKind, ResolveDisplayMode(model, isCumulative));
        var resolvedCapability = capability ?? CapabilityRequest.FromProgramRequest(resolvedProgramRequest);
        var resolvedDelivery = delivery ?? ChartProgramDeliveryTargetResolver.CreateDelivery(resolvedProgramRequest.Kind);

        var program = new ChartProgram(
            resolvedProgramRequest.Kind,
            resolvedProgramRequest.DisplayMode,
            title ?? model.MetricType ?? model.PrimarySeriesName,
            timeline.Count > 0 ? timeline[0] : DateTime.MinValue,
            timeline.Count > 0 ? timeline[^1] : DateTime.MinValue,
            timeline,
            BuildSeriesPrograms(model),
            sourceSignature);

        var plan = renderPlanProjector.ProjectCartesian(program);
        return plan with
        {
            Metadata = BuildMetadata(model, resolvedProgramRequest, resolvedCapability, resolvedDelivery, sourceSignature),
            OverlaySeries = BuildOverlaySeries(model, timeline, resolvedProgramRequest.Kind, renderPlanProjector)
        };
    }

    private static ChartDisplayMode ResolveDisplayMode(ChartRenderModel model, bool isCumulative)
    {
        if (model.IsStacked)
            return ChartDisplayMode.Stacked;
        return isCumulative ? ChartDisplayMode.Summed : ChartDisplayMode.Regular;
    }

    private static IReadOnlyList<ChartSeriesProgram> BuildSeriesPrograms(ChartRenderModel model)
    {
        if (model.Series != null && model.Series.Count > 0)
        {
            return model.Series
                .Select(s => new ChartSeriesProgram(s.SeriesId, s.DisplayName, s.RawValues, s.Smoothed ?? s.RawValues))
                .ToArray();
        }

        var programs = new List<ChartSeriesProgram>
        {
            new("primary", model.PrimarySeriesName, model.PrimaryRaw.ToList(), model.PrimarySmoothed.ToList())
        };

        if (model.SecondaryRaw != null && model.SecondaryRaw.Count > 0)
            programs.Add(new ChartSeriesProgram(
                "secondary",
                model.SecondarySeriesName,
                model.SecondaryRaw.ToList(),
                (model.SecondarySmoothed ?? model.SecondaryRaw).ToList()));

        return programs;
    }

    private static IReadOnlyList<ChartSeriesPlan> BuildOverlaySeries(
        ChartRenderModel model,
        IReadOnlyList<DateTime> timeline,
        ChartProgramKind programKind,
        ChartRenderPlanProjector renderPlanProjector)
    {
        if (model.OverlaySeries == null || model.OverlaySeries.Count == 0)
            return Array.Empty<ChartSeriesPlan>();

        var overlayPrograms = model.OverlaySeries
            .Select((s, i) => new ChartSeriesProgram(
                string.IsNullOrWhiteSpace(s.SeriesId) ? $"overlay_{i}" : s.SeriesId,
                s.DisplayName,
                s.RawValues,
                s.Smoothed ?? s.RawValues))
            .ToArray();

        var overlayProgram = new ChartProgram(
            programKind,
            ChartDisplayMode.Regular,
            model.MetricType ?? model.PrimarySeriesName,
            timeline.Count > 0 ? timeline[0] : DateTime.MinValue,
            timeline.Count > 0 ? timeline[^1] : DateTime.MinValue,
            timeline,
            overlayPrograms,
            $"{BuildSignature(model, timeline)}:overlay");

        return renderPlanProjector.ProjectCartesian(overlayProgram).Series;
    }

    private static List<DateTime> ResolveTimeline(ChartRenderModel model)
    {
        if (model.Timestamps.Count > 0)
            return model.Timestamps.ToList();

        if (model.Series == null || model.Series.Count == 0)
            return [];

        return model.Series
            .SelectMany(s => s.Timestamps)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    private static string BuildSignature(ChartRenderModel model, IReadOnlyList<DateTime> timeline)
    {
        var from = timeline.Count > 0 ? timeline[0].ToString("O") : string.Empty;
        var to = timeline.Count > 0 ? timeline[^1].ToString("O") : string.Empty;
        return $"legacy-render:{model.MetricType}:{model.PrimarySeriesName}:{model.SecondarySeriesName}:{from}:{to}:{timeline.Count}";
    }

    private static IReadOnlyDictionary<string, string> BuildMetadata(
        ChartRenderModel model,
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery,
        string sourceSignature)
    {
        var metadata = new Dictionary<string, string>
        {
            ["Projection"] = "ChartRenderModel",
            ["ProgramKind"] = programRequest.Kind.ToString(),
            [LiveChartsRenderPlanAdapter.TickIntervalMetadataKey] = model.TickInterval.ToString(),
            [LiveChartsRenderPlanAdapter.SeriesModeMetadataKey] = model.SeriesMode.ToString(),
            [LiveChartsRenderPlanAdapter.IsStackedMetadataKey] = model.IsStacked.ToString(),
            [LiveChartsRenderPlanAdapter.IsOperationChartMetadataKey] = model.IsOperationChart.ToString()
        };

        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.UnitMetadataKey, model.Unit);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.MetricTypeMetadataKey, model.MetricType);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.PrimaryMetricTypeMetadataKey, model.PrimaryMetricType);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.SecondaryMetricTypeMetadataKey, model.SecondaryMetricType);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.PrimarySubtypeMetadataKey, model.PrimarySubtype);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.SecondarySubtypeMetadataKey, model.SecondarySubtype);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.DisplayPrimaryMetricTypeMetadataKey, model.DisplayPrimaryMetricType);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.DisplaySecondaryMetricTypeMetadataKey, model.DisplaySecondaryMetricType);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.DisplayPrimarySubtypeMetadataKey, model.DisplayPrimarySubtype);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.DisplaySecondarySubtypeMetadataKey, model.DisplaySecondarySubtype);
        AddIfPresent(metadata, LiveChartsRenderPlanAdapter.OperationTypeMetadataKey, model.OperationType);

        ChartRenderPlanVocabularyMetadata.AddTo(
            metadata,
            programRequest,
            capability,
            delivery,
            sourceSignature,
            overlayCount: model.OverlaySeries?.Count ?? 0);

        return metadata;
    }

    private static void AddIfPresent(IDictionary<string, string> metadata, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            metadata[key] = value;
    }
}
