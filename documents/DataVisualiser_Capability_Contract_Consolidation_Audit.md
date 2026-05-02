# DataVisualiser Capability / Contract Consolidation Audit

Phase: 33 - Consolidate Capability / Contract Families

Date: 2026-05-02

Purpose:

```text
Compare migrated capability and delivery contracts, consolidate only proven common shape, and preserve real family-specific distinctions.
```

## Compared Families

```text
Distribution
WeekdayTrend
BarPie
Transform
SyncfusionSunburst
Main / Normalized / Difference / Ratio CartesianMetric
MovingAverage
Operation Chain
```

## Shared Shape

```text
The migrated family capability contracts all carry:
- ChartProgramRequest
- CapabilityRequest
- ConsumerDeliveryContract

This shape is now represented by IAnalyticalCapabilityContract.
```

## Preserved Differences

```text
Distribution remains SingleSeries / Distribution / Chart delivery.
WeekdayTrend remains SingleSeries / TemporalTrend / Chart delivery.
BarPie remains SingleSeries / Distribution / Chart delivery with BarPie-specific render routes.
Transform remains DerivedSeries / Transform / Chart delivery.
SyncfusionSunburst remains Hierarchy / Hierarchy / HierarchyChart delivery.
CartesianMetric remains restricted to Main, Normalized, Difference, and Ratio.
MovingAverage remains DerivedSeries / Smoothing / Chart delivery with a non-default provider path.
Operation Chain remains a multi-step derived-series workflow that produces derived datasets and export/workbench delivery, not a chart-family capability contract.
```

## Consolidation Decision

```text
Consolidated:
- common capability-contract shape through IAnalyticalCapabilityContract

Not consolidated:
- family-specific Create(...) factories
- render request records
- route/backend qualification records
- Operation Chain request/program/trace/result contracts
- VNext consumption-contract builders

Reason:
These areas still encode real family differences or terminal delivery details.
Flattening them now would create a new mega-object or hide legitimate distinctions.
```

## Test Evidence

```text
CapabilityContractConsolidationTests.CapabilityContracts_ShouldShareCommonContractShapeWithoutFlatteningFamilyDifferences
```
