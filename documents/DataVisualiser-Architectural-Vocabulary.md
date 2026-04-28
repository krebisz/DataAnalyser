# DataVisualiser Vocabulary Rebuild — Step 1

## Purpose

Extract reusable architectural vocabulary from current project evidence and prepare it for grammar reduction.

```text
as-is vocabulary
-> stripped compound concepts
-> as-is code map
-> atomized vocabulary
-> extension vocabulary
-> full language pool
-> Step 2 grammar reduction
```

## Source Priority

1. Type dependency diagram
2. Codebase index
3. Project tree
4. Dependency summary
5. Existing vocabulary document
6. Foundational project documents

## Working Table of Contents

This table of contents is provisional and may be adjusted as the rebuild proceeds.

```text
1. Extract vocabulary and map current state
2. Build flexible grammar
3. Promote core concepts
4. Map concept relationships
5. Define target architecture
6. Compare current vs target
7. Identify gaps and risks
8. Define migration plan
9. Set guardrails
10. Define completion criteria
```

Working flow:

```text
evidence
-> language
-> concepts
-> relationships
-> target
-> gap
-> plan
-> enforcement
-> closure
```

---

## Reduction Rule

Strip concrete family, vendor, UI, and scenario qualifiers before vocabulary reduction.

```text
BarPieRenderingContract          -> RenderingContract
CartesianMetricChartRenderHost   -> RenderHost
SyncfusionSunburstRenderSurface  -> RenderSurface
DistributionRenderPlanBuilder    -> RenderPlanBuilder
ChartControllerFactoryContext    -> ControllerFactoryContext
```

---

## 1. Non-Atomized Vocabulary / Concepts

Implementation-stripped compound vocabulary discovered from the as-is project.

```text
AccessDefaults, AlignmentHelper, AlignmentMode, AnalyticalAuthority
AnalyticalCapabilityKind, AnalyticalExecutionResult, AnalyticalIntent, AnalyticalIntentFactory
AnalyticalIntentSet, AnalyticalInterpretation, AnalyticalInterpretationBuilder, AnalyticalInterpretationResult
AnalyticalInterpretationSetResult, AnalyticalRenderPlanPipeline, AnalyticalRenderPlanResult, AnalyticalRenderPlanSetResult
AnalyticalResultSet, BackendCandidateSet, BackendCapabilities, BackendKey
BackendQualification, BackendSelector, BasedShadingStrategy, BaseService
BinningHelper, Breakdown, Builder, Bundle
BusyStateTracker, Calculator, CapabilityRequest, CompositionKind
ComputationDefaults, ComputationEngine, ComputationHelper, ComputationParityHarness
ComputationResult, ComputationService, ComputationStrategy, ConfidenceAnnotation
ConfidenceAnnotationEvaluator, ConfidenceAnnotationKind, ConfidenceAnnotationSet, ConfidenceSeverity
Configuration, ConsumerDeliveryContract, ConsumerKind, ConsumerProviderContract
ConsumerProviderContracts, ConsumerProviderRegistry, Context, ContextBuilder
ContextDiagnosticsSnapshot, ContextHelper, ContextSelectionGuard, Control
Controller, Controller2, ControllerAdapter, ControllerAdapterBase
ControllerCoordinator, ControllerFactory, ControllerFactoryContext, ControllerFactoryResult
ControllerHost, ControllerKeys, ControllerRegistry, ConversionHelper
Converter, Coordinator, DebugSummaryLogger, DefaultOperationProvider
Defaults, DiagnosticsSnapshot, DisplayMode, Entry
ERenderer, ESurface, EventArgs, EventBinder
EventSource, EvidenceDiagnosticsBuilder, EvidenceExportCoordinator, EvidenceExportService
EvidenceParityBuilder, EvidenceParityBundle, EvidenceParityComputer, EvidenceParityEvaluator
EvidenceParityResolver, EvidenceParitySummaryBuilder, EvidenceResolutionHelper, EvidenceRuntimePath
EvidenceStrategyParityExecutor, ExecutionResult, Expression, ExpressionBuilder
ExpressionEvaluator, Factory, Fetcher, FetcherCatalogQueries
FetcherQueries, FetcherQueryGroup, FetcherRangeQueries, FetcherRepository
Filter, Formatter, FormattingHelper, GridPresentationCoordinator
Handlers, Helper, HierarchyNodePlan, Host
HostDiagnosticsSnapshot, HostRangeCoordinator, HostSelectionCoordinator, Input
InteractionKind, InteractionModel, InteractionPlan, InteractionRequest
InteractionVisualHelper, InterpretationDiagnosticsSnapshot, InterpretationResultDiagnosticsSnapshot, InterpretiveOverlayPlanner
LabelFormatter, Layout, LayoutCapabilities, LegacyExecutionResult
LegacyGateway, LegacyProgramProjector, LoadCoordinator, Loader
LoadExecutionActions, LoadLifecycle, LoadRequest, LoadResult
LoadRuntimeState, LoadSnapshot, LoadState, LoadStrategy
LoadStrategyResolver, LoadValidationInput, LoadValidator, Manager
ManagerCoordinator, Materializer, MathHelper, ModeCatalog
ModeDefinition, Model, ModeSettings, MultiplyConverter
NormalizationMode, NormalizationModeConverter, Operand, Operation
OperationExecutionCoordinator, OperationKernel, OperationKind, OperationProvider
OperationRegistry, OperationRequest, OperationRequestMapper, OperationStateCoordinator
OperatorRegistry, Operators, OrchestrationPipeline, OrchestrationRequest
OverlayKind, OverlayModel, OverlayPlan, Palette
Parity, ParityComparer, ParityFailure, ParityHarness
ParityLayer, ParityMode, ParityResult, ParityResultAdapter
ParityResultSnapshot, ParitySnapshot, ParitySummarySnapshot, ParityTolerance
ParityValidationService, Parser, ParticipationCalculator, PerformanceTimingSnapshot
PipelineDiagnosticsSnapshot, PipelineFactory, Placement, Plan
PreparationHelper, PreparationService, PreparationStage, Prepared
PresentationCoordinator, PresentationSpine, PresentationState, Program
ProgramDeliveryTargetResolver, ProgramKind, ProgramPlanner, ProgramRequest
Projection, ProjectionInteraction, ProjectionInteractionFactory, ProvenanceDescriptor
ProvenanceTrust, QueryBuilder, QueryMode, RangeEventArgs
RangeResult, RangeResultBuilder, RangeUiDiagnosticsSnapshot, ReachabilityDiagnosticsSnapshot
ReachabilityEvidenceExportResult, ReachabilityEvidenceStore, ReachabilityExportPathResolver, ReachabilityExportWriter
ReasoningEngine, ReasoningEngineFactory, ReasoningSessionCoordinator, ReasoningSessionState
ReasoningSessionTransitions, ReflectionHelper, RegistryCoordinator, Render
RenderAdapterResult, RenderBuffer, RenderCoordinator, RenderDefaults
RenderDeliveryBinding, RenderDensityMode, RenderDensityPlan, RenderDensityPolicy
RenderEngine, Renderer, RendererCore, RendererKind
RendererResolver, RenderGate, RenderHost, RenderingCapabilities
RenderingContext, RenderingContextAdapter, RenderingContract, RenderingDefaults
RenderingHostLifecycleAdapterHelper, RenderingHostTarget, RenderingOrchestrator, RenderingQualification
RenderingQualificationProbe, RenderingQualificationProbeResult, RenderingQualificationProbeSupport, RenderingRoute
RenderingRouteResolver, RenderingService, RenderInput, RenderInvocationStage
RenderInvoker, RenderModel, RenderModelBuilder, RenderPlan
RenderPlanAdapter, RenderPlanAdapterDispatcher, RenderPlanAdapterQualification, RenderPlanAdapterQualificationRules
RenderPlanBuilder, RenderPlanDiagnosticsSnapshot, RenderPlanHistorySnapshot, RenderPlanKind
RenderPlanMetadataKeys, RenderPlanProjector, RenderPlanProviderMetadata, RenderPlanVocabularyDiagnosticsSnapshot
RenderPlanVocabularyMetadata, RenderRequest, RenderState, RenderSurface
RenderTarget, Repository, Request, ResetCoordinator
ResolutionCoordinator, ResolutionResetCoordinator, ResolutionResult, Result
ResultStrategy, Route, RuntimeConfiguration, SaveResult
Scaffold, Selection, SelectionActions, SelectionAdapterHelper
SelectionCache, SelectionCoordinator, SelectionDiagnosticsSnapshot, SelectionEventBinder
SelectionInteractionCoordinator, SelectionQueries, SelectionReader, SelectionRequest
SelectionResolution, SelectionService, SelectionServiceLoader, SelectionState
SelectionStateBatchScope, SelectionSuppressionScope, SelectorManager, Service
SessionDiagnosticsRecorder, SessionMilestoneRecorder, SessionMilestoneSnapshot, Shading
ShadingCalculator, ShadingContext, ShadingDefaults, ShadingRenderer
ShadingStrategy, SharedModelContext, SharedModelProvider, SmokeHeuristicsSnapshot
Snapshot, StartupCoordinator, State, StateCoordinator
StateEvaluator, StateSyncCoordinator, Strategy, StrategyComputationHelper
StrategyCreationParameters, StrategyCutOverService, StrategyDecision, StrategyDecisionEvaluator
StrategyFactory, StrategyFactoryBase, StrategyInfo, StrategyInput
StrategyMetadata, StrategyParityContext, StrategyParityHarness, StrategyParityValidationService
StrategyPlan, StrategyReachability, StrategyReachabilityEvidenceStore, StrategyReachabilityProbe
StrategyReachabilityStoreProbe, StrategySelectionService, StrategySelectionStage, Surface
SurfaceCoordinator, SurfaceFactory, SurfaceHelper, SyncActions
TemporalHelper, Tick, TimeAggregationHelper, TimeAlignmentKernel
TimeHelper, TimeRenderAggregationKernel, TimestampSink, Totals
TrackedContentSurface, TrackedSurface, TransitionDiagnosticsSnapshot, TrendBackendKey
TrendBackendQualification, TrendComputationStrategy, TrendController, TrendControllerAdapter
TrendDefaults, TrendEventArgs, TrendMode, TrendRenderHost
TrendRenderingCapabilities, TrendRenderingContract, TrendRenderingQualification, TrendRenderingQualificationProbe
TrendRenderingQualificationProbeResult, TrendRenderingRoute, TrendRenderingRouteResolver, TrendRenderingService
TrendRenderPlanAdapter, TrendRenderPlanBuilder, TrendRenderRequest, TrendRenderSurface
TrendResult, TrendResultProvider, TrendStrategy, TrendStrategyFactory
TrendUpdateCoordinator, UiBusyScopeLease, UiDefaults, UiDiagnosticsSnapshot
UiHelper, UiRenderModel, UiRenderPlanAdapter, UiRenderSurface
UiState, UiSurfaceDiagnosticsReader, UiSurfaceDiagnosticsSnapshot, UnitResolutionService
Up, UpdateCoordinator, UpdateRequestedEventArgs, UserNotificationService
ValidationActions, ValidationHelper, Viewport, VisibilityController
VisibilityEventArgs, VisibilityHelper, VNextDiagnosticsSnapshot, VNextIntegrationCoordinator
VNextLoadCoordinator, VNextLoadResult, VNextProgramRequestPlanner, VNextResolutionHelper
VNextRoutePolicy, WorkflowCoordinator, WorkflowPlanRequest, WorkflowState
YBuilder
```

---

## 2. Current As-Is Architecture / Code Map

Descriptive map using the non-atomized vocabulary.

