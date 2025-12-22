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
│   └── WeekdayTrendStrategyTests.cs
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

## Module 6: Helper Tests

### 6.1 ChartHelperTests

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

### 6.2 MathHelperTests

**File**: `DataVisualiser.Tests/Helpers/MathHelperTests.cs`  
**Status**: ❌ TODO

**Required Tests**:

```csharp
[Fact] public void DetermineTickInterval_ShouldReturnCorrectInterval()
[Fact] public void CreateSmoothedData_ShouldSmoothCorrectly()
[Fact] public void InterpolateSmoothedData_ShouldInterpolateCorrectly()
```

**Key Files to Reference**:

- `DataVisualiser/Helper/MathHelper.cs`

---

### 6.3 CmsConversionHelperTests

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

## Module 7: State Tests

### 7.1 ChartStateTests

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

### 7.2 MetricStateTests

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

### 7.3 CmsConfigurationTests

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
2. **Strategy Tests** (Core functionality)
3. **Service Tests** (Integration points)
4. **Repository Tests** (Data access - consider integration tests)
5. **ViewModel Tests** (UI logic)
6. **Helper Tests** (Utility functions)
7. **State Tests** (State management)

---

## Notes

- **SQL Testing**: Consider using in-memory SQLite for repository tests instead of mocking SQL
- **WPF Dependencies**: Some components depend on WPF. Use `[Fact(Skip = "Requires WPF")]` or test logic separately
- **Async Testing**: Use `async Task` for async methods, await in tests
- **Test Coverage Goal**: Aim for 70-80% coverage of business logic (exclude UI code)

---

**Last Updated**: 2025-01-XX  
**Status**: Minimal parity tests complete. Remaining tests can be generated incrementally using this specification.
