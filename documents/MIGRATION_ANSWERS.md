# Migration Questions: Direct Answers

## Question 1: What Went Wrong?

### Answer: Fragmented Cut-Over Logic

**The Problem**: Instead of having **one place** where the application decides "use CMS or legacy?", there are **at least 4 different places** making this decision:

1. **ChartDataContextBuilder** - Converts CMS to legacy format (defeats the purpose)
2. **MainWindow.CreateSingleMetricStrategy** - Checks if CMS exists (good, but only for single metric)
3. **WeeklyDistributionService** - Null check: `useCmsStrategy = (cmsSeries != null)` (unverifiable)
4. **CombinedMetricStrategy** - Hard-coded flag: `ENABLE_COMBINED_METRIC_PARITY = false` (disabled)

**Why This Is Critical**:
- ❌ **Not uniform**: Each strategy uses different logic
- ❌ **Not verifiable**: Can't test "does CMS work?" in one place
- ❌ **Not predictable**: Multiple decision points = ambiguous execution
- ❌ **Not well-defined**: No clear pattern to follow

### The Cascade Effect

When fixes were made to individual strategies:
- **Fix 1**: Convert CMS to legacy in `ChartDataContextBuilder` → All strategies receive legacy format
- **Fix 2**: Change `Math.Min` to `Math.Max` in `CombinedMetricStrategy` → Broke legacy behavior
- **Fix 3**: Fix timezone in `CmsWeeklyDistributionStrategy` → Legacy still uses different logic

**Result**: Fixes in isolation created side-effects because there was no **unified migration cycle**.

---

## Question 2: How Do We Start with the Simplest Migration?

### Answer: SingleMetricStrategy as Reference Implementation

**Why SingleMetricStrategy is Simplest**:
1. ✅ **Single data source**: Only `PrimaryCms`, no subtypes
2. ✅ **No alignment**: One series, no merging needed
3. ✅ **No derived calculations**: No difference, ratio, normalization
4. ✅ **Already partially done**: Cut-over method exists
5. ✅ **Parity tests exist**: Can validate correctness

### The Fix: Three Steps

#### Step 1: Create Unified Cut-Over Service
**File**: `DataVisualiser/Services/StrategyCutOverService.cs`

**Purpose**: **One place** that decides legacy vs CMS for all strategies.

```csharp
// Single decision point
public static IChartComputationStrategy CreateSingleMetricStrategy(...)
{
    if (ShouldUseCms(ctx, "SingleMetric"))
    {
        // CMS path with parity validation
        return CreateWithParityValidation(...);
    }
    return new SingleMetricLegacyStrategy(...);
}
```

#### Step 2: Remove CMS Conversion
**File**: `DataVisualiser/Services/ChartDataContextBuilder.cs`

**Change**: Don't convert CMS to legacy. Pass CMS directly to strategies.

**Why**: Converting CMS to legacy defeats the purpose. Strategies should receive CMS directly.

#### Step 3: Wire Parity Validation
**At cut-over point**: Run legacy and CMS side-by-side, compare results.

**If parity fails**: Fall back to legacy, log error.

**If parity passes**: Use CMS strategy.

---

## The Correct Pattern (For All Strategies)

### Single Cut-Over Point

**One method** decides legacy vs CMS:
```csharp
StrategyCutOverService.CreateSingleMetricStrategy(ctx, data, label, from, to)
```

### Parity Validation

**At cut-over point**: Execute both legacy and CMS, compare results.

**If different**: Use legacy, log failure.

**If same**: Use CMS.

### Configuration Flag

**Enable/disable per strategy**:
```csharp
CmsConfiguration.UseCmsForSingleMetric = true;  // Enable CMS
CmsConfiguration.EnableParityValidation = true; // Run parity checks
```

### Migration Cycle

For **each strategy**:
1. ✅ Implement CMS strategy (isolated)
2. ✅ Add parity tests (validate)
3. ✅ Create cut-over method (single point)
4. ✅ Wire parity validation (safety)
5. ✅ Add configuration flag (opt-in)
6. ✅ Test end-to-end (validate)
7. ✅ Enable in production (with flag)
8. ✅ Promote to default (after validation)

**Current state**: Steps 1-2 done, steps 3-8 incomplete.

---

## Implementation Plan

### Start Here: SingleMetricStrategy

1. **Create `StrategyCutOverService`** (unified decision point)
2. **Update `ChartDataContextBuilder`** (remove CMS conversion)
3. **Update `MainWindow.CreateSingleMetricStrategy`** (use service)
4. **Add configuration flags** (enable/disable)
5. **Wire parity validation** (safety mechanism)
6. **Test end-to-end** (validate correctness)
7. **Document the pattern** (for other strategies)

### Then: Apply to Other Strategies

Once SingleMetricStrategy is working:
- Weekly Distribution → Use same pattern
- Combined Metric → Use same pattern
- Multi Metric → Use same pattern
- (Repeat for all strategies)

---

## Success Criteria

A migration is **complete** when:

1. ✅ **Single cut-over point**: One method decides legacy vs CMS
2. ✅ **Parity validation**: Legacy and CMS executed side-by-side
3. ✅ **Configuration flag**: Can enable/disable per strategy
4. ✅ **Verifiable**: Can test cut-over logic in isolation
5. ✅ **Documented**: Pattern is clear and repeatable
6. ✅ **Tested**: End-to-end validation passes
7. ✅ **Legacy preserved**: Legacy path still available

---

## Files Created

1. **`MIGRATION_ROOT_CAUSE_ANALYSIS.md`** - Detailed analysis of what went wrong
2. **`SINGLE_METRIC_MIGRATION_PLAN.md`** - Step-by-step implementation plan
3. **`MIGRATION_ANSWERS.md`** - This document (direct answers)

---

## Next Steps

1. **Review the analysis** (understand the problem)
2. **Review the plan** (understand the solution)
3. **Implement Step 1-5** (create unified cut-over service)
4. **Test Step 6** (validate correctness)
5. **Apply pattern to other strategies** (replicate success)

---

**Last Updated**: 2025-01-04  
**Status**: Ready for implementation

