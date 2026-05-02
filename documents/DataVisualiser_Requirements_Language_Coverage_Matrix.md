# DataVisualiser Requirements-to-Language Coverage Matrix

Phase: 36 — Requirements-to-Language Coverage Matrix

Generated: 2026-05-02

## Purpose

Map the known requirements, planned constructions, and implemented production capabilities onto the formal architectural language grammar. Identify coverage gaps, semantic collapses, ambiguities, and language terms with no current expression.

## Formal Language Reference

Eight concern layers from the architectural source context:

```text
Layer 1 — Authority:    Authority / Semantics / Provenance / Traceability / Envelope
Layer 2 — Fidelity:     Fidelity / Determinism / Reversibility / Constraint / Governance
Layer 3 — Computation:  Intent / Capability / Composition / Transformation / Interpretation / Confidence / Overlay
Layer 4 — Structure:    Program / Policy / Contract / Boundary / Neutrality / Qualification
Layer 5 — Consumer:     Provider / Consumer / Interaction / SurfaceModel / Binding
Layer 6 — Adapter:      Projection / Adapter / Resolver / Selector / Registry
Layer 7 — Delivery:     Delivery / Backend / RuntimeBoundary / VendorBoundary / Lifecycle
Layer 8 — Evidence:     Evidence / Diagnostics / Parity / Reachability / Validation / Audit / Record
```

## Coverage Statuses

- **covered** — requirement maps directly to a named language term with a concrete implementation
- **partially covered** — requirement maps to a language term but expression is incomplete or collapsed into another term
- **collapsed** — two or more distinct language terms share the same implementation type or path
- **ambiguous** — the requirement could map to multiple language terms without clear canonical assignment
- **missing** — a language term exists but no current implementation or requirement addresses it
- **deferred** — implementation is planned (bounded bridge retirement, Operation Chain workflow) but not yet active

---

## Requirements-to-Language Coverage Matrix

### R1 — Load metric data from external service

| Language Term | Coverage | Notes |
| --- | --- | --- |
| Provider | covered | `MetricSelectionService`, `MetricSelectionServiceSeriesLoader` |
| Backend | covered | service-backed metric loading through `MetricLoadSnapshotGateway` |
| Delivery | covered | `MetricLoadSnapshot` delivered to reasoning |
| Authority | covered | canonical meaning originates in VNext/CMS-facing contracts, not in UI |
| Provenance | covered | `ProvenanceDescriptor` carried in `VNextUiConsumptionContract` |

### R2 — Execute analytical reasoning over loaded metric data

| Language Term | Coverage | Notes |
| --- | --- | --- |
| Intent | covered | `AnalyticalIntent`, `AnalyticalIntentFactory`, `AnalyticalIntentSet` |
| Capability | covered | `IAnalyticalCapabilityContract` generalized across families |
| Composition | covered | `ReasoningSessionCoordinator`, `OperationChainExecutor` |
| Program | covered | `ChartProgram`, `ChartProgramRequest`, `ChartProgramKind` |
| Qualification | covered | `ChartProgramDeliveryTargetResolver` resolves qualified delivery target |
| Authority | covered | reasoning is upstream; UI/adapters/delivery do not own authority |
| Semantics | partially covered | metric title, series labels, and units carried but not formalized as a `Semantics` type |
| Determinism | partially covered | same inputs produce same outputs by construction; not formally expressed as an invariant |

### R3 — Produce consumer-neutral surface output from reasoning

| Language Term | Coverage | Notes |
| --- | --- | --- |
| SurfaceModel | covered | `ConsumerSurfaceModel` produced before terminal delivery |
| Neutrality | covered | `ConsumerSurfaceModel` is consumer-neutral; downstream adapters read it |
| Contract | covered | `VNextUiConsumptionContract` carries full consumption metadata |
| Consumer | covered | chart-family adapters are the consumers; Operation Chain executor is a non-chart consumer |
| Binding | partially covered | adapters bind to specific render pipelines via `VNextUiConsumptionContract.ConsumerDeliveryContract`; no explicit `Binding` type |

### R4 — Deliver chart output through family-specific render pipelines

| Language Term | Coverage | Notes |
| --- | --- | --- |
| Delivery | covered | `ConsumerDeliveryContract`, family render invokers, chart view rendering |
| VendorBoundary | covered | Syncfusion is isolated as a terminal vendor boundary |
| RuntimeBoundary | covered | UI thread / WPF rendering is the runtime boundary for terminal delivery |
| Adapter | covered | `BarPieChartControllerAdapter`, `DistributionChartControllerAdapter`, etc. |
| Projection | covered (bridge) | `LegacyChartProgramProjector` is a bounded production bridge; native surface output is partially active |
| Lifecycle | partially covered | chart refresh lifecycle managed through `ChartPresentationSpine`; no formal `Lifecycle` type |
| Overlay | partially covered | `OverlayPlan` exists in `VNextUiConsumptionContract`; no active overlay rendering wired |

