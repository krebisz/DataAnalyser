# MetricCompatibilityHelper Integration Status

## Current State

**Status**: ✅ **Created but Not Yet Integrated**

The `MetricCompatibilityHelper` class has been created as **foundational infrastructure** but is not yet actively used in any code paths. This is intentional - it's an additive, opt-in utility that can be integrated when needed.

---

## Integration Points

### 1. **MultiMetricStrategy** (Priority 4 - Future)

**Location**: `DataVisualiser/MultiMetricStrategy.cs`

**When CMS constructor is added**, validation should be included:

```csharp
public MultiMetricStrategy(
    IReadOnlyList<ICanonicalMetricSeries> cmsSeries,
    IReadOnlyList<string> labels,
    DateTime from,
    DateTime to)
{
    // Validate compatibility before processing
    var canonicalIds = cmsSeries.Select(cms => cms.MetricId.Value).ToList();
    if (!MetricCompatibilityHelper.ValidateCompatibility(canonicalIds))
    {
        var reason = MetricCompatibilityHelper.GetIncompatibilityReason(canonicalIds);
        throw new ArgumentException(
            $"Cannot combine incompatible metrics: {reason}",
            nameof(cmsSeries));
    }

    // Convert each CMS to HealthMetricData using helper
    var series = cmsSeries.Select(cms =>
        CmsConversionHelper.ConvertSamplesToHealthMetricData(cms, from, to)
            .ToList())
        .ToList();

    _series = series;
    _labels = labels;
    _from = from;
    _to = to;
    _unit = cmsSeries.FirstOrDefault()?.Unit.Symbol;
}
```

**Benefit**: Prevents invalid metric combinations at construction time.

---

### 2. **CombinedMetricStrategy** (Future CMS Support)

**Location**: `DataVisualiser/CombinedMetricStrategy.cs`

**When CMS constructor is added**:

```csharp
public CombinedMetricStrategy(
    ICanonicalMetricSeries leftCms,
    ICanonicalMetricSeries rightCms,
    string labelLeft,
    string labelRight,
    DateTime from,
    DateTime to)
{
    // Validate compatibility
    if (!MetricCompatibilityHelper.AreCompatible(
        leftCms.MetricId.Value,
        rightCms.MetricId.Value))
    {
        var reason = MetricCompatibilityHelper.GetIncompatibilityReason(
            new[] { leftCms.MetricId.Value, rightCms.MetricId.Value });
        throw new ArgumentException(
            $"Cannot combine incompatible metrics: {reason}",
            nameof(leftCms));
    }

    // Convert and proceed...
}
```

**Benefit**: Ensures left and right metrics are semantically compatible.

---

### 3. **DifferenceStrategy** (Future CMS Support)

**Location**: `DataVisualiser/DifferenceStrategy.cs`

**When CMS constructor is added**, validation ensures subtraction is meaningful:

```csharp
// Validate: Can only subtract compatible metrics
if (!MetricCompatibilityHelper.AreCompatible(
    primaryCms.MetricId.Value,
    secondaryCms.MetricId.Value))
{
    throw new ArgumentException("Cannot subtract incompatible metrics");
}
```

**Benefit**: Prevents nonsensical operations (e.g., Body Weight - Sleep Duration).

---

### 4. **RatioStrategy** (Future CMS Support)

**Location**: `DataVisualiser/RatioStrategy.cs`

**When CMS constructor is added**:

```csharp
// Validate: Can only divide compatible metrics
if (!MetricCompatibilityHelper.AreCompatible(
    numeratorCms.MetricId.Value,
    denominatorCms.MetricId.Value))
{
    throw new ArgumentException("Cannot divide incompatible metrics");
}
```

**Benefit**: Ensures ratio operations are semantically valid.

---

### 5. **CmsDataService** (Optional - Proactive Validation)

**Location**: `DataVisualiser/Data/Repositories/CmsDataService.cs`

**Optional enhancement** - Add validation method:

```csharp
/// <summary>
/// Validates that multiple canonical metric IDs are compatible.
/// </summary>
public static bool ValidateMetricCompatibility(IEnumerable<string> canonicalIds)
{
    return MetricCompatibilityHelper.ValidateCompatibility(canonicalIds);
}
```

**Benefit**: Provides validation service for UI layer.

---

### 6. **UI Layer** (Future - User Experience)

**Location**: `DataVisualiser/MainWindow.xaml.cs` or ViewModel

**Proactive validation** before chart rendering:

```csharp
// Before creating MultiMetricStrategy
var selectedMetricIds = GetSelectedCanonicalIds();
if (!MetricCompatibilityHelper.ValidateCompatibility(selectedMetricIds))
{
    var reason = MetricCompatibilityHelper.GetIncompatibilityReason(selectedMetricIds);
    ShowError($"Cannot combine metrics: {reason}");
    return;
}
```

**Benefit**: Prevents user errors before computation starts.

---

## Current Usage

**None** - The helper is available but not yet called by any code.

This is by design:

- ✅ **Additive**: No breaking changes to existing code
- ✅ **Opt-in**: Can be integrated incrementally
- ✅ **Foundation**: Ready when needed

---

## Integration Strategy

### Phase 1: Foundation (✅ Complete)

- Helper class created
- Methods implemented
- Documentation complete

### Phase 2: Priority 4 Integration (Next)

- Add validation to `MultiMetricStrategy` CMS constructor
- Validate when multiple CMS series are provided

### Phase 3: Other Strategies (Future)

- Add validation to `CombinedMetricStrategy`, `DifferenceStrategy`, `RatioStrategy`
- Ensure all multi-metric operations are validated

### Phase 4: UI Integration (Future)

- Add validation in ViewModel/UI layer
- Provide user feedback for invalid combinations
- Disable invalid options proactively

---

## Example Usage

### Current (No Integration)

```csharp
// No validation - metrics could be incompatible
var strategy = new MultiMetricStrategy(series, labels, from, to);
```

### With Integration (Priority 4)

```csharp
// Validation ensures compatibility
var canonicalIds = cmsSeries.Select(cms => cms.MetricId.Value).ToList();
if (!MetricCompatibilityHelper.ValidateCompatibility(canonicalIds))
{
    var reason = MetricCompatibilityHelper.GetIncompatibilityReason(canonicalIds);
    throw new ArgumentException($"Incompatible metrics: {reason}");
}
var strategy = new MultiMetricStrategy(cmsSeries, labels, from, to);
```

---

## Benefits of Current Approach

1. **No Breaking Changes**: Existing code continues to work
2. **Incremental Adoption**: Can be added strategy-by-strategy
3. **Foundation Ready**: Available when Priority 4 is implemented
4. **Low Risk**: Additive only, no side effects

---

## Next Steps

1. **Priority 4**: Integrate into `MultiMetricStrategy` CMS constructor
2. **Other Strategies**: Add validation as CMS support is added
3. **UI Layer**: Add proactive validation for better UX

The helper is **ready to use** - it just needs to be called at the appropriate integration points.
