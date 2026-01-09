# CMS Migration Status and Implementation Plan

**Last Updated:** Based on analysis of current codebase state  
**Purpose:** Guide for implementing remaining CMS strategy migrations  
**Target Audience:** ChatGPT Codex / AI coding assistants

---

## Executive Summary

**Current Status:**
- ✅ **5 strategies migrated** (ready for testing)
- ❌ **4 strategies pending migration** (fall back to legacy)

**Migration Completion:** 55% (5 of 9 strategies)

---

## Update (2026-01-09)

Additive alignment with current repo state:

- Strategy cut-over is centralized in `StrategyCutOverService` and used by orchestration.
- CMS toggles exist in the UI (global + per-strategy) for reachability testing.
- Canonical ID mapping is now subtype-aware and sourced from the runtime mapping table.
- Factory TODOs remain for **MultiMetric**, **Normalized**, **Difference**, **Ratio**.

---

## Part 1: Migration Status Overview

### ✅ Migrated Strategies (Ready to Test)

These strategies have full CMS implementations and can be tested by enabling their respective toggles in MainWindow:

1. **SingleMetricStrategy** ✅
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/SingleMetricStrategyFactory.cs`
   - **Implementation:** `DataVisualiser/Core/Strategies/Implementations/SingleMetricStrategy.cs`
   - **Type:** Unified strategy (single class handles both CMS and legacy via constructor overload)
   - **CMS Constructor:** `SingleMetricStrategy(ICanonicalMetricSeries cmsData, string label, DateTime from, DateTime to, ...)`
   - **Status:** ✅ Tested and working (confirmed in output.txt analysis)

2. **CombinedMetricStrategy** ✅
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/CombinedMetricStrategyFactory.cs`
   - **Implementation:** `DataVisualiser/Core/Strategies/Implementations/CombinedMetricStrategy.cs`
   - **Type:** Unified strategy (single class handles both CMS and legacy)
   - **CMS Constructor:** `CombinedMetricStrategy(ICanonicalMetricSeries left, ICanonicalMetricSeries right, string labelLeft, string labelRight, DateTime from, DateTime to, ...)`
   - **Status:** ✅ Ready to test (requires 2 metric subtypes selected)

3. **WeeklyDistributionStrategy** ✅
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/WeeklyDistributionStrategyFactory.cs`
   - **Implementation:** `DataVisualiser/Core/Strategies/Implementations/CmsWeeklyDistributionStrategy.cs`
   - **Type:** Separate CMS class (inherits from `CmsBucketDistributionStrategy`)
   - **CMS Constructor:** `CmsWeeklyDistributionStrategy(ICanonicalMetricSeries series, DateTime from, DateTime to, string label, ...)`
   - **Status:** ✅ Ready to test

4. **WeekdayTrendStrategy** ✅
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/WeekdayTrendStrategyFactory.cs`
   - **Implementation:** `DataVisualiser/Core/Strategies/Implementations/WeekdayTrendComputationStrategy.cs`
   - **Type:** Unified strategy (single class handles both CMS and legacy)
   - **CMS Constructor:** `WeekdayTrendComputationStrategy(ICanonicalMetricSeries cmsData, string label, DateTime from, DateTime to)`
   - **Status:** ✅ Ready to test

5. **HourlyDistributionStrategy** ✅
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/HourlyDistributionStrategyFactory.cs`
   - **Implementation:** `DataVisualiser/Core/Strategies/Implementations/CmsHourlyDistributionStrategy.cs`
   - **Type:** Separate CMS class (inherits from `CmsBucketDistributionStrategy`)
   - **CMS Constructor:** `CmsHourlyDistributionStrategy(ICanonicalMetricSeries series, DateTime from, DateTime to, string label, ...)`
   - **Status:** ✅ Ready to test

### ❌ Pending Migration Strategies

These strategies currently fall back to legacy implementations even when CMS is enabled. They have TODO comments in their factories:

1. **MultiMetricStrategy** ❌
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/MultiMetricStrategyFactory.cs`
   - **Current Issue:** Line 12 has `// TODO: Implement CMS MultiMetric strategy`
   - **Factory Behavior:** Both `CreateCmsStrategy` and `CreateLegacyStrategy` call `CreateLegacy(p)`
   - **Implementation Status:** `MultiMetricStrategy` class has CMS constructor, but factory doesn't use it
   - **CMS Constructor Exists:** `MultiMetricStrategy(IReadOnlyList<ICanonicalMetricSeries> cmsSeries, IReadOnlyList<string> labels, DateTime from, DateTime to)`