```text
CURRENT AS-IS CONCEPT MAP

Authority / Truth Carriers
├── AnalyticalAuthority
├── AnalyticalIntent
├── ProvenanceDescriptor
├── ProvenanceTrustClass
├── ExecutionResult
├── ResultSet
├── SelectionRequest
├── LoadSnapshot
└── SeriesSnapshot

Reasoning / Capability Spine
├── ReasoningEngine
├── ReasoningSessionCoordinator
├── CapabilityRequest
├── CompositionKind
├── ProgramPlanner
├── ProgramRequest
├── OperationKernel
├── AlignmentKernel
├── InterpretationBuilder
├── InterpretationResult
├── ConfidenceAnnotation
├── OverlayPlan
└── RenderPlanPipeline

Program / Plan Structures
├── Program
├── ProgramRequest
├── ProgramDeliveryTargetResolver
├── RenderPlan
├── RenderPlanProjector
├── RenderPlanProviderMetadata
├── RenderPlanVocabularyMetadata
├── RenderDeliveryBinding
├── HierarchyNodePlan
├── InteractionPlan
├── DensityPlan
└── RenderPlanBuilder

Contract / Boundary Structures
├── ConsumerDeliveryContract
├── ConsumerProviderContract
├── ConsumerProviderRegistry
├── BackendCandidateSet
├── BackendCapabilities
├── BackendSelector
├── AdapterQualification
├── AdapterQualificationRules
├── AdapterDispatcher
├── RenderingContract
├── RenderingQualification
├── QualificationProbe
└── RenderingCapabilities

Consumer / Interaction Structures
├── InteractionRequest
├── InteractionKind
├── InteractionPlan
├── InteractionModel
├── TooltipManager
├── TimestampSink
├── TooltipFactory
├── ProjectionInteractionFactory
├── ProjectionInteraction
├── EventArgs
├── EventBinder
└── Handlers

Terminal Delivery Structures
├── RenderEngine
├── RenderModel
├── RenderAdapterResult
├── RenderPlanAdapter
├── RenderSurface
├── RendererResolver
├── Renderer
├── RendererKind
├── SurfaceFactory
├── Surface
├── PanelHost
├── LifecycleAdapter
└── RenderTarget

Process / Orchestration Mesh
├── Coordinator
├── Orchestrator
├── Pipeline
├── Stage
├── Workflow
├── Transition
├── Route
├── Resolver
├── Selector
├── Factory
├── Loader
├── ControllerFactory
├── ControllerFactoryContext
├── UpdateCoordinator
├── RenderingOrchestrator
├── LoadCoordinator
├── OrchestrationPipeline
├── PreparationStage
├── SelectionStage
└── InvocationStage

Evidence / Diagnostics Sidecar
├── DiagnosticsSnapshot
├── EvidenceDiagnosticsBuilder
├── EvidenceParityBuilder
├── EvidenceBundle
├── EvidenceRuntimePath
├── DataResolutionHelper
├── ParityEvaluator
├── ParityExecutor
├── EvidenceExportService
├── SurfaceDiagnosticsReader
├── RenderPlanDiagnosticsSnapshot
├── VocabularyDiagnosticsSnapshot
├── InterpretationDiagnosticsSnapshot
├── ReachabilityDiagnosticsSnapshot
├── ParityResult
├── ReachabilityRecord
├── ExportWriter
├── Validator
├── Probe
├── Harness
├── Recorder
└── Writer

State / Context Carriers
├── State
├── RuntimeState
├── PresentationState
├── ReasoningSessionState
├── WorkflowState
├── SelectionState
├── DataContext
├── FactoryContext
├── SharedViewModelContext
├── Context
├── Result
├── Request
└── Snapshot

Presentation / UI Shell
├── MainView
├── VendorView
├── ViewModel
├── PresentationSpine
├── RenderingContextAdapter
├── TabHost
├── SelectionPanel
├── Controller
├── ControllerAdapter
├── UiHelper
├── Defaults
├── BusyStateTracker
├── VisibilityController
├── SelectorManager
└── ToggleManager
```

### 2.1 As-Is Reading

```text
Target spine exists:
Authority -> Reasoning -> Program/Plan -> Contract/Boundary -> Delivery

Old mesh remains active:
Process/Orchestration + Rendering/Delivery + Presentation/UI still carry too much integration weight.

Main risk:
Older hubs and rendering contracts can absorb VNext authority before the contract boundary is fully locked.
```

---

## 3. Unique Atomized Terms

Atomized terms extracted from Section 1.

```text
Access, Actions, Adapter, Aggregation, Alignment, Analytical, Annotation, Args
Authority, Backend, Base, Batch, Binder, Binding, Binning, Breakdown
Buffer, Builder, Bundle, Busy, Cache, Calculator, Candidate, Capabilities
Capability, Catalog, Comparer, Composition, Computation, Computer, Confidence, Configuration
Consumer, Content, Context, Contract, Contracts, Control, Controller, Conversion
Converter, Coordinator, Core, Creation, Cut, Debug, Decision, Default
Defaults, Definition, Delivery, Density, Descriptor, Diagnostics, Dispatcher, Display
Engine, Entry, Evaluator, Event, Evidence, Execution, Executor, Export
Expression, Factory, Failure, Fetcher, Filter, Formatter, Gateway, Guard
Handlers, Harness, Helper, Hierarchy, History, Host, Intent, Interaction
Interpretation, Interpretive, Invoker, Kernel, Key, Keys, Kind, Layout
Lifecycle, Load, Loader, Manager, Mapper, Materializer, Metadata, Milestone
Mode, Model, Notification, Operand, Operation, Operator, Operators, Orchestration
Orchestrator, Overlay, Parity, Performance, Plan, Planner, Preparation, Prepared
Presentation, Probe, Program, Projector, Provider, Qualification, Queries, Query
Reader, Reasoning, Recorder, Registry, Request, Reset, Resolution, Resolver
Result, Runtime, Scope, Selection, Selector, Service, Session, Shading
Snapshot, Source, Spine, Stage, State, Strategy, Support, Suppression
Surface, Sync, Target, Timing, Transition, Transitions, Trust, Validation
Validator, Visibility, Visual, Vocabulary, Writer
```

---

## 4. Extension-Only Atomized Vocabulary

Candidate language extensions for the enhanced target architecture.

### 4.1 Extension Compound Concepts

```text
TruthEnvelope                 -> Truth, Envelope
SemanticEnvelope              -> Semantic, Envelope
AuthorityEnvelope             -> Authority, Envelope
ProvenanceEnvelope            -> Provenance, Envelope
CapabilityEnvelope            -> Capability, Envelope
ResultEnvelope                -> Result, Envelope
RequestEnvelope               -> Request, Envelope

SemanticNeutrality            -> Semantic, Neutrality
ConsumerNeutrality            -> Consumer, Neutrality
BackendNeutrality             -> Backend, Neutrality
VendorNeutrality              -> Vendor, Neutrality
PresentationNeutrality        -> Presentation, Neutrality

SurfaceModel                  -> Surface, Model
DeliverySurface               -> Delivery, Surface
InteractionSurface            -> Interaction, Surface
ConsumerSurface               -> Consumer, Surface
EvidenceSurface               -> Evidence, Surface

DeliveryBinding               -> Delivery, Binding
ExecutionBinding              -> Execution, Binding
CapabilityBinding             -> Capability, Binding
ProviderBinding               -> Provider, Binding
ConsumerBinding               -> Consumer, Binding

RuntimeBoundary               -> Runtime, Boundary
VendorBoundary                -> Vendor, Boundary
ConsumerBoundary              -> Consumer, Boundary
AuthorityBoundary             -> Authority, Boundary
EvidenceBoundary              -> Evidence, Boundary

SemanticContract              -> Semantic, Contract
CapabilityContract            -> Capability, Contract
ConsumerContract              -> Consumer, Contract
ProviderContract              -> Provider, Contract
DeliveryContract              -> Delivery, Contract
InteractionContract           -> Interaction, Contract
EvidenceContract              -> Evidence, Contract

InterpretationLayer           -> Interpretation, Layer
ConfidenceLayer               -> Confidence, Layer
OverlayLayer                  -> Overlay, Layer
CapabilityLayer               -> Capability, Layer
EvidenceLayer                 -> Evidence, Layer

SemanticPolicy                -> Semantic, Policy
CapabilityPolicy              -> Capability, Policy
DeliveryPolicy                -> Delivery, Policy
QualificationPolicy           -> Qualification, Policy
EvidencePolicy                -> Evidence, Policy

TrustModel                    -> Trust, Model
ConfidenceModel               -> Confidence, Model
ProvenanceModel               -> Provenance, Model
CapabilityModel               -> Capability, Model
InterpretationModel           -> Interpretation, Model

SemanticProjection            -> Semantic, Projection
CapabilityProjection          -> Capability, Projection
ConsumerProjection            -> Consumer, Projection
DeliveryProjection            -> Delivery, Projection
EvidenceProjection            -> Evidence, Projection

ResultLineage                 -> Result, Lineage
DataLineage                   -> Data, Lineage
ProvenanceLineage             -> Provenance, Lineage
DecisionLineage               -> Decision, Lineage
InterpretationLineage         -> Interpretation, Lineage

SemanticIdentity              -> Semantic, Identity
ResultIdentity                -> Result, Identity
CapabilityIdentity            -> Capability, Identity
ConsumerIdentity              -> Consumer, Identity
ProviderIdentity              -> Provider, Identity

SemanticRole                  -> Semantic, Role
ConsumerRole                  -> Consumer, Role
ProviderRole                  -> Provider, Role
EvidenceRole                  -> Evidence, Role
DeliveryRole                  -> Delivery, Role

DecisionRecord                -> Decision, Record
EvidenceRecord                -> Evidence, Record
ProvenanceRecord              -> Provenance, Record
InterpretationRecord          -> Interpretation, Record
QualificationRecord           -> Qualification, Record

AnalyticalSurface             -> Analytical, Surface
AnalyticalEnvelope            -> Analytical, Envelope
AnalyticalContract            -> Analytical, Contract
AnalyticalBoundary            -> Analytical, Boundary
AnalyticalLineage             -> Analytical, Lineage

CompositionGraph              -> Composition, Graph
CapabilityGraph               -> Capability, Graph
DependencyGraph               -> Dependency, Graph
EvidenceGraph                 -> Evidence, Graph
ProvenanceGraph               -> Provenance, Graph

SemanticKernel                -> Semantic, Kernel
CapabilityKernel              -> Capability, Kernel
InterpretationKernel          -> Interpretation, Kernel
EvidenceKernel                -> Evidence, Kernel
ProjectionKernel              -> Projection, Kernel

SystemContour                 -> System, Contour
ArchitectureContour           -> Architecture, Contour
BoundaryContour               -> Boundary, Contour
CapabilityContour             -> Capability, Contour

SemanticGravity               -> Semantic, Gravity
ArchitecturalGravity          -> Architectural, Gravity
AuthorityGravity              -> Authority, Gravity
CapabilityGravity             -> Capability, Gravity

ArchitecturalInvariant        -> Architectural, Invariant
SemanticInvariant             -> Semantic, Invariant
ProvenanceInvariant           -> Provenance, Invariant
CapabilityInvariant           -> Capability, Invariant
BoundaryInvariant             -> Boundary, Invariant

SemanticFlow                  -> Semantic, Flow
AuthorityFlow                 -> Authority, Flow
EvidenceFlow                  -> Evidence, Flow
DeliveryFlow                  -> Delivery, Flow
InteractionFlow               -> Interaction, Flow

SystemGrammar                 -> System, Grammar
ArchitectureGrammar           -> Architecture, Grammar
CapabilityGrammar             -> Capability, Grammar
InteractionGrammar            -> Interaction, Grammar
EvidenceGrammar               -> Evidence, Grammar
```

### 4.2 Unique Extension Atoms

```text
Analytical
Architecture
Architectural
Authority
Backend
Binding
Boundary
Capability
Confidence
Consumer
Contour
Contract
Data
Decision
Delivery
Dependency
Envelope
Evidence
Execution
Flow
Grammar
Graph
Gravity
Identity
Interaction
Interpretation
Invariant
Kernel
Layer
Lineage
Model
Neutrality
Overlay
Policy
Projection
Provider
Provenance
Qualification
Record
Request
Result
Role
Semantic
Surface
System
Trust
Truth
Vendor
```

### 4.3 Strongest Extension Candidates to Test

```text
Envelope
Neutrality
Surface
Binding
Lineage
Identity
Invariant
Grammar
```

### 4.4 Foundational Discipline Atoms

These atoms are added from the foundational project goals rather than from current codebase naming.

```text
Canonical
Semantics
Lossless
Determinism
Reversibility
Fidelity
Constraint
Governance
Traceability
Transformation
```

Reason:

```text
single semantic authority
lossless ingestion
deterministic behavior
reversible transformation
fidelity preservation
explicit constraint
governance discipline
traceable evolution
```


---

## 5. Current Vocabulary Families

### 5.1 Authority / Truth

```text
AnalyticalAuthority
ProvenanceDescriptor
ProvenanceTrustClass
AnalyticalIntent
AnalyticalIntentSet
AnalyticalExecutionResult
AnalyticalResultSet
MetricSelectionRequest
MetricSeriesRequest
MetricLoadSnapshot
MetricSeriesSnapshot
LoadedMetricSeries
```

Use:

```text
authority, provenance, intent, trust, request/result lineage
```

