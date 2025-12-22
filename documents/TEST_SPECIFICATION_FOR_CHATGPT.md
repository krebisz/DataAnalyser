# Unit Test Specification for DataVisualiser

**Purpose**: This document provides a structured, modular breakdown of unit tests needed for DataVisualiser. Use this with the actual code files to generate comprehensive unit tests.

**Test Framework**: xUnit 2.9.2  
**Mocking Framework**: Moq 4.20.72  
**Target Framework**: .NET 9.0-windows

---

## Test Organization

```
DataVisualiser.Tests/
├── Parity/              (Priority 1 - Phase 4A)
│   ├── CombinedMetricParityTests.cs ✅ (DONE)
│   ├── SingleMetricParityTests.cs ✅ (DONE)
│   └── MultiMetricParityTests.cs
├── Strategies/          (Priority 2)
│   ├── SingleMetricStrategyTests.cs
│   ├── CombinedMetricStrategyTests.cs
│   ├── MultiMetricStrategyTests.cs
│   ├── DifferenceStrategyTests.cs
│   ├── RatioStrategyTests.cs
│   ├── NormalizedStrategyTests.cs
│   ├── WeeklyDistributionStrategyTests.cs
│   ├── WeekdayTrendStrategyTests.cs
│   └── TransformResultStrategyTests.cs (Phase 4)
├── Transforms/          (Priority 2 - Phase 4)
│   ├── TransformExpressionEvaluatorTests.cs
│   ├── TransformExpressionBuilderTests.cs
│   ├── TransformDataHelperTests.cs
│   └── TransformOperationRegistryTests.cs
├── Services/            (Priority 3)
│   ├── MetricSelectionServiceTests.cs
│   ├── ChartUpdateCoordinatorTests.cs
│   ├── WeeklyDistributionServiceTests.cs
│   └── ChartDataContextBuilderTests.cs
├── Repositories/        (Priority 3)
│   ├── DataFetcherTests.cs
│   └── CmsDataServiceTests.cs
├── ViewModels/          (Priority 4)
│   └── MainWindowViewModelTests.cs
├── Helpers/             (Priority 5)
│   ├── TestDataBuilders.cs ✅ (DONE)
│   ├── TestHelpers.cs ✅ (DONE)
│   ├── ChartHelperTests.cs
│   ├── MathHelperTests.cs
│   └── CmsConversionHelperTests.cs
└── State/               (Priority 5)
    ├── ChartStateTests.cs
    ├── MetricStateTests.cs
    └── CmsConfigurationTests.cs
```

---

## Module 1: Parity Tests (CRITICAL - Phase 4A)

### 1.1 CombinedMetricParityTests ✅ COMPLETE

**File**: `DataVisualiser.Tests/Parity/CombinedMetricParityTests.cs`  
**Status**: ✅ Implemented

**Tests**:

- ✅ `Parity_ShouldPass_WithIdenticalData`
- ✅ `Parity_ShouldPass_WithEmptyData`
- ✅ `Parity_ShouldPass_WithMismatchedCounts`

**Dependencies**:

- `CombinedMetricStrategy` (legacy)
- `CombinedMetricCmsStrategy` (CMS)
- `CombinedMetricParityHarness`
- `TestDataBuilders`

---

### 1.2 SingleMetricParityTests ✅ COMPLETE

**File**: `DataVisualiser.Tests/Parity/SingleMetricParityTests.cs`  
**Status**: ✅ Implemented

**Tests**:

- ✅ `Parity_ShouldPass_WithIdenticalData`
- ✅ `Parity_ShouldPass_WithEmptyData`
- ✅ `Parity_ShouldPass_WithNullValues`

**Dependencies**:

- `SingleMetricStrategy` (legacy)
- `SingleMetricCmsStrategy` (CMS)
- `TestDataBuilders`

---

### 1.3 MultiMetricParityTests