2. **NormalizedStrategy** ❌
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/NormalizedStrategyFactory.cs`
   - **Current Issue:** Line 12 has `// TODO: Implement CMS Normalized strategy`
   - **Factory Behavior:** Both methods call `CreateLegacy(p)`
   - **Implementation Status:** No CMS constructor exists in `NormalizedStrategy` class

3. **DifferenceStrategy** ❌
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/DifferenceStrategyFactory.cs`
   - **Current Issue:** Line 12 has `// TODO: Implement CMS Difference strategy`
   - **Factory Behavior:** Both methods call `CreateLegacy(p)`
   - **Implementation Status:** No CMS constructor exists in `DifferenceStrategy` class

4. **RatioStrategy** ❌
   - **Factory:** `DataVisualiser/Core/Strategies/Factories/RatioStrategyFactory.cs`
   - **Current Issue:** Line 12 has `// TODO: Implement CMS Difference strategy`
   - **Factory Behavior:** Both methods call `CreateLegacy(p)`
   - **Implementation Status:** No CMS constructor exists in `RatioStrategy` class

---

## Part 2: Implementation Patterns Reference

### Pattern 1: Unified Strategy (Recommended)

**Used by:** SingleMetric, CombinedMetric, WeekdayTrend

**Structure:**
- Single class with two constructors:
  - Legacy: `Strategy(IEnumerable<MetricData> data, ...)`
  - CMS: `Strategy(ICanonicalMetricSeries cmsData, ...)`
- Internal flag `_useCms` determines which data source to use
- `Compute()` method checks flag and routes to appropriate implementation

**Example Reference:**
- `DataVisualiser/Core/Strategies/Implementations/SingleMetricStrategy.cs`
- `DataVisualiser/Core/Strategies/Implementations/CombinedMetricStrategy.cs`

**Factory Pattern:**
```csharp
public sealed class StrategyFactory : StrategyFactoryBase
{
    public StrategyFactory() : base(
        // CMS factory delegate
        (ctx, p) => new Strategy(
            ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
            p.Label1, p.From, p.To),
        // Legacy factory delegate
        p => new Strategy(
            p.LegacyData1 ?? Array.Empty<MetricData>(),
            p.Label1, p.From, p.To))
    {
    }
}
```

### Pattern 2: Separate CMS Class

**Used by:** WeeklyDistribution, HourlyDistribution

**Structure:**
- Base class: `BucketDistributionStrategy` (legacy) or `CmsBucketDistributionStrategy` (CMS)
- Separate implementations for CMS and legacy
- Both implement `IChartComputationStrategy`

**Example Reference:**
- `DataVisualiser/Core/Strategies/Implementations/CmsWeeklyDistributionStrategy.cs`
- `DataVisualiser/Core/Strategies/Implementations/CmsHourlyDistributionStrategy.cs`

**Factory Pattern:**
```csharp
public sealed class StrategyFactory : StrategyFactoryBase
{
    public StrategyFactory() : base(
        // CMS factory delegate
        (ctx, p) => new CmsStrategy(
            ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
            p.From, p.To, p.Label1),
        // Legacy factory delegate
        p => new LegacyStrategy(
            p.LegacyData1 ?? Array.Empty<MetricData>(),
            p.Label1, p.From, p.To))
    {
    }
}
```

### Pattern 3: CMS Data Conversion

**When to use:** When strategy needs to work with both CMS and legacy data

**Helper Available:**
- `DataVisualiser/Shared/Helpers/CmsConversionHelper.cs`
- `ConvertSamplesToHealthMetricData(ICanonicalMetricSeries cms, DateTime? from, DateTime? to)`

**Note:** Prefer using CMS directly when possible. Only convert if absolutely necessary for compatibility.

---

## Part 3: Step-by-Step Implementation Plan

### Task 1: MultiMetricStrategy CMS Migration

**Priority:** Medium (has CMS constructor, just needs factory update)

**Steps:**

1. **Verify CMS Constructor Exists**
   - File: `DataVisualiser/Core/Strategies/Implementations/MultiMetricStrategy.cs`
   - Check: Does `MultiMetricStrategy(IReadOnlyList<ICanonicalMetricSeries> cmsSeries, ...)` exist?
   - If YES: Proceed to step 2
   - If NO: Implement CMS constructor first (see Task 2 pattern)