Risk:

```text
Request / Result / Snapshot must not become generic transport bags.
```

### 5.2 Reasoning / Capability

```text
ReasoningEngine
ReasoningSessionCoordinator
ReasoningSessionState
AnalyticalCapabilityKind
CapabilityRequest
CompositionKind
ChartProgramPlanner
ChartProgram
ChartSeriesProgram
OperationKernel
TimeSeriesAlignmentKernel
AnalyticalInterpretationBuilder
AnalyticalInterpretationResult
ConfidenceAnnotation
ConfidenceAnnotationEvaluator
InterpretiveOverlayPlanner
OverlayPlan
AnalyticalRenderPlanPipeline
```

Use:

```text
reasoning, capability, composition, interpretation, confidence, overlay, analytical program
```

Risk:

```text
ChartProgram is currently chart-shaped, but its target role is broader analytical-program planning.
```

### 5.3 Program / Plan

```text
ChartProgram
ChartProgramRequest
ChartProgramKind
ChartProgramDeliveryTargetResolver
ChartRenderPlan
ChartRenderPlanProjector
ChartRenderPlanProviderMetadata
ChartRenderPlanVocabularyMetadata
ChartRenderDeliveryBinding
ChartHierarchyNodePlan
ChartSeriesPlan
ChartInteractionPlan
RenderDensityPlan
RenderDensityPolicy
*RenderPlanBuilder
```

Use:

```text
program planning, render planning, metadata preservation, delivery binding
```

Risk:

```text
Family-specific *RenderPlanBuilder types preserve old micro-framework pressure.
```

### 5.4 Contract / Boundary

```text
ConsumerDeliveryContract
ConsumerProviderContract
ConsumerProviderContracts
ConsumerProviderRegistry
ConsumerKind
ChartBackendCandidateSet
ChartBackendCapabilities
ChartBackendSelector
ChartRenderPlanAdapterQualification
ChartRenderPlanAdapterQualificationRules
ChartRenderPlanAdapterDispatcher
*RenderingContract
*RenderingQualification
*RenderingQualificationProbe
*BackendQualification
*RenderingCapabilities
```

Use:

```text
contract seam, provider compatibility, backend qualification, delivery enforcement
```

Risk:

```text
Older *RenderingContract families may keep terminal delivery too central.
```

### 5.5 Consumer / Interaction

```text
InteractionRequest
InteractionKind
ChartInteractionPlan
ChartInteractionModel
ChartTooltipManager
IChartTimestampSink
IDistributionTooltipFactory
*TooltipFactory
DistributionPolarProjectionInteractionFactory
IDistributionPolarProjectionInteraction
*Tooltip
*EventArgs
EventBinder
Handlers
ConsumerKind
```

Use:

```text
consumer behavior, interaction relay, tooltip/timestamp/event handling
```

Risk:

```text
Interaction must relay behavior only; it must not own semantic interpretation or provider policy.
```

### 5.6 Rendering / Delivery

```text
ChartRenderEngine
ChartRenderModel
UiChartRenderModel
ChartRenderAdapterResult
*RenderPlanAdapter
*RenderSurface
ChartRendererResolver
IChartRenderer
ChartRendererKind
LiveChartsChartRenderer
EChartsChartRenderer
ChartSurfaceFactory
IChartSurface
IChartPanelHost
RenderingHostLifecycleAdapterHelper
RenderingHostTarget
ISyncfusionSunburstRenderTarget
```

Use:

```text
terminal delivery, render surfaces, backend/vendor adaptation
```

Risk:

```text
Rendering is still large; it must remain terminal and replaceable.
```

### 5.7 Process / Orchestration

```text
Coordinator
Orchestrator
Pipeline
Stage
Workflow
Transition
Route
Resolver
Selector
Factory
Loader
Lifecycle
ChartRenderingOrchestrator
ChartUpdateCoordinator
MetricLoadCoordinator
ChartControllerFactory
ChartControllerFactoryContext
*OrchestrationPipeline
*PreparationStage
*StrategySelectionStage
*RenderInvocationStage
StrategySelectionService
StrategyCutOverService
VNextMainChartIntegrationCoordinator
VNextSeriesLoadCoordinator
```

Use:

```text
workflow, sequencing, routing, transition, coexistence, orchestration
```

Risk:

```text
Process terms are overrepresented; old hubs may absorb VNext authority.
```

### 5.8 Evidence / Diagnostics

```text
DiagnosticsSnapshot
EvidenceDiagnosticsBuilder
EvidenceParityBuilder
EvidenceParityBundle
EvidenceRuntimePath
EvidenceDataResolutionHelper
*ParityEvaluator
EvidenceStrategyParityExecutor
MainChartsEvidenceExportService
MainChartsUiSurfaceDiagnosticsReader
RenderPlanDiagnosticsSnapshot
RenderPlanVocabularyDiagnosticsSnapshot
InterpretationDiagnosticsSnapshot
ReachabilityDiagnosticsSnapshot
UiSurfaceDiagnosticsSnapshot
VNextDiagnosticsSnapshot
ParityResult
StrategyReachabilityRecord
ReachabilityEvidenceExportResult
ReachabilityExportWriter
Validator
Probe
Harness
Recorder
Writer
```

Use:

```text
evidence, diagnostics, parity, reachability, validation, audit
```

Risk:

```text
Evidence must observe only; it must not control live routing, provider selection, or semantics.
```

### 5.9 State / Context / Snapshot

```text
ChartState
MetricState
UiState
LoadState
LoadRuntimeState
PresentationState
ReasoningSessionState
WorkflowState
SelectionState
ChartDataContext
ChartControllerFactoryContext
Context
Result
Request
Snapshot
MetricLoadSnapshot
MetricSeriesSnapshot
DiagnosticsSnapshot
SessionMilestoneSnapshot
```

Use:

```text
state, context, lifecycle, request/result carriers, snapshots
```

Risk:

```text
State and Context are the highest overload risks. No universal context. No generic state authority.
```

### 5.10 UI / Presentation

```text
MainChartsView
SyncfusionChartsView
MainWindowViewModel
ChartPresentationSpine
ChartRenderingContextAdapter
ChartTabHost
MetricSelectionPanel
*ChartController
*ChartControllerAdapter
ChartAxisModel
ChartFacetModel
ChartLegendModel
ChartOverlayModel
ChartSeriesModel
ChartUiHelper
BusyStateTracker
ChartVisibilityController
SubtypeSelectorManager
LegendToggleManager
```

Use:

```text
consumer surface, UI state, presentation adaptation, visual assembly
```

Risk:

```text
UI must not become the integration shell for authority, reasoning, provider policy, or evidence policy.
```

---

## 6. Weak / Overloaded Terms

```text
Actions
Result
Context
State
Helper
Manager
Coordinator
Factory
Resolver
Selector
Route
Host
Surface
Model
Request
Snapshot
Defaults
Provider
Registry
```

Rule:

```text
Keep as descriptive vocabulary.
Do not let them hide authority, policy, routing, or semantic decisions.
```

---

## 7. Family / Vendor / Scenario Qualifiers

```text
Chart
Main
Secondary
BarPie
Cartesian
Distribution
Hourly
Weekly
WeekdayTrend
Transform
DiffRatio
Normalized
Combined
Single
Multi
Bucket
Cms
Admin
Syncfusion
Sunburst
LiveCharts
ECharts
Wpf
DateRange
Metric
Subtype
Toggle
Zoom
Theme
Tooltip
```

Rule:

```text
Useful locally.
Not target-architecture concepts unless explicitly discussing terminal delivery or current structure.
```

---

## 8. Step 1 Findings

### 8.1 Target Spine Exists

```text
AnalyticalAuthority
AnalyticalIntent
CapabilityRequest
CompositionKind
ReasoningEngine
ChartProgram
AnalyticalInterpretationResult
ConfidenceAnnotation
OverlayPlan
ConsumerDeliveryContract
ConsumerProviderContract
ConsumerProviderRegistry
ChartRenderPlan
ChartRenderDeliveryBinding
ChartRenderPlanProviderMetadata
ChartRenderPlanVocabularyMetadata
EvidenceDiagnosticsBuilder
```

Meaning:

```text
The intended target architecture exists materially in code.
```

### 8.2 Old Mesh Remains Active

```text
ChartUpdateCoordinator
ChartRenderingOrchestrator
ChartControllerFactory
ChartControllerFactoryContext
MetricLoadCoordinator
MainChartsView*Coordinator
*ChartControllerAdapter
*RenderingContract
*RenderingRoute
*RenderHost
*RenderRequest
ChartState
ChartDataContext
MainWindowViewModel
```

Meaning:

```text
Older coordination, rendering, and UI integration paths still carry significant architectural weight.
```

### 8.3 Current State

```text
Direction: valid
Target spine: visible
Old mesh: active
Risk: old hubs and rendering contracts absorbing VNext authority
Status: yellow
```

---

## 9. Candidate Target Vocabulary

Carry these into Step 2.

```text
Authority
Provenance
Trust
Intent
Capability
Composition
Interpretation
Confidence
Overlay
Program
Plan
Contract
Boundary
Provider
Consumer
Interaction
Qualification
Metadata
Surface Model
Delivery Binding
Backend Capability
Evidence
Diagnostics
Parity
Reachability
Audit
```

Possible refinements to test:

```text
Envelope
Neutrality
```

Rule:

```text
Add only if the term improves the target architecture.
```

---

## 10. Terms to Scrutinize Later

```text
Controller
ViewModel
Renderer
Route
Host
Manager
Helper
Args
Defaults
Tracker
Context
State
Result
Coordinator
Factory
Resolver
Selector
```

Rule:

```text
Do not remove yet.
Do not promote blindly.
Reassess during grammar and target-architecture rebuild.
```

---

## 11. Full Atomized Vocabulary

Combined atomized vocabulary: extracted atoms plus extension atoms.

```text
Access, Actions, Adapter, Aggregation, Alignment, Analytical, Annotation, Architectural
Architecture, Args, Authority, Backend, Base, Batch, Binder, Binding
Binning, Boundary, Breakdown, Buffer, Builder, Bundle, Busy, Cache
Calculator, Candidate, Canonical, Capabilities, Capability, Catalog, Comparer, Composition
Computation, Computer, Confidence, Configuration, Constraint, Consumer, Content, Context
Contour, Contract, Contracts, Control, Controller, Conversion, Converter, Coordinator
Core, Creation, Cut, Data, Debug, Decision, Default, Defaults
Definition, Delivery, Density, Dependency, Descriptor, Determinism, Diagnostics, Dispatcher
Display, Engine, Entry, Envelope, Evaluator, Event, Evidence, Execution
Executor, Export, Expression, Factory, Failure, Fetcher, Fidelity, Filter
Flow, Formatter, Gateway, Governance, Grammar, Graph, Gravity, Guard
Handlers, Harness, Helper, Hierarchy, History, Host, Identity, Intent
Interaction, Interpretation, Interpretive, Invariant, Invoker, Kernel, Key, Keys
Kind, Layer, Layout, Lifecycle, Lineage, Load, Loader, Lossless
Manager, Mapper, Materializer, Metadata, Milestone, Mode, Model, Neutrality
Notification, Operand, Operation, Operator, Operators, Orchestration, Orchestrator, Overlay
Parity, Performance, Plan, Planner, Policy, Preparation, Prepared, Presentation
Probe, Program, Projection, Projector, Provenance, Provider, Qualification, Queries
Query, Reader, Reasoning, Record, Recorder, Registry, Request, Reset
Resolution, Resolver, Result, Reversibility, Role, Runtime, Scope, Selection
Selector, Semantic, Semantics, Service, Session, Shading, Snapshot, Source
Spine, Stage, State, Strategy, Support, Suppression, Surface, Sync
System, Target, Timing, Traceability, Transformation, Transition, Transitions, Trust
Truth, Validation, Validator, Vendor, Visibility, Visual, Vocabulary, Writer
```

### 11.1 Count

```text
Existing extracted atoms:       157
Extension-only atoms:           48
Foundational discipline atoms:  10
Combined unique atoms:          192
```

---

## Step 1 Output

```text
The target architecture is visible in the vocabulary,
but the old integration mesh is still structurally active.
```

Next:

```text
Step 2 — Reduce the full atomized vocabulary into a working architectural grammar.
```

---

---

---


---

## Step 1 Capability Notes

These are information-only current-state capabilities derived from the same evidence used in Step 1.

They are not closure claims and not migration instructions.