**File**: `DataVisualiser.Tests/Parity/MultiMetricParityTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Parity_ShouldPass_WithThreeMetrics()
[Fact] public void Parity_ShouldPass_WithEmptyData()
[Fact] public void Parity_ShouldPass_WithMismatchedCounts()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/MultiMetricStrategy.cs`
- `DataVisualiser/Charts/Parity/IStrategyParityHarness.cs`

**Test Pattern**:

1. Create 3+ legacy HealthMetricData series
2. Create 3+ matching CMS series
3. Execute both strategies
4. Compare results via parity harness (if exists) or direct comparison
5. Assert structural, temporal, and value parity

**Mock Requirements**:

- Use `TestDataBuilders.HealthMetricData()` for legacy
- Use `TestDataBuilders.CanonicalMetricSeries()` for CMS

---

## Module 2: Strategy Tests

### 2.1 SingleMetricStrategyTests

**File**: `DataVisualiser.Tests/Strategies/SingleMetricStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldReturnNull_WhenDataIsEmpty()
[Fact] public void Compute_ShouldReturnNull_WhenAllValuesAreNull()
[Fact] public void Compute_ShouldFilterNullValues()
[Fact] public void Compute_ShouldOrderByTimestamp()
[Fact] public void Compute_ShouldFilterByDateRange()
[Fact] public void Compute_ShouldGenerateSmoothedData()
[Fact] public void Compute_ShouldSetUnit_FromFirstDataPoint()
[Fact] public void Compute_ShouldHandleCmsData_WhenCmsConstructorUsed()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/SingleMetricStrategy.cs`
- `DataVisualiser/Charts/Computation/ChartComputationResult.cs`
- `DataVisualiser/Helper/MathHelper.cs`

**Test Data**:

- Use `TestDataBuilders.HealthMetricData().BuildSeries(count, interval)`
- Test with various date ranges, null values, empty collections

---

### 2.2 CombinedMetricStrategyTests

**File**: `DataVisualiser.Tests/Strategies/CombinedMetricStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldReturnNull_WhenBothSeriesEmpty()
[Fact] public void Compute_ShouldReturnNull_WhenOneSeriesEmpty()
[Fact] public void Compute_ShouldAlignByIndex_NotTimestamp()
[Fact] public void Compute_ShouldUseMinCount_ForAlignment()
[Fact] public void Compute_ShouldResolveUnit_FromLeftSeries()
[Fact] public void Compute_ShouldGenerateBothSmoothedSeries()
[Fact] public void Compute_ShouldFilterByDateRange()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/CombinedMetricStrategy.cs`
- `DataVisualiser/Helper/StrategyComputationHelper.cs`

**Test Scenarios**:

- Left=10 points, Right=10 points → 10 aligned points
- Left=10 points, Right=8 points → 8 aligned points
- Different timestamps but same count → aligned by index
- Empty left, non-empty right → null result
- Non-empty left, empty right → null result

---

### 2.3 MultiMetricStrategyTests

**File**: `DataVisualiser.Tests/Strategies/MultiMetricStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldReturnNull_WhenNoSeries()
[Fact] public void Compute_ShouldReturnNull_WhenAllSeriesEmpty()
[Fact] public void Compute_ShouldCreateSeriesResult_ForEachInput()
[Fact] public void Compute_ShouldGenerateSmoothedData_ForEachSeries()
[Fact] public void Compute_ShouldHandleVariableSeriesCounts()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/MultiMetricStrategy.cs`
- `DataVisualiser/Charts/Computation/SeriesResult.cs`

**Test Data**:

- 3+ series with different counts
- Series with overlapping/non-overlapping timestamps
- Series with null values

---

### 2.4 DifferenceStrategyTests

**File**: `DataVisualiser.Tests/Strategies/DifferenceStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldCalculateDifference_Correctly()
[Fact] public void Compute_ShouldReturnNull_WhenInputsEmpty()
[Fact] public void Compute_ShouldHandleNegativeDifferences()
[Fact] public void Compute_ShouldAlignByTimestamp()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/DifferenceStrategy.cs`

