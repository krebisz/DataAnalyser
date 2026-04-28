# DataVisualiser Governance Constraints Audit

Recorded: 2026-04-28

Phase: 13 - Add Governance Constraints

## Scope

This audit turns the reduced architectural grammar into reviewable constraints for future growth. It does not add runtime behavior. It defines the checks that future vocabulary, concepts, capabilities, transformations, consumers, delivery backends, and evidence paths must satisfy before implementation expands again.

Inputs inspected:

- `documents/DataVisualiser-Architectural-Vocabulary.md`
- `documents/DataVisualiser_Migration_Plan_and_Guardrails.md`
- Phase 5 through Phase 12 audit notes
- existing architecture guardrail tests

## Reduced Grammar Anchor

Future additions should map to the promoted grammar:

```text
Authority
-> Envelope
-> Provenance
-> Intent
-> Capability
-> Composition
-> Interpretation
-> Confidence
-> Overlay
-> Program
-> Contract
-> Boundary
-> Neutrality
-> Qualification
-> Provider
-> Consumer
-> Interaction
-> SurfaceModel
-> Binding
-> Delivery
-> Evidence
-> Audit
```

The governed extension pool remains open, but additions must improve architectural force rather than naming preference.

## Growth Guardrails

New vocabulary must improve clarity in at least one promoted grammar area. It should be rejected when it only renames an implementation detail, duplicates an existing concept, or introduces aesthetic vocabulary without a responsibility boundary.

New concepts must declare ownership, misuse boundaries, and their relation to the reduced grammar. Concepts that hide authority, policy, provider selection, delivery mechanics, or evidence control should be rejected.

New capabilities must enter through intent, capability, composition/transformation, program, contract, boundary, qualification, and provider/consumer seams. Controllers, renderers, terminal delivery, evidence, and diagnostics must not become capability owners.

New transformations must preserve provenance, traceability, determinism, and reversibility where possible. Lossy transformations must be explicit and inspectable rather than hidden in projection, rendering, or export code.

New consumers must receive authoritative output through contracts, boundaries, surface models, bindings, or equivalent consumer-neutral shapes. Consumers and interactions must relay behavior without redefining semantic truth or provider policy.

New delivery backends must remain terminal and replaceable. Backend or vendor code must not define semantics, capability planning, interpretation, confidence policy, evidence policy, or upstream contracts.

New evidence paths must remain observational. Evidence may prove, compare, diagnose, validate, export, and audit; it must not route live rendering, select providers/backends, mutate semantic output, or become hidden runtime authority.

## Review Gate

Before implementing new architectural growth, the work should answer:

```text
Which promoted grammar concept owns the change?
Which boundary or contract does it cross?
What provenance, traceability, fidelity, determinism, or reversibility is preserved?
What qualification or compatibility rule applies?
Which consumer-neutral shape exists before terminal delivery?
What evidence or audit path proves the behavior?
Which old hub is explicitly prevented from absorbing the responsibility?
```

## Static Guardrail Additions

Phase 13 adds architecture tests that verify:

- the vocabulary document retains the governed extension pool and growth/rejection criteria
- the migration plan keeps governance before capability expansion
- this audit records growth guardrails for vocabulary, concepts, capabilities, transformations, consumers, backends, and evidence paths

These tests are documentation-backed because Phase 13 governs future growth. Existing source guardrails from Phases 6 through 12 continue to enforce the current code boundaries.

## Findings

The plan is ready to move toward Phase 14 only if new capability work uses this governance gate first. The most important implementation consequence is that Phase 14 should select a narrow capability slice and prove it through the target spine, rather than adding behavior through UI, rendering, evidence, or a transitional hub.

No production refactor is justified in Phase 13.
