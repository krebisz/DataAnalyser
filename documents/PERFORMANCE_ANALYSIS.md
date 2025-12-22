# Performance Analysis & Optimization Roadmap

**Date**: 2025-01-XX  
**Scope**: Data loading pipeline from button click to chart rendering  
**Problem**: Severe performance degradation with large datasets (2+ years of records)

---

## Executive Summary

The data loading pipeline has multiple performance bottlenecks across 5 layers:

1. **SQL/Database Layer** (High Impact, Medium Effort)
2. **Data Materialization Layer** (High Impact, Low Effort)
3. **Computation Layer** (Medium Impact, High Effort)
4. **Rendering Layer** (Medium Impact, Medium Effort)
5. **UI Thread Management** (Low Impact, Low Effort)

**Estimated Total Improvement**: 60-80% reduction in load time for large datasets  
**Priority Order**: Based on ROI (Impact × User Benefit / Implementation Effort)

---

## Pipeline Call Chain

```
OnLoadData (UI Thread)
  ↓
LoadMetricDataAsync (ViewModel)
  ↓
LoadMetricDataWithCmsAsync (MetricSelectionService)
  ↓ [Parallel]
    ├─ GetHealthMetricsDataByBaseType (DataFetcher) → SQL Query
    ├─ GetHealthMetricsDataByBaseType (DataFetcher) → SQL Query
    ├─ GetCmsByCanonicalIdAsync (CmsDataService) → SQL Query
    └─ GetCmsByCanonicalIdAsync (CmsDataService) → SQL Query
  ↓
ChartDataContextBuilder.Build (creates context)
  ↓
LoadDataCommand.Execute → DataLoaded event
  ↓
RenderChartsFromLastContext (MainWindow)
  ↓ [Sequential]
    ├─ RenderPrimaryChart → Strategy.Compute() → MathHelper operations
    ├─ RenderNormalized → Strategy.Compute() → MathHelper operations
    ├─ RenderDifference → Strategy.Compute() → MathHelper operations
    ├─ RenderRatio → Strategy.Compute() → MathHelper operations
    ├─ RenderWeeklyDistribution → Strategy.Compute() → MathHelper operations
    └─ RenderWeeklyTrend → Strategy.Compute() → MathHelper operations
  ↓
ChartRenderEngine.Render (UI Thread - LiveCharts)
```

---

## Bottleneck Analysis

### 🔴 **CRITICAL: SQL Query Performance**

**Location**: `DataFetcher.GetHealthMetricsDataByBaseType()`

**Issues**:

1. **No result limiting**: Fetches ALL records matching criteria (potentially 100,000+ rows)
2. **No pagination**: Entire dataset loaded into memory at once
3. **Inefficient ORDER BY**: Sorting large result sets in SQL
4. **Missing indexes**: No explicit index hints or query optimization
5. **Multiple round trips**: Separate queries for primary/secondary/CMS data

**Current Query Pattern**:

```sql
SELECT NormalizedTimestamp, Value, Unit, Provider
FROM [dbo].[HealthMetrics]
WHERE MetricType = @MetricType
  AND MetricSubtype = @Subtype
  AND NormalizedTimestamp >= @FromDate
  AND NormalizedTimestamp <= @ToDate
  AND Value IS NOT NULL
ORDER BY NormalizedTimestamp  -- ⚠️ Expensive on large datasets
```

**Impact**:

- **2 years of daily data** (~730 records) = ~1-2 seconds
- **2 years of hourly data** (~17,520 records) = ~10-30 seconds
- **2 years of minute data** (~1,051,200 records) = **Minutes to hours**

**ROI**: ⭐⭐⭐⭐⭐ (Very High - 80% of total improvement potential)

---

### 🟠 **HIGH: Excessive Data Materialization**

**Location**: Throughout strategies and helpers

**Issues**:

1. **Multiple `.ToList()` calls**: Data materialized 5-10 times per chart
2. **IEnumerable → List conversions**: Early materialization prevents lazy evaluation
3. **Redundant enumerations**: Same data enumerated multiple times
4. **Memory pressure**: Large lists held in memory simultaneously

**Examples**:

```csharp
// SingleMetricStrategy.cs - Data materialized 6+ times
var orderedData = _data.Where(...).OrderBy(...).ToList();  // Materialization #1
var dataList = orderedData.ToList();                       // Materialization #2 (redundant)
var rawTimestamps = dataList.Select(...).ToList();         // Materialization #3
var intervalIndices = rawTimestamps.Select(...).ToList(); // Materialization #4
var smoothedData = MathHelper.CreateSmoothedData(...);    // Materialization #5
var smoothedValues = MathHelper.InterpolateSmoothedData(...); // Materialization #6
```

