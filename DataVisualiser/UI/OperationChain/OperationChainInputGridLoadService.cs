using System.Globalization;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.UI.OperationChain;

internal sealed class OperationChainInputGridLoadService
{
    private readonly MetricLoadSnapshotGateway _gateway;
    private readonly OperationChainInputDateRangeResolver? _dateRangeResolver;

    public OperationChainInputGridLoadService(MetricSelectionService metricSelectionService)
        : this(
            new MetricSelectionServiceSeriesLoader(metricSelectionService),
            new OperationChainInputDateRangeResolver((selection, tableName) =>
                metricSelectionService.LoadDateRangeAsync(selection.MetricType, selection.QuerySubtype, tableName)))
    {
    }

    internal OperationChainInputGridLoadService(
        IMetricSeriesLoader loader,
        OperationChainInputDateRangeResolver? dateRangeResolver = null)
    {
        ArgumentNullException.ThrowIfNull(loader);
        _gateway = new MetricLoadSnapshotGateway(loader);
        _dateRangeResolver = dateRangeResolver;
    }

    public async Task<OperationChainInputGridLoadResult> LoadAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one operation-chain input series is required.", nameof(series));

        var request = new MetricSelectionRequest(
            ResolveSelectionMetricType(series),
            series,
            from,
            to,
            resolutionTableName);

        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        return OperationChainInputGridPresenter.Build(snapshot);
    }

    public async Task<OperationChainComputationGridResult> ComputeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        string operationTag,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count < 2)
            throw new ArgumentException("At least two operation-chain input series are required.", nameof(series));
        if (!OperationChainOperationMapper.TryCreateStep(operationTag, series, out var step) || step == null)
            throw new InvalidOperationException($"Operation Chain operation '{operationTag}' is not valid for {series.Count} selected input series.");

        var request = new MetricSelectionRequest(
            ResolveSelectionMetricType(series),
            series,
            from,
            to,
            resolutionTableName);

        if (OperationChainCorrelationMapper.IsCorrelationOperation(operationTag))
            return await ComputeCorrelationAsync(request, operationTag, cancellationToken);

        var executor = new OperationChainExecutor(new SnapshotReasoningEngine(_gateway));
        var result = await executor.ExecuteAsync(
            new OperationChainRequest(request, [step], title: step.Operation.Label),
            cancellationToken);

        return OperationChainResultGridPresenter.Build(result);
    }

    private async Task<OperationChainComputationGridResult> ComputeCorrelationAsync(
        MetricSelectionRequest request,
        string operationTag,
        CancellationToken cancellationToken)
    {
        var snapshot = await _gateway.LoadAsync(request, cancellationToken);
        var aligned = new TimeSeriesAlignmentKernel().Align(snapshot);
        var summary = OperationChainCorrelationMapper.Compute(aligned, operationTag);
        return OperationChainCorrelationGridPresenter.Build(summary, snapshot.Signature);
    }

    public Task<OperationChainInputDateRange?> ResolveDateRangeAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (_dateRangeResolver == null)
            throw new InvalidOperationException("Operation Chain date-range resolution is not configured.");

        return _dateRangeResolver.ResolveAsync(series, resolutionTableName, cancellationToken);
    }

    private static string ResolveSelectionMetricType(IReadOnlyList<MetricSeriesRequest> series)
    {
        var distinct = series
            .Select(item => item.MetricType)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return distinct.Length == 1 ? distinct[0] : "Mixed";
    }
}

internal static class OperationChainInputGridPresenter
{
    public static OperationChainInputGridLoadResult Build(MetricLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var inputs = snapshot.Series
            .Select(series => new OperationChainInputGrid(
                series.Request.DisplayName,
                series.RawData
                    .Where(item => item.Value.HasValue)
                    .OrderBy(item => item.NormalizedTimestamp)
                    .Select(item => new OperationChainInputGridRow(
                        item.NormalizedTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        item.Value!.Value.ToString("F4", CultureInfo.InvariantCulture)))
                    .ToArray()))
            .ToArray();

        return new OperationChainInputGridLoadResult(
            snapshot,
            inputs,
            $"{inputs.Length} input series loaded for Operation Chain.");
    }
}

