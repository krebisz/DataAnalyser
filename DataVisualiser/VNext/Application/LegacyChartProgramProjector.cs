using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class LegacyChartProgramProjector
{
    public ChartDataContext ProjectToChartContext(ChartProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);

        var primarySeries = program.Series.Count > 0 ? program.Series[0] : null;
        var secondarySeries = program.Series.Count > 1 ? program.Series[1] : null;

        return new ChartDataContext
        {
            Data1 = primarySeries == null ? Array.Empty<MetricData>() : BuildMetricData(program.Timeline, primarySeries.RawValues),
            Data2 = secondarySeries == null ? Array.Empty<MetricData>() : BuildMetricData(program.Timeline, secondarySeries.RawValues),
            DisplayName1 = primarySeries?.Label ?? string.Empty,
            DisplayName2 = secondarySeries?.Label ?? string.Empty,
            ActualSeriesCount = program.Series.Count,
            MetricType = program.Title,
            PrimaryMetricType = program.Title,
            SecondaryMetricType = program.Series.Count > 1 ? program.Title : null,
            PrimarySubtype = primarySeries?.Id,
            SecondarySubtype = secondarySeries?.Id,
            DisplayPrimaryMetricType = program.Title,
            DisplaySecondaryMetricType = program.Series.Count > 1 ? program.Title : null,
            DisplayPrimarySubtype = primarySeries?.Label,
            DisplaySecondarySubtype = secondarySeries?.Label,
            LoadRequestSignature = program.SourceSignature,
            From = program.From,
            To = program.To
        };
    }

    private static IReadOnlyList<MetricData> BuildMetricData(IReadOnlyList<DateTime> timeline, IReadOnlyList<double> values)
    {
        var result = new List<MetricData>(Math.Min(timeline.Count, values.Count));

        for (var index = 0; index < Math.Min(timeline.Count, values.Count); index++)
        {
            result.Add(new MetricData
            {
                NormalizedTimestamp = timeline[index],
                Value = double.IsNaN(values[index]) ? null : Convert.ToDecimal(values[index])
            });
        }

        return result;
    }
}