```text
VNext analytical intent exists.
Reasoning-engine structures exist.
Capability request and composition vocabulary exists.
Analytical program planning exists.
Interpretation result structures exist.
Confidence annotation structures exist.
Overlay planning structures exist.
Render-plan projection exists.
Provider-aware render-plan metadata exists.
Vocabulary metadata exists.
Consumer/provider contract structures exist.
Provider registry structures exist.
Backend capability and qualification structures exist.
Adapter qualification structures exist.
Evidence, parity, reachability, validation, and diagnostics structures exist.
Current UI/presentation integration remains active.
Legacy/VNext coexistence remains active.
Older coordination and rendering hubs remain structurally important.
```

Current-state reading:

```text
The project has real target-aligned capabilities in code,
but they are not yet fully protected by the final target architecture.
```

---

# Step 2 — Exploratory Architectural Grammar

## Purpose

```text
Create a flexible grammar pool from the extracted and extended vocabulary.
Do not promote concepts yet.
Do not prune vocabulary yet.
Do not finalize substitutions yet.
Use this section only to prepare target-architecture language.
```

Step 2 remains exploratory.

```text
full language pool
-> possible compound grammar
-> grooming hypotheses
-> constrained terms
-> implementation-facing terms
-> generalization strip terms
```

---

## 1. Grammar Pools

### 1.1 Authority / Truth

```text
AuthorityEnvelope
ProvenanceEnvelope
TruthEnvelope
SemanticEnvelope
ResultEnvelope
RequestEnvelope
ProvenanceDescriptor
TrustModel
SemanticIdentity
ResultIdentity
CapabilityIdentity
DecisionLineage
ResultLineage
InterpretationLineage
ProvenanceLineage
SemanticInvariant
ProvenanceInvariant
```

Use:

```text
truth, identity, trust, provenance, reversibility, lineage
```

Open:

```text
Envelope
Lineage
Identity
Invariant
```

---

### 1.2 Reasoning / Capability

```text
ReasoningEngine
CapabilityRequest
CapabilityModel
CapabilityPolicy
CapabilityContract
CapabilityGraph
CapabilityKernel
CompositionGraph
AnalyticalProgram
AnalyticalIntent
SemanticPolicy
InterpretationModel
InterpretationKernel
ConfidenceModel
OverlayLayer
SemanticProjection
CapabilityProjection
```

Use:

```text
reasoning, capability, composition, interpretation, confidence, overlays
```

Open:

```text
Strategy -> Capability / Policy / distinct
Operation -> Capability / Composition / primitive
Kernel -> computation-core only?
Interpretation -> promoted concept?
```

---

### 1.3 Program / Plan

```text
AnalyticalProgram
ProgramRequest
ProgramPlan
ExecutionPlan
RenderPlan
InteractionPlan
DeliveryPlan
DensityPlan
SurfaceModel
ProgramDeliveryBinding
DeliveryBinding
ExecutionBinding
ProviderBinding
ConsumerBinding
```

Use:

```text
planned analytical work before consumer-specific delivery
```

Open:

```text
ChartProgram vs AnalyticalProgram
Plan vs Program
Route vs Binding
SurfaceModel placement
```

---

### 1.4 Contract / Boundary

```text
SemanticContract
CapabilityContract
ProviderContract
ConsumerContract
DeliveryContract
InteractionContract
EvidenceContract
ContractBoundary
ProviderBoundary
ConsumerBoundary
RuntimeBoundary
VendorBoundary
AuthorityBoundary
EvidenceBoundary
QualificationPolicy
BackendCapability
BackendQualification
AdapterQualification
BoundaryInvariant
NeutralityInvariant
```

Use:

```text
lawful crossings, neutrality, compatibility, qualification
```

Open:

```text
Contract / Boundary: one container or paired concepts?
Qualification: promoted concept?
Neutrality: principle / property / concept?
RenderingContract -> DeliveryContract / ProviderContract?
```

---

### 1.5 Projection / Translation

```text
SemanticProjection
CapabilityProjection
ConsumerProjection
DeliveryProjection
EvidenceProjection
ProjectionKernel
Projector
Adapter
Resolver
Selector
Mapper
Converter
Formatter
Materializer
```

Use:

```text
translation across boundaries without semantic authority
```

Open:

```text
Projection bridge status
Adapter / Resolver / Selector separation
Mapper / Converter / Formatter / Materializer separation
Policy leakage in translation roles
```

---

### 1.6 Consumer / Interaction

```text
ConsumerSurface
ConsumerState
ConsumerRole
ConsumerContract
ConsumerBinding
ConsumerAdapter
InteractionSurface
InteractionState
InteractionRequest
InteractionContract
InteractionFlow
InteractionGrammar
EventBinding
```

Use:

```text
consumer behavior, local state, event flow, interaction relay
```

Open:

```text
Controller -> ConsumerAdapter?
ViewModel -> ConsumerState?
Event -> InteractionFlow / EventBinding?
Interaction separate from Consumer?
```

---

### 1.7 Terminal Delivery

```text
DeliverySurface
DeliveryAdapter
DeliveryBinding
DeliveryContract
DeliveryPolicy
DeliveryFlow
RenderSurface
RenderAdapter
RenderTarget
BackendCapability
VendorBoundary
RuntimeBoundary
Lifecycle
```

Use:

```text
replaceable terminal infrastructure
```

Open:

```text
Renderer -> DeliveryAdapter?
Host -> RuntimeBoundary / DeliverySurface?
Surface -> SurfaceModel / DeliverySurface?
VendorBoundary promoted?
```

---

### 1.8 Governance / Evidence

```text
EvidenceLayer
EvidenceContract
EvidenceSurface
EvidenceRecord
EvidenceGraph
EvidenceFlow
EvidenceKernel
EvidenceProjection
DiagnosticsSnapshot
ParityEvidence
ReachabilityEvidence
ValidationEvidence
AuditRecord
QualificationRecord
DecisionRecord
```

Use:

```text
observation, proof, validation, audit, parity, reachability
```

Open:

```text
Diagnostics -> Evidence when proof?
Diagnostics separate for runtime inspection?
EvidenceContract real target concept?
Evidence observes contract seams?
```

---

### 1.9 Process / Execution

```text
Workflow
ExecutionBinding
ExecutionPlan
ProcessCoordinator
OrchestrationPipeline
Stage
Transition
FallbackPolicy
CoexistencePolicy
RuntimeState
Lifecycle
Observability
```

Use:

```text
sequencing, workflow, fallback, coexistence, transition
```

Open:

```text
Coordinator / Orchestrator -> ProcessCoordinator?
Pipeline / Stage: target or implementation?
Factory -> Provider / Registry / Builder / Adapter?
Observability: process-side / evidence-side / both?
```

---

## 2. Grooming Hypotheses

| Current / source language | Candidate target language | Test reason |
|---|---|---|
| `ChartProgram` | `AnalyticalProgram` | reduce chart-first bias |
| `Request`, `Result`, `Snapshot` | `Envelope` | preserve provenance / confidence / lineage |
| `Route` | `DeliveryBinding` | express lawful binding |
| `Renderer` | `DeliveryAdapter` | generalize delivery |
| `Controller` | `ConsumerAdapter` | avoid controller-first architecture |
| `ViewModel` | `ConsumerState` | keep state consumer-side |
| `Host` | `RuntimeBoundary` / `DeliverySurface` | reduce host ambiguity |
| `Diagnostics` | `Evidence` | when proof/audit is intended |
| `Manager` | `Policy` / `Coordinator` / `Registry` | expose ownership |
| `Factory` | `Builder` / `Provider` / `Registry` / `Adapter` | expose actual role |
| `Selection` | `Qualification` | when compatibility is tested |
| `Layer` | `Boundary` | when crossing rules matter |
| UI/render model | `SurfaceModel` | preserve consumer neutrality |

Rule:

```text
Hypotheses only.
No removals yet.
No forced renames yet.
```

---

## 3. Constrained Terms

```text
Context
State
Request
Result
Snapshot
Factory
Coordinator
Resolver
Selector
Builder
Adapter
Provider
Registry
Surface
Model
```

Required checks:

```text
owner
authority carried
boundary crossed
meaning vs transport
semantic / process / consumer / delivery / evidence placement
```

---

## 4. Likely Implementation-Facing Terms

```text
Controller
ViewModel
Renderer
Route
Host
Manager
Helper
Args
Defaults
Tracker
```

Candidate target-language alternatives:

```text
Controller -> ConsumerAdapter
ViewModel  -> ConsumerState
Renderer   -> DeliveryAdapter
Route      -> DeliveryBinding
Host       -> RuntimeBoundary / DeliverySurface
Manager    -> Policy / Coordinator / Registry / StateOwner
Helper     -> Adapter / Projector / Formatter / Utility
Args       -> Request / Event / Contract
Defaults   -> Policy / Configuration / Convention
Tracker    -> Recorder / State / Evidence
```

Rule:

```text
Keep available.
Do not promote blindly.
Reassess during target architecture design.
```

---

## 5. Generalization Strip Terms

```text
Chart
Main
Secondary
BarPie
Cartesian
Distribution
Hourly
Weekly
WeekdayTrend
Transform
DiffRatio
Normalized
Combined
Single
Multi
Bucket
Cms
Admin
Syncfusion
Sunburst
LiveCharts
ECharts
Wpf
DateRange
Metric
Subtype
Toggle
Zoom
Theme
Tooltip
```

Use for:

```text
current code
family slices
vendor delivery
concrete migration work
terminal rendering detail
```

Do not use for:

```text
target architecture
semantic authority
capability grammar
consumer-general contracts
evidence model
```

---

## 6. Step 2 Output

```text
Flexible grammar pool established.
Concept promotion deferred to Step 3.
No pruning or binding semantic rules applied yet.
```

Next:

```text
Step 3 — Promote core concepts and define semantic rules.
```


---

---

# Step 3 — Promoted Core Concepts and Semantic Rules

## Purpose

Promote selected vocabulary into binding architectural concepts.

Step 3 is no longer exploratory language gathering. It defines the concepts that should guide ownership, naming, boundaries, implementation decisions, and future refactoring.

```text
vocabulary
-> promoted concept
-> architectural force
-> misuse boundary
-> implementation implication
```

Rule:

```text
A promoted concept is not just a useful word.
It must improve architectural direction, ownership, or enforcement.
```

---

## 1. Promoted Concept Set

