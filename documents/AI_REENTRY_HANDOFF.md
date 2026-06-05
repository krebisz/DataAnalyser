# AI RE-ENTRY HANDOFF
**Status:** Session / machine-migration handoff (read this first on a new machine)  
**Created:** 2026-06-05  
**Solution:** `DataAnalyser` (`C:\Development\POCs\DataAnalyser\` on prior machine — path may differ)  
**Primary subsystem:** `DataVisualiser`

---

## 0. How to use this file

**You (human):** Point the AI at this document as the **first** context load after clone/checkout on a new machine.

**AI default workflow after reading this file:**

1. Run validation (Section 8).
2. Read `documents/AI_BOOTSTRAP_ENTRY.md` for document routing and authority order.
3. For implementation sequencing, use `documents/DataVisualiser_Migration_Plan_and_Guardrails.md`.
4. For structural law, use `documents/Project Bible.md` and `documents/SYSTEM_MAP.md`.
5. Do **not** assume this handoff replaces those documents — it captures **where we left off** in this consolidation thread.

---

## 1. What we were doing

**Goal:** Reduce folder sprawl and cognitive overhead while preserving behavior; standardise architectural layers/pipeline **without** perfect alignment.

**Posture:** Bounded passes — one primary objective per pass, `dotnet build` + `dotnet test` every significant refactor, manual smoke only when live UI/render/export changes.

---

## 2. Completed structural work (already in repo or merged with later commits)

### DataFileReader

| Before | After |
|--------|--------|
| `DataFileReader/Class/` | `DataFileReader/Models/` |
| `Class/JSON/` | `Models/Json/` |
| Namespace `DataFileReader.Class` | `DataFileReader.Models` |
| Namespace `DataFileReader.Class.JSON` | `DataFileReader.Models.Json` |

### Repo root / docs

- `log.md` moved to `documents/log.md` (update any personal bookmarks).

### DataVisualiser — pipeline spine (presentation load path)

- **`DataVisualiser/UI/ChartPresentationSpine.cs`** — thin façade for interactive load:
  - `LoadMetricDataIntoLastContextAsync` → `MainWindowViewModel.LoadMetricDataAsync`
  - `PublishLastContextAndRequestChartUpdate` → `LoadDataCommand.Execute`
- Used from **`MainChartsView`** and **`SyncfusionChartsView`** (Syncfusion may now live under `UI/Charts/Syncfusion/` — verify path after checkout).

### DataVisualiser — validation layout

- `Core/Helpers/Validation/` → **`Core/Validation/DataLoad/`**
- Namespace: **`DataVisualiser.Core.Validation.DataLoad`**
- Empty **`Core/Helpers`** removed.

### DataVisualiser — UI/Charts consolidation

Merged into **`UI/Charts/Presentation/`**:

- Former `Adapters/`, `Helpers/`, `Infrastructure/`
- Former `Rendering/` → **`Presentation/Rendering/`** (keeps `LiveCharts/`, `ECharts/` subfolders)

Namespaces:

- `DataVisualiser.UI.Charts.Presentation` — adapters, helpers, factory, registry, keys
- `DataVisualiser.UI.Charts.Presentation.Rendering` (+ `.LiveCharts`, `.ECharts`)

**Unchanged:** `UI/Charts/Controllers/`, `Converters/`, and (on current tree) additional folders such as `Interaction/`, `Syncfusion/` under Charts.

### Core → UI boundary (partial fix)

- **`ErrorEventArgs`** lives in **`DataVisualiser.Shared`** (`Shared/ErrorEventArgs.cs`), not `UI.Events`.
- **`MetricDataValidationHelper`** must **not** reference `DataVisualiser.UI.Events` (enforced by **`ArchitectureGuardrailTests.CoreValidation_DataLoadHelpers_ShouldNotReferenceUiEventsNamespace`**).

---

## 3. Presentation pipeline spine (happy path)

Use this as the mental model; legacy/side paths still exist.

```
UI validation (selection + dates)
  → MainWindowViewModel.LoadMetricDataAsync
  → MetricLoadCoordinator → MetricSelectionService → ChartDataContextBuilder
  → ChartState.LastContext / LoadedChartDataSnapshot (per migration plan)
  → ChartPresentationSpine.PublishLastContextAndRequestChartUpdate
  → DataLoaded + RequestChartUpdate
  → MainChartsViewChartPipelineFactory → ChartRenderingOrchestrator / coordinators
  → per-chart orchestration + terminal delivery (LiveCharts / Syncfusion / etc.)
```

**Key types:** `MetricLoadCoordinator`, `ChartDataContextBuilder`, `ChartRenderingOrchestrator`, `MainChartsViewChartPipelineFactory`, `ChartPresentationSpine`.

**Parallel / legacy (not the spine):** Syncfusion-only paths, per-adapter `LoadMetricDataAsync` calls, `DataFileReader` CLI ingest (meets app at DB/storage).

---

## 4. Architecture findings (still true)

### Critical debt (structural, not “app broken”)

1. **`Core` still references `UI`** — e.g. `UI.State`, `UI.Charts.Presentation`, chart/render types under `Core/Rendering/Contracts/*`. Single WPF project tolerates this; it blocks clean Core extraction or a second UI.
2. **`Core/Rendering` is WPF/chart-stack coupled** — LiveCharts / LiveChartsCore / WPF types in Core. Guardrails limit **Syncfusion/LiveChartsCore** in `Core/Orchestration` and `UI/MainHost` only, not all of Core.

### Acceptable for now

- Large **`UI/Charts/Presentation`** bucket — fewer top-level folders, more files in one place.
- **`ArchitectureGuardrailTests`** — extensive; pins `MainChartsView` / MainHost composition rules. Treat as law for refactors.

### Deferred (needs dedicated pass — do not mix with feature work)

| Item | Why deferred |
|------|----------------|
| Extract Core-facing interfaces for `ChartState` / `MetricState` | Touches orchestration + all render contracts |
| Move chart render **models** out of `UI.Charts.Presentation.Rendering` into Core/Shared | Large namespace ripple |
| Mirror `DataVisualiser.Tests` folders to `Core` / `UI` / `Shared` | ~100+ file moves |
| Retire `TransformDataPanelController` vs `V2` | Behavior + XAML |
| Split `Presentation/` by capability (host vs models vs backends) | After spine stabilises |

---

## 5. Git state at handoff (verify after checkout)

**Last known commit on prior machine:** `e60b243` — *Stop to expand note.*

**Uncommitted at handoff (include in check-in if intentional):**

```
 M DataVisualiser/UI/OperationChain/OperationChainWorkbenchView.xaml
 M DataVisualiser/UI/OperationChain/OperationChainWorkbenchView.xaml.cs
 M documents/DataVisualiser_Migration_Plan_and_Guardrails.md
?? DataVisualiser.Tests/UI/Charts/Presentation/ChartUiHelperTests.cs
?? DataVisualiser.Tests/UI/OperationChain/OperationChainInputGridLoadServiceTests.cs
?? DataVisualiser/UI/Export/
?? DataVisualiser/UI/OperationChain/OperationChainEvidenceExportService.cs
?? DataVisualiser/UI/OperationChain/OperationChainInputGridLoadService.cs
```

**Do not commit:** `*.lscache`, `bin/`, `obj/`.

**Safe to check in (from validation on prior machine):** structural refactor + tests green; manual smoke **not required** for namespace/folder-only changes.

---

## 6. Solution layout (quick map)

```
DataAnalyser.sln
├── DataFileReader/          # Ingest, normalization, CMS, SQL services
│   └── Models/              # Was "Class" — domain + Json types
├── DataFileReader.Tests/
├── DataVisualiser/
│   ├── Core/                # Orchestration, rendering contracts, strategies, validation
│   ├── Shared/              # Models, helpers, ErrorEventArgs
│   └── UI/
│       ├── ChartPresentationSpine.cs
│       ├── MainChartsView.*
│       ├── MainHost/        # Coordinators, pipeline factory, export seams
│       ├── Charts/
│       │   ├── Controllers/
│       │   ├── Presentation/    # Merged adapters/helpers/infrastructure/rendering
│       │   ├── Interaction/
│       │   └── Syncfusion/
│       ├── OperationChain/  # WIP at handoff
│       ├── Export/          # WIP at handoff
│       ├── ViewModels/
│       └── State/
└── DataVisualiser.Tests/    # Architecture guardrails + parity + UI tests
```

---

## 7. Authority order (when documents conflict)

1. `documents/Project Bible.md`
2. `documents/SYSTEM_MAP.md`
3. `documents/DataVisualiser-Architectural-Vocabulary.md`
4. `documents/Project Roadmap.md`
5. `documents/MASTER_OPERATING_PROTOCOL.md`
6. `documents/DataVisualiser_Migration_Plan_and_Guardrails.md` — **active implementation checklist**
7. `documents/AI_BOOTSTRAP_ENTRY.md` — routing after this handoff
8. **This file** — session state only

---

## 8. Validation (run first on new machine)

```powershell
dotnet build DataAnalyser.sln -c Debug
dotnet test DataAnalyser.sln -c Debug -m:1
```

**Last known results (prior machine, 2026-06-05):**

- Build: **succeeded**
- `DataFileReader.Tests`: **15** passed
- `DataVisualiser.Tests`: **1092** passed (count may grow — zero failures is the gate)

**Manual smoke:** Not required for structural-only work. Optional: load metrics → charts render; trigger one load-validation error and confirm message still appears.

---

## 9. Suggested next work (pick one objective)

1. **Finish OperationChain / Export WIP** — commit untracked files or complete integration; add tests if behavior is new.
2. **Core/UI boundary pass** — introduce `IMetricLoadSelectionState` (or similar) so `DataLoadValidator` stops referencing `UI.State.MetricState`.
3. **Chart render models** — move `Presentation/Rendering/*Model` types toward `Shared` or `Core.Rendering.Models` so `Core/Rendering/Contracts` stops referencing `UI.Charts.Presentation`.
4. **Follow Migration Plan** — next unchecked phase in `DataVisualiser_Migration_Plan_and_Guardrails.md` (status: target spine represented; old mesh active; yellow not green).

---

## 10. Prompt to give the AI on re-entry

Copy/paste:

```text
Read documents/AI_REENTRY_HANDOFF.md first. Then run dotnet build and dotnet test on DataAnalyser.sln.
Continue from Section 9 using Conservative-Pragmatic posture: one primary objective per pass.
Do not commit *.lscache. Ask before large folder moves or behavior changes.
```

---

## 11. Notes from consolidation conversation

- **Imperfect alignment is OK** — document spine + thin façades + guardrails before big-bang renames.
- **“Big diffs” to defer:** full test-tree mirror, Shared→Core merge, Transform V1/V2 retirement, flattening all of `Presentation/` without a capability model.
- **`codebase-index.md`** may be stale on paths/namespaces — regenerate or grep if you rely on it.
- **`DATAVISUALISER_PIPELINE_SPINE.md`** was created in an earlier pass but may not exist on all branches; Section 3 of this file replaces it for re-entry.