internal sealed record OperationChainInputGridLoadResult(
    MetricLoadSnapshot Snapshot,
    IReadOnlyList<OperationChainInputGrid> Inputs,
    string Summary);

internal sealed record OperationChainInputGrid(
    string Title,
    IReadOnlyList<OperationChainInputGridRow> Rows);

internal sealed record OperationChainInputGridRow(
    string Timestamp,
    string Value);

internal static class OperationChainResultGridPresenter
{
    public static OperationChainComputationGridResult Build(OperationChainResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var dataset = result.DerivedDatasets.LastOrDefault()
            ?? throw new InvalidOperationException("Operation Chain did not produce a derived dataset.");
        var rows = dataset.Timeline
            .Select((timestamp, index) => new OperationChainResultGridRow(
                timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                FormatValue(dataset.RawValues[index]),
                FormatValue(dataset.SmoothedValues[index])))
            .ToArray();

        return new OperationChainComputationGridResult(
            result,
            dataset.Label,
            rows,
            $"{dataset.Label}: {rows.Length} result points computed.")
        {
            Evidence = $"OperationChain trace: {result.Evidence.TraceSignature}; plan: {result.Evidence.PlanSignature}; contract: {result.Evidence.ContractSignature}"
        };
    }

    private static string FormatValue(double value) =>
        double.IsNaN(value) || double.IsInfinity(value)
            ? string.Empty
            : value.ToString("F4", CultureInfo.InvariantCulture);
}

internal sealed record OperationChainComputationGridResult(
    OperationChainResult? Result,
    string Title,
    IReadOnlyList<OperationChainResultGridRow> Rows,
    string Summary)
{
    public string? Evidence { get; init; }
    public OperationChainCorrelationSummary? Correlation { get; init; }
}

internal sealed record OperationChainResultGridRow(
    string Timestamp,
    string Raw,
    string Smoothed);

internal static class OperationChainCorrelationGridPresenter
{
    public static OperationChainComputationGridResult Build(
        OperationChainCorrelationSummary summary,
        string sourceSignature)
    {
        var rows = new[]
        {
            new OperationChainResultGridRow("Correlation", FormatValue(summary.Correlation), string.Empty),
            new OperationChainResultGridRow("95% CI lower", FormatValue(summary.ConfidenceLower), string.Empty),
            new OperationChainResultGridRow("95% CI upper", FormatValue(summary.ConfidenceUpper), string.Empty),
            new OperationChainResultGridRow("Sample count", summary.SampleCount.ToString(CultureInfo.InvariantCulture), string.Empty)
        };

        return new OperationChainComputationGridResult(
            null,
            summary.Label,
            rows,
            $"{summary.Label}: r={FormatValue(summary.Correlation)}, 95% CI [{FormatValue(summary.ConfidenceLower)}, {FormatValue(summary.ConfidenceUpper)}], n={summary.SampleCount}.")
        {
            Evidence = $"Correlation source: {sourceSignature}; {summary.SourceLabel} -> {summary.TargetLabel}",
            Correlation = summary
        };
    }

    private static string FormatValue(double value) =>
        double.IsNaN(value) || double.IsInfinity(value)
            ? string.Empty
            : value.ToString("F4", CultureInfo.InvariantCulture);
}

internal sealed record OperationChainCorrelationSummary(
    string Label,
    string SourceLabel,
    string TargetLabel,
    double Correlation,
    double ConfidenceLower,
    double ConfidenceUpper,
    int SampleCount);

