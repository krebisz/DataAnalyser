# Migration Root Cause Analysis: What Went Wrong

## Executive Summary

The CMS migration failed due to **fragmented cut-over logic** scattered across multiple layers without a single, verifiable decision point. This created:
1. **Non-uniform cut-over**: Different strategies use different conditional logic
2. **Unverifiable behavior**: No single point to test or validate the migration
3. **Side-effect cascades**: Fixes in isolation broke other parts of the system
4. **Unpredictable execution**: Multiple decision points create ambiguous execution paths

---

## Problem 1: Non-Uniform, Unverifiable Cut-Over Logic

### Current State: Multiple Decision Points

The application has **at least 4 different cut-over mechanisms**:

#### 1. **ChartDataContextBuilder** (Data Conversion Layer)
```csharp
// Line 102-108: Converts CMS to HealthMetricData if CMS exists
var effectiveData1 = primaryCms != null
    ? ConvertCmsToHealthMetricData(primaryCms, from, to)
    : data1;
```
**Problem**: This converts CMS to legacy format, losing the benefit of CMS. Strategies downstream still receive `HealthMetricData`, not `ICanonicalMetricSeries`.

#### 2. **MainWindow.CreateSingleMetricStrategy** (Strategy Selection)
```csharp
// Line 2019: Checks if CMS exists, creates CMS strategy
if (ctx.PrimaryCms is ICanonicalMetricSeries cms)
{
    return new SingleMetricCmsStrategy(cms, label, from, to);
}
return new SingleMetricLegacyStrategy(data, label, from, to);
```
**Problem**: This is a **good pattern** but only works for single metric. Other strategies use different logic.

#### 3. **WeeklyDistributionService** (Service Layer)
```csharp
// Line 85: Ad-hoc decision based on null check
var useCmsStrategy = (cmsSeries != null);
```
**Problem**: No parity validation, no configuration flag, just a null check. This is **unverifiable**.

#### 4. **CombinedMetricStrategy** (Parity Service)
```csharp
// ParityValidationService: Hard-coded flag
const bool ENABLE_COMBINED_METRIC_PARITY = false;
```
**Problem**: Parity is disabled by default, but there's no clear path to enable it safely.

### Why This Is Critical

**No Single Source of Truth**: There's no one place where you can:
- ✅ Test: "Does CMS work for this metric?"
- ✅ Verify: "Is CMS being used correctly?"
- ✅ Debug: "Why is legacy being used instead of CMS?"
- ✅ Validate: "Are legacy and CMS producing the same results?"

---

## Problem 2: Corrective Behaviors in Isolation

### The Cascade Effect

When fixes were made to individual strategies, they created side-effects:

#### Example 1: ChartDataContextBuilder Conversion
- **Fix**: Convert CMS to HealthMetricData so strategies can use it
- **Side-effect**: All strategies receive legacy format, CMS benefits lost
- **Result**: CMS strategies never receive CMS data, they receive converted legacy data

#### Example 2: CombinedMetricStrategy Alignment
- **Fix**: Changed `Math.Min` to `Math.Max` to handle mismatched counts
- **Side-effect**: Broke legacy strategy behavior (used by default)
- **Result**: Main chart broken for all users, not just CMS users

#### Example 3: Weekly Distribution Timezone
- **Fix**: Changed `LocalDateTime` to `DateTime` for timezone consistency
- **Side-effect**: Only fixed in CMS strategy, legacy still uses different logic
- **Result**: CMS and legacy produce different results (parity failure)

### Root Cause: No Migration Cycle

Each strategy was "fixed" independently without:
1. **Parity validation** at the cut-over point
2. **Integration testing** with the full pipeline
3. **Rollback mechanism** if parity fails
4. **Documentation** of what changed and why

---

## What Should Have Happened: The Correct Pattern

### Single Cut-Over Point Pattern

For **each strategy**, there should be **exactly one method** that decides legacy vs CMS:

```csharp
// ✅ CORRECT PATTERN (SingleMetricStrategy example)
private static IChartComputationStrategy CreateSingleMetricStrategy(
    ChartDataContext ctx,
    IEnumerable<HealthMetricData> legacyData,
    string label,
    DateTime from,
    DateTime to)
{
    // SINGLE DECISION POINT
    if (ShouldUseCms(ctx, "SingleMetric"))
    {
        // CMS path with parity validation
        return CreateCmsStrategyWithParity(ctx, legacyData, label, from, to);
    }
    
    // Legacy path (always available)
    return new SingleMetricLegacyStrategy(legacyData, label, from, to);
}

private static bool ShouldUseCms(ChartDataContext ctx, string strategyType)
{
    // Verifiable, testable, configurable
    return CmsConfiguration.ShouldUseCms(strategyType) 
        && ctx.PrimaryCms != null;
}
```

