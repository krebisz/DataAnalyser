# DataVisualiser — presentation pipeline spine

**Scope:** Primary path from “user loads metrics” to “charts refresh,” without claiming all code fits this model.

## Ordered stages (happy path)

1. **UI validation** — Selection + date range; `MainWindowViewModel.ValidateDataLoadRequirements` / view guards.
2. **Load metrics into context** — `MainWindowViewModel.LoadMetricDataAsync` → `MetricLoadCoordinator.LoadMetricDataAsync`:
   - **VNext path** (when only Main chart visible): `VNextMainChartIntegrationCoordinator` → fresh `ReasoningSessionCoordinator` → `LoadAsync` → `BuildMainProgram` → `LegacyChartProgramProjector.ProjectToChartContext` → `ChartState.LastContext`. Runtime tracked via `ChartState.LastLoadRuntime` (`EvidenceRuntimePath.VNextMain`).
   - **Legacy path** (when extended charts visible, or VNext fails): `MetricSelectionService.LoadMetricDataWithCmsAsync` → `ChartDataContextBuilder.Build` → `ChartState.LastContext`. Runtime tracked via `ChartState.LastLoadRuntime` (`EvidenceRuntimePath.Legacy`).
3. **Publish + schedule chart work** — `LoadDataCommand` → `LoadData()` raises `DataLoaded` and calls `RequestChartUpdate()`. Facade: `ChartPresentationSpine.PublishLastContextAndRequestChartUpdate`.
4. **Composition of engines** — `MainChartsViewChartPipelineFactory` builds `ChartUpdateCoordinator`, `ChartRenderingOrchestrator`, distribution services, etc.
5. **Per-chart orchestration** — `ChartRenderingOrchestrator` / `*OrchestrationPipeline` / `ChartUpdateCoordinator` drive prep and render for each chart family.

## Key types (navigation)

| Stage | Types |
|--------|--------|
| Load + context (VNext) | `VNextMainChartIntegrationCoordinator`, `ReasoningSessionCoordinator`, `LegacyChartProgramProjector` |
| Load + context (Legacy) | `MetricLoadCoordinator`, `MetricSelectionService`, `ChartDataContextBuilder` |
| Runtime state | `ChartState.LastLoadRuntime` (`LoadRuntimeState`), `EvidenceRuntimePath` |
| VM seam | `MainWindowViewModel` (`LoadMetricDataAsync`, `LoadDataCommand`, `RequestChartUpdate`) |
| Factory | `MainChartsViewChartPipelineFactory`, `MainChartsViewChartPipelineFactoryResult` |
| Render | `ChartRenderingOrchestrator`, `ChartUpdateCoordinator` |
| Evidence | `EvidenceExportModels`, `EvidenceDiagnosticsBuilder`, `MainChartsEvidenceExportService` |

## VNext routing decision

The routing gate lives in `MetricLoadCoordinator.ShouldUseVNextMainPath()`. It evaluates `ChartState` visibility flags directly:
- VNext activates when `IsMainVisible == true` and all extended charts (`Normalized`, `DiffRatio`, `Distribution`, `WeeklyTrend`, `Transform`, `BarPie`) are hidden.
- Legacy activates otherwise, or as automatic fallback on VNext failure.
- Routing is independent of CMS configuration.

## Legacy / parallel paths (not the spine)

- **Syncfusion host** — Own view and render path; still uses the same VM load + `LoadDataCommand` sequence where wired. Clears `LastLoadRuntime` on reset paths.
- **Ad-hoc reloads** — Several adapters call `MetricSelectionService.LoadMetricDataAsync` directly for a subset of charts; spine remains “full context load” above.
- **DataFileReader ingest** — Separate CLI pipeline (files → DB); meets the app at storage, not at `ChartState`.

## Code facade

`DataVisualiser.UI.ChartPresentationSpine` (`UI/ChartPresentationSpine.cs`) — thin forwards to the VM for stages 2–3 so the spine has a single type to open first.