2. **Update Factory**
   - File: `DataVisualiser/Core/Strategies/Factories/MultiMetricStrategyFactory.cs`
   - Replace line 12: Change `(ctx, p) => CreateLegacy(p)` to proper CMS factory delegate
   - Pattern: Extract CMS series from `ctx.PrimaryCms` and any additional series from context
   - Reference: Check how `ChartDataContext` provides multiple CMS series (if available)
   - If context doesn't provide multiple CMS series, may need to convert legacy series to CMS or use hybrid approach

3. **Test**
   - Enable `UseCmsForMultiMetric` toggle
   - Select 3+ metric subtypes
   - Verify `UseCms=True` in debug output
   - Verify chart renders correctly

**Key Considerations:**
- MultiMetric requires multiple series (3+)
- May need to check if `ChartDataContext` supports multiple CMS series
- If not, may need to convert legacy series to CMS using `CmsConversionHelper`

---

### Task 2: NormalizedStrategy CMS Migration

**Priority:** High (commonly used strategy)

**Steps:**

1. **Examine Legacy Implementation**
   - File: `DataVisualiser/Core/Strategies/Implementations/NormalizedStrategy.cs`
   - Understand: How normalization is computed (percentage of max, z-score, etc.)
   - Identify: Data dependencies (needs two series: primary and secondary)

2. **Choose Implementation Pattern**
   - **Option A (Recommended):** Unified strategy pattern
     - Add CMS constructor to existing `NormalizedStrategy` class
     - Add `_useCms` flag and routing logic
   - **Option B:** Separate CMS class
     - Create `CmsNormalizedStrategy` class
     - Inherit from base or implement `IChartComputationStrategy` directly

3. **Implement CMS Constructor**
   - Signature: `NormalizedStrategy(ICanonicalMetricSeries primaryCms, ICanonicalMetricSeries secondaryCms, string label1, string label2, DateTime from, DateTime to, NormalizationMode mode)`
   - Extract samples from CMS series within date range
   - Convert to numeric arrays for computation
   - Reference: `CombinedMetricStrategy` for dual-CMS pattern

4. **Implement Compute Logic**
   - Filter CMS samples by date range: `from` to `to`
   - Extract values: `cms.Samples.Where(s => s.Value.HasValue && s.Timestamp.DateTime >= from && s.Timestamp.DateTime <= to)`
   - Align primary and secondary series (similar to `CombinedMetricStrategy`)
   - Apply normalization formula based on `NormalizationMode`
   - Return `ChartComputationResult`

5. **Update Factory**
   - File: `DataVisualiser/Core/Strategies/Factories/NormalizedStrategyFactory.cs`
   - Replace line 12: Implement CMS factory delegate
   - Pattern: Extract `ctx.PrimaryCms` and `ctx.SecondaryCms`
   - Pass `p.NormalizationMode` to constructor

6. **Test**
   - Enable `UseCmsForNormalized` toggle
   - Select 2 metric subtypes
   - Verify normalization chart renders correctly
   - Compare with legacy output for parity

**Key Considerations:**
- Requires both primary and secondary CMS series
- Must handle alignment of two CMS series (timestamp matching)
- Normalization modes: `PercentageOfMax`, `ZScore`, etc. (check enum)

---

### Task 3: DifferenceStrategy CMS Migration

**Priority:** Medium

**Steps:**

1. **Examine Legacy Implementation**
   - File: `DataVisualiser/Core/Strategies/Implementations/DifferenceStrategy.cs`
   - Understand: How difference is computed (primary - secondary)
   - Identify: Data dependencies (needs two series)

2. **Choose Implementation Pattern**
   - **Recommended:** Unified strategy pattern (simpler maintenance)

3. **Implement CMS Constructor**
   - Signature: `DifferenceStrategy(ICanonicalMetricSeries primaryCms, ICanonicalMetricSeries secondaryCms, string label1, string label2, DateTime from, DateTime to)`
   - Extract and align samples from both CMS series
   - Reference: `CombinedMetricStrategy.AlignSeriesCms()` for alignment pattern

4. **Implement Compute Logic**
   - Filter and align both CMS series by timestamp
   - Compute difference: `primaryValue - secondaryValue` for each aligned pair
   - Handle missing values appropriately (skip or use last known value)
   - Return `ChartComputationResult` with difference values

5. **Update Factory**
   - File: `DataVisualiser/Core/Strategies/Factories/DifferenceStrategyFactory.cs`
   - Replace line 12: Implement CMS factory delegate
   - Extract `ctx.PrimaryCms` and `ctx.SecondaryCms`