| Concept | Definition | Architectural Force | Misuse Boundary | Implementation Implication |
|---|---|---|---|---|
| **Authority** | The right to define semantic truth, legitimacy, and canonical meaning. | Meaning flows from authority downward. | Do not confuse with orchestration or coordination. | UI, rendering, evidence, and process must not invent semantic truth. |
| **Canonical Semantics** | The single authoritative meaning assigned to data, requests, results, and interpretations. | Prevents competing meanings for the same thing. | Do not confuse with labels, display text, or convenience naming. | Semantic meaning must be defined upstream and preserved through contracts. |
| **Lossless Fidelity** | Preservation of original meaning, structure, and recoverable detail through ingestion and transformation. | Prevents destructive simplification. | Do not confuse with visual fidelity. | Transformations must preserve or explicitly annotate loss. |
| **Determinism** | Repeatable behavior for the same inputs, rules, and context. | Makes reasoning, testing, and audit possible. | Do not confuse with lack of flexibility. | Analytical and migration behavior should be reproducible unless variation is explicit. |
| **Reversibility** | Ability to trace or recover prior state, source meaning, or transformation path. | Protects experimentation and correction. | Do not confuse with undo UI only. | Transform/projection paths should preserve lineage and recovery information. |
| **Constraint** | Explicit lawful limit on capability, composition, delivery, or interpretation. | Prevents unbounded feature growth and hidden policy. | Do not confuse with arbitrary restriction. | Constraints should be visible in contracts, policies, and qualifications. |
| **Governance** | Oversight discipline for rules, evidence, constraints, audit, and architectural legitimacy. | Keeps evolution aligned with project law. | Do not confuse with diagnostics. | Governance shapes guardrails without becoming live semantic execution. |
| **Provenance** | The lineage of where data, meaning, decisions, and derived results came from. | Every meaningful result must remain traceable. | Do not reduce to diagnostics. | Results, envelopes, interpretations, and evidence must preserve source lineage. |
| **Envelope** | A truth-carrying wrapper for request/result/state crossing boundaries. | Preserves meaning, provenance, confidence, and reversibility across seams. | Do not use as a generic bag. | Requests/results/snapshots that cross boundaries should carry semantic context explicitly. |
| **Intent** | The declared analytical purpose before execution. | Drives program/capability selection. | Do not confuse with UI event or user gesture. | Analytical work should begin from intent, not from presentation state. |
| **Capability** | A reusable analytical ability that can be requested, composed, qualified, and delivered. | Replaces feature-by-feature growth. | Do not confuse with visible feature. | New analytical behavior should enter through capability structures, not UI/controller paths. |
| **Composition** | Lawful combination of capabilities, operations, results, or overlays. | Enables higher-order analytical behavior. | Do not confuse with builder mechanics. | Composition rules belong upstream of delivery and presentation. |
| **Interpretation** | Explanation or semantic reading of analytical output. | Makes meaning explicit without mutating truth. | Do not confuse with rendering or annotation text. | Interpretive output should be separate from raw computation and presentation. |
| **Confidence** | Explicit certainty, risk, or trust annotation. | Qualifies meaning without changing the result. | Do not use as filtering authority unless explicitly promoted by policy. | Confidence remains annotation unless a later policy consumes it. |
| **Overlay** | Additional interpretive layer applied over authoritative output. | Adds context without altering canonical truth. | Do not confuse with rendering overlay. | Overlay planning belongs in reasoning/interpretation, not terminal rendering. |
| **Program** | Planned analytical work derived from intent and capability. | Bridges reasoning to contract-bound delivery. | Do not equate with chart-only implementation. | Prefer target language like AnalyticalProgram when generalizing. |
| **Contract** | A declared handoff shape between ownership containers. | Makes boundaries explicit and testable. | Do not use as passive DTO naming. | Contracts must prevent semantic leakage and consumer/vendor assumptions. |
| **Boundary** | A governed crossing between responsibility containers. | Controls what may pass and what may not. | Do not confuse with folder/layer grouping. | Boundary violations should be testable or reviewable. |
| **Neutrality** | Freedom from premature UI, vendor, backend, or consumer assumptions. | Protects upstream structures from terminal concerns. | Do not confuse with abstraction for its own sake. | Reasoning/program/contracts should stay consumer/backend neutral until the correct boundary. |
| **Qualification** | Lawful acceptance of provider, backend, adapter, or delivery compatibility. | Prevents invalid handoffs. | Do not reduce to selection. | Selection should be justified by qualification where compatibility matters. |
| **Provider** | A source of capability, delivery, data, or implementation fulfilment behind a contract. | Supplies capability without owning consumer meaning. | Do not let provider become authority. | Provider registration and lookup must respect contracts and qualification. |
| **Consumer** | A downstream user of authoritative output. | Receives meaning without owning truth. | Do not equate with presentation only. | Charts, exports, APIs, plugins, and future clients are consumer families. |
| **Interaction** | Consumer-side behavior, gesture, event, or flow. | Relays behavior without redefining meaning. | Do not confuse with semantic intent. | Tooltips, events, timestamps, and binders must not own provider/semantic policy. |
| **SurfaceModel** | Consumer-neutral shape prepared for delivery or interaction. | Prevents raw semantic internals from leaking into terminal presentation. | Do not confuse with UI model or render model. | Surface models sit between contract and delivery/consumer adaptation. |
| **Binding** | Lawful connection between plan, provider, consumer, execution, or delivery. | Replaces ad hoc route semantics where compatibility matters. | Do not use as hidden policy. | DeliveryBinding / ProviderBinding should be inspectable and qualified. |
| **Delivery** | Terminal realization of output through renderer, export, host, or vendor adapter. | Keeps output mechanisms replaceable. | Do not confuse with semantics. | Rendering/export/vendor lifecycle belongs downstream of contracts. |
| **Evidence** | Observational proof, parity, diagnostics, audit, reachability, or validation. | Proves behavior without controlling it. | Do not let evidence become live authority. | Evidence reads and records; it must not route, select, or mutate semantic output. |
| **Audit** | Durable review path for decisions, evidence, provenance, and migration progress. | Supports reversibility and accountability. | Do not confuse with runtime logging only. | Migration and architecture claims should remain auditable. |

---

## 2. Candidate Grammar Spine

This spine is the promoted conceptual flow used to shape later diagrams.

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

Short form:

```text
Authority
-> Capability
-> Program
-> Contract
-> Consumer
-> Delivery
-> Evidence
```

Interpretation:

```text
Authority defines.
Capability reasons.
Program plans.
Contract constrains.
Consumer receives.
Delivery terminalizes.
Evidence proves.
```

---

## 3. Promoted Concept Families

### 3.1 Authority Family

```text
Authority
Provenance
Envelope
Trust
Lineage
Identity
Invariant
```

Purpose:

```text
preserve truth, source, legitimacy, reversibility, and semantic identity
```

Implementation pressure:

```text
Result / Request / Snapshot should carry explicit semantic context when crossing boundaries.
```

---

### 3.2 Reasoning Family

```text
Intent
Capability
Composition
Interpretation
Confidence
Overlay
Program
Policy
```

Purpose:

```text
turn purpose into lawful analytical work and explicit meaning
```

Implementation pressure:

```text
New capability should enter through reasoning/program structures, not controllers or rendering paths.
```

---

### 3.3 Contract Family

```text
Contract
Boundary
Neutrality
Qualification
Provider
Consumer
Interaction
SurfaceModel
Binding
```

Purpose:

```text
govern crossings, compatibility, consumer neutrality, and delivery readiness
```

Implementation pressure:

```text
Provider/consumer/render-plan/backend handoffs should be qualified and contract-bound.
```

---

### 3.4 Delivery Family

```text
Delivery
Backend
VendorBoundary
RuntimeBoundary
Lifecycle
Adapter
Surface
```

Purpose:

```text
terminalize output through replaceable infrastructure
```

Implementation pressure:

```text
Rendering and vendor-specific delivery must remain downstream and replaceable.
```

---

### 3.5 Evidence Family

```text
Evidence
Diagnostics
Parity
Reachability
Validation
Audit
Record
```

Purpose:

```text
observe, prove, validate, compare, and preserve auditability
```

Implementation pressure:

```text
Evidence must not control live semantic decisions, provider selection, or routing.
```

---

## 4. Semantic Rules

These distinctions are binding unless deliberately changed later.

```text
Authority != Orchestration
Provenance != Diagnostics
Envelope != Bag
Capability != Feature
Composition != Builder
Interpretation != Rendering
Confidence != Truth Mutation
Overlay != Render Decoration
Program != Presentation Path
Contract != DTO
Boundary != Folder
Neutrality != Generic Abstraction
Qualification != Selection
Provider != Authority
Consumer != Presentation
Interaction != Event
SurfaceModel != UI Model
Binding != Hidden Route Policy
Delivery != Semantics
Evidence != Control
Audit != Logging Only
```

---

## 5. Grooming Rules from Step 2

The following are not mandatory renames yet. They are preferred target-language directions.

| Current / implementation-shaped language | Preferred target-language direction |
|---|---|
| `ChartProgram` | `AnalyticalProgram` |
| `Request`, `Result`, `Snapshot` | `Envelope`, where semantic crossing is involved |
| `Route` | `Binding`, where compatibility or policy matters |
| `Renderer` | `DeliveryAdapter`, where delivery is broader than rendering |
| `Controller` | `ConsumerAdapter`, where consumer adaptation is the real role |
| `ViewModel` | `ConsumerState`, where state is consumer-side |
| `Host` | `RuntimeBoundary` or `DeliverySurface` |
| `Diagnostics` | `Evidence`, where proof/audit is intended |
| `Manager` | `Policy`, `Coordinator`, `Registry`, or `StateOwner` |
| `Factory` | `Builder`, `Provider`, `Registry`, or `Adapter` |
| `Selection` | `Qualification`, where compatibility is being tested |
| `Layer` | `Boundary`, where crossing rules matter |
| UI/render model | `SurfaceModel`, where consumer neutrality matters |

Rule:

```text
Use these directions when designing the target architecture.
Do not force renames into current code without implementation justification.
```

---

## 6. Stable Safety Constraints

```text
UI must not own authority.
Rendering must not own meaning.
Evidence must not control live behavior.
Process must not define semantic truth.
Adapters must not become policy owners.
Contexts must not become service locators.
Results must not lose provenance.
Contracts must not smuggle vendor assumptions upstream.
Consumers must not redefine canonical meaning.
Delivery must remain replaceable.
```

---

## 7. Concept Promotion Result

Promoted concept set:

```text
Authority
Canonical Semantics
Provenance
Traceability
Envelope
Lossless Fidelity
Determinism
Reversibility
Constraint
Governance
Intent
Capability
Composition
Transformation
Interpretation
Confidence
Overlay
Program
Contract
Boundary
Neutrality
Qualification
Provider
Consumer
Interaction
SurfaceModel
Binding
Delivery
Evidence
Audit
```

Working architecture phrase:

```text
Authority-bound, canonically semantic, lossless, and traceable analytical capability
expressed through neutral contracts,
adapted by consumers,
terminalized by replaceable delivery,
and governed by observational evidence.
```

---

## 8. Step 3 Output

Step 3 promotes the vocabulary that should now govern target-architecture design.

Next:

```text
Step 4 — Map concept relationships.
```

---

# Step 4 — Concept Relationship Graph

## Purpose

Map the promoted and candidate concepts as a graph.

This step is exploratory.

It does not define final ownership, implementation placement, dependency rules, or migration sequence.

```text
atoms
-> parentage
-> compounds
-> relationship patterns
-> loose strength notes
-> target-architecture input
```

---

## 1. Relationship Model

```text
Concepts form a graph, not a strict tree.
Atomic concepts may have multiple parents.
Compound concepts inherit meaning from both their atoms and their parent context.
A concept may be central in one context and supporting in another.
```

Use this step to discover:

```text
where concepts can live
how concepts combine
which compounds appear architecturally useful
which relationships should be carried into target design
```

Do not use this step to decide:

```text
final ownership
namespace placement
code movement
dependency rules
implementation sequence
vocabulary pruning
```

---

## 2. Atomic Parentage Map

This section maps atomic concepts to possible parent concepts.

The parent list is non-unique and non-final.

| Atomic Concept | Possible Parents | Meaning Under Parent |
|---|---|---|
| `Authority` | Governance, Boundary, Contract, Evidence | semantic legitimacy, decision legitimacy, rule source |
| `Canonical` | Authority, Semantics, Contract, Evidence | single accepted form or meaning |
| `Semantics` | Authority, Interpretation, Contract, Consumer | meaning carried through the system |
| `Provenance` | Authority, Envelope, Evidence, Audit | source lineage and derivation trail |
| `Traceability` | Provenance, Evidence, Audit, Reversibility | ability to follow decisions/results backward |
| `Envelope` | Authority, Contract, Boundary, Evidence | meaning-preserving carrier across seams |
| `Lossless` | Fidelity, Transformation, Envelope, Contract | no unacknowledged information loss |
| `Fidelity` | Lossless, Delivery, Projection, Evidence | preservation of meaning through transformation |
| `Determinism` | Governance, Evidence, Capability, Audit | repeatable behavior under same inputs/rules |
| `Reversibility` | Provenance, Transformation, Audit, Evidence | recovery or reconstruction of prior state/path |
| `Constraint` | Governance, Policy, Contract, Boundary | lawful limitation or control rule |
| `Governance` | Evidence, Audit, Constraint, Authority | oversight discipline and alignment enforcement |
| `Intent` | Capability, Program, Interpretation, Consumer | declared purpose before execution |
| `Capability` | Reasoning, Composition, Contract, Provider | reusable analytical ability |
| `Composition` | Capability, Program, Transformation, Interpretation | lawful combination of capabilities/results |
| `Transformation` | Capability, Composition, Projection, Reversibility | change in form/structure/representation |
| `Interpretation` | Semantics, Confidence, Overlay, Evidence | explicit reading of analytical meaning |
| `Confidence` | Interpretation, Evidence, Trust, Qualification, Envelope | certainty, risk, or reliability annotation |
| `Overlay` | Interpretation, Delivery, Consumer, SurfaceModel | additional context over canonical result |
| `Program` | Intent, Capability, Contract, Execution | planned analytical work |
| `Policy` | Governance, Constraint, Qualification, Process | explicit decision rule |
| `Contract` | Boundary, Provider, Consumer, Delivery | declared handoff shape |
| `Boundary` | Contract, Neutrality, Governance, Runtime | governed crossing between responsibilities |
| `Neutrality` | Contract, Boundary, SurfaceModel, Program | absence of premature consumer/vendor assumptions |
| `Qualification` | Provider, Delivery, Evidence, Contract | compatibility proof or acceptance gate |
| `Provider` | Capability, Contract, Registry, Delivery | fulfiller behind a contract |
| `Consumer` | Contract, Interaction, SurfaceModel, Delivery | downstream receiver of authoritative output |
| `Interaction` | Consumer, SurfaceModel, Event, Delivery | consumer-side behavior or flow |
| `SurfaceModel` | Consumer, Contract, Delivery, Neutrality | consumer-neutral representation for delivery |
| `Binding` | Program, Provider, Consumer, Delivery | lawful connection between selected parts |
| `Delivery` | Consumer, SurfaceModel, Backend, VendorBoundary | terminal realization of output |
| `Evidence` | Governance, Audit, Diagnostics, Qualification | observational proof |
| `Audit` | Evidence, Provenance, Governance, Traceability | durable review path |

