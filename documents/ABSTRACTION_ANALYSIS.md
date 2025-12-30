# Abstraction Analysis: Strategy/Distribution/Service Layers

## Executive Summary

Analysis of all strategy, distribution, and service layers reveals **8 major abstraction opportunities** that would:
1. **Eliminate duplication** across 16+ strategy files
2. **Enable extensibility** for future phases
3. **Unify cut-over logic** (critical for migration)
4. **Standardize data handling** (CMS vs Legacy)
5. **Enable advanced functionality** (caching, batching, parallelization)

---

## Current State: Duplication and Fragmentation

### Duplicated Patterns Across Strategies

#### 1. **Data Preparation & Filtering** (Duplicated in 10+ strategies)
```csharp
// Pattern repeated in: SingleMetricStrategy, CombinedMetricStrategy, 
// DifferenceStrategy, RatioStrategy, NormalizedStrategy, etc.
private List<HealthMetricData> FilterAndOrder(IEnumerable<HealthMetricData> source)
{
    return source
        .Where(d => d != null && d.Value.HasValue && 
                   d.NormalizedTimestamp >= _from && 
                   d.NormalizedTimestamp <= _to)
        .OrderBy(d => d.NormalizedTimestamp)
        .ToList();
}
```
**Current**: `StrategyComputationHelper.FilterAndOrderByRange()` exists but not all strategies use it consistently.

**Abstraction Needed**: `IDataPreparationService` with:
- Filtering by date range
- Null value handling
- Ordering
- CMS-to-legacy conversion (unified)
- Validation

---

#### 2. **Timeline & Interval Generation** (Duplicated in ALL strategies)
```csharp
// Pattern repeated in EVERY strategy:
var dateRange = _to - _from;
var tickInterval = MathHelper.DetermineTickInterval(dateRange);
var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(_from, _to, tickInterval);
var intervalIndices = timestamps.Select(ts => 
    MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();
```
**Current**: Logic exists in `MathHelper` but strategies manually compose it.

**Abstraction Needed**: `ITimelineService` with:
- Unified timeline generation
- Interval calculation
- Index mapping
- Timezone handling (critical for CMS)
- Caching (for performance)

---

#### 3. **Data Alignment** (Duplicated in 5+ strategies)
```csharp
// Pattern repeated in: CombinedMetricStrategy, DifferenceStrategy, 
// RatioStrategy, NormalizedStrategy
private static (List<DateTime> Timestamps, List<double> Primary, List<double> Secondary)
    AlignByIndex(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right, int count)
{
    // Complex alignment logic duplicated
}
```
**Current**: Each strategy implements its own alignment.

**Abstraction Needed**: `IDataAlignmentService` with:
- Index-based alignment
- Timestamp-based alignment
- Gap filling strategies (NaN, interpolation, last value)
- Multi-series alignment (for 3+ metrics)
- CMS-aware alignment (timezone handling)

---

#### 4. **Smoothing** (Duplicated in 8+ strategies)
```csharp
// Pattern repeated in: SingleMetricStrategy, CombinedMetricStrategy, 
// DifferenceStrategy, RatioStrategy, NormalizedStrategy
private List<double> CreateSmoothedSeries(List<HealthMetricData> orderedData, List<DateTime> timestamps)
{
    var smoothedData = MathHelper.CreateSmoothedData(orderedData, _from, _to);
    return MathHelper.InterpolateSmoothedData(smoothedData, timestamps);
}
```
**Current**: Logic exists in `MathHelper` but strategies manually compose it.

**Abstraction Needed**: `ISmoothingService` with:
- Multiple smoothing algorithms (moving average, exponential, etc.)
- Interpolation strategies
- Caching (smoothing is expensive)
- Configurable window sizes
- CMS-aware smoothing (timezone handling)

---

#### 5. **CMS Conversion** (Duplicated in 3+ strategies)
```csharp
// Pattern repeated in: SingleMetricStrategy, CombinedMetricCmsStrategy
var healthMetricData = CmsConversionHelper.ConvertSamplesToHealthMetricData(
    _cmsData, _from, _to).ToList();
```
**Current**: Conversion happens in strategies, but should be in orchestration layer.

