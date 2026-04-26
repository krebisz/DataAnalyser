# AI BOOTSTRAP ENTRY
**Status:** Bootstrap / Routing Entry Point  
**Scope:** Minimal workspace grounding, document routing, escalation levels, and default loading discipline  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `DataVisualiser-Architectural-Vocabulary.md`, `Project Roadmap.md`, and `MASTER_OPERATING_PROTOCOL.md`  
**Applies To:** AI assistant initialization and ongoing task routing for the `DataAnalyser` solution  
**Last Updated:** 2026-04-26

---

## 1. Purpose

This document is the main entry point for AI-assisted work on the `DataAnalyser` solution.

It exists to:

- provide a minimal but sufficient starting context
- prevent loading the entire document corpus by default
- route the assistant to the correct existing document(s) only when needed
- reduce contextual drift, token waste, and unnecessary rehydration cost
- preserve authority discipline without requiring a full document-registry system
- route architectural vocabulary, ownership-container, and promoted-concept questions to the correct conceptual source

This document is intentionally small.  
It does **not** replace higher-authority documents.  
It tells the assistant **where to go next**.

Its routing role remains useful after current implementation work is complete: it preserves how future assistants should re-enter the project, classify tasks, and avoid drifting away from the established architecture.

If a conflict exists:

1. `Project Bible.md` wins on architectural law
2. `SYSTEM_MAP.md` wins on structural boundaries
3. `DataVisualiser-Architectural-Vocabulary.md` wins on canonical architectural grammar, promoted concepts, ownership containers, and do-not-confuse distinctions
4. `Project Roadmap.md` wins on sequencing and phase legality
5. `MASTER_OPERATING_PROTOCOL.md` wins on execution discipline
6. `Project Overview.md` provides descriptive orientation
7. `DataVisualiser_Subsystem_Plan.md` provides active subsystem execution authority
8. this document only governs bootstrap loading and routing

---

## 2. Minimal Current Truth

Unless newer authoritative documents explicitly replace this state, assume the following:

- Solution root: `C:\Development\POCs\DataAnalyser\`
- Primary active subsystem: `DataVisualiser`
- Current project direction: canonical data reasoning platform centered on a reasoning engine, not chart-specific architecture
- Active roadmap state: **Phase 6 closed; Phase 7 entry gate satisfied; pre-Phase-7 render-plan primer closed as a preservation baseline**
- Active execution posture: **Conservative-Pragmatic**
- Default execution style:
  - one iteration must have one primary objective
  - prefer bounded slices
  - validation is required on every significant refactor
  - do not open more than one live-behavior risk front at a time
- Known current debt concentrations:
  - `MainChartsView`
  - `SyncfusionChartsView`
  - managed legacy / VNext coexistence
- Current architectural direction:
  - Phase 7 capability expansion should proceed through forward stages, not by reopening closed Phase 5/6/6.3 or pre-Phase-7 work
  - authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, and governance/evidence are the current ownership containers
  - `DataVisualiser-Architectural-Vocabulary.md` is the canonical conceptual grammar for promoted concepts, ownership containers, and do-not-confuse distinctions
- Default validation commands:
  1. `dotnet build DataAnalyser.sln -c Debug`
  2. `dotnet test DataVisualiser.Tests\DataVisualiser.Tests.csproj -c Debug -m:1`

This document assumes the current working reality already described in the attached April 2026 documentation set.

---

## 3. Core Project Identity

Treat the project as the following:

- a trustworthy canonical data reasoning environment
- a system that preserves raw truth, assigns meaning declaratively, computes deterministically, and exposes provenance explicitly
- a platform whose reasoning engine is the architectural center
- a system where authority and provenance stay upstream and explicit
- a system where reasoning grows through reusable capability and composition rather than chart-specific feature exceptions
- a system where consumer-agnostic downstream contracts sit between engine output and concrete clients
- a system where charts, exports, APIs, and future clients are consumer families, not semantic authorities
- a system where interaction is contract-mediated rather than hidden in event/controller convenience
- a system where presentation and rendering are terminal, replaceable delivery infrastructure
- a project maintained by one person, so bounded execution and disciplined context use matter
- a project whose future architectural decisions should use shared vocabulary rather than ad hoc organic interpretation

Never reduce the project to “just a charting application” or “just a reporting tool”.

---

## 4. Non-Negotiable Working Constraints

Always preserve these truths unless a higher-authority document explicitly changes them:

1. Truth comes before insight.
2. Raw data must not be silently altered.
3. Meaning is assigned declaratively, not inferred.
4. Canonical semantics remain authoritative.
5. Authority flows downward only:
   `Truth -> Reasoning/Derivation -> Interpretation -> Process -> Contracts -> Consumers -> Terminal Delivery`
6. Rendering and UI layers are downstream consumers/delivery infrastructure, not semantic authorities.
7. Consumer-agnostic output contracts outrank presentation convenience; concrete clients must not become the primary owners of analytical output shape.
8. Capability is not the same as feature delivery; new user-visible power should strengthen reusable reasoning capability where possible.
9. Composition belongs upstream of terminal delivery unless explicitly proven otherwise.
10. Interaction is not event wiring; it should cross boundaries through explicit contracts.
11. Overlays are interpretive outputs, not rendering conveniences.
12. Legacy may remain as compatibility / fallback / projection during migration, but not as forward architectural truth.
13. Closed Phase 5/6/6.3 and pre-Phase-7 work are preservation baselines, not active workstreams.
14. No currently shipped capability may be removed accidentally during refactor work.
15. Execution reachability must be observable.
16. Architectural vocabulary must remain stable: capability, composition, consumer, interaction, boundary, overlay, provenance, and authority must not collapse into their common-but-wrong neighbors.
17. The assistant must avoid loading more context than the task truly requires.

---

## 5. Default Bootstrap Load

For a normal task, the assistant should begin with:

1. this document
2. `DataVisualiser_Subsystem_Plan.md` **Section 1.5** (“START HERE - Handoff and Execution Defaults”)

That is the default minimal execution context for most `DataVisualiser` work.

Also load `DataVisualiser-Architectural-Vocabulary.md` when the task involves:
- ownership-container placement
- promoted concepts
- do-not-confuse distinctions
- target hierarchy
- concept collision
- boundary migration
- architectural naming or responsibility classification

Do not track new progress against closed historical phases or closed primer work unless the user explicitly asks for historical review.

Do **not** load the full document set by default unless the task explicitly requires cross-boundary reasoning, rehydration, or phase/legality review.

Protocol, workflow, and collaboration documents should not be treated as obsolete merely because the current development mode changes. They remain available escalation references for future agents, recovery cases, multi-agent work, and audit-safe alignment.

---

## 6. Routing Table — Read Only What the Task Needs

Use the following routing rules.

### 6.1 Architectural law or invariant questions
Read:
- `Project Bible.md`

Use when:
- the task may alter architectural law
- canonical semantics, provenance, determinism, reversibility, or identity law are involved
- there is uncertainty about what is absolutely forbidden

Do not read only for:
- local UI bugfixes
- narrow implementation cleanup
- generated artifact lookup

---

### 6.2 Structural boundaries, layers, or execution-flow questions
Read:
- `SYSTEM_MAP.md`

Use when:
- the task touches process/execution, reasoning/capability, contracts, rendering, terminal delivery, presentation, consumer/interaction, or boundary placement
- the task crosses layers or ownership containers
- there is uncertainty about where code should live
- rendering contracts, delivery boundaries, consumer-agnostic contracts, interaction contracts, overlays, or analytical/chart-program placement are involved

Do not read only for:
- local implementation details that do not cross structural seams

---

### 6.3 Architectural vocabulary, ownership containers, or concept-language questions
Read:
- `DataVisualiser-Architectural-Vocabulary.md`

Use when:
- the task asks what a concept means architecturally
- promoted concepts are involved: Authority, Provenance, Capability, Composition, Consumer, Interaction, Boundary, Overlay
- there is risk of confusing capability with feature, consumer with presentation, interaction with event, composition with builder, boundary with layer, overlay with rendering, provenance with diagnostics, or authority with orchestration
- a responsibility must be classified into an ownership container
- a proposed type, module, or refactor needs target-hierarchy placement
- the task concerns concept collision, vocabulary reduction, naming, or architectural grammar

Do not treat this as:
- architectural law
- sequencing authority
- execution governance
- implementation instruction

---

### 6.4 Phase legality, closure, or “what may proceed next”
Read:
- `Project Roadmap.md`

Use when:
- deciding whether work is allowed now
- checking closure status
- checking whether a capability is open, blocked, deferred, or closed
- deciding whether a future idea belongs on the current critical path
- confirming that closed phases or closed preservation baselines are not being reopened accidentally

Do not read only for:
- local bugfixes that clearly stay within already-permitted work

---

### 6.5 Execution discipline, grounding, failure, recovery, or change protocol
Read:
- `MASTER_OPERATING_PROTOCOL.md`

Use when:
- the task requires strict execution discipline
- recovery mode, grounding failure, or drift is in question
- implementation guidance must be structured for safety
- there is confusion about how work should proceed rather than what the system is

Do not read only for:
- passive orientation

---

### 6.6 Project orientation or high-level understanding
Read:
- `Project Overview.md`

Use when:
- the assistant needs a descriptive explanation of the system
- the task is conceptual or orienting rather than execution-heavy
- broader capability direction matters
- the user wants descriptive orientation rather than implementation or sequencing authority

Do not treat this as:
- architectural law
- sequencing authority
- execution governance

---

### 6.7 Active `DataVisualiser` implementation and current execution posture
Read:
- `DataVisualiser_Subsystem_Plan.md`

Read first:
- **Section 1.5** for default handoff state
- deeper sections only if the task requires them

Use when:
- the task is inside `DataVisualiser`
- current execution defaults, current debt, active phase work, or subsystem-specific constraints matter
- the assistant needs the current “working truth” for the main active subsystem
- the assistant must preserve the direction that presentation becomes thinner and more replaceable over time
- the assistant must preserve forward-stage tracking instead of reopening Phase 5/6/6.3 or closed primer work
- the assistant must classify responsibilities through the current ownership containers when boundary placement matters

When the classification itself is uncertain, pair this with `DataVisualiser-Architectural-Vocabulary.md`.

This is the highest-value practical working document for most nontrivial `DataVisualiser` tasks.

For architectural migration progress questions:
- use `DataVisualiser-Architectural-Vocabulary.md` for the accepted progress estimate and conceptual interpretation
- use `DataVisualiser_Subsystem_Plan.md` for execution-plan implications and provider/consumer boundary audit targets
- use `Project Roadmap.md` only when sequencing, closure, or phase legality is being decided

---

### 6.8 Workspace initialization, refresh, or rehydration
Read:
- `Workspace Workflow.md`

Use when:
- starting a new AI workspace
- refreshing a stale one
- checking the rehydration bundle
- declaring workspace state
- deciding what documents must be present for initialization

Do not use as architectural authority.

---

### 6.9 Quick interaction alignment only
Read:
- `Collaboration Protocol.md`

Use when:
- a lightweight behavioral reminder is enough
- the user needs quick-reference collaboration rules

Do not use for:
- authority decisions
- governance conflicts
- architecture or sequencing disputes

---

### 6.10 Values, framing, and trade-off interpretation
Read:
- `Project Philosophy.md`

Use when:
- framing long-term intent
- weighing tradeoffs where multiple valid downstream options exist
- reminding the assistant what kind of system this is trying to become

Do not use as execution or architectural authority.

---

### 6.11 Future ideas, backlog, and optional enhancements
Read:
- `TODO Enhancements.md`

Use when:
- discussing possible future capability expansion
- triaging ideas
- checking whether an enhancement is already noted

Do not treat this as:
- roadmap authority
- approval to begin implementation
- current critical path

---

### 6.12 Structural lookup only
Read only when lookup is actually needed:
- `codebase-index.md`
- `dependency-summary.md`
- `project-tree.txt`

Use when:
- locating files
- locating symbols
- checking declared dependencies
- navigating the codebase quickly

These are lookup artifacts, not standing context.

Do **not** preload them for ordinary reasoning.

---

## 7. Escalation Levels

Use the smallest sufficient level.

### Level 0 — Bootstrap Only
Load:
- this document

Use for:
- very small clarification
- deciding where to route next
- determining whether more context is needed at all

---

### Level 1 — Normal Working Context
Load:
- this document
- `DataVisualiser_Subsystem_Plan.md` Section 1.5

Optionally add:
- `DataVisualiser-Architectural-Vocabulary.md`, if the task touches ownership containers, promoted concepts, or architectural naming

Use for:
- most `DataVisualiser` tasks
- bounded refactors
- local implementation work
- active subsystem maintenance

This should be the default.

---

### Level 2 — Boundary-Aware Context
Load:
- Level 1
- plus **one** of:
  - `Project Bible.md`
  - `SYSTEM_MAP.md`
  - `DataVisualiser-Architectural-Vocabulary.md`
  - `Project Roadmap.md`
  - `MASTER_OPERATING_PROTOCOL.md`

Use when:
- the task clearly crosses into architecture, structure, ownership containers, vocabulary/grammar, legality, or governance

Do not load multiple governing documents unless the task actually requires it.

---

### Level 3 — Deep / Cross-Cutting Context
Load:
- Level 2
- plus:
  - `Project Overview.md`, if descriptive orientation helps
  - generated lookup artifacts, only if locating files/symbols/dependencies is necessary
  - concern-specific or future documents, if later added

Use when:
- work is complex, cross-cutting, or ambiguous
- the task spans subsystem execution plus architecture plus structural lookup

This level should be the exception, not the norm.

---

## 8. Default “Do Not Load” List

Unless the task requires them, do **not** load by default:

- `codebase-index.md`
- `dependency-summary.md`
- `project-tree.txt`
- `TODO Enhancements.md`
- `Project Philosophy.md`
- `Collaboration Protocol.md`
- `Workspace Workflow.md`

They are useful, but not default context.

---

## 9. Recommended Assistant Workflow

For each new task, proceed in this order:

1. Read this document.
2. Classify the task:
   - local implementation
   - subsystem execution
   - architecture
   - structural boundary
   - architectural vocabulary / concept grammar
   - ownership-container placement
   - roadmap / legality
   - governance / recovery
   - orientation
   - lookup
   - ideation
3. Start at the lowest escalation level that can safely answer the task.
4. Pull in only the matching document(s).
   - use `DataVisualiser-Architectural-Vocabulary.md` for concept-language, ownership-container, or do-not-confuse questions
5. Avoid broad rehydration unless the task genuinely requires it.
6. If the task becomes cross-cutting, escalate deliberately rather than preloading everything.
7. If the task touches completed Phase 5/6/6.3 or pre-Phase-7 work, treat those as closed baselines unless the user explicitly asks for historical review.
8. If generated lookup artifacts are needed, use them as lookup only, not as standing reasoning context.

---

## 10. Suggested Future Evolution

This bootstrap document is intentionally transitional.

Future evolution may add:

- a formal document registry
- automated routing based on architectural vocabulary / ownership-container concern
- explicit concern-level navigation documents
- task packs
- automated document load rules
- a deeper hierarchy for subsystem or concern-specific routing

Until then, this file is the main disciplined entry point.

---

## 11. Practical Command to the Assistant

When beginning work in this project, behave as if the following instruction has been issued:

> Start with `AI_BOOTSTRAP_ENTRY.md`.
> Use it to classify the task and load only the minimum necessary additional documents.
> Default to `DataVisualiser_Subsystem_Plan.md` Section 1.5 for ordinary `DataVisualiser` work.
> Escalate to higher-authority or broader documents only when the task clearly requires architecture, structure, legality, governance, or lookup support.
> Do not load generated artifacts, philosophy, backlog, or collaboration quick-reference by default.
> When reasoning about architecture, default to treating charts/UI/rendering as downstream, replaceable consumers over engine-owned and contract-owned output.
> Treat Phase 5, Phase 6, Phase 6.3, and the pre-Phase-7 render-plan primer as closed preservation baselines unless the user explicitly asks for historical review.
> For boundary questions, classify responsibilities through the current ownership containers: authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, and governance/evidence.
> Load `DataVisualiser-Architectural-Vocabulary.md` when the task depends on promoted concepts, ownership containers, target hierarchy, concept collision, or do-not-confuse distinctions.

---

## 12. End State

This document is successful if it causes the assistant to:

- stop loading the whole corpus by default
- start from current execution truth
- preserve authority discipline
- avoid reopening closed historical phases by accident
- route itself correctly into existing documents
- use the architectural vocabulary document when conceptual grammar or ownership language matters
- use deep context only when justified

It is not a substitute for the project's real governing documents.
It is the gatekeeper for loading them efficiently.
