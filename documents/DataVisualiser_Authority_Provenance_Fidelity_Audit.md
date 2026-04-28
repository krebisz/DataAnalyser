# DataVisualiser Authority, Provenance, and Fidelity Audit

Recorded: 2026-04-28

Phase: 5 - Lock Authority / Semantics / Provenance / Fidelity

## Scope

This audit inspects whether analytical meaning, authority, provenance, traceability, fidelity, confidence, and interpretation state survive the current VNext handoffs without being silently dropped or mutated.

Inputs inspected:

- `documents/DataVisualiser_Migration_Plan_and_Guardrails.md`
- `documents/DataVisualiser-Architectural-Vocabulary.md`
- `DataVisualiser/VNext/Contracts`
- `DataVisualiser/VNext/Application`
- `DataVisualiser/VNext/Rendering`
- `DataVisualiser/Core/Rendering/Adapters`
- `DataVisualiser.Tests/VNext`
- `DataVisualiser.Tests/UI/MainHost`

## Authority and Provenance Carriers

The current authority/provenance-carrying structures are:

- `MetricSelectionRequest` and `MetricSeriesRequest`, which define the requested metric sources and stable selection identity.
- `MetricLoadSnapshot` and `MetricSeriesSnapshot`, which carry loaded series data and the selection signature used by downstream validation.
- `ProvenanceDescriptor`, which records source signature, authority, trust class, metadata, and a derived provenance signature.
- `AnalyticalIntent`, which binds selection request, program request, provenance, delivery, capability, overlays, and interactions into an intent-level contract.
- `ChartProgram`, which preserves source signature on the canonical analytical program.
- `AnalyticalExecutionResult`, which binds intent, load snapshot, and program and rejects mismatched source signatures or program kinds.
- `AnalyticalResultSet`, which groups execution results while preserving per-result intent and provenance.
- `ChartRenderPlan`, which carries rendered shape plus provider, vocabulary, consumer, delivery, capability, provenance, and intent metadata.
- `ChartRenderAdapterResult`, which carries adapter output plus metadata copied from the render plan.
- `ConfidenceAnnotationSet`, `OverlayPlan`, and `AnalyticalInterpretationResult`, which preserve confidence and interpretation metadata as annotations around the canonical execution result.

## Major Seam Crossings

The main authority/provenance path is:

```text
MetricSelectionRequest
-> AnalyticalIntent
-> MetricLoadSnapshot
-> ChartProgram
-> AnalyticalExecutionResult
-> ChartRenderPlan
-> ChartRenderAdapterResult
-> diagnostics / evidence export
```

The current compatibility path is:

```text
ChartProgram
-> LegacyChartProgramProjector
-> ChartDataContext
-> existing UI/chart-family adapters
```

The compatibility path preserves `ChartProgram.SourceSignature` as `ChartDataContext.LoadRequestSignature`, but it does not preserve the full VNext intent/provenance envelope. This is acceptable only as a transitional bridge and should not be treated as the long-term authority model.

## Envelope-Like Carriers

Envelope-like carriers already exist at the main contract points:

- `AnalyticalIntent` is the request/intent envelope.
- `AnalyticalExecutionResult` is the canonical execution result envelope.
- `AnalyticalResultSet` is the multi-result envelope.
- `ChartRenderPlan` is the surface/delivery envelope.
- `ChartRenderAdapterResult` is the adapter delivery result envelope.
- Interpretation and confidence structures are annotation envelopes around execution results, not replacements for canonical results.

No new production envelope type is required for Phase 5. The immediate gap was guardrail coverage and documentation, not missing code structure.

## Semantic Preservation

`AnalyticalExecutionResult` enforces the critical semantic invariants at construction time:

- intent selection signature must match the load snapshot selection signature
- load snapshot selection signature must match the chart program source signature
- chart program kind must match the intent program request kind

`ChartRenderPlanProjector` preserves intent and provenance metadata when projecting from an `AnalyticalExecutionResult`. The metadata includes:

- intent signature
- provenance signature
- consumer kind
- delivery target
- capability kind
- composition kind
- overlay count
- interaction count
- provider key
- provider signature

`ChartRenderPlanVocabularyMetadata` adds vocabulary and provider metadata for legacy-family render plan builders. Production render adapters then copy render-plan metadata into adapter results.

## Fidelity and Traceability

Transformations and projections are traceable, but not all are reversible.

`ChartProgramPlanner` preserves source signatures on derived programs and series. This supports traceability back to the source selection, but derived outputs are not guaranteed to be reversible to the original raw input. This is acceptable where derivation is explicit and the source signature is preserved.

Render density policy can reduce rendered points for viewport or aggregation modes. These projections are intentionally not lossless; they are annotated through density metadata such as source count, rendered count, mode, and tolerance. They must continue to be treated as rendered views, not replacements for canonical analytical truth.

`LegacyChartProgramProjector` is a compatibility projection. It preserves a source/load request signature but drops richer VNext intent and provenance structure. This remains a transitional bridge risk to revisit when legacy/VNext coexistence is reduced.

## Confidence and Interpretation

Confidence is currently modeled as annotation rather than canonical result mutation.

`ConfidenceAnnotationEvaluator` creates `ConfidenceAnnotationSet` values from a `ChartProgram` without changing the program. `AnalyticalInterpretationBuilder` returns the original `AnalyticalExecutionResult` and can build a separate overlay program when explicit policy excludes critical confidence series from overlays. Overlay plans mark themselves non-authoritative and carry the source signature.

Phase 5 tests now prove that confidence annotations and interpretation overlays do not replace or mutate canonical program truth.

## Tests Added or Strengthened

Phase 5 strengthened VNext guardrails in:

- `DataVisualiser.Tests/VNext/AnalyticalIntentContractsTests.cs`
- `DataVisualiser.Tests/VNext/AnalyticalInterpretationBuilderTests.cs`

New or expanded coverage proves:

- cartesian render-plan projection from `AnalyticalExecutionResult` preserves provider metadata
- hierarchy render-plan projection from `AnalyticalExecutionResult` preserves provenance, vocabulary, delivery, capability, composition, and provider metadata
- interpretation preserves the original execution result and chart program when confidence annotations are present
- confidence remains annotation metadata and overlays remain non-authoritative

Validation:

```text
dotnet test DataVisualiser.Tests\DataVisualiser.Tests.csproj -c Debug -m:1 --no-restore --filter "FullyQualifiedName~AnalyticalIntentContractsTests|FullyQualifiedName~AnalyticalInterpretationBuilderTests|FullyQualifiedName~ChartRenderPlanProjectorTests|FullyQualifiedName~ChartRenderPlanAdapterTests"

Passed: 70
Failed: 0
Skipped: 0
```

## Findings

Phase 5 is satisfied for the current VNext authority/provenance handoffs. The existing production code already carries the required semantic metadata across the main VNext result and render-plan boundaries.

The remaining risks are transitional rather than immediate Phase 5 blockers:

- legacy compatibility projection preserves only reduced provenance
- derived analytical programs are traceable but not fully reversible
- density projection is intentionally lossy and must remain clearly annotated

These risks should be carried into later containment and projection/translation phases rather than expanded in Phase 5.