**Abstraction Needed**: `ICmsConversionService` with:
- CMS-to-legacy conversion (when needed)
- Legacy-to-CMS conversion (for parity)
- Timezone normalization
- Unit conversion
- Validation

---

#### 6. **Cut-Over Logic** (Fragmented across 4+ locations)
```csharp
// Pattern fragmented in: MainWindow, WeeklyDistributionService, 
// ParityValidationService, ChartDataContextBuilder
if (ctx.PrimaryCms != null) { /* use CMS */ } else { /* use legacy */ }
```
**Current**: No unified cut-over mechanism.

**Abstraction Needed**: `IStrategyCutOverService` with:
- Single decision point for all strategies
- Parity validation integration
- Configuration flag support
- Fallback mechanisms
- Logging/observability

---

#### 7. **Unit Resolution** (Duplicated in 5+ strategies)
```csharp
// Pattern repeated in: SingleMetricStrategy, CombinedMetricStrategy, 
// DifferenceStrategy, RatioStrategy, NormalizedStrategy
private string? ResolveUnit(List<HealthMetricData> left, List<HealthMetricData> right)
{
    return left.FirstOrDefault()?.Unit ?? right.FirstOrDefault()?.Unit;
}
```
**Current**: Simple logic but duplicated.

**Abstraction Needed**: `IUnitResolutionService` with:
- Unit extraction from data
- Unit validation
- Unit conversion (future)
- CMS unit handling (more authoritative)

---

#### 8. **Chart Result Construction** (Duplicated in ALL strategies)
```csharp
// Pattern repeated in EVERY strategy:
return new ChartComputationResult
{
    Timestamps = timestamps,
    IntervalIndices = intervalIndices,
    NormalizedIntervals = normalizedIntervals,
    PrimaryRawValues = primaryRaw,
    PrimarySmoothed = primarySmoothed,
    SecondaryRawValues = secondaryRaw,
    SecondarySmoothed = secondarySmoothed,
    TickInterval = tickInterval,
    DateRange = dateRange,
    Unit = Unit
};
```
**Current**: Manual construction in every strategy.

**Abstraction Needed**: `IChartResultBuilder` with:
- Builder pattern for result construction
- Validation
- Default value handling
- Extension points for custom fields

---

## Proposed Abstractions

### 1. IDataPreparationService

**Purpose**: Unified data preparation, filtering, and conversion.

**Interface**:
```csharp
public interface IDataPreparationService
{
    // Filter and order legacy data
    IReadOnlyList<HealthMetricData> PrepareLegacyData(
        IEnumerable<HealthMetricData>? source, 
        DateTime from, 
        DateTime to);
    
    // Filter and order CMS data (returns CMS, not converted)
    IReadOnlyList<ICanonicalMetricSample> PrepareCmsData(
        ICanonicalMetricSeries? cms, 
        DateTime from, 
        DateTime to);
    
    // Convert CMS to legacy (when needed for compatibility)
    IReadOnlyList<HealthMetricData> ConvertCmsToLegacy(
        ICanonicalMetricSeries cms, 
        DateTime from, 
        DateTime to);
    
    // Validate data
    bool ValidateData(IReadOnlyList<HealthMetricData> data);
    bool ValidateCmsData(ICanonicalMetricSeries cms);
}
```

**Benefits**:
- ✅ Unified filtering logic
- ✅ CMS conversion in one place
- ✅ Timezone handling centralized
- ✅ Validation standardized

---

### 2. ITimelineService

**Purpose**: Unified timeline and interval generation.

**Interface**:
```csharp
public interface ITimelineService
{
    // Generate unified timeline
    TimelineResult GenerateTimeline(DateTime from, DateTime to, IReadOnlyList<DateTime>? dataTimestamps = null);
    
    // Map timestamps to intervals
    IReadOnlyList<int> MapToIntervals(IReadOnlyList<DateTime> timestamps, TimelineResult timeline);
    
    // Cache management
    void ClearCache();
}

public class TimelineResult
{
    public TimeSpan DateRange { get; }
    public TickInterval TickInterval { get; }
    public IReadOnlyList<DateTime> NormalizedIntervals { get; }
}
```

