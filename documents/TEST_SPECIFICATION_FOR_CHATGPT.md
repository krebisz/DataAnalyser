Unit Test Specification for DataVisualiser

Purpose

Define and track required unit tests to validate correctness and parity between Legacy and CMS computation pipelines.

Test Framework

Framework: xUnit

Target: .NET 9.0-windows

Test Structure
DataVisualiser.Tests/
├── Parity/
├── Strategies/
├── Transforms/
├── Helpers/
├── Services/
├── Repositories/
├── ViewModels/
└── State/

Helpers (FOUNDATION)
Helpers/
├── TestDataBuilders.cs                  DONE
├── TestHelpers.cs                       DONE
├── MathHelperTests.cs                   DONE
├── StrategyComputationHelperTests.cs    DONE
├── ChartHelperTests.cs                  TODO
├── CmsConversionHelperTests.cs          TODO

Parity Tests (Phase 4A)
Parity/
├── SingleMetricParityTests.cs           DONE
├── CombinedMetricParityTests.cs         DONE
├── MultiMetricParityTests.cs            DONE

Strategy Tests
Strategies/
├── SingleMetricStrategyTests.cs         DONE
├── CombinedMetricStrategyTests.cs       DONE
├── MultiMetricStrategyTests.cs          DONE
├── DifferenceStrategyTests.cs           DONE
├── RatioStrategyTests.cs                DONE
├── NormalizedStrategyTests.cs           DONE
├── WeeklyDistributionStrategyTests.cs   TODO (conditional)
├── WeekdayTrendStrategyTests.cs         TODO (conditional)
├── TransformResultStrategyTests.cs      DONE (Phase 4)

Transform Tests (Phase 4)
Transforms/
├── TransformExpressionEvaluatorTests.cs     DONE
├── TransformExpressionBuilderTests.cs       DONE
├── TransformOperationRegistryTests.cs       DONE
├── TransformDataHelperTests.cs              DONE

Phase 4 Parity Test Groups (Execution Order)
Group 1 — Transform Pipeline Parity
Transforms/
├── TransformExpressionEvaluatorTests.cs     DONE
├── TransformExpressionBuilderTests.cs       DONE
├── TransformOperationRegistryTests.cs       DONE
└── TransformDataHelperTests.cs              DONE


Required references

TransformExpressionEvaluator.cs
TransformExpressionBuilder.cs
TransformOperationRegistry.cs
TransformDataHelper.cs
TransformExpression.cs

Group 2 — TransformResultStrategy Parity
Strategies/
└── TransformResultStrategyTests.cs          DONE


Required references

TransformResultStrategy.cs
ChartComputationResult.cs
IChartComputationStrategy.cs

Group 3 — Weekly / Temporal Strategies (Conditional)
Strategies/
├── WeeklyDistributionStrategyTests.cs       TODO
└── WeekdayTrendStrategyTests.cs             TODO


Required references

WeeklyDistributionStrategy.cs
WeekdayTrendStrategy.cs

Group 4 — End-to-End Parity (Optional)
Parity/
└── EndToEndTransformParityTests.cs          TODO


Required references

ChartDataContextBuilder.cs
Transforms/*
Strategies/*

Services
Services/
├── MetricSelectionServiceTests.cs       TODO
├── ChartUpdateCoordinatorTests.cs       TODO
├── WeeklyDistributionServiceTests.cs   TODO
├── ChartDataContextBuilderTests.cs      TODO

Repositories
Repositories/
├── DataFetcherTests.cs                  TODO
├── CmsDataServiceTests.cs               TODO

ViewModels
ViewModels/
└── MainWindowViewModelTests.cs          TODO

State
State/
├── ChartStateTests.cs                   TODO
├── MetricStateTests.cs                  TODO
├── CmsConfigurationTests.cs             TODO

Phase 4 Status

Core computation parity: COMPLETE

Helper parity: COMPLETE

Strategy parity: COMPLETE

Transform parity: COMPLETE

TransformResultStrategy parity: COMPLETE

Weekly / temporal parity: PENDING (migration required)

Status: Phase 4 blocked only on migration + testing of weekly / temporal strategies
Next step: Migrate WeeklyDistributionStrategy and WeekdayTrendStrategy into CMS, then implement tests