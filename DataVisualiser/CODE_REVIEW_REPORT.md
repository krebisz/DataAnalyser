# DataVisualiser Code Review Report

## Executive Summary

This report identifies code issues, logic gaps, and quality improvements across the DataVisualiser project. The analysis covers error handling, code quality, architecture, performance, and maintainability concerns.

---

## 1. Critical Issues

### 1.1 Silent Exception Swallowing

**Location:** `ChartComputationEngine.cs:17-20`

```csharp
catch
{
    // swallow here and return null so callers can clear charts safely
    return null;
}
```

**Issue:** Exceptions are silently swallowed without logging, making debugging difficult.
**Impact:** High - Production issues will be hard to diagnose.
**Recommendation:** Log exceptions before returning null, or rethrow with context.

### 1.2 Null Return Pattern with Null-Forgiving Operator

**Location:** Multiple strategy files (e.g., `SingleMetricStrategy.cs:28`)

```csharp
if (!orderedData.Any()) return null!; // engine will treat null as no-data
```

**Issue:** Using `null!` suppresses null warnings but doesn't match the return type (`ChartComputationResult?`).
**Impact:** Medium - Potential null reference exceptions if not handled correctly.
**Recommendation:** Return `null` without the null-forgiving operator, or use a Result pattern.

### 1.3 Inconsistent Null Handling

**Location:** `ChartHelper.cs:116, 202`

```csharp
var val = Convert.ToDouble(raw);
```

**Issue:** `raw` could be null, causing runtime exceptions.
**Impact:** Medium - Potential crashes on invalid data.
**Recommendation:** Add null checks before conversion.

---

## 2. Code Quality Issues

### 2.1 Commented-Out Code

**Location:** `MainWindow.xaml.cs:75-86`

```csharp
//_uiState.IsLoadingMetricTypes = false;
_viewModel.SetLoadingMetricTypes(false);
```

**Issue:** Commented code indicates incomplete refactoring.
**Impact:** Low - Code clutter and confusion.
**Recommendation:** Remove all commented-out code.

### 2.2 Debug Statements in Production Code

**Location:** Multiple files

- `MainWindow.xaml.cs:287, 346, 728, 765`
- `ChartUpdateCoordinator.cs:101`
- `WeeklyDistributionService.cs:173`

**Issue:** `System.Diagnostics.Debug.WriteLine` statements left in production code.
**Impact:** Low - Performance impact and code clutter.
**Recommendation:** Use proper logging framework (e.g., ILogger) or remove debug statements.

### 2.3 Magic Numbers and Strings

**Location:** Multiple files

- `ChartHelper.cs:581` - `targetTicks = 10.0`
- `ChartHelper.cs:680` - `tickSpacingPx = 30.0`
- `ChartHelper.cs:687` - `maxHeight = 2000.0`
- `WeeklyDistributionService.cs:141-142` - Division by 5.0

**Issue:** Magic numbers make code harder to maintain and understand.
**Impact:** Medium - Maintenance difficulty.
**Recommendation:** Extract to named constants or configuration.

### 2.4 Inconsistent State Management

**Location:** `MainWindow.xaml.cs` throughout
**Issue:** Mix of direct property access (`_viewModel.ChartState.IsNormalizedVisible`) and method calls (`_viewModel.SetNormalizedVisible(false)`).
**Impact:** Medium - Inconsistent patterns make code harder to follow.
**Recommendation:** Standardize on one approach (prefer methods for encapsulation).

### 2.5 Typo in Variable Name

**Location:** `NormalizedStrategy.cs:76`

```csharp
List<double> rawResults12 = null!;
```

**Issue:** Variable name `rawResults12` should be `rawResults2` for consistency.
**Impact:** Low - Code readability.
**Recommendation:** Rename variable.

---

## 3. Logic Gaps

### 3.1 Missing Input Validation

**Location:** `ChartHelper.cs:286-345` - `UpdateVerticalLineForChart`
**Issue:** Method doesn't validate `index` parameter (could be negative or out of bounds).
**Impact:** Medium - Potential index out of range exceptions.
**Recommendation:** Add bounds checking.

### 3.2 Potential Race Condition

