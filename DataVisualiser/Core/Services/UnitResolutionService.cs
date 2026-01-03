using DataFileReader.Canonical;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Implementation of IUnitResolutionService.
///     Provides unified unit resolution logic with consistent behavior across all strategies.
/// </summary>
public sealed class UnitResolutionService : IUnitResolutionService
{
    public string? ResolveUnit(IReadOnlyList<HealthMetricData> data)
    {
        if (data == null || data.Count == 0)
            return null;

        return data.FirstOrDefault()?.
            Unit;
    }

    public string? ResolveUnit(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right)
    {
        if (left == null || left.Count == 0)
            return right?.FirstOrDefault()?.
                Unit;

        if (right == null || right.Count == 0)
            return left.FirstOrDefault()?.
                Unit;

        var leftUnit = left.FirstOrDefault()?.
            Unit;
        var rightUnit = right.FirstOrDefault()?.
            Unit;

        // If both units match, return that unit
        if (leftUnit == rightUnit)
            return leftUnit;

        // Otherwise, prefer left unit, fallback to right
        return leftUnit ?? rightUnit;
    }

    public string? ResolveUnit(ICanonicalMetricSeries cms)
    {
        if (cms == null)
            return null;

        return cms.Unit?.Symbol;
    }

    public string? ResolveUnit(ICanonicalMetricSeries left, ICanonicalMetricSeries right)
    {
        if (left == null)
            return right?.Unit?.Symbol;

        if (right == null)
            return left.Unit?.Symbol;

        var leftUnit = left.Unit?.Symbol;
        var rightUnit = right.Unit?.Symbol;

        // If left unit is empty, use right unit
        if (string.IsNullOrWhiteSpace(leftUnit))
            return string.IsNullOrWhiteSpace(rightUnit) ? null : rightUnit;

        // Prefer left unit (authoritative, even if mismatch)
        return leftUnit;
    }

    public string? ResolveRatioUnit(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right)
    {
        if (left == null || left.Count == 0 || right == null || right.Count == 0)
            return null;

        var unitLeft = left.FirstOrDefault()?.
            Unit;
        var unitRight = right.FirstOrDefault()?.
            Unit;

        // Return compound unit only if both are non-empty
        if (!string.IsNullOrEmpty(unitLeft) && !string.IsNullOrEmpty(unitRight))
            return $"{unitLeft}/{unitRight}";

        return null;
    }
}