### R5 — Carry family-specific capability metadata through the render chain

| Language Term | Coverage | Notes |
| --- | --- | --- |
| Capability | covered | `IAnalyticalCapabilityContract` generalized; per-family types extend it |
| Contract | covered | capability contracts thread through render requests to invokers |
| Qualification | covered | provider qualification confirmed per family |
| Interpretation | partially covered | series labels, metric types, and display names carried in `ChartDataContext` and `ConsumerSurfaceModel`; no distinct `Interpretation` type |
| Confidence | missing | no `Confidence` or reliability indicator is expressed in current constructs |

### R6 — Preserve evidence of each load/render cycle

| Language Term | Coverage | Notes |
| --- | --- | --- |
| Evidence | covered | `EvidenceDiagnosticsBuilder`, `EvidenceExportService`, reachability exports |
| Parity | covered | parity builders observe legacy vs VNext output alignment |
| Reachability | covered | `documents/reachability-*.json` exports confirm live render-plan history |
| Audit | covered | `AdminSessionMilestoneRecorder`, `DistributionSessionMilestoneRecorder`, etc. |
| Diagnostics | covered | runtime path diagnostics, performance timings, failure reasons |
| Record | covered | `LoadRuntimeState`, family runtime records |
| Validation | covered | focused test suites validate each migration phase |
| Traceability | covered | `ProvenanceDescriptor`, `LoadRequestSignature`, `SnapshotSignature`, `ProgramSourceSignature` |

### R7 — Support Operation Chain derived dataset execution

| Language Term | Coverage | Notes |
| --- | --- | --- |
| Composition | covered | `OperationChainExecutor` composes operations over derived dataset |
| Transformation | covered | transformation lossiness and reversibility metadata preserved |
| Reversibility | covered | reversibility metadata in `ConsumerSurfaceModel` for Operation Chain |
| Fidelity | covered | lossiness and fidelity metadata carried in derived dataset surface output |
| SurfaceModel | covered | derived dataset output is expressed through `ConsumerSurfaceModel` |
| Consumer | partially covered | Operation Chain workbench UI is display-only; interactive workflow is deferred |
| Interaction | deferred | no interactive user-authored operation chain is wired in the UI tab |
| Provenance | covered | provenance and trace metadata preserved in Operation Chain output |

### R8 — Bound and govern what is "new" analytical work

| Language Term | Coverage | Notes |
| --- | --- | --- |
| Governance | covered | `ArchitectureGuardrailTests` enforce structural invariants |
| Constraint | covered | guardrail sections in Migration Plan; enforced in tests |
| Boundary | covered | explicit boundary between VNext reasoning, adapters, rendering, and evidence |
| Policy | partially covered | `ChartProgramRequest` / `ChartProgramKind` acts as policy for what program is produced; no formal `Policy` type |
| Envelope | missing | no `Envelope` type or enveloped contract construct is expressed |
| Authority | covered | authority remains upstream; explicit guardrails prevent UI/adapters from owning authority |

---

## Missing Language Records

| Language Term | Layer | Status | Notes |
| --- | --- | --- | --- |
| `Confidence` | Computation | missing | No reliability or confidence indicator expressed in current constructs. Relevant when analytical results have variable quality or when data coverage is partial. |
| `Envelope` | Authority | missing | No enveloped contract or envelope type exists. Envelope could formalize what a given contract guarantees as its outer boundary (e.g., "this program is always available under these conditions"). |
| `Policy` | Structure | partially covered | `ChartProgramRequest` acts as a program-selection policy, but is not named or typed as a `Policy`. Could formalize which programs are produced under which conditions. |
| `Registry` | Adapter | partially covered | Selector/resolver patterns exist but no explicit `Registry` type or registry lookup mechanism is expressed. |
| `Lifecycle` | Delivery | partially covered | Chart refresh lifecycle is managed through `ChartPresentationSpine` and WPF event flow but is not formalized as a `Lifecycle` or `LifecycleStage` construct. |
| `Interpretation` | Computation | partially covered | Series labels, metric types, and display names are carried, but no distinct `Interpretation` type separates semantic labeling from raw metric data. |

---

## Collapsed Concern Records

