# Canonical Mapping Limitation: Multi-Subtype Support

## Issue Summary

The canonical metric mapping system (`CanonicalMetricMapping`) does not currently distinguish between metric subtypes when mapping to canonical IDs. This creates a fundamental limitation when attempting to use CMS (Canonical Metric Series) data for multi-subtype scenarios.

## Update (2026-01-09)

This limitation has been addressed in the current codebase:

- Canonical mappings are now sourced from a runtime mapping table (`CanonicalMetricMappings`).
- Canonical IDs are subtype-aware and follow `<metric>.<metric subtype>` in lowercase.
- Empty subtypes are represented as `(all)` to preserve the `<metric>.<subtype>` shape.
- The table is auto-seeded from distinct `HealthMetrics` metric/subtype pairs.

This document is retained as historical context; the limitation described below applies to pre-mapping-table implementations.

## Root Cause

### Current Implementation

The `FromLegacyFields()` method in `CanonicalMetricMapping` only considers the `metricType` parameter when resolving canonical IDs:

```csharp
public static string? FromLegacyFields(string metricType, string? metricSubtype = null)
{
    // Only uses metricType, ignores metricSubtype
    var normalizedType = metricType.Trim().ToLowerInvariant();
    return normalizedType switch
    {
        "weight" => "metric.body_weight",
        "com.samsung.shealth.sleep" => "metric.sleep",
        _ => null
    };
}
```

### Impact

When multiple subtypes of the same metric type are selected (e.g., "weight" and "fat_free_mass"), both resolve to the same canonical ID (`"metric.body_weight"`). This causes:

1. **Identical CMS Data**: Both `PrimaryCms` and `SecondaryCms` contain the same aggregated data
2. **Chart Rendering Issues**: Main chart shows (n-1) graphs when n subtypes are selected
3. **Data Loss**: Subtype-specific distinctions are lost in CMS representation

### Example Scenario

- **Primary Subtype**: "weight" → maps to `"metric.body_weight"`
- **Secondary Subtype**: "fat_free_mass" → also maps to `"metric.body_weight"`
- **Result**: Both CMS series contain identical aggregated weight data (171 samples with same values)

## Why This Is a Design Limitation (Historical Context)

This is not a wiring bug, but a **fundamental architectural limitation**:

1. **Canonical ID Structure**: Canonical IDs are designed to represent metric types, not subtypes
2. **Database Schema**: CMS data is aggregated by canonical ID, not by subtype
3. **Mapping Philosophy**: The mapping system assumes one canonical ID per metric type

## Current Workaround

For single-subtype scenarios, the system works correctly because:

- Only one subtype is selected
- One canonical ID is resolved
- CMS data matches the selected subtype

## Future Solutions

To support multi-subtype CMS scenarios, one of the following architectural changes is required:

### Option A: Extend Canonical IDs

Create subtype-specific canonical IDs:

- `"metric.body_weight.weight"`
- `"metric.body_weight.fat_free_mass"`
- `"metric.body_weight.skeletal_mass"`

**Pros**: Clear separation, easy to query
**Cons**: Requires database schema changes, migration of existing data

### Option B: Composite Canonical IDs at Query Time

Generate composite IDs dynamically:

- Query time: `"metric.body_weight" + ":" + subtype`
- Storage: Still use base canonical ID
- Filtering: Apply subtype filter in `CmsDataService.GetCmsByCanonicalIdAsync()`

**Pros**: No schema changes, backward compatible
**Cons**: Requires filtering logic, potential performance impact

### Option C: Subtype Parameter in Mapping

Extend mapping functions to accept and use subtype:

- `FromLegacyFields(metricType, subtype)` returns different IDs based on subtype
- Update `CmsDataService` to filter by subtype when loading

**Pros**: Minimal changes to existing code
**Cons**: Requires extending canonical ID space, database queries need subtype filtering

## Recommended Approach

**Incremental Migration Strategy**:

1. **Phase 1**: Document limitation (this document) ✅
2. **Phase 2**: Extend `CanonicalMetricMapping` to support subtype-aware mapping
3. **Phase 3**: Update `CmsDataService.GetCmsByCanonicalIdAsync()` to filter by subtype
4. **Phase 4**: Add parity validation for multi-subtype scenarios
5. **Phase 5**: Test with 2 subtypes, then extend to 3+

## Related Files

- `DataFileReader/Canonical/CanonicalMetricMapping.cs` - Mapping logic
- `DataVisualiser/Data/Repositories/CmsDataService.cs` - CMS data loading
- `DataVisualiser/Services/MetricSelectionService.cs` - Metric data loading with CMS

## Known Parity Test Failure

### CombinedMetricParityTests.Parity_ShouldPass_WithMismatchedCounts

**Status**: ❌ FAILING  
**Test Location**: `DataVisualiser.Tests/Parity/CombinedMetricParityTests.cs:159`

**Test Scenario**:

- Left series: 10 data points
- Right series: 8 data points (mismatched counts)
- Expected: Both legacy and CMS strategies should align series correctly and produce equivalent results

**Failure Analysis**:
The `CombinedMetricCmsStrategy` does not correctly handle cases where the two input series have different sample counts. The legacy `CombinedMetricStrategy` uses index-based alignment with padding, but the CMS strategy may not be implementing the same alignment logic.

**Impact**:

- Edge case: Only affects scenarios with mismatched data counts
- Main functionality: Other parity tests pass (13/14 passing)
- Production impact: Low - most real-world data has similar counts or can be handled by legacy fallback

**Recommended Fix**:

1. Review `CombinedMetricCmsStrategy.AlignByIndex()` implementation
2. Ensure it matches `CombinedMetricStrategy.AlignByIndex()` behavior
3. Add padding with NaN values for missing data points
4. Re-run parity test to verify fix

## Status

- **Current State**: Subtype-aware canonical mapping is in place via mapping table
- **Multi-Subtype**: No longer blocked by canonical ID collisions (data availability and reachability still apply)
- **Weekly Distribution**: CMS path depends on reachability and CMS data availability
- **Parity Tests**: Verify current results against the latest test run

---

**Last Updated**: 2025-01-04  
**Related Issue**: Main chart rendering (n-1) graphs when n subtypes selected
