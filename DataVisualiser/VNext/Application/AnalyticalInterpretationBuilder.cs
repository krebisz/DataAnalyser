using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class AnalyticalInterpretationBuilder
{
    private readonly ConfidenceAnnotationEvaluator _confidenceEvaluator;
    private readonly InterpretiveOverlayPlanner _overlayPlanner;

    public AnalyticalInterpretationBuilder(
        ConfidenceAnnotationEvaluator? confidenceEvaluator = null,
        InterpretiveOverlayPlanner? overlayPlanner = null)
    {
        _confidenceEvaluator = confidenceEvaluator ?? new ConfidenceAnnotationEvaluator();
        _overlayPlanner = overlayPlanner ?? new InterpretiveOverlayPlanner();
    }

    public AnalyticalInterpretationResult Build(
        AnalyticalExecutionResult execution,
        bool includeAverageLines = false,
        bool includeMedianLines = false)
    {
        return Build(
            execution,
            new AnalyticalInterpretationOptions(
                includeAverageLines,
                includeMedianLines));
    }

    public AnalyticalInterpretationResult Build(
        AnalyticalExecutionResult execution,
        AnalyticalInterpretationOptions? options)
    {
        ArgumentNullException.ThrowIfNull(execution);
        options ??= AnalyticalInterpretationOptions.None;

        var confidence = _confidenceEvaluator.Evaluate(execution);
        var overlayProgram = options.ExcludeCriticalConfidenceSeriesFromOverlays
            ? ExcludeCriticalSeries(execution.Program, confidence)
            : execution.Program;
        var overlays = new List<OverlayPlan>();
        if (options.IncludeAverageLines)
            overlays.AddRange(_overlayPlanner.PlanAverageLines(overlayProgram));
        if (options.IncludeMedianLines)
            overlays.AddRange(_overlayPlanner.PlanMedianLines(overlayProgram));

        return new AnalyticalInterpretationResult(
            execution,
            confidence,
            overlays);
    }

    public AnalyticalInterpretationSetResult BuildSet(
        AnalyticalResultSet executionSet,
        bool includeAverageLines = false,
        bool includeMedianLines = false)
    {
        return BuildSet(
            executionSet,
            new AnalyticalInterpretationOptions(
                includeAverageLines,
                includeMedianLines));
    }

    public AnalyticalInterpretationSetResult BuildSet(
        AnalyticalResultSet executionSet,
        AnalyticalInterpretationOptions? options)
    {
        ArgumentNullException.ThrowIfNull(executionSet);
        options ??= AnalyticalInterpretationOptions.None;

        var interpretations = executionSet.Results
            .Select(result => Build(result, options))
            .ToArray();

        return new AnalyticalInterpretationSetResult(executionSet, interpretations);
    }

    private static ChartProgram ExcludeCriticalSeries(
        ChartProgram program,
        ConfidenceAnnotationSet confidence)
    {
        var criticalSeries = confidence.Annotations
            .Where(annotation => annotation.Severity == ConfidenceSeverity.Critical)
            .Select(annotation => annotation.SeriesId)
            .ToHashSet(StringComparer.Ordinal);

        if (criticalSeries.Count == 0)
            return program;

        return program with
        {
            Series = program.Series
                .Where(series => !criticalSeries.Contains(series.Id))
                .ToArray()
        };
    }
}