| Collapse | Terms Involved | Current Expression | Risk |
| --- | --- | --- | --- |
| Contract and Capability conflation | `Contract`, `Capability` | `IAnalyticalCapabilityContract` fuses capability description with contract carriage | Low — generalization was earned by repeated family slices; but the two concerns could separate if capability description diverges from contract shape |
| SurfaceModel and Consumer overlap | `SurfaceModel`, `Consumer` | `ConsumerSurfaceModel` names both concern and its consumer-neutral orientation | Low — naming is intentional and descriptive |
| Projection bridge collapses Adapter and Projection | `Adapter`, `Projection` | `LegacyChartProgramProjector` is both a projection type and adapter-adjacent bridge | Low — bounded compatibility; no authority confusion |
| Evidence and Validation share test infrastructure | `Evidence`, `Validation` | focused test suites and reachability exports both express "evidence"; test isolation is partial | Medium — over-counting evidence in test validation could mask real observable divergence |

---

## Ambiguity Risk Records

| Ambiguity | Terms Involved | Description |
| --- | --- | --- |
| `Transformation` vs `Projection` | `Transformation`, `Projection` | Both transform a value from one shape to another. Formal distinction: Transformation carries reversibility/lossiness semantics and is analytical; Projection is a non-authoritative translation for compatibility or rendering. This distinction is held informally but not enforced by type. |
| `Provider` vs `Backend` | `Provider`, `Backend` | Both describe the upstream metric loading path. Provider emphasizes "who supplies the data"; Backend emphasizes "what serves it at runtime". Both map to `MetricSelectionService` and `MetricLoadSnapshotGateway`. The formal distinction is present in vocabulary but not type-enforced. |
| `Delivery` vs `Consumer` | `Delivery`, `Consumer` | Delivery describes what the output format/transport is; Consumer describes who receives it. Both appear in `ConsumerDeliveryContract` which fuses the two. No current ambiguity causes harm, but separation may be needed for multi-consumer delivery. |

---

## Formal Expression Gaps

| Gap | Notes |
| --- | --- |
| No formal `Confidence` type | Analytical result quality is implicit. Confidence gaps may surface when data is sparse or when series overlap is uncertain. |
| No formal `Envelope` | The outer boundary guarantee of each contract is implicit in the types it carries, not expressed as a separate concern. |
| `Overlay` is defined in `OverlayPlan` within `VNextUiConsumptionContract` but is not rendered or activated | Overlay rendering is not wired. The language term exists in the contract shape but has no production consumer. |
| `Interaction` is declared in `VNextUiConsumptionContract.InteractionRequest` but the Operation Chain UI is display-only | Interactive user-authored workflows are deferred. The language term has no active production expression. |
| `Lifecycle` has no explicit type | WPF/Syncfusion lifecycle events drive chart refresh, but no formal lifecycle stage model governs them. |

---

## Coverage Summary

| Layer | Terms | Covered | Partially Covered | Missing | Deferred |
| --- | --- | --- | --- | --- | --- |
| Authority | Authority, Semantics, Provenance, Traceability, Envelope | 4 | 1 (Semantics) | 1 (Envelope) | 0 |
| Fidelity | Fidelity, Determinism, Reversibility, Constraint, Governance | 4 | 1 (Determinism) | 0 | 0 |
| Computation | Intent, Capability, Composition, Transformation, Interpretation, Confidence, Overlay | 4 | 2 (Interpretation, Overlay) | 1 (Confidence) | 0 |
| Structure | Program, Policy, Contract, Boundary, Neutrality, Qualification | 5 | 1 (Policy) | 0 | 0 |
| Consumer | Provider, Consumer, Interaction, SurfaceModel, Binding | 4 | 1 (Binding) | 0 | 1 (Interaction) |
| Adapter | Projection, Adapter, Resolver, Selector, Registry | 4 | 1 (Registry) | 0 | 0 |
| Delivery | Delivery, Backend, RuntimeBoundary, VendorBoundary, Lifecycle | 4 | 1 (Lifecycle) | 0 | 0 |
| Evidence | Evidence, Diagnostics, Parity, Reachability, Validation, Audit, Record | 7 | 0 | 0 | 0 |
| **Total** | **40** | **36** | **8** | **2** | **1** |

Overall language coverage: **36 / 40 terms covered or partially covered** (90%).

Missing: `Confidence`, `Envelope`.

Deferred: `Interaction` (Operation Chain interactive workflow).

---

## Phase 36 Assessment

The formal architectural language is substantially covered by the current implementation. The primary gaps are:

1. `Confidence` — not expressed. Would require an explicit analytical quality or reliability indicator on computation output.
2. `Envelope` — not expressed. Would require an outer boundary guarantee construct on contracts.
3. `Interaction` — deferred. Exists in `InteractionRequest` and the Operation Chain workbench tab, but no interactive user workflow is wired.

The primary collapses are:
- `Contract` and `Capability` fused in `IAnalyticalCapabilityContract` — low risk.
- Evidence and validation overlap — medium risk of audit masking.

These gaps and collapses are candidates for Phase 37 Construction Algebra Baseline work, which must decide which formal constructions are needed and which gaps remain tolerated.