---

### 2.5 RatioStrategyTests

**File**: `DataVisualiser.Tests/Strategies/RatioStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldCalculateRatio_Correctly()
[Fact] public void Compute_ShouldHandleDivisionByZero()
[Fact] public void Compute_ShouldReturnNull_WhenInputsEmpty()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/RatioStrategy.cs`

---

### 2.6 NormalizedStrategyTests

**File**: `DataVisualiser.Tests/Strategies/NormalizedStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldNormalizeValues_Correctly()
[Fact] public void Compute_ShouldReturnNull_WhenInputsEmpty()
[Fact] public void Compute_ShouldHandleZeroBaseline()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/NormalizedStrategy.cs`

---

### 2.7 WeeklyDistributionStrategyTests

**File**: `DataVisualiser.Tests/Strategies/WeeklyDistributionStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldGroupByDayOfWeek()
[Fact] public void Compute_ShouldCalculateMinMax_ForEachDay()
[Fact] public void Compute_ShouldReturnNull_WhenDataEmpty()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/WeeklyDistributionStrategy.cs`
- `DataVisualiser/Models/WeeklyDistributionResult.cs`

---

### 2.8 WeekdayTrendStrategyTests

**File**: `DataVisualiser.Tests/Strategies/WeekdayTrendStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldGroupByWeekday()
[Fact] public void Compute_ShouldCalculateTrends_Correctly()
[Fact] public void Compute_ShouldReturnNull_WhenDataEmpty()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/WeekdayTrendStrategy.cs`
- `DataVisualiser/Models/WeekdayTrendResult.cs`

---

## Module 3: Service Tests

### 3.1 MetricSelectionServiceTests

**File**: `DataVisualiser.Tests/Services/MetricSelectionServiceTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public async Task LoadMetricDataAsync_ShouldReturnData_WhenValid()
[Fact] public async Task LoadMetricDataAsync_ShouldReturnEmpty_WhenNoData()
[Fact] public async Task LoadMetricDataWithCmsAsync_ShouldReturnBothLegacyAndCms()
[Fact] public async Task LoadMetricDataWithCmsAsync_ShouldReturnNullCms_WhenNotAvailable()
```

**Key Files to Reference**:

- `DataVisualiser/Services/MetricSelectionService.cs`
- `DataVisualiser/Data/Repositories/DataFetcher.cs`
- `DataVisualiser/Data/Repositories/CmsDataService.cs`

**Mock Requirements**:

- Mock `DataFetcher` (SQL dependency)
- Mock `CmsDataService` (CMS dependency)
- Use `Moq` to create mocks

**Example Mock Setup**:

```csharp
var mockDataFetcher = new Mock<DataFetcher>(connectionString);
mockDataFetcher.Setup(x => x.GetHealthMetricsDataByBaseType(...))
    .ReturnsAsync(testData);
```

---

### 3.2 ChartUpdateCoordinatorTests

**File**: `DataVisualiser.Tests/Services/ChartUpdateCoordinatorTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void UpdateChart_ShouldCallComputationEngine()
[Fact] public void UpdateChart_ShouldCallRenderEngine()
[Fact] public void UpdateChart_ShouldHandleNullResult()
```

**Key Files to Reference**:

- `DataVisualiser/Services/ChartUpdateCoordinator.cs`
- `DataVisualiser/Charts/Computation/ChartComputationEngine.cs`
- `DataVisualiser/Charts/Rendering/ChartRenderEngine.cs`

**Mock Requirements**:

- Mock `ChartComputationEngine`
- Mock `ChartRenderEngine`
- Mock `ChartTooltipManager`

---

### 3.3 WeeklyDistributionServiceTests

**File**: `DataVisualiser.Tests/Services/WeeklyDistributionServiceTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void UpdateWeeklyDistribution_ShouldCreateColumns_ForEachDay()
[Fact] public void UpdateWeeklyDistribution_ShouldCalculateMinMax_Correctly()
[Fact] public void UpdateWeeklyDistribution_ShouldHandleEmptyData()
```