**Benefits**:
- ✅ Consistent timeline generation
- ✅ Caching for performance
- ✅ Timezone handling
- ✅ Reusable across strategies

---

### 3. IDataAlignmentService

**Purpose**: Unified data alignment across multiple series.

**Interface**:
```csharp
public interface IDataAlignmentService
{
    // Align two series by index
    AlignmentResult AlignByIndex<T>(
        IReadOnlyList<T> left, 
        IReadOnlyList<T> right,
        Func<T, DateTime> timestampSelector,
        Func<T, double> valueSelector);
    
    // Align multiple series by timestamp
    MultiSeriesAlignmentResult AlignByTimestamp(
        IReadOnlyList<IReadOnlyList<HealthMetricData>> series,
        DateTime from,
        DateTime to);
    
    // Gap filling strategies
    IReadOnlyList<double> FillGaps(
        IReadOnlyList<double> values,
        GapFillingStrategy strategy);
}

public enum GapFillingStrategy
{
    NaN,
    LastValue,
    Interpolate,
    Zero
}
```

**Benefits**:
- ✅ Unified alignment logic
- ✅ Support for 3+ metrics
- ✅ Configurable gap filling
- ✅ CMS-aware alignment

---

### 4. ISmoothingService

**Purpose**: Unified smoothing and interpolation.

**Interface**:
```csharp
public interface ISmoothingService
{
    // Smooth single series
    IReadOnlyList<double> Smooth(
        IReadOnlyList<HealthMetricData> data,
        DateTime from,
        DateTime to,
        SmoothingAlgorithm algorithm = SmoothingAlgorithm.MovingAverage);
    
    // Interpolate smoothed data to target timestamps
    IReadOnlyList<double> Interpolate(
        IReadOnlyList<SmoothedDataPoint> smoothedData,
        IReadOnlyList<DateTime> targetTimestamps);
    
    // Smooth multiple series
    IReadOnlyList<IReadOnlyList<double>> SmoothMultiple(
        IReadOnlyList<IReadOnlyList<HealthMetricData>> series,
        DateTime from,
        DateTime to);
}

public enum SmoothingAlgorithm
{
    MovingAverage,
    Exponential,
    Polynomial,
    SavitzkyGolay
}
```

**Benefits**:
- ✅ Multiple smoothing algorithms
- ✅ Caching for performance
- ✅ Configurable algorithms
- ✅ Batch processing

---

### 5. IStrategyCutOverService

**Purpose**: Unified cut-over mechanism for all strategies.

**Interface**:
```csharp
public interface IStrategyCutOverService
{
    // Create strategy with cut-over logic
    IChartComputationStrategy CreateStrategy(
        StrategyType strategyType,
        ChartDataContext ctx,
        StrategyCreationParameters parameters);
    
    // Check if CMS should be used
    bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx);
    
    // Execute parity validation
    ParityResult ValidateParity(
        IChartComputationStrategy legacyStrategy,
        IChartComputationStrategy cmsStrategy);
}

public enum StrategyType
{
    SingleMetric,
    CombinedMetric,
    MultiMetric,
    Difference,
    Ratio,
    Normalized,
    WeeklyDistribution,
    WeekdayTrend
}
```

**Benefits**:
- ✅ Single cut-over point
- ✅ Parity validation integrated
- ✅ Configuration flag support
- ✅ Extensible for new strategies

---

### 6. IUnitResolutionService

**Purpose**: Unified unit resolution and validation.

**Interface**:
```csharp
public interface IUnitResolutionService
{
    // Resolve unit from legacy data
    string? ResolveFromLegacy(IReadOnlyList<HealthMetricData> data);
    
    // Resolve unit from CMS (more authoritative)
    string? ResolveFromCms(ICanonicalMetricSeries cms);
    
    // Resolve unit from multiple sources (with priority)
    string? ResolveFromMultiple(
        IReadOnlyList<HealthMetricData>? legacyData,
        ICanonicalMetricSeries? cms);
    
    // Validate unit consistency
    bool ValidateUnitConsistency(IReadOnlyList<string?> units);
}
```