---

## 3. Compound Formation Map

This section records useful compound concepts that may help describe the target architecture later.

These compounds are not final type names.

| Compound Concept | Atomized Parts | Lives Under | Purpose |
|---|---|---|---|
| `CanonicalSemantics` | Canonical + Semantics | Authority, Contract | preserve one accepted meaning |
| `TruthEnvelope` | Truth + Envelope | Authority, Boundary | carry authoritative meaning safely |
| `SemanticEnvelope` | Semantic + Envelope | Authority, Contract | transport meaning without losing context |
| `ProvenanceEnvelope` | Provenance + Envelope | Authority, Evidence | carry source lineage across seams |
| `ResultEnvelope` | Result + Envelope | Contract, Evidence | preserve result meaning and metadata |
| `CapabilityContract` | Capability + Contract | Capability, Boundary | declare capability handoff shape |
| `ProviderContract` | Provider + Contract | Boundary, Provider | define provider obligations |
| `ConsumerContract` | Consumer + Contract | Boundary, Consumer | define consumer-facing expectations |
| `DeliveryContract` | Delivery + Contract | Boundary, Delivery | define terminal delivery handoff |
| `InteractionContract` | Interaction + Contract | Consumer, Interaction | constrain consumer behavior exchange |
| `EvidenceContract` | Evidence + Contract | Evidence, Audit | define observational proof shape |
| `SurfaceModel` | Surface + Model | Contract, Consumer, Delivery | neutral output shape before terminal delivery |
| `DeliveryBinding` | Delivery + Binding | Program, Delivery | connect plan to delivery target |
| `ProviderBinding` | Provider + Binding | Contract, Provider | connect qualified provider to need |
| `ConsumerBinding` | Consumer + Binding | Consumer, Contract | connect output to consumer family |
| `RuntimeBoundary` | Runtime + Boundary | Delivery, Process | isolate runtime/host concerns |
| `VendorBoundary` | Vendor + Boundary | Delivery | isolate vendor-specific behavior |
| `QualificationPolicy` | Qualification + Policy | Governance, Contract | define lawful compatibility rules |
| `BackendCapability` | Backend + Capability | Delivery, Provider | describe backend ability |
| `AdapterQualification` | Adapter + Qualification | Delivery, Evidence | prove adapter compatibility |
| `DecisionLineage` | Decision + Lineage | Governance, Audit | trace decision path |
| `InterpretationLineage` | Interpretation + Lineage | Interpretation, Evidence | trace interpretive derivation |
| `EvidenceRecord` | Evidence + Record | Evidence, Audit | durable proof item |
| `AuditRecord` | Audit + Record | Audit, Governance | durable review item |
| `CompositionGraph` | Composition + Graph | Capability, Program | model combinable analytical structure |
| `CapabilityGraph` | Capability + Graph | Capability, Provider | model available capabilities |
| `EvidenceGraph` | Evidence + Graph | Evidence, Audit | model proof relationships |
| `ProvenanceGraph` | Provenance + Graph | Provenance, Audit | model source lineage |
| `SemanticInvariant` | Semantic + Invariant | Authority, Contract | rule preserving meaning |
| `BoundaryInvariant` | Boundary + Invariant | Boundary, Governance | rule preserving seam correctness |
| `NeutralityInvariant` | Neutrality + Invariant | Boundary, Contract | rule preventing premature assumptions |

---

## 4. Relationship Patterns

### 4.1 Carrier Pattern

```text
Envelope carries:
- provenance
- canonical semantics
- confidence
- traceability
- reversibility metadata
```

### 4.2 Qualification Pattern

```text
Qualification relates:
- provider
- backend
- adapter
- contract
- delivery
- evidence
```

### 4.3 Neutrality Pattern

```text
Neutrality protects:
- reasoning from UI assumptions
- contracts from vendor assumptions
- programs from presentation assumptions
- surface models from backend assumptions
```

### 4.4 Evidence Pattern

```text
Evidence observes:
- authority
- provenance
- contracts
- qualification
- delivery
- migration progress
```

### 4.5 Delivery Pattern

```text
Delivery terminalizes:
- surface models
- bindings
- vendor adapters
- runtime boundaries
- lifecycle handling
```

---

## 5. Compound Strength Notes

Loose classification only.

No pruning yet.

| Strength | Meaning |
|---|---|
| `Core` | likely central to final target architecture |
| `Supporting` | useful but probably not central |
| `Candidate` | promising, requires testing in target design |
| `Transitional` | useful for current migration/state description |
| `Deferred` | interesting but not needed now |

Initial classification:

| Compound | Strength |
|---|---|
| `CanonicalSemantics` | Core |
| `ProvenanceEnvelope` | Core |
| `SemanticEnvelope` | Candidate |
| `CapabilityContract` | Core |
| `ProviderContract` | Core |
| `ConsumerContract` | Core |
| `DeliveryContract` | Core |
| `InteractionContract` | Supporting |
| `EvidenceContract` | Candidate |
| `SurfaceModel` | Core |
| `DeliveryBinding` | Core |
| `ProviderBinding` | Supporting |
| `ConsumerBinding` | Supporting |
| `RuntimeBoundary` | Supporting |
| `VendorBoundary` | Supporting |
| `QualificationPolicy` | Supporting |
| `BackendCapability` | Supporting |
| `AdapterQualification` | Supporting |
| `DecisionLineage` | Supporting |
| `InterpretationLineage` | Supporting |
| `EvidenceRecord` | Supporting |
| `AuditRecord` | Supporting |
| `CompositionGraph` | Candidate |
| `CapabilityGraph` | Candidate |
| `EvidenceGraph` | Candidate |
| `ProvenanceGraph` | Candidate |
| `SemanticInvariant` | Candidate |
| `BoundaryInvariant` | Candidate |
| `NeutralityInvariant` | Candidate |

---

## 6. Step 4 Output

Step 4 establishes a concept graph for later target architecture design.

```text
atomic concepts may have many parents
compounds are allowed when purpose-bounded
compound strength is loose, not final
ownership and implementation placement are deferred
```

Next:

```text
Step 5 — Define target architecture.
```

---

## 7. Reduced Grammar from Step 4

This reduction is based on the Step 4 concept graph.

It is architecture-language reduction, not implementation pruning.

The grammar is reduced, not closed. It is optimized for the current target direction while remaining open to governed growth where new concepts improve the project’s ultimate goals.

### 7.1 Reduced Atomized Grammar

```text
Authority
Semantics
Provenance
Traceability
Envelope
Fidelity
Determinism
Reversibility
Constraint
Governance

Intent
Capability
Composition
Transformation
Interpretation
Confidence
Overlay
Program
Policy

Contract
Boundary
Neutrality
Qualification
Provider
Consumer
Interaction
Surface
Model
Binding

Projection
Adapter
Resolver
Selector
Registry

Delivery
Backend
Runtime
Vendor
Lifecycle

Evidence
Diagnostics
Parity
Reachability
Validation
Audit
Record
```

### 7.2 Folded / Collapsed Atom Notes

```text
Canonical -> Semantics / Authority
Trust -> Provenance / Evidence / Confidence
Lineage -> Provenance / Traceability
Identity -> Authority / Semantics / Envelope
Invariant -> Constraint / Governance / Boundary
Layer -> Contextual grouping only
Graph -> Relationship representation only
Kernel -> Implementation detail unless computation-core specific
```

### 7.3 Current Reduced Atomic Grammar

```text
Authority
Semantics
Provenance
Traceability
Envelope
Fidelity
Determinism
Reversibility
Constraint
Governance
Intent
Capability
Composition
Transformation
Interpretation
Confidence
Overlay
Program
Policy
Contract
Boundary
Neutrality
Qualification
Provider
Consumer
Interaction
Surface
Model
Binding
Projection
Adapter
Resolver
Selector
Registry
Delivery
Backend
Runtime
Vendor
Lifecycle
Evidence
Diagnostics
Parity
Reachability
Validation
Audit
Record
```

### 7.4 Current Reduced Compound Grammar

```text
CanonicalSemantics
SemanticAuthority
ProvenanceEnvelope
SemanticEnvelope
ResultEnvelope
RequestEnvelope
CapabilityContract
ProviderContract
ConsumerContract
DeliveryContract
InteractionContract
EvidenceContract
ContractBoundary
ProviderBoundary
ConsumerBoundary
RuntimeBoundary
VendorBoundary
EvidenceBoundary
SurfaceModel
DeliveryBinding
ProviderBinding
ConsumerBinding
ExecutionBinding
CapabilityBinding
QualificationPolicy
BackendCapability
BackendQualification
AdapterQualification
DecisionTrace
ResultTrace
InterpretationTrace
EvidenceRecord
AuditRecord
DiagnosticRecord
ValidationRecord
ParityEvidence
ReachabilityEvidence
TransformationPolicy
CompositionPolicy
DeliveryPolicy
GovernancePolicy
NeutralityConstraint
BoundaryConstraint
SemanticConstraint
FidelityConstraint
ReversibilityConstraint
```

### 7.5 Reduced Grammar Spine

```text
Authority
-> Semantics
-> Provenance / Traceability
-> Envelope
-> Intent
-> Capability
-> Composition / Transformation
-> Interpretation / Confidence / Overlay
-> Program
-> Contract / Boundary
-> Neutrality / Qualification
-> Provider / Consumer / Interaction
-> SurfaceModel
-> Binding
-> Delivery
-> Evidence / Audit
```

Short form:

```text
Authority
-> Capability
-> Contract
-> Consumer
-> Delivery
-> Evidence
```

### 7.6 Governed Extension Pool

Use this pool to describe future code maps and target-architecture permutations. This pool is the current reduced language, not a permanent closure of architectural vocabulary.

```text
Authority / Semantics / Provenance / Traceability / Envelope
Fidelity / Determinism / Reversibility / Constraint / Governance
Intent / Capability / Composition / Transformation / Interpretation / Confidence / Overlay
Program / Policy / Contract / Boundary / Neutrality / Qualification
Provider / Consumer / Interaction / SurfaceModel / Binding
Projection / Adapter / Resolver / Selector / Registry
Delivery / Backend / RuntimeBoundary / VendorBoundary / Lifecycle
Evidence / Diagnostics / Parity / Reachability / Validation / Audit / Record
```

### 7.7 Governed Growth Criteria

New concepts may be added later when they materially improve one or more of:

```text
authority, provenance, semantics, or traceability
losslessness, fidelity, determinism, or reversibility
capability, composition, transformation, or interpretation
contract, boundary, neutrality, or qualification
consumer / delivery separation
observational evidence, validation, audit, or governance
current-code clarity or future code-map clarity
```

Reject new concepts when they only:

```text
rename implementation detail
duplicate an existing concept
add aesthetic vocabulary without architectural force
hide ownership, policy, or authority
centralize delivery, UI, or vendor concerns
```

Growth rule:

```text
optimized
bounded
extensible
governed by project goals
```

---

### 7.8 Step 4 Reduction Output

```text
The concept graph reduces to a compact but extensible grammar capable of describing future code maps:
authority-bound meaning,
traceable and reversible envelopes,
capability-driven composition,
contract-bound neutrality,
qualified provider/consumer delivery,
observational evidence,
and governed growth.
```

Next:

```text
Step 5 — Define target architecture.
```

---

---

# Step 5 — Target Architecture Specification

## 1. Target Architecture Specification

DataVisualiser must become an authority-bound analytical architecture.

It must preserve canonical semantics, provenance, traceability, fidelity, determinism, and reversibility from ingestion through delivery.

Analytical work must be expressed through intent, capability, composition, transformation, interpretation, confidence, overlay, and program structures.

Downstream use must cross explicit contracts, boundaries, bindings, and qualifications.

Consumers must remain thin.

Delivery must remain terminal and replaceable.

Evidence must remain observational, auditable, and non-controlling.

Governed growth is allowed only when it strengthens the project’s core architectural aims.

