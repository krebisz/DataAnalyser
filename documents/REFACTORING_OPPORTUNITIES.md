# High-Impact, Low-Risk, Low-Effort Refactoring Opportunities

## Overview

This document identifies refactoring opportunities that provide maximum value with minimal risk and effort, bringing the project closer to its intended implementation.

**Status**: Some opportunities have been completed. See `PRIORITY_TASKS_COMPLETION_STATUS.md` for details.

---

## ✅ Priority 1: Extract CMS-to-HealthMetricData Conversion Helper

**Status**: ✅ **COMPLETE** (See PRIORITY_TASKS_COMPLETION_STATUS.md)

**Impact**: ⭐⭐⭐ High - Eliminates duplication, improves maintainability  
**Risk**: ⭐ Very Low - Pure extraction, no behavior change  
**Effort**: ⭐ Very Low - ~30 minutes

### Current State

The conversion from `ICanonicalMetricSeries.Samples` to `HealthMetricData` is duplicated in:

- `SingleMetricStrategy.ComputeFromCms()` (lines 92-102)
- `CmsDataService.GetHealthMetricDataFromCmsAsync()` (lines 93-101)

### Proposed Change

**Create**: `DataVisualiser/Helper/CmsConversionHelper.cs`

```csharp
public static class CmsConversionHelper
{
    public static IEnumerable<HealthMetricData> ConvertSamplesToHealthMetricData(
        ICanonicalMetricSeries cms,
        DateTime? from = null,
        DateTime? to = null)
    {
        return cms.Samples
            .Where(s => s.Value.HasValue &&
                       (!from.HasValue || s.Timestamp.DateTime >= from.Value) &&
                       (!to.HasValue || s.Timestamp.DateTime <= to.Value))
            .Select(s => new HealthMetricData
            {
                NormalizedTimestamp = s.Timestamp.DateTime,
                Value = s.Value,
                Unit = cms.Unit.Symbol,
                Provider = cms.Provenance.SourceProvider
            })
            .OrderBy(d => d.NormalizedTimestamp);
    }
}
```

**Benefits**:

- Single source of truth for conversion logic
- Easier to maintain and test
- Reusable across strategies
- Aligns with DRY principle

---

## ✅ Priority 2: Centralize Canonical ID Mapping

**Status**: ✅ **COMPLETE** (See PRIORITY_TASKS_COMPLETION_STATUS.md)

**Impact**: ⭐⭐⭐ High - Enables reuse, reduces duplication risk  
**Risk**: ⭐ Very Low - Static utility, no side effects  
**Effort**: ⭐ Very Low - ~20 minutes

### Current State

The mapping from canonical ID to legacy `MetricType`/`MetricSubtype` exists only in `CmsDataService.MapCanonicalIdToLegacyFields()` (private method).

### Proposed Change

**Create**: `DataFileReader/Canonical/CanonicalMetricMapping.cs`

```csharp
public static class CanonicalMetricMapping
{
    /// <summary>
    /// Maps canonical metric ID to legacy MetricType/MetricSubtype for database queries.
    /// Phase 4: Temporary mapping until CMS is stored directly.
    /// </summary>
    public static (string? MetricType, string? Subtype) ToLegacyFields(string canonicalMetricId)
    {
        return canonicalMetricId switch
        {
            "metric.body_weight" => ("weight", null),
            "metric.sleep" => ("com.samsung.shealth.sleep", null),
            _ => (null, null)
        };
    }

    /// <summary>
    /// Gets human-readable display name for canonical metric ID.
    /// </summary>
    public static string GetDisplayName(string canonicalMetricId)
    {
        return canonicalMetricId switch
        {
            "metric.body_weight" => "Body Weight",
            "metric.sleep" => "Sleep",
            _ => canonicalMetricId.Replace("metric.", "").Replace("_", " ")
        };
    }
}
```

**Update**: `CmsDataService` to use `CanonicalMetricMapping.ToLegacyFields()`

**Benefits**:

- Reusable across components
- Single place to add new metric mappings
- Enables display name generation for UI
- Prepares for future reverse mapping (legacy → canonical)

---

## ✅ Priority 3: Extract Common Computation Logic in SingleMetricStrategy

**Status**: ✅ **COMPLETE** (See PRIORITY_TASKS_COMPLETION_STATUS.md)

**Impact**: ⭐⭐ Medium - Reduces duplication, improves maintainability  
**Risk**: ⭐ Very Low - Internal refactor, same behavior  
**Effort**: ⭐ Low - ~45 minutes

### Current State

`SingleMetricStrategy` has nearly identical computation logic in:

- `Compute()` (legacy path, lines 57-80)
- `ComputeFromCms()` (CMS path, lines 107-128)

Only difference: data source (HealthMetricData vs converted from CMS).

### Proposed Change

**Refactor**: Extract common computation to private method

```csharp
private ChartComputationResult? ComputeFromHealthMetricData(
    IEnumerable<HealthMetricData> orderedData)
{
    if (!orderedData.Any())
        return null;

    var dateRange = _to - _from;
    var tickInterval = MathHelper.DetermineTickInterval(dateRange);
    var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();
    var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
    var intervalIndices = rawTimestamps.Select(ts =>
        MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();
    var smoothedData = MathHelper.CreateSmoothedData(orderedData, _from, _to);
    var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, rawTimestamps);

    Unit = orderedData.FirstOrDefault()?.Unit;

    return new ChartComputationResult
    {
        Timestamps = rawTimestamps,
        IntervalIndices = intervalIndices,
        NormalizedIntervals = normalizedIntervals,
        PrimaryRawValues = orderedData.Select(d =>
            d.Value.HasValue ? (double)d.Value.Value : double.NaN).ToList(),
        PrimarySmoothed = smoothedValues,
        TickInterval = tickInterval,
        DateRange = dateRange,
        Unit = Unit
    };
}
```

**Benefits**:

- Eliminates ~20 lines of duplication
- Single place to update computation logic
- Easier to test computation independently
- Cleaner separation of concerns

---

## ✅ Priority 4: Add CMS Support to MultiMetricStrategy

**Status**: ✅ **COMPLETE** (See PRIORITY_TASKS_COMPLETION_STATUS.md)

**Impact**: ⭐⭐⭐ High - Advances Phase 4, demonstrates pattern  
**Risk**: ⭐⭐ Low - Follows proven SingleMetricStrategy pattern  
**Effort**: ⭐⭐ Low - ~1 hour (copy-paste-adapt)

### Current State

`MultiMetricStrategy` only accepts `IEnumerable<HealthMetricData>` collections. No CMS support.

### Proposed Change

**Add**: Constructor accepting `IReadOnlyList<ICanonicalMetricSeries>`

```csharp
public MultiMetricStrategy(
    IReadOnlyList<ICanonicalMetricSeries> cmsSeries,
    IReadOnlyList<string> labels,
    DateTime from,
    DateTime to)
{
    // Convert each CMS to HealthMetricData using helper
    var series = cmsSeries.Select(cms =>
        CmsConversionHelper.ConvertSamplesToHealthMetricData(cms, from, to)
            .ToList())
        .ToList();

    // Reuse existing constructor
    _series = series;
    _labels = labels;
    _from = from;
    _to = to;
    _unit = cmsSeries.FirstOrDefault()?.Unit.Symbol;
}
```

**Benefits**:

- Extends Phase 4 progress (2 strategies now support CMS)
- Reuses existing computation logic (no changes needed)
- Demonstrates pattern for other strategies
- Low risk (wraps existing functionality)

---

## ✅ Priority 5: Add Semantic Compatibility Check Helper

**Status**: ✅ **COMPLETE** (See PRIORITY_TASKS_COMPLETION_STATUS.md)

**Impact**: ⭐⭐ Medium - Prevents invalid metric combinations  
**Risk**: ⭐ Very Low - Additive only, no breaking changes  
**Effort**: ⭐ Low - ~30 minutes