**Key Files to Reference**:

- `DataVisualiser/Services/WeeklyDistributionService.cs`
- `DataVisualiser/Models/WeeklyDistributionResult.cs`

---

### 3.4 ChartDataContextBuilderTests

**File**: `DataVisualiser.Tests/Services/ChartDataContextBuilderTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Build_ShouldCreateContext_FromLegacyData()
[Fact] public void Build_ShouldCreateContext_FromCmsData()
[Fact] public void Build_ShouldHandleNullInputs()
```

**Key Files to Reference**:

- `DataVisualiser/Services/ChartDataContextBuilder.cs`
- `DataVisualiser/Charts/ChartDataContext.cs`

---

## Module 4: Repository Tests

### 4.1 DataFetcherTests

**File**: `DataVisualiser.Tests/Repositories/DataFetcherTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public async Task GetHealthMetricsDataByBaseType_ShouldReturnData()
[Fact] public async Task GetHealthMetricsDataByBaseType_ShouldFilterBySubtype()
[Fact] public async Task GetHealthMetricsDataByBaseType_ShouldFilterByDateRange()
[Fact] public void Constructor_ShouldThrow_WhenConnectionStringNull()
```

**Key Files to Reference**:

- `DataVisualiser/Data/Repositories/DataFetcher.cs`
- `DataVisualiser/Data/SqlQueryBuilder.cs`

**Mock Requirements**:

- Mock SQL connection (use in-memory database or mock `SqlConnection`)
- Consider using `Microsoft.Data.Sqlite` for in-memory testing

**Note**: SQL mocking is complex. Consider integration tests instead of unit tests for repositories.

---

### 4.2 CmsDataServiceTests

**File**: `DataVisualiser.Tests/Repositories/CmsDataServiceTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public async Task GetCmsByCanonicalIdAsync_ShouldReturnCms()
[Fact] public async Task GetCmsByCanonicalIdAsync_ShouldReturnEmpty_WhenNotAvailable()
[Fact] public async Task IsCmsAvailableAsync_ShouldReturnTrue_WhenAvailable()
```

**Key Files to Reference**:

- `DataVisualiser/Data/Repositories/CmsDataService.cs`
- `DataFileReader/Canonical/HealthMetricToCmsMapper.cs`

**Mock Requirements**:

- Mock `DataFetcher`
- Mock `HealthMetricToCmsMapper`

---

## Module 5: ViewModel Tests

### 5.1 MainWindowViewModelTests

**File**: `DataVisualiser.Tests/ViewModels/MainWindowViewModelTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public async Task LoadAndValidateMetricDataAsync_ShouldLoadData()
[Fact] public async Task LoadAndValidateMetricDataAsync_ShouldRaiseError_WhenDataEmpty()
[Fact] public void SetNormalizedVisible_ShouldUpdateState()
[Fact] public void RequestChartUpdate_ShouldRaiseEvent()
```

**Key Files to Reference**:

- `DataVisualiser/ViewModels/MainWindowViewModel.cs`
- `DataVisualiser/State/ChartState.cs`
- `DataVisualiser/State/MetricState.cs`

**Mock Requirements**:

- Mock `MetricSelectionService`
- Mock `ChartUpdateCoordinator`
- Mock `WeeklyDistributionService`

---

## Module 6: Transform Tests (Phase 4)

### 6.1 TransformExpressionEvaluatorTests