internal static class OperationChainCorrelationMapper
{
    public static bool IsCorrelationOperation(string operationTag) =>
        string.Equals(operationTag, "Correlation", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(operationTag, "Sum3Correlation", StringComparison.OrdinalIgnoreCase);

    public static OperationChainCorrelationSummary Compute(AlignedSeriesBundle aligned, string operationTag)
    {
        ArgumentNullException.ThrowIfNull(aligned);

        return operationTag switch
        {
            "Correlation" when aligned.Series.Count >= 2 => ComputePair(
                aligned.Series[0].RawValues,
                aligned.Series[1].RawValues,
                aligned.Series[0].Request.DisplayName,
                aligned.Series[1].Request.DisplayName,
                $"{aligned.Series[0].Request.DisplayName} ~ {aligned.Series[1].Request.DisplayName}"),
            "Sum3Correlation" when aligned.Series.Count >= 4 => ComputePair(
                Sum(aligned.Series.Take(3).Select(series => series.RawValues), aligned.Timeline.Count),
                aligned.Series[3].RawValues,
                $"{ResolveShortLabel(aligned.Series[0])} + {ResolveShortLabel(aligned.Series[1])} + {ResolveShortLabel(aligned.Series[2])}",
                aligned.Series[3].Request.DisplayName,
                $"Correlation: ({ResolveShortLabel(aligned.Series[0])} + {ResolveShortLabel(aligned.Series[1])} + {ResolveShortLabel(aligned.Series[2])}) ~ {ResolveShortLabel(aligned.Series[3])}"),
            "Sum3Correlation" => throw new InvalidOperationException("Ternary correlation requires three source inputs plus one comparison input."),
            _ => throw new InvalidOperationException($"Unsupported correlation operation '{operationTag}'.")
        };
    }

    private static OperationChainCorrelationSummary ComputePair(
        IReadOnlyList<double> left,
        IReadOnlyList<double> right,
        string leftLabel,
        string rightLabel,
        string label)
    {
        var pairs = left.Zip(right)
            .Where(pair => double.IsFinite(pair.First) && double.IsFinite(pair.Second))
            .ToArray();
        if (pairs.Length < 2)
            return new OperationChainCorrelationSummary(label, leftLabel, rightLabel, double.NaN, double.NaN, double.NaN, pairs.Length);

        var leftMean = pairs.Average(pair => pair.First);
        var rightMean = pairs.Average(pair => pair.Second);
        var numerator = pairs.Sum(pair => (pair.First - leftMean) * (pair.Second - rightMean));
        var leftVariance = pairs.Sum(pair => Math.Pow(pair.First - leftMean, 2));
        var rightVariance = pairs.Sum(pair => Math.Pow(pair.Second - rightMean, 2));
        var denominator = Math.Sqrt(leftVariance * rightVariance);
        var correlation = denominator <= double.Epsilon ? double.NaN : numerator / denominator;
        var (lower, upper) = CalculateConfidenceInterval(correlation, pairs.Length);

        return new OperationChainCorrelationSummary(label, leftLabel, rightLabel, correlation, lower, upper, pairs.Length);
    }

    private static (double Lower, double Upper) CalculateConfidenceInterval(double correlation, int sampleCount)
    {
        if (!double.IsFinite(correlation) || sampleCount < 4)
            return (double.NaN, double.NaN);

        var bounded = Math.Clamp(correlation, -0.999999d, 0.999999d);
        var z = 0.5d * Math.Log((1d + bounded) / (1d - bounded));
        var delta = 1.96d / Math.Sqrt(sampleCount - 3d);
        return (Math.Tanh(z - delta), Math.Tanh(z + delta));
    }

    private static IReadOnlyList<double> Sum(IEnumerable<IReadOnlyList<double>> seriesValues, int count)
    {
        var inputs = seriesValues.ToArray();
        var result = new double[count];
        for (var index = 0; index < count; index++)
            result[index] = inputs.Sum(values => double.IsNaN(values[index]) ? 0d : values[index]);

        return result;
    }

    private static string ResolveShortLabel(AlignedMetricSeries series) =>
        string.IsNullOrWhiteSpace(series.Request.DisplaySubtype)
            ? series.Request.DisplayName
            : series.Request.DisplaySubtype;
}

internal static class OperationChainOperationMapper
{
    public static bool TryCreateStep(
        string? operationTag,
        IReadOnlyList<MetricSeriesRequest> series,
        out OperationChainStep? step)
    {
        step = null;
        if (string.IsNullOrWhiteSpace(operationTag) || series.Count == 0)
            return false;

        var first = ResolveLabel(series[0]);
        var second = series.Count > 1 ? ResolveLabel(series[1]) : "Input 2";
        var third = series.Count > 2 ? ResolveLabel(series[2]) : "Input 3";
        var request = operationTag switch
        {
            "Correlation" when series.Count >= 2 => SeriesOperationRequest.Identity(0, "operation-chain::correlation", $"{first} ~ {second}"),
            "Log" => SeriesOperationRequest.Logarithm(0, "operation-chain::log", $"Log({first})"),
            "Sqrt" => SeriesOperationRequest.SquareRoot(0, "operation-chain::sqrt", $"Sqrt({first})"),
            "Add" when series.Count >= 2 => SeriesOperationRequest.Sum([0, 1], $"{first} + {second}"),
            "Subtract" when series.Count >= 2 => SeriesOperationRequest.Difference(0, 1, $"{first} - {second}"),
            "Divide" when series.Count >= 2 => SeriesOperationRequest.Ratio(0, 1, $"{first} / {second}"),
            "Sum3" when series.Count >= 3 => SeriesOperationRequest.Sum([0, 1, 2], $"{first} + {second} + {third}"),
            "Sum3Correlation" when series.Count >= 4 => SeriesOperationRequest.Sum([0, 1, 2], $"{first} + {second} + {third}"),
            _ => null
        };

        if (request == null)
            return false;

        step = new OperationChainStep(
            request,
            reversible: request.Kind == SeriesOperationKind.Identity,
            SeriesOperationRules.DefaultLossiness(request.Kind),
            new Dictionary<string, string>
            {
                ["Source"] = "OperationChainWorkbench",
                ["OperationTag"] = operationTag
            });
        return true;
    }

