using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services.Abstractions;

/// <summary>
///     Unified unit resolution service for chart strategies.
///     Eliminates duplication across all strategies.
/// </summary>
public interface IUnitResolutionService
{
    /// <summary>
    ///     Resolves unit from a single data series (legacy).
    /// </summary>
    string? ResolveUnit(IReadOnlyList<MetricData> data);

    /// <summary>
    ///     Resolves unit from two data series (legacy).
    ///     Returns left unit if both match, otherwise left unit or right unit if left is null.
    /// </summary>
    string? ResolveUnit(IReadOnlyList<MetricData> left, IReadOnlyList<MetricData> right);

    /// <summary>
    ///     Resolves unit from a single CMS series.
    /// </summary>
    string? ResolveUnit(ICanonicalMetricSeries cms);

    /// <summary>
    ///     Resolves unit from two CMS series.
    ///     Returns left unit if both match, otherwise left unit or right unit if left is null.
    /// </summary>
    string? ResolveUnit(ICanonicalMetricSeries left, ICanonicalMetricSeries right);

    /// <summary>
    ///     Resolves compound unit for ratio operations (e.g., "kg/kg").
    ///     Returns null if either unit is null or empty.
    /// </summary>
    string? ResolveRatioUnit(IReadOnlyList<MetricData> left, IReadOnlyList<MetricData> right);
}