**Location:** `MainWindow.xaml.cs:146-191` - `LoadMetricTypes`
**Issue:** Async method without cancellation token support. Multiple rapid selections could cause race conditions.
**Impact:** Medium - UI state inconsistencies.
**Recommendation:** Add `CancellationToken` support and check loading state before operations.

### 3.3 Missing Disposal Pattern

**Location:** `ChartTooltipManager.cs`
**Issue:** Implements `IDisposable` but `MainWindow` doesn't dispose it.
**Impact:** Medium - Potential memory leaks from event handlers.
**Recommendation:** Ensure disposal in `MainWindow` cleanup.

### 3.4 Incomplete ViewModel Implementation

**Location:** `MainWindowViewModel.cs:62-73`
**Issue:** Command methods (`LoadMetrics`, `LoadSubtypes`, `LoadData`) are empty stubs.
**Impact:** Medium - ViewModel pattern is incomplete.
**Recommendation:** Either implement commands or remove unused ViewModel pattern.

### 3.5 Missing Error Recovery

**Location:** `DataFetcher.cs` - Multiple async methods
**Issue:** Database connection failures don't have retry logic or graceful degradation.
**Impact:** Medium - Poor user experience on transient failures.
**Recommendation:** Add retry logic with exponential backoff for transient errors.

---

## 4. Architecture Concerns

### 4.1 Mixed Responsibilities in MainWindow

**Location:** `MainWindow.xaml.cs`
**Issue:** MainWindow handles UI, data loading, chart updates, and business logic.
**Impact:** High - Violates Single Responsibility Principle, hard to test.
**Recommendation:** Move data loading logic to services, chart updates to coordinator.

### 4.2 Direct Database Access in UI Layer

**Location:** `MainWindow.xaml.cs:154, 256, 334, 401`

```csharp
var dataFetcher = new DataFetcher(_connectionString);
```

**Issue:** Creating `DataFetcher` instances directly in UI code.
**Impact:** Medium - Tight coupling, harder to test.
**Recommendation:** Inject `DataFetcher` through constructor or use service layer.

### 4.3 Inconsistent Service Usage

**Location:** `MainWindow.xaml.cs`
**Issue:** Sometimes uses `MetricSelectionService`, sometimes creates `DataFetcher` directly.
**Impact:** Medium - Inconsistent patterns.
**Recommendation:** Always use service layer for data access.

### 4.4 Missing Dependency Injection

**Location:** Throughout project
**Issue:** Manual dependency creation instead of DI container.
**Impact:** Medium - Harder to test and maintain.
**Recommendation:** Consider adding DI container (e.g., Microsoft.Extensions.DependencyInjection).

---

## 5. Performance Issues

### 5.1 Inefficient LINQ Queries

**Location:** `CombinedMetricStrategy.cs:39-43`

```csharp
var combinedTimestamps = ordered1.Select(d => d.NormalizedTimestamp)
    .Concat(ordered2.Select(d => d.NormalizedTimestamp))
    .Distinct()
    .OrderBy(dt => dt)
    .ToList();
```

**Issue:** Multiple enumerations and unnecessary `Distinct()` after `Concat`.
**Impact:** Low - Performance impact on large datasets.
**Recommendation:** Use `Union` instead of `Concat().Distinct()`.

### 5.2 Potential Memory Leaks

**Location:** `ChartTooltipManager.cs` - Event handlers
**Issue:** Event handlers attached to charts may not be properly cleaned up.
**Impact:** Medium - Memory leaks over time.
**Recommendation:** Ensure all event handlers are detached in `Dispose()`.

### 5.3 Missing Async Optimization

**Location:** `MainWindow.xaml.cs:406`

```csharp
await Task.WhenAll(dataTask1, dataTask2);
var data1 = dataTask1.Result;
var data2 = dataTask2.Result;
```

**Issue:** Using `.Result` after `WhenAll` is redundant.
**Impact:** Low - Minor performance issue.
**Recommendation:** Use `await` directly on tasks.

### 5.4 Repeated Dictionary Lookups

**Location:** `CombinedMetricStrategy.cs:58-59`

```csharp
var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).ToList();
```

**Issue:** Pattern is fine, but could be optimized with pre-allocation.
**Impact:** Low - Minor optimization opportunity.
**Recommendation:** Pre-allocate list capacity.

---

## 6. Security Concerns

### 6.1 SQL Injection Risk (Mitigated)