**Impact**:

- **Memory**: 2-3x memory usage vs. necessary
- **CPU**: Unnecessary allocations and GC pressure
- **Time**: 10-20% overhead on large datasets

**ROI**: ⭐⭐⭐⭐ (High - Low effort, immediate benefit)

---

### 🟡 **MEDIUM: Computation Algorithm Inefficiency**

**Location**: `MathHelper.CreateSmoothedData()` and `InterpolateSmoothedData()`

**Issues**:

1. **O(n²) complexity in smoothing**: For each data point, scans all points in time window
2. **O(n×m) interpolation**: For each raw timestamp (n), binary searches smoothed data (m)
3. **No caching**: Same computations repeated for each chart
4. **Synchronous computation**: Blocks even though wrapped in Task.Run

**Current Algorithm**:

```csharp
// CreateSmoothedData - O(n²) for large windows
foreach (var point in data) {
    var window = data.Where(p => IsInWindow(p, point)).ToList(); // ⚠️ O(n) per point
    var smoothed = CalculateAverage(window);
}

// InterpolateSmoothedData - O(n×log(m)) per chart
foreach (var timestamp in rawTimestamps) {  // n iterations
    var value = InterpolateValueAtTimestamp(sortedSmoothed, timestamp); // O(log m) binary search
}
```

**Impact**:

- **10,000 records**: ~500ms per chart × 6 charts = 3 seconds
- **100,000 records**: ~5 seconds per chart × 6 charts = 30 seconds

**ROI**: ⭐⭐⭐ (Medium - High effort, significant benefit)

---

### 🟡 **MEDIUM: Sequential Chart Rendering**

**Location**: `RenderChartsFromLastContext()`

**Issues**:

1. **Charts render sequentially**: Each chart waits for previous to complete
2. **All charts render even if hidden**: Wasted computation
3. **No parallelization**: Independent charts could render concurrently
4. **UI thread blocking**: Rendering happens on UI thread (LiveCharts limitation)

**Current Pattern**:

```csharp
await RenderPrimaryChart(ctx);      // Wait
await RenderNormalized(ctx);        // Wait
await RenderDifference(ctx);        // Wait
await RenderRatio(ctx);             // Wait
await RenderWeeklyDistribution(ctx); // Wait
RenderWeeklyTrend(ctx);             // Wait
```

**Impact**:

- **6 charts × 500ms each** = 3 seconds sequential
- **Could be**: 6 charts × 500ms parallel = ~500ms total

**ROI**: ⭐⭐⭐ (Medium - Medium effort, good benefit)

---

### 🟢 **LOW: UI Thread Blocking**

**Location**: Chart rendering operations

**Issues**:

1. **LiveCharts requires UI thread**: Cannot be easily moved to background
2. **Synchronous operations**: Some operations block UI unnecessarily
3. **No progress indication**: User sees frozen UI during long loads

**Impact**:

- **User experience**: Appears unresponsive
- **Actual blocking**: Minimal (most work is async)

**ROI**: ⭐⭐ (Low - Low effort, UX improvement)

---

## Recommended Optimizations (Prioritized by ROI)

### **Priority 1: SQL Query Optimization** ⭐⭐⭐⭐⭐

**Effort**: Medium (2-3 days)  
**Impact**: High (60-70% improvement)  
**ROI**: Very High

#### 1.1 Add Result Limiting with Downsampling

**Approach**: Limit SQL results to reasonable size, downsample on client if needed

```csharp
// Option A: SQL-level limiting (fastest)
SELECT TOP 10000 NormalizedTimestamp, Value, Unit, Provider
FROM [dbo].[HealthMetrics]
WHERE ...
ORDER BY NormalizedTimestamp

// Option B: Intelligent sampling (better quality)
// Sample every Nth record based on date range
// 2 years = ~730 days → sample every 1-2 days for daily data
```

**Implementation**:

- Add `maxRecords` parameter to `GetHealthMetricsDataByBaseType()`
- Calculate optimal sample rate based on date range
- Apply sampling in SQL or immediately after query

**Files to Modify**:

- `DataFetcher.GetHealthMetricsDataByBaseType()` - Add limiting
- `MetricSelectionService.LoadMetricDataWithCmsAsync()` - Pass max records
- `MainWindowViewModel.LoadMetricDataAsync()` - Calculate sample rate

**Expected Improvement**: 70-90% reduction in query time for large datasets

---

#### 1.2 Add Database Indexes

**Approach**: Ensure proper indexes exist for common query patterns

**Required Indexes**:

```sql
CREATE NONCLUSTERED INDEX IX_HealthMetrics_Type_Subtype_Date
ON HealthMetrics (MetricType, MetricSubtype, NormalizedTimestamp)
INCLUDE (Value, Unit, Provider)
WHERE Value IS NOT NULL;
```

