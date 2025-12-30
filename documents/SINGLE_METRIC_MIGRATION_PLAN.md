# SingleMetricStrategy Migration: Reference Implementation Plan

## Objective

Establish `SingleMetricStrategy` as the **gold standard** CMS migration pattern that can be replicated for all other strategies.

## Current State

### ✅ What Exists
- `SingleMetricCmsStrategy` - CMS implementation (tested)
- `SingleMetricLegacyStrategy` - Legacy implementation (stable)
- `CreateSingleMetricStrategy` - Cut-over method (partial)
- Parity tests exist (not wired to cut-over)

### ❌ What's Broken
1. **ChartDataContextBuilder converts CMS to legacy** before strategy selection
2. **No parity validation** at cut-over point
3. **No configuration flag** to enable/disable
4. **No unified decision point** (logic scattered)

---

## Implementation Plan

### Step 1: Create StrategyCutOverService

**File**: `DataVisualiser/Services/StrategyCutOverService.cs`

**Purpose**: Single, verifiable decision point for all strategy cut-overs.

```csharp
public class StrategyCutOverService
{
    /// <summary>
    /// Determines if CMS should be used for a strategy.
    /// Single source of truth for cut-over decisions.
    /// </summary>
    public static bool ShouldUseCms(
        ChartDataContext ctx,
        string strategyType)
    {
        // Check configuration flag
        if (!CmsConfiguration.ShouldUseCms(strategyType))
            return false;
        
        // Check if CMS data is available
        if (strategyType == "SingleMetric" && ctx.PrimaryCms == null)
            return false;
        
        // Add other strategy-specific checks here
        
        return true;
    }
    
    /// <summary>
    /// Creates a single metric strategy with parity validation.
    /// </summary>
    public static IChartComputationStrategy CreateSingleMetricStrategy(
        ChartDataContext ctx,
        IEnumerable<HealthMetricData> legacyData,
        string label,
        DateTime from,
        DateTime to)
    {
        if (ShouldUseCms(ctx, "SingleMetric"))
        {
            return CreateWithParityValidation(ctx, legacyData, label, from, to);
        }
        
        return new SingleMetricLegacyStrategy(legacyData, label, from, to);
    }
    
    private static IChartComputationStrategy CreateWithParityValidation(
        ChartDataContext ctx,
        IEnumerable<HealthMetricData> legacyData,
        string label,
        DateTime from,
        DateTime to)
    {
        var cms = ctx.PrimaryCms as ICanonicalMetricSeries;
        if (cms == null)
            return new SingleMetricLegacyStrategy(legacyData, label, from, to);
        
        // Create both strategies
        var legacyStrategy = new SingleMetricLegacyStrategy(legacyData, label, from, to);
        var cmsStrategy = new SingleMetricCmsStrategy(cms, label, from, to);
        
        // Run parity validation if enabled
        if (CmsConfiguration.EnableParityValidation)
        {
            var parityResult = ValidateParity(legacyStrategy, cmsStrategy);
            if (!parityResult.Passed)
            {
                // Log failure, use legacy
                System.Diagnostics.Debug.WriteLine(
                    $"[PARITY] SingleMetric parity failed: {parityResult.Message}");
                return legacyStrategy;
            }
        }
        
        // Parity passed (or disabled), use CMS
        return cmsStrategy;
    }
    
    private static ParityResult ValidateParity(
        IChartComputationStrategy legacy,
        IChartComputationStrategy cms)
    {
        // Execute both strategies
        var legacyResult = legacy.Compute();
        var cmsResult = cms.Compute();
        
        // Compare results (simplified - use existing parity harness)
        // TODO: Wire in existing SingleMetricParityHarness
        
        return new ParityResult { Passed = true };
    }
}
```

**Success Criteria**:
- ✅ Single method decides legacy vs CMS
- ✅ Parity validation integrated
- ✅ Configuration flag respected
- ✅ Testable in isolation

---

### Step 2: Update ChartDataContextBuilder

**File**: `DataVisualiser/Services/ChartDataContextBuilder.cs`

**Change**: Remove CMS-to-legacy conversion for single metric path.

**Current** (Line 102-108):
```csharp
var effectiveData1 = primaryCms != null
    ? ConvertCmsToHealthMetricData(primaryCms, from, to)
    : data1;
```

**New**: Keep CMS as-is, don't convert. Strategies will receive CMS directly.

```csharp
// For single metric: don't convert CMS, pass it through
// Strategies will handle CMS vs legacy internally
var effectiveData1 = data1; // Always use legacy data
// CMS is passed separately via ctx.PrimaryCms
```

**Rationale**: 
- Strategies should receive CMS directly, not converted legacy
- Conversion loses CMS benefits (timezone, precision, etc.)
- Cut-over service handles CMS vs legacy decision

---

### Step 3: Update MainWindow.CreateSingleMetricStrategy

**File**: `DataVisualiser/MainWindow.xaml.cs`

**Change**: Use `StrategyCutOverService` instead of inline logic.