### Current State

No validation that metrics being compared/combined are semantically compatible (e.g., can't compare Mass to Duration).

### Proposed Change

**Create**: `DataFileReader/Canonical/MetricCompatibilityHelper.cs`

```csharp
public static class MetricCompatibilityHelper
{
    /// <summary>
    /// Checks if two canonical metric IDs have compatible dimensions for computation.
    /// </summary>
    public static bool AreCompatible(string canonicalId1, string canonicalId2)
    {
        // For now, only same dimension is compatible
        // Future: could allow Mass + Mass, Duration + Duration, etc.
        return canonicalId1 == canonicalId2;
    }

    /// <summary>
    /// Gets the dimension for a canonical metric ID.
    /// </summary>
    public static MetricDimension GetDimension(string canonicalMetricId)
    {
        return canonicalMetricId switch
        {
            "metric.body_weight" => MetricDimension.Mass,
            "metric.sleep" => MetricDimension.Duration,
            _ => MetricDimension.Unknown
        };
    }

    /// <summary>
    /// Validates that metrics can be used together in a computation.
    /// </summary>
    public static bool ValidateCompatibility(IEnumerable<string> canonicalIds)
    {
        var ids = canonicalIds.ToList();
        if (ids.Count < 2)
            return true;

        var firstDimension = GetDimension(ids[0]);
        return ids.Skip(1).All(id => GetDimension(id) == firstDimension);
    }
}
```

**Benefits**:

- Prevents semantic errors early
- Foundation for future validation
- Can be used in UI to disable invalid combinations
- Aligns with Project Bible's explicit semantics principle

---

## Summary: Quick Wins

| Priority | Refactoring               | Impact | Risk     | Effort   | Value Score |
| -------- | ------------------------- | ------ | -------- | -------- | ----------- |
| 1        | CMS Conversion Helper     | High   | Very Low | Very Low | ⭐⭐⭐⭐⭐  |
| 2        | Centralize ID Mapping     | High   | Very Low | Very Low | ⭐⭐⭐⭐⭐  |
| 3        | Extract Computation Logic | Medium | Very Low | Low      | ⭐⭐⭐⭐    |
| 4        | MultiMetricStrategy CMS   | High   | Low      | Low      | ⭐⭐⭐⭐    |
| 5        | Compatibility Check       | Medium | Very Low | Low      | ⭐⭐⭐      |

**Recommended Order**: 1 → 2 → 3 → 4 → 5

**Total Estimated Time**: ~3-4 hours for all five

**Expected Benefits**:

- Reduced code duplication
- Improved maintainability
- Progress on Phase 4 (2 strategies with CMS support)
- Foundation for semantic validation
- Better alignment with intended architecture

---

## Implementation Notes

### Dependencies

- Priority 1 should be done first (used by Priority 4)
- Priority 2 is independent and can be done anytime
- Priority 3 is independent
- Priority 4 depends on Priority 1
- Priority 5 is independent

---

## ✅ Completed: Functional Decomposition - Shared Filtering Logic

**Status**: ✅ **COMPLETE** (See PRIORITY_TASKS_COMPLETION_STATUS.md and REFACTORING_IMPACT_ASSESSMENT.md)

**Summary**: Extracted `FilterAndOrderByRange()` method into `StrategyComputationHelper` and updated 5 strategies to use it. Eliminated ~50 lines of duplication and improved maintainability.

**Strategies Updated**:

- CombinedMetricStrategy
- DifferenceStrategy
- RatioStrategy
- WeekdayTrendStrategy
- WeeklyDistributionStrategy

### Testing Strategy

- All changes are additive or internal refactors
- Existing tests should continue to pass
- New helper methods can be unit tested independently
- No breaking changes to public APIs

### Risk Mitigation

- All changes are backward compatible
- Existing code paths remain unchanged
- New functionality is opt-in
- Can be implemented incrementally