**Implementation**:

- Create migration script or verify indexes exist
- Document index requirements

**Expected Improvement**: 30-50% query speedup

---

#### 1.3 Optimize Query Structure

**Approach**: Reduce query complexity and improve execution plan

**Changes**:

- Remove unnecessary `WHERE 1=1` pattern
- Use parameterized queries (already done)
- Consider CTEs for complex date range logic

**Expected Improvement**: 10-20% query speedup

---

### **Priority 2: Reduce Data Materialization** ⭐⭐⭐⭐

**Effort**: Low (1 day)  
**Impact**: Medium (15-20% improvement)  
**ROI**: High

#### 2.1 Eliminate Redundant ToList() Calls

**Approach**: Materialize once, reuse lists

**Before**:

```csharp
var orderedData = _data.Where(...).OrderBy(...).ToList();
var dataList = orderedData.ToList(); // ⚠️ Redundant
var timestamps = dataList.Select(...).ToList();
```

**After**:

```csharp
var orderedData = _data.Where(...).OrderBy(...).ToList(); // Materialize once
var timestamps = orderedData.Select(...).ToList(); // Reuse
```

**Files to Modify**:

- `SingleMetricStrategy.cs`
- `CombinedMetricStrategy.cs`
- `MultiMetricStrategy.cs`
- All strategy classes

**Expected Improvement**: 15-20% memory reduction, 10% CPU reduction

---

#### 2.2 Use IReadOnlyList Instead of List

**Approach**: Pass immutable collections to prevent accidental modifications

**Benefits**:

- Clearer intent
- Prevents unnecessary copies
- Better performance characteristics

**Expected Improvement**: 5-10% memory reduction

---

#### 2.3 Lazy Materialization for Large Datasets

**Approach**: Only materialize when absolutely necessary

**Example**:

```csharp
// Don't materialize until needed
IEnumerable<HealthMetricData> filtered = _data.Where(...).OrderBy(...);

// Only materialize for operations that require it
if (filtered.Any()) { // Uses lazy evaluation
    var materialized = filtered.ToList(); // Materialize once
    // Use materialized for all subsequent operations
}
```

**Expected Improvement**: 20-30% memory reduction for large datasets

---

### **Priority 3: Optimize Computation Algorithms** ⭐⭐⭐

**Effort**: High (3-5 days)  
**Impact**: Medium (20-30% improvement)  
**ROI**: Medium

#### 3.1 Optimize Smoothing Algorithm

**Current**: O(n²) - scans all points for each point  
**Target**: O(n log n) or O(n) with sliding window

**Approach**: Use sliding window technique

```csharp
// Instead of scanning all points for each point
// Maintain a sliding window of points within time range
var window = new Queue<HealthMetricData>();
foreach (var point in data) {
    // Remove points outside window
    while (window.Count > 0 && IsOutsideWindow(window.Peek(), point)) {
        window.Dequeue();
    }
    window.Enqueue(point);
    var smoothed = CalculateAverage(window); // O(1) average calculation
}
```

**Files to Modify**:

- `MathHelper.CreateSmoothedData()`

**Expected Improvement**: 50-70% faster smoothing for large datasets

---

#### 3.2 Cache Computation Results

**Approach**: Cache smoothed data per dataset to avoid recomputation

**Implementation**:

```csharp
// Cache key: dataset hash + date range
private static Dictionary<string, List<SmoothedDataPoint>> _smoothedCache;

public static List<SmoothedDataPoint> CreateSmoothedData(
    List<HealthMetricData> data,
    DateTime from,
    DateTime to)
{
    var cacheKey = $"{data.GetHashCode()}_{from:O}_{to:O}";
    if (_smoothedCache.TryGetValue(cacheKey, out var cached)) {
        return cached;
    }

    var result = ComputeSmoothedData(data, from, to);
    _smoothedCache[cacheKey] = result;
    return result;
}
```

**Expected Improvement**: 80-90% faster for repeated computations

---

#### 3.3 Optimize Interpolation

**Current**: O(n×log(m)) - binary search for each timestamp  
**Target**: O(n+m) - single pass with two pointers

**Approach**: Use two-pointer technique instead of binary search

```csharp
// Instead of binary search for each timestamp
// Use two pointers to traverse both lists once
int smoothedIndex = 0;
foreach (var timestamp in rawTimestamps) {
    // Advance smoothed pointer until we find bounding points
    while (smoothedIndex < smoothedData.Count - 1 &&
           smoothedData[smoothedIndex + 1].Timestamp <= timestamp) {
        smoothedIndex++;
    }
    var value = Interpolate(smoothedData[smoothedIndex],
                           smoothedData[smoothedIndex + 1],
                           timestamp);
}
```