**Current** (Line 2012-2039):
```csharp
private static IChartComputationStrategy CreateSingleMetricStrategy(
    ChartDataContext ctx,
    IEnumerable<HealthMetricData> data,
    string label,
    DateTime from,
    DateTime to)
{
    if (ctx.PrimaryCms is ICanonicalMetricSeries cms)
    {
        return new SingleMetricCmsStrategy(cms, label, from, to);
    }
    return new SingleMetricLegacyStrategy(data, label, from, to);
}
```

**New**:
```csharp
private static IChartComputationStrategy CreateSingleMetricStrategy(
    ChartDataContext ctx,
    IEnumerable<HealthMetricData> data,
    string label,
    DateTime from,
    DateTime to)
{
    return StrategyCutOverService.CreateSingleMetricStrategy(
        ctx, data, label, from, to);
}
```

**Success Criteria**:
- ✅ All cut-over logic in one place
- ✅ Parity validation active
- ✅ Configuration flag respected

---

### Step 4: Add Configuration Flag

**File**: `DataVisualiser/State/CmsConfiguration.cs`

**Change**: Add flag for single metric and parity validation.

```csharp
public static bool UseCmsForSingleMetric { get; set; } = false; // ✅ Exists
public static bool EnableParityValidation { get; set; } = false; // ➕ Add this
```

**Usage**:
- `UseCmsForSingleMetric = true` → Enable CMS for single metric
- `EnableParityValidation = true` → Run parity checks at cut-over

---

### Step 5: Wire Parity Harness

**File**: `DataVisualiser/Services/StrategyCutOverService.cs`

**Change**: Use existing `SingleMetricParityHarness` if it exists.

**Check**: Does `SingleMetricParityHarness` exist?
- If yes: Wire it into `ValidateParity` method
- If no: Create simple parity check (compare results)

**Parity Check** (simplified):
```csharp
private static ParityResult ValidateParity(
    IChartComputationStrategy legacy,
    IChartComputationStrategy cms)
{
    var legacyResult = legacy.Compute();
    var cmsResult = cms.Compute();
    
    if (legacyResult == null && cmsResult == null)
        return new ParityResult { Passed = true };
    
    if (legacyResult == null || cmsResult == null)
        return new ParityResult { 
            Passed = false, 
            Message = "One strategy returned null, other did not" 
        };
    
    // Compare timestamps, values, etc.
    // Use existing parity test logic
    return new ParityResult { Passed = true };
}
```

---

### Step 6: Testing Plan

#### Unit Tests
1. **StrategyCutOverService.ShouldUseCms**
   - Returns false when flag disabled
   - Returns false when CMS not available
   - Returns true when flag enabled and CMS available

2. **StrategyCutOverService.CreateSingleMetricStrategy**
   - Returns legacy when flag disabled
   - Returns CMS when flag enabled and parity passes
   - Returns legacy when parity fails

3. **Parity Validation**
   - Passes when results match
   - Fails when results differ
   - Logs failures correctly

#### Integration Tests
1. **End-to-End with Flag Disabled**
   - Application uses legacy strategy
   - Charts render correctly
   - No errors or warnings

2. **End-to-End with Flag Enabled**
   - Application uses CMS strategy
   - Charts render correctly
   - Parity validation runs (if enabled)
   - Results match legacy

3. **Parity Failure Handling**
   - Parity fails → falls back to legacy
   - Error logged
   - Application continues working

---

### Step 7: Documentation

**File**: `documents/SINGLE_METRIC_MIGRATION_PATTERN.md`

**Content**:
- Pattern description
- Code examples
- Testing approach
- How to replicate for other strategies

---

## Success Criteria

✅ **Single cut-over point**: `StrategyCutOverService.CreateSingleMetricStrategy`  
✅ **Parity validation**: Active at cut-over point  
✅ **Configuration flag**: `CmsConfiguration.UseCmsForSingleMetric`  
✅ **Verifiable**: Can test cut-over logic in isolation  
✅ **Documented**: Pattern is clear and repeatable  
✅ **Tested**: Unit and integration tests pass  
✅ **Legacy preserved**: Legacy path still available  

---

## Rollout Plan

### Phase 1: Implementation (This Session)
1. Create `StrategyCutOverService`
2. Update `ChartDataContextBuilder` (remove conversion)
3. Update `MainWindow.CreateSingleMetricStrategy`
4. Add configuration flags
5. Wire parity validation

### Phase 2: Testing (Next Session)
1. Write unit tests
2. Write integration tests
3. Test with flag disabled (baseline)
4. Test with flag enabled (CMS path)
5. Test parity failure handling

### Phase 3: Validation (After Testing)
1. Enable flag in development
2. Validate in production-like environment
3. Compare results with legacy
4. Fix any parity issues
5. Document findings

### Phase 4: Promotion (After Validation)
1. Enable flag by default
2. Monitor for issues
3. Remove legacy path (future, after full validation)

---

## Next Steps

1. **Implement Step 1-5** (this session)
2. **Test Step 6** (next session)
3. **Document Step 7** (after testing)
4. **Apply pattern to other strategies** (after validation)

---

**Last Updated**: 2025-01-04  
**Status**: Ready for implementation

