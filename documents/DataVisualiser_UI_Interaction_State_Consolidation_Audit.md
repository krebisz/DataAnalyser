# DataVisualiser UI / Interaction / State Consolidation Audit

Phase: 31 - Thin UI / Interaction / State Layer

Date: 2026-05-01

Purpose:

```text
Track UI, interaction, and state responsibilities that still carry semantic, provider, evidence, or bridge policy, then move only behavior already replaced by VNext-native consumption contracts.
```

## Current Classification

```text
UI relay:
- MainChartsView tab, toggle, event, and command coordination
- chart controller adapters dispatching user-selected options to orchestration/rendering contracts
- OperationChainWorkbenchView display surface

State relay:
- ChartState visibility, selected subtype/options, diagnostics snapshots, render-plan history, and session/performance records
- MainWindowViewModel state setters and chart visibility coordination

Interaction relay:
- tooltip display helpers
- zoom/reset and chart visibility handlers
- transform panel operation selection UI
```

## Remaining Transitional Responsibilities

```text
ChartDataContext still exists in UI-facing adapters as the loaded-data carrier.
VNextDataResolutionHelper remains a UI-adjacent bridge for family-specific VNext data resolution.
ChartState still stores diagnostics and LastContext for evidence/export compatibility.
Tooltip helpers still read terminal chart surfaces to explain rendered values.
These are not removed in this slice because Phase 31 only moves responsibilities that are already replaced by VNext-native consumption output.
```

## Completed Slice

```text
OperationChainWorkbenchView no longer projects OperationChainResult into display rows directly.
OperationChainWorkbenchPresenter now converts authoritative OperationChainResult output into summary, trace rows, dataset rows, and evidence text.
The workbench view remains a display relay: it binds prepared presentation output and does not execute operations, resolve data, access ChartDataContext, or import vendor chart types.
```

## Guardrail Result

```text
ArchitectureGuardrailTests.OperationChainWorkbench_ShouldKeepExecutionOutsideUiSurface now requires OperationChainWorkbenchPresenter and blocks projection logic in the view code-behind.
OperationChainWorkbenchPresenterTests.Build_ShouldProjectResultToDisplayRowsWithoutExecutingOperations proves the projection is deterministic and independent of execution.
```

## Deferrals

```text
Do not remove ChartDataContext from UI adapters until replacement request/context surfaces exist for the relevant families.
Do not remove VNextDataResolutionHelper until its remaining family-specific bridge duties have named replacements.
Do not move tooltip logic upstream; tooltips remain explanatory terminal interaction behavior unless a later phase defines contract-mediated interaction output.
Do not move evidence export into live UI behavior; evidence remains observational.
```