### Migration Cycle Per Strategy

For **each strategy migration**, follow this cycle:

1. **Implement CMS Strategy** (isolated, tested)
2. **Add Parity Tests** (validate correctness)
3. **Create Cut-Over Method** (single decision point)
4. **Wire Parity Validation** (at cut-over point)
5. **Enable with Flag** (opt-in, not default)
6. **Validate in Production** (with flag enabled)
7. **Promote to Default** (after validation)
8. **Remove Legacy Path** (only after full validation)

**Current state**: Steps 1-2 done, steps 3-8 incomplete or inconsistent.

---

## The Simplest Migration: SingleMetricStrategy

### Why It's the Simplest

1. **Single data source**: Only `PrimaryCms`, no subtypes
2. **No alignment needed**: One series, no merging
3. **No derived calculations**: No difference, ratio, normalization
4. **Already partially migrated**: `CreateSingleMetricStrategy` exists
5. **Parity tests exist**: Can validate correctness

### Current State Analysis

**✅ What Works**:
- `SingleMetricCmsStrategy` exists and is tested
- `CreateSingleMetricStrategy` has cut-over logic
- Parity tests exist (though not wired to cut-over)

**❌ What's Broken**:
- `ChartDataContextBuilder` converts CMS to legacy before strategy selection
- No parity validation at cut-over point
- No configuration flag to enable/disable
- Legacy path still used even when CMS is available

### The Fix: Start Here

**SingleMetricStrategy** should be the **reference implementation** for all other migrations:

1. **Remove CMS conversion in ChartDataContextBuilder** for single metric
2. **Add parity validation** to `CreateSingleMetricStrategy`
3. **Add configuration flag** (`CmsConfiguration.UseCmsForSingleMetric`)
4. **Test end-to-end** with flag enabled
5. **Document the pattern** for other strategies

---

## Recommended Fix: Unified Cut-Over Architecture

### Phase 1: Establish SingleMetricStrategy as Reference

**Goal**: Make SingleMetricStrategy the **gold standard** for CMS migration.

**Steps**:
1. Create `StrategyCutOverService` (single decision point)
2. Move cut-over logic from `CreateSingleMetricStrategy` to service
3. Add parity validation at cut-over point
4. Add configuration flag
5. Test and validate
6. Document the pattern

### Phase 2: Apply Pattern to Other Strategies

**Goal**: Use SingleMetricStrategy pattern for all other strategies.

**Steps**:
1. Weekly Distribution: Move cut-over to `StrategyCutOverService`
2. Combined Metric: Move cut-over to `StrategyCutOverService`
3. Multi Metric: Move cut-over to `StrategyCutOverService`
4. (Repeat for all strategies)

### Phase 3: Remove Fragmented Logic

**Goal**: Remove all ad-hoc cut-over decisions.

**Steps**:
1. Remove CMS conversion in `ChartDataContextBuilder`
2. Remove null checks in service layers
3. Remove hard-coded flags
4. All strategies use `StrategyCutOverService`

---

## Success Criteria

A migration is **complete** when:

1. ✅ **Single cut-over point**: One method decides legacy vs CMS
2. ✅ **Parity validation**: Legacy and CMS executed side-by-side
3. ✅ **Configuration flag**: Can enable/disable per strategy
4. ✅ **Verifiable**: Can test cut-over logic in isolation
5. ✅ **Documented**: Pattern is clear and repeatable
6. ✅ **Tested**: End-to-end validation passes
7. ✅ **Promoted**: CMS is default after validation
8. ✅ **Legacy preserved**: Legacy path still available for rollback

---

## Next Steps

1. **Start with SingleMetricStrategy** (simplest case)
2. **Create StrategyCutOverService** (unified decision point)
3. **Add parity validation** (safety mechanism)
4. **Test and validate** (end-to-end)
5. **Document the pattern** (for other strategies)
6. **Apply to other strategies** (one at a time)

---

**Last Updated**: 2025-01-04  
**Status**: Analysis complete, ready for implementation