**Benefits**:
- ✅ Unified unit resolution
- ✅ CMS priority handling
- ✅ Validation
- ✅ Future: unit conversion

---

### 7. IChartResultBuilder

**Purpose**: Unified chart result construction.

**Interface**:
```csharp
public interface IChartResultBuilder
{
    ChartResultBuilder ForTimeline(DateTime from, DateTime to);
    ChartResultBuilder WithTimestamps(IReadOnlyList<DateTime> timestamps);
    ChartResultBuilder WithPrimaryValues(IReadOnlyList<double> raw, IReadOnlyList<double> smoothed);
    ChartResultBuilder WithSecondaryValues(IReadOnlyList<double> raw, IReadOnlyList<double> smoothed);
    ChartResultBuilder WithUnit(string? unit);
    ChartComputationResult Build();
}

// Usage:
var result = _chartResultBuilder
    .ForTimeline(_from, _to)
    .WithTimestamps(timestamps)
    .WithPrimaryValues(primaryRaw, primarySmoothed)
    .WithSecondaryValues(secondaryRaw, secondarySmoothed)
    .WithUnit(unit)
    .Build();
```

**Benefits**:
- ✅ Builder pattern (fluent API)
- ✅ Validation
- ✅ Default values
- ✅ Extensible

---

### 8. IComputationPipeline (Orchestration)

**Purpose**: Unified computation pipeline for strategies.

**Interface**:
```csharp
public interface IComputationPipeline
{
    // Execute full pipeline: prepare → align → smooth → build result
    ChartComputationResult? Execute(
        IChartComputationStrategy strategy,
        ChartDataContext ctx);
    
    // Execute with caching
    ChartComputationResult? ExecuteWithCache(
        IChartComputationStrategy strategy,
        ChartDataContext ctx,
        string cacheKey);
    
    // Execute with parity validation
    ChartComputationResult? ExecuteWithParity(
        IChartComputationStrategy legacyStrategy,
        IChartComputationStrategy cmsStrategy,
        ChartDataContext ctx);
}
```

**Benefits**:
- ✅ Unified pipeline execution
- ✅ Caching support
- ✅ Parity integration
- ✅ Observability

---

## Implementation Strategy

### Phase 1: Core Abstractions (Foundation)
1. **IDataPreparationService** - Critical for CMS migration
2. **IStrategyCutOverService** - Critical for migration
3. **ITimelineService** - Used by all strategies

### Phase 2: Data Processing Abstractions
4. **IDataAlignmentService** - Used by multi-metric strategies
5. **ISmoothingService** - Used by all strategies
6. **IUnitResolutionService** - Simple but duplicated

### Phase 3: Result Construction
7. **IChartResultBuilder** - Builder pattern for results

### Phase 4: Pipeline Orchestration
8. **IComputationPipeline** - Full pipeline orchestration

---

## Benefits Summary

### Immediate Benefits
- ✅ **Eliminate duplication**: 50+ duplicated methods → 8 services
- ✅ **Unify cut-over logic**: Single decision point
- ✅ **Standardize CMS handling**: One place for conversion
- ✅ **Enable caching**: Performance improvements

### Future Benefits
- ✅ **Extensibility**: Easy to add new strategies
- ✅ **Testability**: Services can be mocked
- ✅ **Performance**: Caching, batching, parallelization
- ✅ **Advanced features**: Multiple smoothing algorithms, gap filling strategies

### Migration Benefits
- ✅ **CMS migration**: Unified conversion and handling
- ✅ **Parity validation**: Integrated into pipeline
- ✅ **Configuration**: Centralized flags
- ✅ **Observability**: Logging and metrics

---

## Next Steps

1. **Create abstraction interfaces** (Phase 1)
2. **Implement core services** (IDataPreparationService, IStrategyCutOverService)
3. **Refactor SingleMetricStrategy** (reference implementation)
4. **Apply to other strategies** (incremental)
5. **Add advanced features** (caching, multiple algorithms)

---

**Status**: Analysis complete, ready for implementation  
**Priority**: Phase 1 abstractions are critical for CMS migration