6. **Test**
   - Enable `UseCmsForDifference` toggle
   - Select 2 metric subtypes
   - Verify difference chart renders correctly

**Key Considerations:**
- Difference = Primary - Secondary
- Must align timestamps between series
- Handle edge cases: missing values, different sample rates

---

### Task 4: RatioStrategy CMS Migration

**Priority:** Medium

**Steps:**

1. **Examine Legacy Implementation**
   - File: `DataVisualiser/Core/Strategies/Implementations/RatioStrategy.cs`
   - Understand: How ratio is computed (primary / secondary)
   - Identify: Data dependencies (needs two series)

2. **Choose Implementation Pattern**
   - **Recommended:** Unified strategy pattern

3. **Implement CMS Constructor**
   - Signature: `RatioStrategy(ICanonicalMetricSeries primaryCms, ICanonicalMetricSeries secondaryCms, string label1, string label2, DateTime from, DateTime to)`
   - Extract and align samples from both CMS series
   - Reference: `CombinedMetricStrategy.AlignSeriesCms()` for alignment pattern

4. **Implement Compute Logic**
   - Filter and align both CMS series by timestamp
   - Compute ratio: `primaryValue / secondaryValue` for each aligned pair
   - Handle division by zero: Skip or use sentinel value
   - Return `ChartComputationResult` with ratio values

5. **Update Factory**
   - File: `DataVisualiser/Core/Strategies/Factories/RatioStrategyFactory.cs`
   - Replace line 12: Implement CMS factory delegate
   - Extract `ctx.PrimaryCms` and `ctx.SecondaryCms`

6. **Test**
   - Enable `UseCmsForRatio` toggle
   - Select 2 metric subtypes
   - Verify ratio chart renders correctly

**Key Considerations:**
- Ratio = Primary / Secondary
- Must handle division by zero (secondary value = 0)
- Align timestamps between series

---

## Part 4: Common Implementation Patterns

### Extracting CMS Samples Within Date Range

```csharp
private List<CmsPoint> FilterAndOrderCms(ICanonicalMetricSeries cms)
{
    return cms.Samples
        .Where(s => s.Value.HasValue)
        .Select(s => new CmsPoint(s.Timestamp.UtcDateTime, s.Value!.Value))
        .Where(p => p.Timestamp >= _from && p.Timestamp <= _to)
        .OrderBy(p => p.Timestamp)
        .ToList();
}

private sealed record CmsPoint(DateTime Timestamp, decimal ValueDecimal);
```

### Aligning Two CMS Series

Reference: `CombinedMetricStrategy.AlignSeriesCms()` method

```csharp
private (List<DateTime> Timestamps, List<double> Primary, List<double> Secondary) 
    AlignSeriesCms(List<CmsPoint> left, List<CmsPoint> right, int count)
{
    var leftTuples = left.Select(p => (p.Timestamp, (decimal?)p.ValueDecimal)).ToList();
    var rightTuples = right.Select(p => (p.Timestamp, (decimal?)p.ValueDecimal)).ToList();

    var (timestamps, primaryRaw, secondaryRaw) = 
        StrategyComputationHelper.AlignByIndex(leftTuples, rightTuples, count);

    return (timestamps, primaryRaw.ToList(), secondaryRaw.ToList());
}
```

### Converting CMS to Legacy (If Needed)

```csharp
using DataVisualiser.Shared.Helpers;

var legacyData = CmsConversionHelper.ConvertSamplesToHealthMetricData(
    cmsSeries, 
    from, 
    to
).ToList();
```

---

## Part 5: Testing Checklist

For each migrated strategy, verify:

- [ ] Factory `CreateCmsStrategy` method implemented
- [ ] CMS constructor exists in strategy class
- [ ] `UseCms=True` appears in debug output when toggle enabled
- [ ] Chart renders with CMS data
- [ ] Sample count matches legacy data count
- [ ] Chart values appear correct (visual inspection)
- [ ] No exceptions thrown during computation
- [ ] Date range filtering works correctly

### Debug Output to Check

When testing, look for these debug messages:

```
[HasSufficientCmsSamples] Total=X, ValidValues=X, Filtered=X
[CMS] StrategyType: PrimarySamples=X (filtered), TotalPrimarySamples=X
[CutOver] Strategy=StrategyType, UseCms=True
```

If `UseCms=False`, check:
- Is `CmsConfiguration.UseCmsData` enabled?
- Is strategy-specific toggle enabled?
- Are there sufficient CMS samples in date range?