---

## 2. Target Architecture Outcome

```text
Authority defines meaning.
Provenance preserves lineage.
Envelopes carry semantic context.
Capabilities express analytical power.
Composition and transformation create higher-order output.
Interpretation, confidence, and overlays explain without mutating truth.
Programs describe intended analytical work.
Contracts and boundaries govern handoff.
Neutrality prevents premature UI/vendor/backend/consumer assumptions.
Qualification prevents invalid provider/backend/adapter/delivery use.
Consumers receive meaning without owning it.
Interaction relays behavior without redefining intent.
Surface models prepare consumer-neutral output.
Bindings connect qualified parts lawfully.
Delivery terminalizes output through replaceable infrastructure.
Evidence proves behavior without controlling behavior.
Governance permits aligned growth.
```

---

## 3. Current vs Target Gap Comparison

| Area | Current Architecture | Target Architecture | Gap |
|---|---|---|---|
| Authority | Authority concepts exist, but older UI/process/rendering paths still carry integration weight. | Semantic authority is explicit and upstream. | Prevent old hubs from absorbing meaning. |
| Semantics | Canonical semantics are partially represented through VNext vocabulary. | Semantics are preserved consistently through envelopes, contracts, and evidence. | Make semantic preservation explicit across handoffs. |
| Provenance / Traceability | Provenance and evidence structures exist. | Lineage is carried through analytical and delivery boundaries. | Ensure results cannot lose source/context lineage. |
| Capability | Capability and reasoning structures exist. | New analytical behavior enters through capability/composition/program structures. | Stop new capability entering through controllers/rendering paths. |
| Composition / Transformation | Composition and operation vocabulary exists. | Transformations are reversible, traceable, and governed. | Generalize transformation beyond current chart-specific paths. |
| Interpretation / Confidence / Overlay | Structures exist but are still young. | Interpretation, confidence, and overlays explain meaning without mutating truth. | Guard them from rendering/UI ownership. |
| Program / Plan | Program and render-plan structures exist. | Programs are analytical and consumer-neutral before delivery. | Reduce chart-first target language over time. |
| Contracts / Boundaries | Consumer/provider contracts and render-plan qualification exist. | Contracts and boundaries are the main downstream fan-out seam. | Harden boundary enforcement and bypass prevention. |
| Neutrality | Some VNext structures are neutral, but old paths remain UI/rendering-shaped. | Upstream architecture remains UI/vendor/backend/consumer neutral. | Prevent terminal assumptions leaking upstream. |
| Qualification | Backend/provider/adapter qualification exists. | Qualification governs compatibility before use. | Treat qualification as enforcement, not incidental selection. |
| Consumer / Interaction | Interaction and tooltip seams exist, but UI remains broad. | Consumers receive output; interaction relays behavior only. | Keep consumer/interaction non-authoritative. |
| Surface / Delivery | Render plans and render surfaces exist; rendering remains large. | Delivery is terminal, replaceable, and vendor-contained. | Demote rendering further and isolate vendor concerns. |
| Evidence / Governance | Evidence, parity, diagnostics, and reachability are strong. | Evidence proves and audits without controlling live behavior. | Guard evidence from becoming execution policy. |
| Process / Orchestration | Coordinators, factories, pipelines, and stages remain heavy. | Process sequences work without owning semantic truth. | Cap hubs and prevent service-locator/context growth. |
| Growth | Vocabulary and architecture are evolving. | Growth is governed by semantic, provenance, capability, contract, and evidence value. | Allow extension without abstraction sprawl. |

---

## 4. Target Non-Goals

```text
not chart-first
not UI-first
not renderer-first
not controller-first
not vendor-first
not diagnostics-controlled
not feature-by-feature accumulation
not adapter/coordinator mesh architecture
```

Forbidden outcomes:

```text
UI owns authority.
Rendering owns meaning.
Evidence controls live behavior.
Process defines semantic truth.
Adapters become policy owners.
Contexts become service locators.
Results lose provenance.
Contracts smuggle vendor assumptions upstream.
Consumers redefine canonical meaning.
Delivery becomes non-replaceable.
```

---

## 5. Target Architecture Success Standard

Future code maps should be describable primarily through:

```text
Authority / Semantics / Provenance / Traceability / Envelope
Fidelity / Determinism / Reversibility / Constraint / Governance
Intent / Capability / Composition / Transformation / Interpretation / Confidence / Overlay
Program / Policy / Contract / Boundary / Neutrality / Qualification
Provider / Consumer / Interaction / SurfaceModel / Binding
Projection / Adapter / Resolver / Selector / Registry
Delivery / Backend / RuntimeBoundary / VendorBoundary / Lifecycle
Evidence / Diagnostics / Parity / Reachability / Validation / Audit / Record
```

Concrete family/vendor/UI terms may remain implementation details, but must not define the architecture.

```text
Chart
Main
Secondary
BarPie
Cartesian
Distribution
Syncfusion
LiveCharts
ECharts
Wpf
Tooltip
```

---

## 6. Step 5 Starting Position

```text
The target architecture is the governing shape that prevents analytical meaning
from being captured by UI, rendering, vendor, or coordination concerns.
```

Next within Step 5:

```text
Define the target architecture containers and flow.
```

---

---

---

# Step 5 — Target Architecture Code Map

```text
TARGET ARCHITECTURE CODE MAP

DataVisualiser
├── Authority
│   ├── SemanticAuthority
│   ├── CanonicalSemantics
│   ├── AuthorityBoundary
│   ├── AuthorityEnvelope
│   ├── AuthorityRecord
│   ├── AuthorityConstraint
│   └── AuthorityPolicy
│
├── Authority.Provenance
│   ├── ProvenanceDescriptor
│   ├── ProvenanceEnvelope
│   ├── ProvenanceRecord
│   ├── ProvenanceTrace
│   ├── TraceabilityRecord
│   ├── DecisionTrace
│   ├── ResultTrace
│   ├── InterpretationTrace
│   ├── TransformationTrace
│   └── ProvenanceAudit
│
├── Authority.Fidelity
│   ├── LosslessConstraint
│   ├── FidelityConstraint
│   ├── ReversibilityConstraint
│   ├── DeterminismConstraint
│   ├── TransformationRecord
│   ├── RecoveryRecord
│   ├── FidelityEvidence
│   └── ReversibilityEvidence
│
├── Envelopes
│   ├── EnvelopeMetadata
│   ├── SemanticEnvelope
│   ├── RequestEnvelope
│   ├── ResultEnvelope
│   ├── ProgramEnvelope
│   ├── InterpretationEnvelope
│   ├── SurfaceEnvelope
│   ├── DeliveryEnvelope
│   ├── EvidenceEnvelope
│   └── AuditEnvelope
│
├── Semantics
│   ├── SemanticModel
│   ├── SemanticDescriptor
│   ├── SemanticIdentity
│   ├── SemanticConstraint
│   ├── SemanticPolicy
│   ├── SemanticProjection
│   ├── SemanticContract
│   └── SemanticEvidence
│
├── Reasoning
│   ├── AnalyticalIntent
│   ├── IntentResolver
│   ├── CapabilityRequest
│   ├── CapabilityModel
│   ├── CapabilityRegistry
│   ├── CapabilityPolicy
│   ├── CapabilityContract
│   ├── CapabilityQualification
│   └── CapabilityDiagnostics
│
├── Reasoning.Composition
│   ├── CompositionModel
│   ├── CompositionPolicy
│   ├── CompositionGraph
│   ├── CompositionNode
│   ├── CompositionEdge
│   ├── CompositionConstraint
│   ├── CompositionEvidence
│   └── CompositionTrace
│
├── Reasoning.Transformation
│   ├── TransformationModel
│   ├── TransformationPolicy
│   ├── TransformationGraph
│   ├── TransformationNode
│   ├── TransformationEdge
│   ├── TransformationConstraint
│   ├── TransformationRecord
│   ├── TransformationTrace
│   └── TransformationEvidence
│
├── Reasoning.Interpretation
│   ├── InterpretationModel
│   ├── InterpretationPolicy
│   ├── InterpretationResult
│   ├── InterpretationTrace
│   ├── InterpretationEnvelope
│   ├── InterpretationEvidence
│   └── InterpretationDiagnostics
│
├── Reasoning.Confidence
│   ├── ConfidenceModel
│   ├── ConfidencePolicy
│   ├── ConfidenceAnnotation
│   ├── ConfidenceEvidence
│   ├── ConfidenceConstraint
│   └── ConfidenceDiagnostics
│
├── Reasoning.Overlay
│   ├── OverlayModel
│   ├── OverlayPolicy
│   ├── OverlayPlan
│   ├── OverlayProjection
│   ├── OverlayEvidence
│   └── OverlayDiagnostics
│
├── Program
│   ├── AnalyticalProgram
│   ├── ProgramRequest
│   ├── ProgramPlan
│   ├── ProgramResult
│   ├── ProgramPolicy
│   ├── ProgramContract
│   ├── ProgramEnvelope
│   ├── ProgramTrace
│   └── ProgramDiagnostics
│
├── Program.Execution
│   ├── ExecutionPlan
│   ├── ExecutionBinding
│   ├── ExecutionPolicy
│   ├── ExecutionState
│   ├── ExecutionRecord
│   ├── ExecutionEvidence
│   └── ExecutionDiagnostics
│
├── Contracts
│   ├── SemanticContract
│   ├── CapabilityContract
│   ├── ProgramContract
│   ├── ProviderContract
│   ├── ConsumerContract
│   ├── InteractionContract
│   ├── SurfaceContract
│   ├── DeliveryContract
│   ├── EvidenceContract
│   ├── AuditContract
│   └── ContractBoundary
│
├── Boundaries
│   ├── AuthorityBoundary
│   ├── SemanticBoundary
│   ├── ContractBoundary
│   ├── ProviderBoundary
│   ├── ConsumerBoundary
│   ├── InteractionBoundary
│   ├── SurfaceBoundary
│   ├── DeliveryBoundary
│   ├── RuntimeBoundary
│   ├── VendorBoundary
│   ├── EvidenceBoundary
│   └── AuditBoundary
│
├── Boundaries.Constraints
│   ├── BoundaryConstraint
│   ├── NeutralityConstraint
│   ├── FidelityConstraint
│   ├── ReversibilityConstraint
│   ├── DeterminismConstraint
│   ├── ConsumerNeutralityConstraint
│   ├── BackendNeutralityConstraint
│   ├── VendorNeutralityConstraint
│   └── PresentationNeutralityConstraint
│
├── Qualification
│   ├── QualificationPolicy
│   ├── CapabilityQualification
│   ├── ProviderQualification
│   ├── ConsumerQualification
│   ├── BackendQualification
│   ├── AdapterQualification
│   ├── SurfaceQualification
│   ├── DeliveryQualification
│   ├── QualificationRecord
│   └── QualificationEvidence
│
├── Providers
│   ├── ProviderRegistry
│   ├── ProviderDescriptor
│   ├── ProviderCapability
│   ├── ProviderContract
│   ├── ProviderBinding
│   ├── ProviderMetadata
│   ├── ProviderProjection
│   ├── ProviderQualification
│   └── ProviderDiagnostics
│
├── Projection
│   ├── SemanticProjector
│   ├── CapabilityProjector
│   ├── ProgramProjector
│   ├── ProviderProjector
│   ├── ConsumerProjector
│   ├── InteractionProjector
│   ├── SurfaceProjector
│   ├── DeliveryProjector
│   ├── EvidenceProjector
│   ├── AuditProjector
│   └── ProjectionRecord
│
├── Consumers
│   ├── ConsumerRegistry
│   ├── ConsumerDescriptor
│   ├── ConsumerContract
│   ├── ConsumerBinding
│   ├── ConsumerSurface
│   ├── ConsumerState
│   ├── ConsumerAdapter
│   ├── ConsumerProjection
│   ├── ConsumerQualification
│   └── ConsumerDiagnostics
│
├── Interaction
│   ├── InteractionRequest
│   ├── InteractionContract
│   ├── InteractionState
│   ├── InteractionBinding
│   ├── InteractionFlow
│   ├── InteractionPolicy
│   ├── EventBinding
│   ├── InteractionAdapter
│   └── InteractionDiagnostics
│
├── Surfaces
│   ├── SurfaceModel
│   ├── SurfaceEnvelope
│   ├── SurfaceContract
│   ├── SurfaceProjection
│   ├── SurfaceBinding
│   ├── SurfaceQualification
│   ├── SurfaceValidation
│   └── SurfaceDiagnostics
│
├── Delivery
│   ├── DeliveryPlan
│   ├── DeliveryContract
│   ├── DeliveryBinding
│   ├── DeliveryPolicy
│   ├── DeliveryAdapter
│   ├── DeliverySurface
│   ├── DeliveryEnvelope
│   ├── DeliveryLifecycle
│   ├── DeliveryQualification
│   └── DeliveryDiagnostics
│
├── Delivery.Backends
│   ├── BackendRegistry
│   ├── BackendCapability
│   ├── BackendContract
│   ├── BackendQualification
│   ├── BackendBinding
│   ├── BackendAdapter
│   ├── BackendLifecycle
│   └── BackendDiagnostics
│
├── Delivery.Vendors
│   ├── VendorBoundary
│   ├── VendorContract
│   ├── VendorAdapter
│   ├── VendorSurface
│   ├── VendorLifecycle
│   ├── VendorQualification
│   └── VendorDiagnostics
│
├── Evidence
│   ├── EvidenceContract
│   ├── EvidenceEnvelope
│   ├── EvidenceRecord
│   ├── EvidenceProjection
│   ├── EvidenceDiagnostics
│   ├── EvidenceExport
│   ├── EvidenceAudit
│   └── EvidencePolicy
│
├── Evidence.Diagnostics
│   ├── DiagnosticRecord
│   ├── DiagnosticSnapshot
│   ├── RuntimeDiagnostic
│   ├── SemanticDiagnostic
│   ├── ContractDiagnostic
│   ├── BoundaryDiagnostic
│   ├── QualificationDiagnostic
│   └── DeliveryDiagnostic
│
├── Evidence.Parity
│   ├── ParityEvidence
│   ├── ParityRecord
│   ├── ParityValidator
│   ├── ParitySnapshot
│   └── ParityReport
│
├── Evidence.Reachability
│   ├── ReachabilityEvidence
│   ├── ReachabilityRecord
│   ├── ReachabilityValidator
│   ├── ReachabilitySnapshot
│   └── ReachabilityReport
│
├── Evidence.Validation
│   ├── ValidationEvidence
│   ├── ValidationRecord
│   ├── ValidationPolicy
│   ├── ValidationResult
│   └── ValidationReport
│
├── Audit
│   ├── AuditRecord
│   ├── AuditEnvelope
│   ├── AuditContract
│   ├── AuditProjection
│   ├── AuditReport
│   └── AuditExport
│
└── Governance
    ├── GovernancePolicy
    ├── GovernanceConstraint
    ├── GrowthConstraint
    ├── SemanticConstraint
    ├── BoundaryConstraint
    ├── NeutralityConstraint
    ├── FidelityConstraint
    ├── ReversibilityConstraint
    ├── DeterminismConstraint
    ├── GovernanceEvidence
    └── GovernanceAudit
```

