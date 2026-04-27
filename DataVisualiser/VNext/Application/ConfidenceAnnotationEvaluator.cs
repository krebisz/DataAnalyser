using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class ConfidenceAnnotationEvaluator
{
    public ConfidenceAnnotationSet Evaluate(AnalyticalExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var annotations = new List<ConfidenceAnnotation>();
        AddSnapshotAnnotations(result.Snapshot, annotations);
        AddProgramAnnotations(result.Program, annotations);

        return annotations.Count == 0
            ? ConfidenceAnnotationSet.Empty(result.Program.SourceSignature)
            : new ConfidenceAnnotationSet(result.Program.SourceSignature, annotations);
    }

    private static void AddSnapshotAnnotations(
        MetricLoadSnapshot snapshot,
        ICollection<ConfidenceAnnotation> annotations)
    {
        foreach (var series in snapshot.Series)
        {
            if (series.HasRawData)
                continue;

            annotations.Add(new ConfidenceAnnotation(
                ConfidenceAnnotationKind.MissingSeriesData,
                ConfidenceSeverity.Warning,
                ResolveSeriesId(series.Request),
                null,
                "Series contains no raw data.",
                new Dictionary<string, string>
                {
                    ["MetricType"] = series.Request.MetricType,
                    ["QuerySubtype"] = series.Request.QuerySubtype ?? string.Empty
                }));
        }
    }

    private static void AddProgramAnnotations(
        ChartProgram program,
        ICollection<ConfidenceAnnotation> annotations)
    {
        foreach (var series in program.Series)
        {
            AddNonFiniteAnnotations(
                annotations,
                series.Id,
                series.RawValues,
                ConfidenceAnnotationKind.NonFiniteRawValue,
                "Raw value is not finite.");
            AddNonFiniteAnnotations(
                annotations,
                series.Id,
                series.SmoothedValues,
                ConfidenceAnnotationKind.NonFiniteSmoothedValue,
                "Smoothed value is not finite.");
        }
    }

    private static void AddNonFiniteAnnotations(
        ICollection<ConfidenceAnnotation> annotations,
        string seriesId,
        IReadOnlyList<double> values,
        ConfidenceAnnotationKind kind,
        string message)
    {
        for (var i = 0; i < values.Count; i++)
        {
            if (double.IsFinite(values[i]))
                continue;

            annotations.Add(new ConfidenceAnnotation(
                kind,
                ConfidenceSeverity.Critical,
                seriesId,
                i,
                message,
                new Dictionary<string, string> { ["Value"] = values[i].ToString("R") }));
        }
    }

    private static string ResolveSeriesId(MetricSeriesRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.QuerySubtype)
            ? request.QuerySubtype
            : request.MetricType;
    }
}
