# 📙 PROJECT BIBLE — DataFileReaderRedux

(Architectural Law — Authoritative, Normative, Binding)

---

## 1. Purpose

This document defines the **non-negotiable architectural laws** governing the DataFileReaderRedux system.

All refactors, extensions, features, and experiments **must comply** with this Bible.

This document:
- defines *what must be true*
- constrains *where logic may live*
- prevents architectural drift

It does **not** describe implementation details or workflows.

---

## 2. System Identity

### 2.1 Core Identity

The system is a **general analytical engine**, composed of two coordinated subsystems:

- **Subsystem A — DataFileReader**
  - Ingestion
  - Normalization
  - Unification

- **Subsystem B — DataVisualiser**
  - Computation
  - Rendering
  - Interaction

Together, these subsystems form a pipeline:

```
Raw Data
 → Ingestion
 → Normalization
 → Canonical Metric Series
 → Computation Strategy
 → Rendering
 → UI
```

---

## 3. Architectural Values (Binding)

The system is governed by the following values:

1. **Generalization over specialization**
2. **Explicit boundaries over convenience**
3. **Centralized meaning over distributed inference**
4. **Inspectability over automation magic**
5. **Determinism over implicit behavior**
6. **Evolution via extension, not mutation**

Any change violating these values is architecturally invalid.

---

## 4. Canonical Ingestion Law (Phase 2 Binding)

### 4.1 Single Canonical Ingestion Pipeline

All data entering the system **must** pass through the following canonical pipeline:

```
Source
 → Parser
 → Raw Record
 → Normalization Pipeline
 → Canonical Metric Series
```

No alternative ingestion paths are permitted.

---

### 4.2 Parser Law — Structural Extraction Only

Parsers **MUST**:
- read external sources (files, databases, APIs)
- traverse schemas and record layouts
- extract raw values without modification
- preserve source metadata and provenance

Parsers **MUST NOT**:
- assign metric identity
- normalize values or units
- interpret timestamps or timezones
- apply aggregation or smoothing
- embed domain semantics

Any semantic logic in a parser is a violation.

---

### 4.3 Raw Record Law — Lossless Neutrality

Raw records **MUST**:
- be lossless representations of source data
- remain domain-neutral
- remain unnormalized
- retain full provenance

Raw records **MUST NOT**:
- encode meaning
- collapse ambiguity
- perform interpretation

Raw records exist solely to decouple parsing from meaning.

---

### 4.4 Normalization Law — Centralized Semantic Authority

All semantic interpretation **MUST** occur in the normalization pipeline.

Normalization **MUST** be the sole authority for:
- metric identity resolution
- time normalization
- timezone handling
- unit normalization and conversion
- value coercion
- dimensional context assignment
- data quality annotation

Semantic logic **MUST NOT** exist:
- in parsers
- in helpers
- in computation strategies
- in rendering code
- in UI or ViewModels

---

### 4.5 Metric Identity Law — Single Resolution Point

Metric identity **MUST**:
- be resolved exactly once
- occur during normalization
- result in a canonical metric key

After normalization:
- metric identity is immutable
- downstream components may not reinterpret identity
- no string heuristics are permitted outside normalization

Metric identity resolution **MAY** use:
- raw field names
- source metadata
- explicit domain mappings

Metric identity resolution **MUST NOT** rely on:
- duplicated logic
- implicit conventions
- downstream inference

---

### 4.6 Time Normalization Law — Canonical Time Guarantee

All time handling **MUST** occur during normalization.

Normalization **MUST** resolve:
- instant vs interval semantics
- timezone normalization
- temporal resolution
- aggregation boundaries

After normalization:
- downstream components assume time is canonical
- no strategy or renderer may correct time

Incorrect time handling is an ingestion defect.

---

### 4.7 Canonical Metric Series Law — Downstream Contract

The output of ingestion **MUST** be a canonical metric series.

A canonical metric series **MUST** guarantee:
- resolved metric identity
- canonical time axis
- normalized values and units
- explicit dimensions and context
- preserved provenance

All downstream components **MUST** operate exclusively on this representation.

---

## 5. Computation Law

### 5.1 Strategy Isolation

All analytical behavior **MUST** be expressed as computation strategies.

Strategies **MUST**:
- consume canonical metric series
- produce computation results
- remain free of ingestion concerns

Strategies **MUST NOT**:
- parse raw data
- infer metric meaning
- normalize values or time

---

### 5.2 Computation Engine Law

The computation engine **MUST**:
- orchestrate strategies
- manage multi-metric coordination
- remain independent of rendering

The computation engine **MUST NOT**:
- embed visualization logic
- perform ingestion or normalization

---

## 6. Rendering Law

Rendering components **MUST**:
- consume computation results only
- remain ignorant of data origin
- remain ignorant of ingestion semantics

Rendering components **MUST NOT**:
- compute analytical values
- reinterpret metrics
- depend on parser-specific details

---

## 7. State Law

State models **MUST**:
- represent user-visible system condition
- remain separate from computation and ingestion

State models **MUST NOT**:
- embed analytical logic
- perform normalization
- infer semantic meaning

---

## 8. Extension Law

New functionality **MUST** be introduced via:
- new parsers (structure-only)
- new normalization mappings
- new computation strategies
- new rendering adapters

New functionality **MUST NOT**:
- modify core ingestion semantics
- introduce cross-layer shortcuts
- bypass canonical pipelines

---

## 9. Evolutionary Direction (Binding Constraints)

Future phases **MUST**:
- preserve ingestion boundaries
- centralize semantic authority
- favor composition over mutation

Reflective, adaptive, or emergent behavior **MUST**:
- operate on canonical metric series
- remain external to ingestion logic

---

## 10. Enforcement

Any change that violates this Bible:
- is architecturally invalid
- must be revised or reverted

Silently proceeding under violation is prohibited.

---

**End of Project Bible**