**File**: `DataVisualiser.Tests/Transforms/TransformExpressionEvaluatorTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Evaluate_ShouldThrow_WhenExpressionNull()
[Fact] public void Evaluate_ShouldThrow_WhenMetricsEmpty()
[Fact] public void Evaluate_ShouldThrow_WhenMetricsNotAligned()
[Fact] public void Evaluate_ShouldEvaluateUnaryOperation_Correctly()
[Fact] public void Evaluate_ShouldEvaluateBinaryOperation_Correctly()
[Fact] public void Evaluate_ShouldHandleNullValues()
[Fact] public void Evaluate_ShouldHandleChainedOperations()
[Fact] public void GenerateLabel_ShouldReturnMetricLabel_ForLeafNode()
[Fact] public void GenerateLabel_ShouldReturnOperationLabel_ForOperationNode()
[Fact] public void GenerateLabel_ShouldHandleChainedExpressions()
[Fact] public void GenerateTransformLabel_ShouldUseNewInfrastructure_WhenExpressionValid()
[Fact] public void GenerateTransformLabel_ShouldUseLegacyFallback_WhenExpressionNull()
[Fact] public void BuildMetricLabelsFromContext_ShouldBuildFromContext()
[Fact] public void BuildMetricLabelsFromContext_ShouldUseFallback_WhenContextNull()
[Fact] public void AlignMetricsByTimestamp_ShouldAlignByTimestamp()
[Fact] public void AlignMetricsByTimestamp_ShouldReturnEmpty_WhenNoMatchingTimestamps()
```

**Key Files to Reference**:

- `DataVisualiser/Helper/TransformExpressionEvaluator.cs`
- `DataVisualiser/Models/TransformExpression.cs`
- `DataVisualiser/Charts/ChartDataContext.cs`

**Test Data**:

- Use `TestDataBuilders.HealthMetricData().BuildSeries()` for aligned metrics
- Test with unary operations (Log, Sqrt)
- Test with binary operations (Add, Subtract)
- Test with chained operations (future expansion)

---

### 6.2 TransformExpressionBuilderTests

**File**: `DataVisualiser.Tests/Transforms/TransformExpressionBuilderTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void BuildFromOperation_ShouldReturnNull_WhenOperationNotFound()
[Fact] public void BuildFromOperation_ShouldReturnNull_WhenArityMismatch()
[Fact] public void BuildFromOperation_ShouldBuildUnaryExpression()
[Fact] public void BuildFromOperation_ShouldBuildBinaryExpression()
[Fact] public void BuildFromOperation_ShouldBuildNaryExpression()
[Fact] public void BuildChained_ShouldBuildChainedExpression()
[Fact] public void BuildNary_ShouldBuildNaryExpression()
```

**Key Files to Reference**:

- `DataVisualiser/Helper/TransformExpressionBuilder.cs`
- `DataVisualiser/Models/TransformOperationRegistry.cs`
- `DataVisualiser/Models/TransformExpression.cs`

**Test Scenarios**:

- Valid operations: "Log", "Sqrt", "Add", "Subtract"
- Invalid operations: "InvalidOp", null, empty string
- Arity validation: unary needs 1 metric, binary needs 2 metrics

---

### 6.3 TransformDataHelperTests

**File**: `DataVisualiser.Tests/Transforms/TransformDataHelperTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void CreateTransformResultData_ShouldCreateData_WithValidInputs()
[Fact] public void CreateTransformResultData_ShouldHandleNaN_Values()
[Fact] public void CreateTransformResultData_ShouldFormatTimestamps_Correctly()
[Fact] public void CreateTransformResultData_ShouldFormatValues_WithFourDecimals()
```

**Key Files to Reference**:

- `DataVisualiser/Helper/TransformDataHelper.cs`
- `DataVisualiser/Models/HealthMetricData.cs`

**Test Data**:

- Use `TestDataBuilders.HealthMetricData()` for input data
- Test with NaN values in results
- Test with various timestamp formats

---

### 6.4 TransformOperationRegistryTests

