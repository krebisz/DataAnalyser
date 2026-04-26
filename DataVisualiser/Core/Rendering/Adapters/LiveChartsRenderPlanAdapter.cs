using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Adapters;

public sealed record LiveChartsRenderSurface(
    CartesianChart Chart,
    ChartRenderEngine RenderEngine,
    double MinHeight);

public sealed class LiveChartsRenderPlanAdapter : IChartRenderPlanAdapter<LiveChartsRenderSurface>
{
    public const string UnitMetadataKey = "Unit";
    public const string TickIntervalMetadataKey = "TickInterval";
    public const string SeriesModeMetadataKey = "SeriesMode";
    public const string MetricTypeMetadataKey = "MetricType";
    public const string PrimaryMetricTypeMetadataKey = "PrimaryMetricType";
    public const string SecondaryMetricTypeMetadataKey = "SecondaryMetricType";
    public const string PrimarySubtypeMetadataKey = "PrimarySubtype";
    public const string SecondarySubtypeMetadataKey = "SecondarySubtype";
    public const string DisplayPrimaryMetricTypeMetadataKey = "DisplayPrimaryMetricType";
    public const string DisplaySecondaryMetricTypeMetadataKey = "DisplaySecondaryMetricType";
    public const string DisplayPrimarySubtypeMetadataKey = "DisplayPrimarySubtype";
    public const string DisplaySecondarySubtypeMetadataKey = "DisplaySecondarySubtype";
    public const string OperationTypeMetadataKey = "OperationType";
    public const string IsOperationChartMetadataKey = "IsOperationChart";
    public const string IsStackedMetadataKey = "IsStacked";

    public ChartBackendCapabilities Capabilities => ChartBackendCapabilities.LiveChartsWpf;

    public bool CanRender(ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return Capabilities.Supports(plan.PlanKind);
    }

    public ValueTask<ChartRenderAdapterResult> ApplyAsync(
        LiveChartsRenderSurface surface,
        ChartRenderPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        var model = BuildRenderModel(plan, surface.Chart);
        surface.RenderEngine.Render(surface.Chart, model, surface.MinHeight);

        var metadata = new Dictionary<string, string>(plan.Metadata)
        {
            ["Adapter"] = nameof(LiveChartsRenderPlanAdapter),
            ["ProgramKind"] = plan.ProgramKind.ToString()
        };

        return ValueTask.FromResult(new ChartRenderAdapterResult(
            Capabilities.BackendKey,
            plan.Id,
            plan.PlanKind,
            plan.Density.Mode,
            plan.Series.Count + plan.OverlaySeries.Count,
            0,
            plan.Series.Sum(series => series.RenderBuffer.RenderedPointCount) +
            plan.OverlaySeries.Sum(series => series.RenderBuffer.RenderedPointCount),
            metadata));
    }

    private static ChartRenderModel BuildRenderModel(ChartRenderPlan plan, CartesianChart chart)
    {
        var renderedSeries = plan.Series.Select(ToSeriesResult).ToList();
        var primary = renderedSeries.Count > 0 ? renderedSeries[0] : null;
        var secondary = renderedSeries.Count > 1 ? renderedSeries[1] : null;
        var timeline = ResolveTimeline(plan, renderedSeries);
        var useMultiSeriesMode = renderedSeries.Count > 2;

        return new ChartRenderModel
        {
            PrimarySeriesName = primary?.DisplayName ?? "Primary",
            SecondarySeriesName = secondary?.DisplayName ?? string.Empty,
            PrimaryRaw = primary?.RawValues ?? [],
            PrimarySmoothed = primary?.Smoothed ?? [],
            SecondaryRaw = secondary?.RawValues,
            SecondarySmoothed = secondary?.Smoothed,
            PrimaryColor = ColourPalette.Next(chart),
            SecondaryColor = secondary != null ? ColourPalette.Next(chart) : Colors.Red,
            Unit = GetMetadata(plan, UnitMetadataKey),
            Timestamps = timeline,
            TickInterval = ParseEnum(GetMetadata(plan, TickIntervalMetadataKey), TickInterval.Day),
            SeriesMode = ParseEnum(GetMetadata(plan, SeriesModeMetadataKey), ChartSeriesMode.RawAndSmoothed),
            IsStacked = plan.DisplayMode == ChartDisplayMode.Stacked || ParseBool(GetMetadata(plan, IsStackedMetadataKey)),
            MetricType = GetMetadata(plan, MetricTypeMetadataKey),
            PrimaryMetricType = GetMetadata(plan, PrimaryMetricTypeMetadataKey),
            SecondaryMetricType = GetMetadata(plan, SecondaryMetricTypeMetadataKey),
            PrimarySubtype = GetMetadata(plan, PrimarySubtypeMetadataKey),
            SecondarySubtype = GetMetadata(plan, SecondarySubtypeMetadataKey),
            DisplayPrimaryMetricType = GetMetadata(plan, DisplayPrimaryMetricTypeMetadataKey),
            DisplaySecondaryMetricType = GetMetadata(plan, DisplaySecondaryMetricTypeMetadataKey),
            DisplayPrimarySubtype = GetMetadata(plan, DisplayPrimarySubtypeMetadataKey),
            DisplaySecondarySubtype = GetMetadata(plan, DisplaySecondarySubtypeMetadataKey),
            OperationType = GetMetadata(plan, OperationTypeMetadataKey),
            IsOperationChart = ParseBool(GetMetadata(plan, IsOperationChartMetadataKey)),
            Series = useMultiSeriesMode ? renderedSeries : null,
            OverlaySeries = plan.OverlaySeries.Count > 0
                ? plan.OverlaySeries.Select(ToSeriesResult).ToList()
                : null
        };
    }

    private static SeriesResult ToSeriesResult(ChartSeriesPlan series)
    {
        var useRenderBuffer = series.RenderBuffer.Points.Count > 0
            && series.RenderBuffer.RenderedPointCount != series.RawValues.Count;
        var timestamps = useRenderBuffer
            ? series.RenderBuffer.Points.Select(point => point.X).ToList()
            : series.Timeline.ToList();
        var values = useRenderBuffer
            ? series.RenderBuffer.Points.Select(point => point.Y ?? double.NaN).ToList()
            : series.RawValues.ToList();

        return new SeriesResult
        {
            SeriesId = series.Id,
            DisplayName = series.Label,
            Timestamps = timestamps,
            RawValues = values,
            Smoothed = useRenderBuffer ? values : series.SmoothedValues.ToList()
        };
    }

    private static List<DateTime> ResolveTimeline(ChartRenderPlan plan, IReadOnlyList<SeriesResult> series)
    {
        if (plan.Series.Count > 0 && plan.Series[0].RenderBuffer.Points.Count > 0)
            return plan.Series[0].RenderBuffer.Points.Select(point => point.X).ToList();

        if (series.Count > 0 && series[0].Timestamps.Count > 0)
            return series[0].Timestamps;

        return [];
    }

    private static string? GetMetadata(ChartRenderPlan plan, string key)
    {
        return plan.Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }

    private static bool ParseBool(string? value)
    {
        return bool.TryParse(value, out var parsed) && parsed;
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback)
        where TEnum : struct
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }
}
