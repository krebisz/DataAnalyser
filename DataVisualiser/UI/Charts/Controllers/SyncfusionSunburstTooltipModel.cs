using System.Collections.Generic;
using System.Windows.Media;

namespace DataVisualiser.UI.Charts.Controllers;

/// <summary>
/// Tooltip model used by <see cref="SyncfusionSunburstChartController"/>.
///
/// This type is intentionally public and simple so WPF bindings can always access it.
/// (Nested/private types have proven brittle in some environments.)
/// </summary>
public sealed class SyncfusionSunburstTooltipModel
{
    public SyncfusionSunburstTooltipModel(
        string periodText,
        string titleText,
        IReadOnlyList<SyncfusionSunburstTooltipLine> lines,
        string? submetricKey = null)
    {
        PeriodText = periodText ?? string.Empty;
        TitleText = titleText ?? string.Empty;
        Lines = lines ?? new List<SyncfusionSunburstTooltipLine>();
        SubmetricKey = submetricKey;
    }

    public string PeriodText { get; }

    /// <summary>
    /// A short title line (e.g. "Weight - Total") or a summary string.
    /// </summary>
    public string TitleText { get; }

    public IReadOnlyList<SyncfusionSunburstTooltipLine> Lines { get; }

    /// <summary>
    /// Optional cache key for tooltip updates. When set, it typically equals the hovered submetric name.
    /// </summary>
    public string? SubmetricKey { get; }

    public bool HasLines => Lines != null && Lines.Count > 0;
}

public sealed class SyncfusionSunburstTooltipLine
{
    public SyncfusionSunburstTooltipLine(string text, Brush brush)
    {
        Text = text ?? string.Empty;
        Brush = brush;
    }

    public string Text { get; }
    public Brush Brush { get; }
}
