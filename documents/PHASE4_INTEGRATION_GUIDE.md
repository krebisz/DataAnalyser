# Phase 4 Integration Guide: DataVisualiser CMS Consumption

## Overview

This document describes the Phase 4 integration path for enabling DataVisualiser to consume Canonical Metric Series (CMS) data alongside the existing legacy HealthMetricData path.

## Architecture

### Dual Data Paths

The system now supports two parallel data paths:

1. **Legacy Path** (Default):

   - `DataFetcher` → `HealthMetricData` → Chart Strategies
   - Uses string-based `MetricType`/`MetricSubtype` identifiers
   - Direct SQL queries to `HealthMetrics` table

2. **CMS Path** (Opt-in):
   - `CmsDataService` → `ICanonicalMetricSeries` → Chart Strategies
   - Uses canonical metric identities (`metric.body_weight`, `metric.sleep`)
   - Converts stored `HealthMetric` records to CMS via `HealthMetricToCmsMapper`

### Key Components

#### 1. CmsDataService

**Location**: `DataVisualiser/Data/Repositories/CmsDataService.cs`

Provides CMS-based data access:

- `GetCmsByCanonicalIdAsync()` - Fetches CMS by canonical identity
- `GetHealthMetricDataFromCmsAsync()` - Converts CMS to HealthMetricData for compatibility
- `IsCmsAvailableAsync()` - Checks CMS availability

#### 2. CmsConfiguration

**Location**: `DataVisualiser/State/CmsConfiguration.cs`

Configuration system for CMS opt-in:

- Global `UseCmsData` flag
- Per-strategy enablement flags
- `ShouldUseCms(strategyType)` method for conditional CMS usage

#### 3. CmsTypeConverter

**Location**: `DataFileReader/Canonical/CmsTypeConverter.cs`

Converts between internal normalization CMS types and consumer-facing interface:

- `ToConsumerCms<TValue>()` - Converts `CanonicalMetricSeries<TValue>` to `ICanonicalMetricSeries`
- Bridges normalization pipeline output to consumer interface

#### 4. Updated SingleMetricStrategy

**Location**: `DataVisualiser/SingleMetricStrategy.cs`

Proof-of-concept CMS-aware strategy:

- New constructor accepting `ICanonicalMetricSeries`
- `ComputeFromCms()` method for CMS-based computation
- Maintains backward compatibility with legacy `HealthMetricData` constructor

## Usage

### Enabling CMS for a Strategy

```csharp
// Enable CMS globally
CmsConfiguration.UseCmsData = true;

// Enable CMS for specific strategy
CmsConfiguration.UseCmsForSingleMetric = true;

// Or check programmatically
if (CmsConfiguration.ShouldUseCms(nameof(SingleMetricStrategy)))
{
    // Use CMS path
    var cmsService = new CmsDataService(connectionString);
    var cmsData = await cmsService.GetCmsByCanonicalIdAsync("metric.body_weight", from, to);
    var strategy = new SingleMetricStrategy(cmsData.First(), label, from, to);
}
else
{
    // Use legacy path
    var dataFetcher = new DataFetcher(connectionString);
    var legacyData = await dataFetcher.GetHealthMetricsDataByBaseType("weight", null, from, to);
    var strategy = new SingleMetricStrategy(legacyData, label, from, to);
}
```

### Canonical Metric ID Mapping

Current mappings (Phase 3):

- `"metric.body_weight"` → `MetricType: "weight"`, `MetricSubtype: null`
- `"metric.sleep"` → `MetricType: "com.samsung.shealth.sleep"`, `MetricSubtype: null`

## Migration Strategy

### Phase 4.1: Infrastructure (Complete)

- ✅ CMS data service created
- ✅ Configuration system in place
- ✅ Type converter available
- ✅ Public API for CMS components

### Phase 4.2: Proof of Concept (Complete)

- ✅ SingleMetricStrategy updated with CMS support
- ✅ Backward compatibility maintained

### Phase 4.3: Gradual Migration (Future)

1. Update remaining strategies one by one:

   - MultiMetricStrategy
   - CombinedMetricStrategy
   - DifferenceStrategy
   - RatioStrategy
   - NormalizedStrategy

2. Update UI to use canonical identities:

   - Replace `MetricType`/`MetricSubtype` strings with canonical IDs
   - Add semantic validation for metric compatibility

3. Add CMS storage (Future Phase):
   - Store CMS directly in database
   - Eliminate HealthMetric → CMS conversion step

## Benefits

1. **Semantic Clarity**: Canonical identities eliminate ambiguity
2. **Type Safety**: Strong typing prevents semantic mismatches
3. **Gradual Migration**: Parallel paths allow incremental adoption
4. **Backward Compatibility**: Legacy code continues to work
5. **Future-Proof**: Foundation for Phase 5 (Derived Metrics)

## Constraints

- CMS path currently reads from legacy `HealthMetrics` table
- Conversion overhead (minimal, but present)
- Only Weight and Sleep metrics supported in Phase 3
- No forced migration - opt-in only

## Next Steps

1. Complete Phase 3: Ensure all metric families have identity resolution
2. Expand CMS support: Add more metric families to `CanonicalMetricIdentityResolver`
3. Migrate strategies: Update remaining strategies to support CMS
4. UI integration: Add canonical identity selection to UI
5. Performance: Consider caching CMS conversions

## See Also

- `Project Roadmap.md` - Overall phase planning
- `SYSTEM_MAP.md` - System architecture
- `Project Bible.md` - Architectural constraints