**Location:** `DataFetcher.cs` - All SQL queries
**Issue:** Using string interpolation for table names (line 151, 188, 255).

```csharp
FROM [dbo].[{tableName}]
```

**Impact:** Low - Table names are controlled, but not parameterized.
**Recommendation:** Validate table names against whitelist or use parameterized queries if possible.

### 6.2 Connection String Handling

**Location:** `MainWindow.xaml.cs:40`
**Issue:** Connection string from config with fallback hardcoded.
**Impact:** Low - Security risk if connection string is exposed.
**Recommendation:** Use secure storage for connection strings in production.

---

## 7. Maintainability Issues

### 7.1 Large Methods

**Location:** `ChartHelper.cs:501-642` - `NormalizeYAxis` (141 lines)
**Issue:** Method is too long and does multiple things.
**Impact:** Medium - Hard to test and maintain.
**Recommendation:** Break into smaller, focused methods.

### 7.2 Duplicate Code

**Location:** Multiple strategy classes
**Issue:** Similar timestamp combination logic repeated in `CombinedMetricStrategy` and `NormalizedStrategy`.
**Impact:** Low - Code duplication.
**Recommendation:** Extract common logic to helper method.

### 7.3 Missing XML Documentation

**Location:** Many public methods
**Issue:** Some methods lack XML documentation comments.
**Impact:** Low - Reduced IntelliSense support.
**Recommendation:** Add XML documentation to all public APIs.

### 7.4 Inconsistent Naming

**Location:** Throughout codebase
**Issue:** Mix of naming conventions (e.g., `_left` vs `data1`, `_labelLeft` vs `displayName1`).
**Impact:** Low - Code readability.
**Recommendation:** Establish and follow consistent naming conventions.

---

## 8. Specific Code Issues

### 8.1 Unused Variable

**Location:** `WeeklyDistributionService.cs:59`

```csharp
var dayLabels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
```

**Issue:** Variable declared but never used.
**Impact:** Low - Code clutter.
**Recommendation:** Remove if unused, or use if needed.

### 8.2 Commented-Out Code Block

**Location:** `WeeklyDistributionService.cs:111-129`
**Issue:** Large block of commented code for axis configuration.
**Impact:** Low - Code clutter.
**Recommendation:** Remove or implement if needed.

### 8.3 Inconsistent Error Messages

**Location:** Various error handling locations
**Issue:** Error messages vary in detail and format.
**Impact:** Low - User experience inconsistency.
**Recommendation:** Standardize error message format.

### 8.4 Missing Null Checks

**Location:** `ChartHelper.cs:433`

```csharp
if (chart != null && chart.AxisX.Count > 0)
```

**Issue:** Checks `chart` but not `chart.AxisX` before accessing `Count`.
**Impact:** Medium - Potential null reference exception.
**Recommendation:** Add null check for `chart.AxisX`.

---

## 9. Recommendations Priority

### High Priority

1. Fix silent exception swallowing in `ChartComputationEngine`
2. Add proper error logging throughout
3. Implement disposal pattern for `ChartTooltipManager`
4. Add input validation to public methods
5. Fix null handling issues

### Medium Priority

1. Remove commented-out code
2. Extract magic numbers to constants
3. Standardize state management approach
4. Add cancellation token support to async methods
5. Refactor large methods
6. Implement proper DI pattern

### Low Priority

1. Remove debug statements or use logging framework
2. Fix typos and naming inconsistencies
3. Add XML documentation
4. Optimize LINQ queries
5. Remove unused variables

---

## 10. Testing Gaps

### Missing Test Coverage

- No unit tests found in the project
- Critical business logic (normalization, smoothing) not tested
- Chart rendering logic not tested
- Data fetching logic not tested

**Recommendation:** Add unit tests for:

- `MathHelper` methods
- Strategy classes
- `DataFetcher` methods
- State management classes

---

## Summary

The DataVisualiser project has a solid foundation but needs improvements in:

- **Error handling and logging** (critical)
- **Code quality and consistency** (high)
- **Architecture and separation of concerns** (medium)
- **Testing** (high priority for maintainability)

Most issues are fixable with refactoring and don't require architectural changes. The codebase would benefit from:

1. Proper logging framework
2. Dependency injection
3. Unit test coverage
4. Code cleanup (remove commented code, debug statements)
5. Consistent patterns throughout
