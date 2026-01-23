using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Controls;

public sealed class MetricSeriesSelectionCache
{
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _dataCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ICanonicalMetricSeries?> _cmsCache = new(StringComparer.OrdinalIgnoreCase);

    public void Clear()
    {
        _dataCache.Clear();
        _cmsCache.Clear();
    }

    public bool TryGetData(string key, out IReadOnlyList<MetricData>? data)
    {
        if (_dataCache.TryGetValue(key, out var cached))
        {
            data = cached;
            return true;
        }

        data = null;
        return false;
    }

    public bool TryGetDataWithCms(string key, out IReadOnlyList<MetricData>? data, out ICanonicalMetricSeries? cms)
    {
        if (_dataCache.TryGetValue(key, out var cached))
        {
            data = cached;
            _cmsCache.TryGetValue(key, out cms);
            return true;
        }

        data = null;
        cms = null;
        return false;
    }

    public void SetData(string key, IReadOnlyList<MetricData> data)
    {
        _dataCache[key] = data;
    }

    public void SetDataWithCms(string key, IReadOnlyList<MetricData> data, ICanonicalMetricSeries? cms)
    {
        _dataCache[key] = data;
        _cmsCache[key] = cms;
    }

    public static string BuildCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
    }

    public static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (!string.Equals(selection.MetricType, metricType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedSubtype = string.IsNullOrWhiteSpace(subtype) || subtype == "(All)" ? null : subtype;
        var selectionSubtype = selection.QuerySubtype;

        return string.Equals(selectionSubtype ?? string.Empty, normalizedSubtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public static ComboBoxItem BuildSeriesComboItem(MetricSeriesSelection selection)
    {
        return new ComboBoxItem
        {
            Content = selection.DisplayName,
            Tag = selection
        };
    }

    public static ComboBoxItem? FindSeriesComboItem(ComboBox combo, MetricSeriesSelection selection)
    {
        return combo.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag is MetricSeriesSelection candidate && string.Equals(candidate.DisplayKey, selection.DisplayKey, StringComparison.OrdinalIgnoreCase));
    }

    public static MetricSeriesSelection? GetSeriesSelectionFromCombo(ComboBox combo)
    {
        if (combo.SelectedItem is ComboBoxItem item && item.Tag is MetricSeriesSelection selection)
            return selection;

        return combo.SelectedItem as MetricSeriesSelection;
    }

    public static MetricSeriesSelection? ResolveSelection(bool allowComboSelection, ComboBox? combo, MetricSeriesSelection? stateSelection, Func<MetricSeriesSelection?> fallbackSelection)
    {
        if (allowComboSelection && combo != null)
        {
            var selection = GetSeriesSelectionFromCombo(combo);
            if (selection != null)
                return selection;
        }

        if (stateSelection != null)
            return stateSelection;

        return fallbackSelection();
    }
}