    private static string ResolveLabel(MetricSeriesRequest request) =>
        string.IsNullOrWhiteSpace(request.DisplaySubtype)
            ? request.DisplayName
            : request.DisplaySubtype;
}

internal sealed class SnapshotReasoningEngine(MetricLoadSnapshotGateway gateway) : IReasoningEngine
{
    public Task<MetricLoadSnapshot> LoadAsync(MetricSelectionRequest request, CancellationToken cancellationToken = default) =>
        gateway.LoadAsync(request, cancellationToken);

    public Task<AnalyticalExecutionResult> ExecuteAsync(AnalyticalIntent intent, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<AnalyticalResultSet> ExecuteAsync(AnalyticalIntentSet intentSet, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, ChartProgramRequest request) =>
        throw new NotSupportedException();

    public ChartProgram BuildProgram(MetricLoadSnapshot snapshot, AnalyticalIntent intent) =>
        throw new NotSupportedException();

    public ChartProgram BuildMainProgram(MetricLoadSnapshot snapshot, ChartDisplayMode displayMode = ChartDisplayMode.Regular) =>
        throw new NotSupportedException();

    public ChartProgram BuildNormalizedProgram(MetricLoadSnapshot snapshot) =>
        throw new NotSupportedException();

    public ChartProgram BuildDifferenceProgram(MetricLoadSnapshot snapshot) =>
        throw new NotSupportedException();

    public ChartProgram BuildRatioProgram(MetricLoadSnapshot snapshot) =>
        throw new NotSupportedException();
}

internal sealed class OperationChainInputDateRangeResolver
{
    private readonly Func<MetricSeriesSelection, string, Task<(DateTime MinDate, DateTime MaxDate)?>> _loadDateRange;

    public OperationChainInputDateRangeResolver(
        Func<MetricSeriesSelection, string, Task<(DateTime MinDate, DateTime MaxDate)?>> loadDateRange)
    {
        _loadDateRange = loadDateRange ?? throw new ArgumentNullException(nameof(loadDateRange));
    }

    public async Task<OperationChainInputDateRange?> ResolveAsync(
        IReadOnlyList<MetricSeriesRequest> series,
        string resolutionTableName,
        CancellationToken cancellationToken = default)
    {
        if (series == null || series.Count == 0)
            return null;

        var ranges = new List<(DateTime MinDate, DateTime MaxDate)>();
        foreach (var request in series)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var range = await _loadDateRange(request.ToLegacySelection(), resolutionTableName);
            if (range.HasValue)
                ranges.Add(range.Value);
        }

        return ranges.Count == 0
            ? null
            : new OperationChainInputDateRange(
                ranges.Min(item => item.MinDate),
                ranges.Max(item => item.MaxDate));
    }
}

internal sealed record OperationChainInputDateRange(DateTime From, DateTime To);