---

# Target Architecture Flow Map

```text
Authority
   ├── CanonicalSemantics
   ├── Provenance
   ├── Traceability
   ├── Fidelity
   ├── Determinism
   └── Reversibility
        │
        v
Envelopes
   ├── RequestEnvelope
   ├── ResultEnvelope
   ├── ProgramEnvelope
   ├── SurfaceEnvelope
   ├── DeliveryEnvelope
   └── EvidenceEnvelope
        │
        v
Reasoning
   ├── Intent
   ├── Capability
   ├── Composition
   ├── Transformation
   ├── Interpretation
   ├── Confidence
   └── Overlay
        │
        v
Program
   ├── AnalyticalProgram
   ├── ProgramPlan
   ├── ExecutionPlan
   └── ExecutionBinding
        │
        v
Contracts / Boundaries
   ├── SemanticContract
   ├── CapabilityContract
   ├── ProviderContract
   ├── ConsumerContract
   ├── SurfaceContract
   ├── DeliveryContract
   └── EvidenceContract
        │
        v
Qualification
   ├── ProviderQualification
   ├── ConsumerQualification
   ├── BackendQualification
   ├── AdapterQualification
   ├── SurfaceQualification
   └── DeliveryQualification
        │
        v
Providers / Projection
   ├── ProviderRegistry
   ├── ProviderBinding
   ├── SemanticProjection
   ├── CapabilityProjection
   ├── ConsumerProjection
   ├── SurfaceProjection
   └── DeliveryProjection
        │
        v
Consumers / Interaction
   ├── ConsumerBinding
   ├── ConsumerSurface
   ├── ConsumerState
   ├── ConsumerAdapter
   ├── InteractionRequest
   ├── InteractionBinding
   └── InteractionFlow
        │
        v
Surfaces / Delivery
   ├── SurfaceModel
   ├── SurfaceBinding
   ├── DeliveryBinding
   ├── DeliveryAdapter
   ├── DeliverySurface
   ├── BackendAdapter
   ├── RuntimeBoundary
   └── VendorBoundary

Evidence observes all seams.
Audit records reviewable lineage.
Governance constrains growth.
Delivery remains terminal.
```

---

# Current vs Target Architecture Comparison

```text
CURRENT ARCHITECTURE

DataVisualiser
├── Core
│   ├── computation
│   ├── contracts
│   ├── rendering contracts
│   ├── strategies
│   ├── diagnostics
│   └── shared state/result models
│
├── VNext
│   ├── analytical intent
│   ├── reasoning engine
│   ├── capability request
│   ├── composition
│   ├── interpretation
│   ├── confidence
│   ├── overlay
│   ├── chart program
│   ├── render plan
│   ├── provider metadata
│   └── delivery binding
│
├── UI
│   ├── views
│   ├── view models
│   ├── chart controllers
│   ├── controller adapters
│   ├── rendering surfaces
│   ├── tooltip / interaction helpers
│   ├── factories
│   ├── coordinators
│   └── presentation state
│
├── Rendering / Delivery
│   ├── render engines
│   ├── render adapters
│   ├── render surfaces
│   ├── backend selectors
│   ├── rendering qualifications
│   └── vendor-specific infrastructure
│
├── Process / Orchestration
│   ├── coordinators
│   ├── orchestrators
│   ├── pipelines
│   ├── stages
│   ├── factories
│   ├── resolvers
│   └── selectors
│
└── Evidence / Diagnostics
    ├── parity
    ├── reachability
    ├── diagnostics snapshots
    ├── evidence exports
    ├── validation
    └── session/runtime records
```

```text
TARGET ARCHITECTURE

DataVisualiser
├── Authority / Provenance / Fidelity
├── Envelopes
├── Semantics
├── Reasoning / Capability / Composition / Transformation
├── Interpretation / Confidence / Overlay
├── Program / Execution
├── Contracts / Boundaries
├── Qualification
├── Providers / Projection
├── Consumers / Interaction
├── Surfaces / Delivery
├── Evidence / Diagnostics / Parity / Reachability / Validation
├── Audit
└── Governance
```

```text
COMPARISON SUMMARY

Current:
- target-aligned VNext vocabulary exists
- authority, reasoning, render-plan, provider, delivery, and evidence structures are visible
- Core, UI, rendering, process, and evidence still operate as strong peer structures
- UI/process/rendering hubs still carry integration pressure
- chart/rendering terminology remains structurally dominant
- evidence is strong but must remain observational

Target:
- authority and canonical semantics are explicit upstream containers
- provenance, traceability, fidelity, determinism, and reversibility are preserved through envelopes
- reasoning owns capability, composition, transformation, interpretation, confidence, and overlay
- program structures bridge reasoning into downstream contracts
- contracts, boundaries, qualifications, and bindings become the required downstream seam
- projection translates without owning authority
- consumers and interaction remain thin and non-authoritative
- surfaces are consumer-neutral before delivery
- delivery is terminal, replaceable, backend/vendor-contained infrastructure
- evidence observes and audits without controlling live behavior
- governance constrains growth without becoming runtime semantic execution
```

```text
TARGET EXPANSION OVER PRIOR MAP

- Semantics is now explicit, not only implied under Authority.
- Fidelity is expanded into lossless, deterministic, reversible movement.
- Transformation is separated from Composition.
- Confidence and Overlay are preserved as reasoning subdomains.
- Program.Execution separates analytical planning from execution state.
- Boundary constraints are explicitly separated from contracts.
- SurfaceModel is elevated as a consumer-neutral bridge before delivery.
- Audit is separated from Evidence as durable review.
- Governance owns growth and constraint discipline.
- Chart/render/vendor/UI language is not structurally central.
```

---

# Step 5 — Gap / Risk Register

| Gap | Current Evidence | Target Requirement | Migration Implication | Risk |
|---|---|---|---|---|
| Authority / Semantics | UI, process, and rendering hubs still carry meaning-adjacent flow. | Meaning remains upstream, canonical, and explicit. | Cap hubs first. | semantic drift |
| Provenance / Traceability | Provenance exists, but lineage is not yet uniformly carried through all seams. | Results, interpretations, surfaces, and evidence remain traceable. | Preserve lineage across handoffs. | lost audit path |
| Fidelity / Reversibility | Transformation exists mostly through current analytical/rendering paths. | Transformations remain lossless, reversible, and explicit. | Generalize transformation discipline. | destructive simplification |
| Capability / Composition | Capability and composition exist in VNext, but old strategy/controller paths remain active. | New analytical behavior enters through capability/composition/program structures. | Route new capability upstream. | feature sprawl |
| Interpretation / Confidence / Overlay | Structures exist, but are still young and near delivery concerns. | Explanation remains reasoning-side and non-mutating. | Guard reasoning ownership. | presentation-owned meaning |
| Program / Execution | Chart-shaped program structures exist. | Program describes analytical work before consumer-specific delivery. | Reduce chart-first framing. | target bias |
| Contracts / Boundaries | Consumer/provider contracts exist beside older rendering contracts. | Contracts and boundaries become required downstream seam. | Harden seam. | bypass paths |
| Neutrality | VNext is partly neutral, but old paths remain UI/render/vendor-shaped. | Upstream remains consumer/backend/vendor neutral. | Block assumption leakage. | terminal capture |
| Qualification | Backend/provider/adapter qualification exists. | Qualification governs compatibility before use. | Treat as enforcement. | invalid handoff |
| Projection | Projection exists through render-plan/projector structures. | Projection translates without authority. | Keep non-authoritative. | hidden policy |
| Consumer / Interaction | Tooltip/interaction seams exist; UI remains broad. | Consumers receive; interactions relay. | Keep interaction thin. | consumer authority |
| SurfaceModel / Delivery | Render surfaces exist; SurfaceModel role needs target centrality. | SurfaceModel bridges consumer-neutral output to delivery. | Elevate surface seam. | delivery coupling |
| Backend / Vendor | Syncfusion/LiveCharts/ECharts concerns remain concrete and visible. | Backend/vendor behavior remains terminal. | Isolate vendor boundary. | vendor lock-in |
| Evidence / Diagnostics | Evidence is strong and broad. | Evidence observes, validates, audits, and exports only. | Guard non-control. | diagnostics control |
| Audit / Governance | Audit/governance are implied more than structurally dominant. | Governance constrains growth; audit preserves reviewability. | Make closure auditable. | unmanaged expansion |
| Legacy Coexistence | Legacy/Core/UI/VNext paths coexist. | Coexistence is transitional and bounded. | Retire selectively. | permanent mesh |
| Dependency Density | Dense hubs remain visible in current structure. | Density is justified only by governed seams. | Classify before refactor. | false cleanup |

---

# Step 6 — Dependency-Density Audit Reconciliation

Dependency-density audit reconciliation is required before any density-driven refactoring. Dense type relationships must be classified before they are treated as design faults.

Classification language:

- Legitimate steady-state coupling: density caused by a stable seam, authority carrier, contract, or common domain type that is expected to remain shared.
- Legitimate transitional coupling: density caused by an active migration bridge, compatibility path, parity lane, or legacy/VNext coexistence path that should shrink only after replacement proof exists.
- Diagram/export noise: density caused by generated diagram shape, repeated edge emission, broad test references, framework plumbing, or non-semantic export artifacts.
- Accidental coupling: density caused by avoidable responsibility spread, hidden ownership, convenience access, or a type absorbing work that belongs behind a clearer seam.

Current remaining architecture plan:

Only one step should be treated as active at a time. The migration sequence remains baseline establishment, generated-governance repair, density classification, refactoring-opportunity classification, seam hardening, hub containment, contract/boundary/qualification enforcement, projection neutrality, consumer thinning, surface elevation, delivery demotion, evidence observation, governance constraints, capability expansion, non-chart consumer proof, legacy bypass retirement, and final repeated-pattern consolidation.
