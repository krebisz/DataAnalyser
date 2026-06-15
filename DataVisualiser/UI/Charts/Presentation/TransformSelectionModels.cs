using System.ComponentModel;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal interface ITransformInputSelection
{
    MetricNameOption? SelectedMetric { get; }
    MetricNameOption? SelectedSubtype { get; }
}

internal static class TransformInputSelectionResolver
{
    public static IReadOnlyList<MetricSeriesRequest> ResolveRequests(
        IReadOnlyList<ITransformInputSelection> rows,
        string labelPrefix = "Input")
    {
        var inputs = new List<MetricSeriesRequest>();
        for (var index = 0; index < rows.Count; index++)
            inputs.Add(ResolveRequest(rows[index], $"{labelPrefix} {index + 1}"));

        if (inputs.Count == 0)
            throw new InvalidOperationException("Operation Chain requires at least one input.");

        return inputs;
    }

    public static bool TryResolveRequests(
        IReadOnlyList<ITransformInputSelection> rows,
        out IReadOnlyList<MetricSeriesRequest> inputs)
    {
        inputs = Array.Empty<MetricSeriesRequest>();
        if (rows.Count == 0)
            return false;

        var resolved = new List<MetricSeriesRequest>();
        foreach (var row in rows)
        {
            if (row.SelectedMetric is not { } metric || row.SelectedSubtype is not { } subtype)
                return false;

            resolved.Add(new MetricSeriesRequest(metric.Value, subtype.Value, metric.Display, subtype.Display));
        }

        inputs = resolved;
        return true;
    }

    public static IReadOnlyList<TransformInputOption> BuildInputOptions(
        IReadOnlyList<ITransformInputSelection> rows)
    {
        var options = new List<TransformInputOption>();
        for (var index = 0; index < rows.Count; index++)
            options.Add(new TransformInputOption(
                index,
                ResolveDisplayLabel(rows[index], index),
                ResolveEquationLabel(index)));

        return options;
    }

    private static MetricSeriesRequest ResolveRequest(ITransformInputSelection row, string label)
    {
        if (row.SelectedMetric is not { } metric)
            throw new InvalidOperationException($"{label} requires a metric.");

        if (row.SelectedSubtype is not { } subtype)
            throw new InvalidOperationException($"{label} requires a submetric.");

        return new MetricSeriesRequest(metric.Value, subtype.Value, metric.Display, subtype.Display);
    }

    private static string ResolveDisplayLabel(ITransformInputSelection row, int index)
    {
        var metric = row.SelectedMetric?.Display ?? $"Input {index + 1}";
        var subtype = row.SelectedSubtype?.Display ?? string.Empty;

        return string.IsNullOrWhiteSpace(subtype)
            ? metric
            : $"{metric} : {subtype}";
    }

    private static string ResolveEquationLabel(int index) =>
        $"S{ToSubscript(index + 1)}";

    private static string ToSubscript(int value)
    {
        const string digits = "\u2080\u2081\u2082\u2083\u2084\u2085\u2086\u2087\u2088\u2089";
        var result = string.Empty;
        foreach (var character in value.ToString())
            result += digits[character - '0'];

        return result;
    }
}

internal sealed record TransformInputOption(int Index, string Display, string EquationLabel)
{
    public override string ToString() => Display;
}

internal sealed class TransformSelectableResultGridRow : ISelectableGridRow, INotifyPropertyChanged
{
    private bool _isIncluded = true;

    public TransformSelectableResultGridRow(TransformResultGridRow row)
    {
        Timestamp = row.Timestamp;
        Raw = row.Raw;
        Smoothed = row.Smoothed;
        Series = row.Series;
    }

    public string Timestamp { get; }
    public string Raw { get; }
    public string Smoothed { get; }
    public string Series { get; }

    public bool IsIncluded
    {
        get => _isIncluded;
        set
        {
            if (_isIncluded == value)
                return;

            _isIncluded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIncluded)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

internal static class TransformResultRowSelection
{
    public static IReadOnlyList<bool> ResolveIncludedRows(IReadOnlyList<TransformSelectableResultGridRow> rows)
    {
        var includedRows = new bool[rows.Count];
        for (var index = 0; index < rows.Count; index++)
            includedRows[index] = rows[index].IsIncluded;

        return includedRows;
    }

    public static int CountIncluded(IReadOnlyList<bool> includedRows)
    {
        var includedCount = 0;
        for (var index = 0; index < includedRows.Count; index++)
            if (includedRows[index])
                includedCount++;

        return includedCount;
    }
}