**Expected Improvement**: 30-50% faster interpolation

---

### **Priority 4: Parallel Chart Rendering** ⭐⭐⭐

**Effort**: Medium (2 days)  
**Impact**: Medium (30-40% improvement)  
**ROI**: Medium

#### 4.1 Render Visible Charts in Parallel

**Approach**: Use `Task.WhenAll()` for independent chart renders

**Before**:

```csharp
await RenderPrimaryChart(ctx);
await RenderNormalized(ctx);
await RenderDifference(ctx);
// ... sequential
```

**After**:

```csharp
var tasks = new List<Task>();
if (IsMainVisible) tasks.Add(RenderPrimaryChart(ctx));
if (IsNormalizedVisible && hasSecondary) tasks.Add(RenderNormalized(ctx));
// ... add all visible charts
await Task.WhenAll(tasks);
```

**Files to Modify**:

- `MainWindow.xaml.cs` - `RenderChartsFromLastContext()`

**Expected Improvement**: 50-70% faster when multiple charts visible

---

#### 4.2 Skip Hidden Chart Computation

**Approach**: Don't compute strategies for hidden charts

**Current**: All charts compute, then visibility checked  
**Target**: Check visibility before computation

**Expected Improvement**: 20-30% faster when charts are hidden

---

### **Priority 5: UI Responsiveness** ⭐⭐

**Effort**: Low (1 day)  
**Impact**: Low (UX improvement)  
**ROI**: Low-Medium

#### 5.1 Add Progress Indicator

**Approach**: Show loading progress during data fetch and computation

**Implementation**:

- Add progress bar or spinner
- Update progress during async operations
- Use `IProgress<T>` for progress reporting

**Expected Improvement**: Better UX, no performance gain

---

#### 5.2 Background Processing with Cancellation

**Approach**: Ensure all heavy operations are truly async

**Implementation**:

- Verify `ConfigureAwait(false)` where appropriate
- Add cancellation token support
- Ensure UI thread is never blocked

**Expected Improvement**: Better responsiveness, no performance gain

---

## Implementation Plan

### **Phase 1: Quick Wins (Week 1)**

1. ✅ Add result limiting to SQL queries (Priority 1.1)
2. ✅ Eliminate redundant ToList() calls (Priority 2.1)
3. ✅ Skip hidden chart computation (Priority 4.2)

**Expected Total Improvement**: 40-50% faster

---

### **Phase 2: Algorithm Optimization (Week 2-3)**

1. ✅ Optimize smoothing algorithm (Priority 3.1)
2. ✅ Optimize interpolation (Priority 3.3)
3. ✅ Add computation caching (Priority 3.2)

**Expected Total Improvement**: Additional 20-30% faster

---

### **Phase 3: Parallelization (Week 4)**

1. ✅ Parallel chart rendering (Priority 4.1)
2. ✅ Verify database indexes (Priority 1.2)

**Expected Total Improvement**: Additional 10-20% faster

---

### **Phase 4: Polish (Week 5)**

1. ✅ Progress indicators (Priority 5.1)
2. ✅ Code cleanup and documentation

**Expected Total Improvement**: UX improvement only

---

## Measurement & Validation

### **Before Optimization**

- Baseline: Load 2 years of hourly data (~17,520 records)
- Measure: Total time from button click to all charts rendered
- Current: ~30-60 seconds (estimated)

### **After Optimization**

- Target: <10 seconds for same dataset
- Measure each phase independently
- Validate correctness (parity tests)

### **Metrics to Track**

1. SQL query execution time
2. Data materialization time
3. Strategy computation time per chart
4. Total rendering time
5. Memory usage (peak and average)
6. UI thread blocking time

---

## Risk Assessment

### **Low Risk**

- Eliminating redundant ToList() calls
- Skipping hidden chart computation
- Adding progress indicators

### **Medium Risk**

- SQL result limiting (must ensure data quality)
- Parallel chart rendering (must ensure thread safety)
- Algorithm optimization (must validate correctness)

### **High Risk**

- Database index changes (requires DBA coordination)
- Caching implementation (must handle cache invalidation)

---

## Notes

- **Data Quality**: Any optimization that reduces data must maintain visual accuracy
- **Backward Compatibility**: Optimizations should not break existing functionality
- **Testing**: All optimizations must pass existing parity tests
- **Incremental**: Implement one optimization at a time, measure, validate, then proceed

---

**Next Steps**:

1. Review and approve optimization priorities
2. Start with Phase 1 (Quick Wins)
3. Measure baseline performance
4. Implement and validate each optimization
5. Document performance improvements
