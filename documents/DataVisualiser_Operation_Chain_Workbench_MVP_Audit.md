# DataVisualiser Operation Chain Workbench MVP Audit

Recorded: 2026-04-30

Phase: 26 - Operation Chain Workbench MVP

## Implementation Scope

Phase 26 adds the first operation-chain MVP slice:

```text
DataVisualiser/VNext/Contracts/OperationChainContracts.cs
DataVisualiser/VNext/Application/OperationChainExecutor.cs
DataVisualiser/UI/OperationChain/OperationChainWorkbenchView.xaml
DataVisualiser/UI/OperationChain/OperationChainWorkbenchView.xaml.cs
```

The workbench tab is registered in:

```text
DataVisualiser/MainWindow.xaml
```

## Execution Shape

The operation-chain core:

```text
loads a MetricLoadSnapshot through IReasoningEngine
aligns source series through TimeSeriesAlignmentKernel
executes ordered SeriesOperationRequest steps through OperationKernel
adds each derived output back into the working series set
allows later steps to consume earlier derived outputs
returns one or more DerivedDataset outputs
```

The initial supported operation family is inherited from the existing VNext operation kernel:

```text
Identity
Normalize
Sum
Difference
Ratio
Logarithm
SquareRoot
MovingAverage
```

The focused tests prove at least three operations in a single chain:

```text
Sum
Ratio
MovingAverage
```

## Metadata, Provenance, and Evidence

The result preserves:

```text
source selection signature
source series signatures
operation plan signature
per-step trace entries
reversibility metadata
lossiness metadata
derived dataset operation signatures
Phase 25 VNextUiConsumptionContract signature
consumer delivery metadata
provider metadata
consumer-neutral surface metadata
```

## UI Boundary

The tab surface is intentionally display-only in this slice.

It can render an `OperationChainResult` into:

```text
summary text
operation trace rows
derived dataset table rows
evidence signature text
```

It does not:

```text
execute operations
load metric data
own analytical authority
use ChartDataContext as its semantic model
import LiveCharts or Syncfusion
choose provider/backend policy
```

## Manual Checkpoint

Automated tests validate the core execution and UI boundary.

Manual smoke testing is warranted after this phase because a new visible tab has been added to the main application shell.

Minimum smoke check:

```text
launch application
confirm the Operation Chain tab appears between Syncfusion and Admin
switch to the Operation Chain tab
confirm the empty tab surface renders without layout exceptions
switch back to Charts and Syncfusion
confirm existing tab switching still works
```