---

## Part 6: File Locations Reference

### Factories
- `DataVisualiser/Core/Strategies/Factories/SingleMetricStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/CombinedMetricStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/MultiMetricStrategyFactory.cs` ⚠️ TODO
- `DataVisualiser/Core/Strategies/Factories/NormalizedStrategyFactory.cs` ⚠️ TODO
- `DataVisualiser/Core/Strategies/Factories/DifferenceStrategyFactory.cs` ⚠️ TODO
- `DataVisualiser/Core/Strategies/Factories/RatioStrategyFactory.cs` ⚠️ TODO
- `DataVisualiser/Core/Strategies/Factories/WeeklyDistributionStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/WeekdayTrendStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/HourlyDistributionStrategyFactory.cs`

### Strategy Implementations
- `DataVisualiser/Core/Strategies/Implementations/SingleMetricStrategy.cs` ✅
- `DataVisualiser/Core/Strategies/Implementations/CombinedMetricStrategy.cs` ✅
- `DataVisualiser/Core/Strategies/Implementations/MultiMetricStrategy.cs` (has CMS constructor)
- `DataVisualiser/Core/Strategies/Implementations/NormalizedStrategy.cs` ⚠️
- `DataVisualiser/Core/Strategies/Implementations/DifferenceStrategy.cs` ⚠️
- `DataVisualiser/Core/Strategies/Implementations/RatioStrategy.cs` ⚠️
- `DataVisualiser/Core/Strategies/Implementations/CmsWeeklyDistributionStrategy.cs` ✅
- `DataVisualiser/Core/Strategies/Implementations/WeekdayTrendComputationStrategy.cs` ✅
- `DataVisualiser/Core/Strategies/Implementations/CmsHourlyDistributionStrategy.cs` ✅

### Helper Classes
- `DataVisualiser/Shared/Helpers/CmsConversionHelper.cs` - CMS to Legacy conversion
- `DataVisualiser/Core/Services/DataPreparationService.cs` - CMS data preparation
- `DataVisualiser/Shared/Helpers/StrategyComputationHelper.cs` - Alignment utilities

### Configuration
- `DataVisualiser/UI/State/CmsConfiguration.cs` - Toggle configuration
- `DataVisualiser/MainWindow.xaml.cs` - Toggle event handlers (lines 961-975)

---

## Part 7: Implementation Order Recommendation

**Suggested Priority:**

1. **MultiMetricStrategy** (Easiest - constructor exists, just update factory)
2. **NormalizedStrategy** (High usage, moderate complexity)
3. **DifferenceStrategy** (Moderate complexity, similar to CombinedMetric)
4. **RatioStrategy** (Moderate complexity, similar to Difference)

**Rationale:**
- Start with MultiMetric (quick win, builds confidence)
- Then Normalized (high value, commonly used)
- Finish with Difference and Ratio (similar patterns, can batch implement)

---

## Part 8: Notes for AI Assistants

### Key Constraints

1. **Date Range Filtering:** Always filter CMS samples by `from` and `to` dates (inclusive, end of day)
2. **Null Safety:** Always check `ctx.PrimaryCms` and `ctx.SecondaryCms` for null before casting
3. **Value Validation:** Only process samples where `s.Value.HasValue == true`
4. **Timestamp Ordering:** Always order samples by timestamp before processing
5. **Alignment:** When working with two series, align by timestamp or index (see `CombinedMetricStrategy`)

### Common Pitfalls to Avoid

- ❌ Don't forget to extend `to` date to end of day: `to.Date.AddDays(1).AddTicks(-1)`
- ❌ Don't assume all samples have values - check `Value.HasValue`
- ❌ Don't mix CMS and legacy data in the same computation path
- ❌ Don't forget to handle edge cases (empty series, single sample, etc.)

### Testing Strategy

1. Enable toggle in MainWindow
2. Select appropriate metric types/subtypes
3. Check debug output for `UseCms=True`
4. Verify chart renders
5. Compare visually with legacy output (if possible)

---

## End of Document

**Next Steps:**
1. Review this document
2. Choose a strategy to implement (recommendation: MultiMetric first)
3. Follow the step-by-step plan for that strategy
4. Test thoroughly
5. Move to next strategy

**Questions or Issues:**
- Check existing migrated strategies for patterns
- Reference `CombinedMetricStrategy` for dual-series patterns
- Reference `SingleMetricStrategy` for single-series patterns
- Check factory base class for delegate patterns



