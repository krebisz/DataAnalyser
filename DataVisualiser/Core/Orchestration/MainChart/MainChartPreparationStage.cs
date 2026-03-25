using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed class MainChartPreparationStage : IMainChartPreparationStage
{
    private readonly string? _connectionString;
    private readonly MetricSelectionService? _metricSelectionService;

    public MainChartPreparationStage(MetricSelectionService? metricSelectionService, string? connectionString)
    {
        _metricSelectionService = metricSelectionService;
        _connectionString = connectionString;
    }

    public async Task<MainChartPreparedData> PrepareAsync(MainChartRenderRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.IsStacked && request.SelectedSeries != null)
        {
            var stackedSelections = request.SelectedSeries
                .Where(selection => selection.QuerySubtype != null)
                .ToList();

            if (stackedSelections.Count >= 2)
            {
                var (stackedSeries, stackedLabels, stackedCmsSeries) =
                    await BuildSeriesFromSelectionsAsync(request.Context, stackedSelections, request.ResolutionTableName);

                if (stackedSeries.Count >= 2)
                {
                    var stackedContext = BuildWorkingContext(request.Context, stackedCmsSeries, stackedSeries.Count);
                    return new MainChartPreparedData(
                        stackedContext,
                        stackedSeries,
                        stackedLabels,
                        request.IsStacked,
                        request.IsCumulative,
                        request.OverlaySeries);
                }
            }
        }

        var (series, labels) = BuildSeriesAndLabels(request.Context, request.AdditionalSeries, request.AdditionalLabels);
        var cmsSeries = BuildInitialCmsSeries(request.Context);

        await LoadAdditionalSubtypesAsync(
            series,
            labels,
            cmsSeries,
            request.Context.MetricType,
            request.Context.From,
            request.Context.To,
            request.SelectedSeries,
            request.ResolutionTableName);

        var workingContext = series.Count > 2
            ? BuildWorkingContext(request.Context, cmsSeries, series.Count)
            : request.Context;

        return new MainChartPreparedData(
            workingContext,
            series,
            labels,
            request.IsStacked,
            request.IsCumulative,
            request.OverlaySeries);
    }

    private async Task<(List<IEnumerable<MetricData>> Series, List<string> Labels, List<ICanonicalMetricSeries> CmsSeries)> BuildSeriesFromSelectionsAsync(
        ChartDataContext context,
        IReadOnlyList<MetricSeriesSelection> selections,
        string? resolutionTableName)
    {
        var series = new List<IEnumerable<MetricData>>();
        var labels = new List<string>();
        var cmsSeries = new List<ICanonicalMetricSeries>();

        if (selections.Count == 0)
            return (series, labels, cmsSeries);

        var tableName = resolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var metricSelectionService = ResolveMetricSelectionService();

        foreach (var selection in selections)
        {
            var data = ResolveContextSeries(context, selection);
            var cms = ResolveContextCmsSeries(context, selection);

            if (data == null)
            {
                if (selection.QuerySubtype == null || string.IsNullOrWhiteSpace(selection.MetricType) || metricSelectionService == null)
                    continue;

                var loaded = await metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, context.From, context.To, tableName);
                data = loaded.PrimaryLegacy.ToList();
                cms = loaded.PrimaryCms;
            }

            if (data == null || !data.Any())
                continue;

            series.Add(data);
            labels.Add(selection.DisplayName);
            if (cms != null)
                cmsSeries.Add(cms);
        }

        return (series, labels, cmsSeries);
    }

    private async Task LoadAdditionalSubtypesAsync(
        List<IEnumerable<MetricData>> series,
        List<string> labels,
        List<ICanonicalMetricSeries> cmsSeries,
        string? metricType,
        DateTime from,
        DateTime to,
        IReadOnlyList<MetricSeriesSelection>? selectedSeries,
        string? resolutionTableName)
    {
        if (selectedSeries == null || selectedSeries.Count <= 2)
            return;

        var metricSelectionService = ResolveMetricSelectionService();
        if (metricSelectionService == null)
            return;

        var tableName = resolutionTableName ?? DataAccessDefaults.DefaultTableName;

        for (var index = 2; index < selectedSeries.Count; index++)
        {
            var selection = selectedSeries[index];
            if (string.IsNullOrWhiteSpace(selection.MetricType))
                continue;

            try
            {
                var (primaryCms, _, primary, _) =
                    await metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);

                if (!primary.Any())
                    continue;

                series.Add(primary);
                labels.Add(selection.DisplayName);
                if (primaryCms != null)
                    cmsSeries.Add(primaryCms);
            }
            catch
            {
                // Preserve existing tolerant behavior for additional series loading.
            }
        }
    }

    private MetricSelectionService? ResolveMetricSelectionService()
    {
        if (_metricSelectionService != null)
            return _metricSelectionService;

        return string.IsNullOrWhiteSpace(_connectionString)
            ? null
            : new MetricSelectionService(_connectionString);
    }

    private static (List<IEnumerable<MetricData>> Series, List<string> Labels) BuildSeriesAndLabels(
        ChartDataContext context,
        IReadOnlyList<IEnumerable<MetricData>>? additionalSeries,
        IReadOnlyList<string>? additionalLabels)
    {
        var series = new List<IEnumerable<MetricData>>
        {
            context.Data1 ?? Array.Empty<MetricData>()
        };

        var labels = new List<string>
        {
            context.DisplayName1 ?? string.Empty
        };

        if (context.Data2 != null && context.Data2.Any())
        {
            series.Add(context.Data2);
            labels.Add(context.DisplayName2 ?? string.Empty);
        }

        if (additionalSeries != null && additionalLabels != null)
        {
            for (var index = 0; index < Math.Min(additionalSeries.Count, additionalLabels.Count); index++)
            {
                if (additionalSeries[index] == null || !additionalSeries[index].Any())
                    continue;

                series.Add(additionalSeries[index]);
                labels.Add(additionalLabels[index]);
            }
        }

        return (series, labels);
    }

    private static List<ICanonicalMetricSeries> BuildInitialCmsSeries(ChartDataContext context)
    {
        if (context.CmsSeries != null && context.CmsSeries.Count > 0)
            return context.CmsSeries.ToList();

        var cmsSeries = new List<ICanonicalMetricSeries>(2);

        if (context.PrimaryCms is ICanonicalMetricSeries primaryCms)
            cmsSeries.Add(primaryCms);

        if (context.SecondaryCms is ICanonicalMetricSeries secondaryCms)
            cmsSeries.Add(secondaryCms);

        return cmsSeries;
    }

    private static ChartDataContext BuildWorkingContext(
        ChartDataContext context,
        IReadOnlyList<ICanonicalMetricSeries>? cmsSeries,
        int seriesCount)
    {
        return new ChartDataContext
        {
            PrimaryCms = context.PrimaryCms,
            SecondaryCms = context.SecondaryCms,
            CmsSeries = cmsSeries != null && cmsSeries.Count == seriesCount ? cmsSeries.ToList() : null,
            Data1 = context.Data1,
            Data2 = context.Data2,
            Timestamps = context.Timestamps,
            RawValues1 = context.RawValues1,
            RawValues2 = context.RawValues2,
            SmoothedValues1 = context.SmoothedValues1,
            SmoothedValues2 = context.SmoothedValues2,
            DifferenceValues = context.DifferenceValues,
            RatioValues = context.RatioValues,
            NormalizedValues1 = context.NormalizedValues1,
            NormalizedValues2 = context.NormalizedValues2,
            DisplayName1 = context.DisplayName1,
            DisplayName2 = context.DisplayName2,
            ActualSeriesCount = seriesCount,
            MetricType = context.MetricType,
            PrimaryMetricType = context.PrimaryMetricType,
            SecondaryMetricType = context.SecondaryMetricType,
            PrimarySubtype = context.PrimarySubtype,
            SecondarySubtype = context.SecondarySubtype,
            DisplayPrimaryMetricType = context.DisplayPrimaryMetricType,
            DisplaySecondaryMetricType = context.DisplaySecondaryMetricType,
            DisplayPrimarySubtype = context.DisplayPrimarySubtype,
            DisplaySecondarySubtype = context.DisplaySecondarySubtype,
            From = context.From,
            To = context.To
        };
    }

    private static IEnumerable<MetricData>? ResolveContextSeries(ChartDataContext context, MetricSeriesSelection selection)
    {
        if (IsMatchingSelection(selection, context.PrimaryMetricType ?? context.MetricType, context.PrimarySubtype))
            return context.Data1;

        if (IsMatchingSelection(selection, context.SecondaryMetricType, context.SecondarySubtype))
            return context.Data2;

        return null;
    }

    private static ICanonicalMetricSeries? ResolveContextCmsSeries(ChartDataContext context, MetricSeriesSelection selection)
    {
        if (IsMatchingSelection(selection, context.PrimaryMetricType ?? context.MetricType, context.PrimarySubtype))
            return context.PrimaryCms as ICanonicalMetricSeries;

        if (IsMatchingSelection(selection, context.SecondaryMetricType, context.SecondarySubtype))
            return context.SecondaryCms as ICanonicalMetricSeries;

        return null;
    }

    private static bool IsMatchingSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(metricType) || string.IsNullOrWhiteSpace(selection.MetricType))
            return false;

        if (!string.Equals(metricType, selection.MetricType, StringComparison.OrdinalIgnoreCase))
            return false;

        var selectionSubtype = selection.Subtype ?? string.Empty;
        var contextSubtype = subtype ?? string.Empty;

        return string.Equals(selectionSubtype, contextSubtype, StringComparison.OrdinalIgnoreCase);
    }
}