**File**: `DataVisualiser.Tests/Transforms/TransformOperationRegistryTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void GetOperation_ShouldReturnOperation_WhenRegistered()
[Fact] public void GetOperation_ShouldReturnNull_WhenNotRegistered()
[Fact] public void Register_ShouldRegisterOperation()
[Fact] public void Register_ShouldThrow_WhenOperationNull()
[Fact] public void Register_ShouldThrow_WhenIdEmpty()
[Fact] public void GetAllOperations_ShouldReturnAllRegistered()
[Fact] public void GetUnaryOperations_ShouldReturnOnlyUnary()
[Fact] public void GetBinaryOperations_ShouldReturnOnlyBinary()
[Fact] public void GetNaryOperations_ShouldReturnOnlyNary()
```

**Key Files to Reference**:

- `DataVisualiser/Models/TransformOperationRegistry.cs`
- `DataVisualiser/Models/TransformOperation.cs`
- `DataVisualiser/Models/UnaryOperators.cs`
- `DataVisualiser/Models/BinaryOperators.cs`

**Test Scenarios**:

- Default registered operations: "Log", "Sqrt", "Add", "Subtract"
- Custom operation registration
- Operation retrieval by ID

---

### 6.5 TransformResultStrategyTests

**File**: `DataVisualiser.Tests/Strategies/TransformResultStrategyTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void Compute_ShouldReturnNull_WhenDataEmpty()
[Fact] public void Compute_ShouldReturnNull_WhenResultsEmpty()
[Fact] public void Compute_ShouldGenerateSmoothedData()
[Fact] public void Compute_ShouldSetUnit_FromData()
[Fact] public void Compute_ShouldFilterByDateRange()
[Fact] public void Compute_ShouldHandleNaN_Values()
[Fact] public void Compute_ShouldCreateNormalizedIntervals()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Strategies/TransformResultStrategy.cs`
- `DataVisualiser/Charts/Computation/ChartComputationResult.cs`
- `DataVisualiser/Helper/MathHelper.cs`

**Test Data**:

- Use `TestDataBuilders.HealthMetricData()` for input data
- Test with computed transform results
- Test with various date ranges

---

## Module 7: Helper Tests

### 7.1 ChartHelperTests

**File**: `DataVisualiser.Tests/Helpers/ChartHelperTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void ClearChart_ShouldClearSeries()
[Fact] public void ClearChart_ShouldClearTimestamps()
```

**Key Files to Reference**:

- `DataVisualiser/Charts/Helpers/ChartHelper.cs`

---

### 7.2 MathHelperTests

**File**: `DataVisualiser.Tests/Helpers/MathHelperTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void DetermineTickInterval_ShouldReturnCorrectInterval()
[Fact] public void CreateSmoothedData_ShouldSmoothCorrectly()
[Fact] public void InterpolateSmoothedData_ShouldInterpolateCorrectly()
[Fact] public void ApplyUnaryOperation_ShouldApplyOperation_Correctly()
[Fact] public void ApplyBinaryOperation_ShouldApplyOperation_Correctly()
[Fact] public void CalculateOptimalMaxRecords_ShouldCalculate_Correctly()
```

**Key Files to Reference**:

- `DataVisualiser/Helper/MathHelper.cs`

---

### 7.3 CmsConversionHelperTests

**File**: `DataVisualiser.Tests/Helpers/CmsConversionHelperTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void ConvertSamplesToHealthMetricData_ShouldConvertCorrectly()
[Fact] public void ConvertSamplesToHealthMetricData_ShouldFilterByDateRange()
```

**Key Files to Reference**:

- `DataVisualiser/Helper/CmsConversionHelper.cs`

---

## Module 8: State Tests

### 8.1 ChartStateTests

**File**: `DataVisualiser.Tests/State/ChartStateTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void SetNormalizedVisible_ShouldUpdateProperty()
[Fact] public void ChartTimestamps_ShouldInitializeEmpty()
```

**Key Files to Reference**:

- `DataVisualiser/State/ChartState.cs`

---

### 8.2 MetricStateTests

**File**: `DataVisualiser.Tests/State/MetricStateTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void SetDateRange_ShouldUpdateProperties()
[Fact] public void SetMetricType_ShouldUpdateProperty()
```

**Key Files to Reference**:

- `DataVisualiser/State/MetricState.cs`

---

### 8.3 CmsConfigurationTests

**File**: `DataVisualiser.Tests/State/CmsConfigurationTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void ShouldUseCms_ShouldReturnFalse_WhenGlobalDisabled()
[Fact] public void ShouldUseCms_ShouldReturnTrue_WhenEnabled()
```

**Key Files to Reference**:

- `DataVisualiser/State/CmsConfiguration.cs`

---

## Test Infrastructure (Already Created)

### TestDataBuilders ✅

**File**: `DataVisualiser.Tests/Helpers/TestDataBuilders.cs`  
**Status**: ✅ Complete

**Usage**:

```csharp
var legacyData = TestDataBuilders.HealthMetricData()
    .WithTimestamp(DateTime.UtcNow)
    .WithValue(100m)
    .WithUnit("kg")
    .BuildSeries(10, TimeSpan.FromDays(1));

var cmsData = TestDataBuilders.CanonicalMetricSeries()
    .WithStartTime(DateTimeOffset.UtcNow)
    .WithInterval(TimeSpan.FromDays(1))
    .WithValue(100m)
    .WithUnit("kg")
    .WithSampleCount(10)
    .Build();
```

---

### TestHelpers ✅

**File**: `DataVisualiser.Tests/Helpers/TestHelpers.cs`  
**Status**: ✅ Complete

**Usage**:

```csharp
var legacyResult = strategy.Compute()?.ToLegacyExecutionResult();
var cmsResult = strategy.Compute()?.ToCmsExecutionResult();
```

---

## Test Generation Instructions for ChatGPT

1. **Read the specification above** for the module you're generating tests for
2. **Read the actual source code files** referenced in "Key Files to Reference"
3. **Use existing test infrastructure**:
   - `TestDataBuilders` for creating test data
   - `TestHelpers` for result conversion
   - xUnit `[Fact]` attributes
   - Moq for mocking dependencies
4. **Follow the test pattern**:
   ```csharp
   [Fact]
   public void TestName_ShouldBehavior_WhenCondition()
   {
       // Arrange
       // Act
       // Assert
   }
   ```
5. **Assert comprehensively**:
   - Check null/empty conditions
   - Verify data transformations
   - Validate edge cases
   - Test error conditions
6. **Use meaningful test names** that describe behavior
7. **Keep tests isolated** - each test should be independent
8. **Mock external dependencies** (SQL, services, etc.)

---

## Priority Order

1. **Parity Tests** (Phase 4A requirement) - ✅ 2/3 complete
2. **Transform Tests** (Phase 4 - New infrastructure) - ❌ 0/4 complete
3. **Strategy Tests** (Core functionality) - Includes TransformResultStrategy
4. **Service Tests** (Integration points)
5. **Repository Tests** (Data access - consider integration tests)
6. **ViewModel Tests** (UI logic)
7. **Helper Tests** (Utility functions)
8. **State Tests** (State management)

---

## Notes

- **SQL Testing**: Consider using in-memory SQLite for repository tests instead of mocking SQL
- **WPF Dependencies**: Some components depend on WPF. Use `[Fact(Skip = "Requires WPF")]` or test logic separately
- **Async Testing**: Use `async Task` for async methods, await in tests
- **Test Coverage Goal**: Aim for 70-80% coverage of business logic (exclude UI code)

---

**Last Updated**: 2025-01-XX  
**Status**:

- Parity tests: 2/3 complete (CombinedMetric, SingleMetric done; MultiMetric pending)
- Transform tests (Phase 4): 0/4 complete - NEW infrastructure added
- Strategy tests: 0/9 complete (includes new TransformResultStrategy)
- Remaining tests can be generated incrementally using this specification.

**Phase 4 Updates**:

- Added Transform module with 4 new test files for transform infrastructure
- Added TransformResultStrategy to Strategy tests
- Transform infrastructure is provisioned for future expansion (N-metrics, chained operations)
