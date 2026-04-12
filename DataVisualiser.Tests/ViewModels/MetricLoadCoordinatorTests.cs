using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Validation.DataLoad;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Kernel;

namespace DataVisualiser.Tests.ViewModels;

public sealed class MetricLoadCoordinatorTests
{
    [Fact]
    public async Task LoadSubtypesAsync_ShouldExposeLoadingFlagAsFalseInsideLoadedCallback()
    {
        var chartState = new ChartState();
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics"
        };
        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message);

        bool? loadingFlagInsideCallback = null;
        IReadOnlyList<MetricNameOption>? loadedSubtypes = null;

        await coordinator.LoadSubtypesAsync(
            args =>
            {
                loadingFlagInsideCallback = uiState.IsLoadingSubtypes;
                loadedSubtypes = args.Subtypes.ToList();
            },
            message => throw new Xunit.Sdk.XunitException(message));

        Assert.False(loadingFlagInsideCallback);
        Assert.False(uiState.IsLoadingSubtypes);
        Assert.NotNull(loadedSubtypes);
        Assert.Equal(2, loadedSubtypes!.Count);
    }

    [Fact]
    public async Task LoadMetricDataAsync_ShouldUseCapturedRequestInsteadOfAmbientMutableState()
    {
        var chartState = new ChartState();
        var metricState = new MetricState
        {
            SelectedMetricType = "(All)",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections([new MetricSeriesSelection("(All)", "(All)")]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message);

        var capturedRequest = new MetricLoadRequest(
            "(All)",
            [new MetricSeriesSelection("(All)", "(All)")],
            new DateTime(2024, 01, 01),
            new DateTime(2024, 01, 02),
            "HealthMetrics");

        metricState.SelectedMetricType = "Weight";
        metricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "morning")]);
        metricState.FromDate = new DateTime(2025, 01, 01);
        metricState.ToDate = new DateTime(2025, 01, 02);

        var loaded = await coordinator.LoadMetricDataAsync(
            capturedRequest,
            args => throw new Xunit.Sdk.XunitException(args.Message));

        Assert.True(loaded);
        Assert.NotNull(chartState.LastContext);
        Assert.Equal(capturedRequest.Signature, chartState.LastContext!.LoadRequestSignature);
        Assert.Equal("(All)", chartState.LastContext.MetricType);
        Assert.Equal(new DateTime(2024, 01, 01), chartState.LastContext.From);
        Assert.Equal(new DateTime(2024, 01, 02), chartState.LastContext.To);
    }

    [Fact]
    public async Task LoadMetricDataAsync_ShouldUseVNextForMainOnlyVisibility()
    {
        var chartState = new ChartState { IsMainVisible = true };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "morning"),
            new MetricSeriesSelection("Weight", "evening")
        ]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(CreateStubSessionCoordinator);
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message,
            vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
        var loaded = await coordinator.LoadMetricDataAsync(request, args => throw new Xunit.Sdk.XunitException(args.Message));

        Assert.True(loaded);
        Assert.NotNull(chartState.LastContext);
        Assert.NotNull(chartState.LastLoadRuntime);
        Assert.Equal(EvidenceRuntimePath.VNextMain, chartState.LastLoadRuntime!.RuntimePath);
        Assert.True(chartState.LastLoadRuntime.SupportsOnlyMainChart);
        Assert.Equal(request.Signature, chartState.LastLoadRuntime.RequestSignature);
        Assert.Equal(request.Signature, chartState.LastContext!.LoadRequestSignature);
    }

    [Fact]
    public async Task LoadMetricDataAsync_ShouldUseVNextForMainAndNormalizedVisibility()
    {
        var chartState = new ChartState
        {
            IsMainVisible = true,
            IsNormalizedVisible = true
        };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "morning"),
            new MetricSeriesSelection("Weight", "evening")
        ]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(CreateStubSessionCoordinator);
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message,
            vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
        var loaded = await coordinator.LoadMetricDataAsync(request, args => throw new Xunit.Sdk.XunitException(args.Message));

        Assert.True(loaded);
        Assert.NotNull(chartState.LastContext);
        Assert.NotNull(chartState.LastLoadRuntime);
        Assert.Equal(EvidenceRuntimePath.VNextMain, chartState.LastLoadRuntime!.RuntimePath);
        Assert.False(chartState.LastLoadRuntime.SupportsOnlyMainChart);
        Assert.Equal(request.Signature, chartState.LastLoadRuntime.RequestSignature);
        Assert.Equal(request.Signature, chartState.LastContext!.LoadRequestSignature);
    }

    [Fact]
    public async Task LoadMetricDataAsync_ShouldUseVNextForMainAndDiffRatioVisibility()
    {
        var chartState = new ChartState
        {
            IsMainVisible = true,
            IsDiffRatioVisible = true
        };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "morning"),
            new MetricSeriesSelection("Weight", "evening")
        ]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(CreateStubSessionCoordinator);
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message,
            vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
        var loaded = await coordinator.LoadMetricDataAsync(request, args => throw new Xunit.Sdk.XunitException(args.Message));

        Assert.True(loaded);
        Assert.NotNull(chartState.LastContext);
        Assert.NotNull(chartState.LastLoadRuntime);
        Assert.Equal(EvidenceRuntimePath.VNextMain, chartState.LastLoadRuntime!.RuntimePath);
        Assert.False(chartState.LastLoadRuntime.SupportsOnlyMainChart);
        Assert.Equal(request.Signature, chartState.LastLoadRuntime.RequestSignature);
        Assert.Equal(request.Signature, chartState.LastContext!.LoadRequestSignature);
    }

    [Fact]
    public async Task LoadMetricDataAsync_ShouldFallbackToLegacyWhenNonSlicedChartsVisible()
    {
        var chartState = new ChartState
        {
            IsMainVisible = true,
            IsDistributionVisible = true
        };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "morning")]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var factoryCalls = 0;
        var vnext = new VNextMainChartIntegrationCoordinator(() =>
        {
            factoryCalls++;
            return CreateStubSessionCoordinator();
        });
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message,
            vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
        var loaded = await coordinator.LoadMetricDataAsync(request, args => throw new Xunit.Sdk.XunitException(args.Message));

        Assert.True(loaded);
        Assert.Equal(0, factoryCalls);
        Assert.NotNull(chartState.LastLoadRuntime);
        Assert.Equal(EvidenceRuntimePath.Legacy, chartState.LastLoadRuntime!.RuntimePath);
        Assert.False(chartState.LastLoadRuntime.SupportsOnlyMainChart);
    }

    [Fact]
    public async Task LoadMetricDataAsync_ShouldFallbackToLegacyWhenVNextFails()
    {
        var chartState = new ChartState { IsMainVisible = true };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "morning")]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(() => throw new InvalidOperationException("boom"));
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState,
            metricState,
            uiState,
            service,
            validator,
            ex => ex.Message,
            vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
        var loaded = await coordinator.LoadMetricDataAsync(request, args => throw new Xunit.Sdk.XunitException(args.Message));

        Assert.True(loaded);
        Assert.NotNull(chartState.LastLoadRuntime);
        Assert.Equal(EvidenceRuntimePath.Legacy, chartState.LastLoadRuntime!.RuntimePath);
        Assert.Equal(request.Signature, chartState.LastLoadRuntime.RequestSignature);
    }

    [Fact]
    public async Task LoadMetricDataAsync_ShouldUseVNextForSingleSubtype()
    {
        var chartState = new ChartState { IsMainVisible = true };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "morning")]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(CreateStubSessionCoordinator);
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState, metricState, uiState, service, validator, ex => ex.Message, vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
        var loaded = await coordinator.LoadMetricDataAsync(request, args => throw new Xunit.Sdk.XunitException(args.Message));

        Assert.True(loaded);
        Assert.Equal(EvidenceRuntimePath.VNextMain, chartState.LastLoadRuntime!.RuntimePath);
        Assert.Equal(1, chartState.LastContext!.ActualSeriesCount);
        Assert.Empty(chartState.LastContext.Data2!);
    }

    [Fact]
    public async Task LoadMetricDataAsync_VNextThenLegacyOnVisibilityChange()
    {
        var chartState = new ChartState { IsMainVisible = true };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "morning"),
            new MetricSeriesSelection("Weight", "evening")
        ]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(CreateStubSessionCoordinator);
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState, metricState, uiState, service, validator, ex => ex.Message, vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);

        // First load: main-only → VNext
        await coordinator.LoadMetricDataAsync(request, _ => { });
        Assert.Equal(EvidenceRuntimePath.VNextMain, chartState.LastLoadRuntime!.RuntimePath);

        // User enables Distribution
        chartState.IsDistributionVisible = true;

        // Second load: should fall back to legacy
        await coordinator.LoadMetricDataAsync(request, _ => { });
        Assert.Equal(EvidenceRuntimePath.Legacy, chartState.LastLoadRuntime!.RuntimePath);
        Assert.False(chartState.LastLoadRuntime.SupportsOnlyMainChart);
    }

    [Fact]
    public async Task LoadMetricDataAsync_VNextFailure_ShouldNotSetSupportsOnlyMainChart()
    {
        var chartState = new ChartState { IsMainVisible = true };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "morning")]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(() => throw new InvalidOperationException("boom"));
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState, metricState, uiState, service, validator, ex => ex.Message, vnext);

        var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
        await coordinator.LoadMetricDataAsync(request, _ => { });

        // After VNext failure + legacy success: final state should be Legacy with SupportsOnlyMainChart=false
        Assert.Equal(EvidenceRuntimePath.Legacy, chartState.LastLoadRuntime!.RuntimePath);
        Assert.False(chartState.LastLoadRuntime.SupportsOnlyMainChart);
    }

    [Fact]
    public async Task LoadMetricDataAsync_VNextRoutingShouldIgnoreCmsConfiguration()
    {
        var chartState = new ChartState { IsMainVisible = true };
        var metricState = new MetricState
        {
            SelectedMetricType = "Weight",
            ResolutionTableName = "HealthMetrics",
            FromDate = new DateTime(2024, 01, 01),
            ToDate = new DateTime(2024, 01, 02)
        };
        metricState.SetSeriesSelections([new MetricSeriesSelection("Weight", "morning")]);

        var uiState = new UiState();
        var validator = new DataLoadValidator(metricState);
        var service = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        var vnext = new VNextMainChartIntegrationCoordinator(CreateStubSessionCoordinator);
        var coordinator = MetricLoadCoordinator.CreateInstance(
            chartState, metricState, uiState, service, validator, ex => ex.Message, vnext);

        // CMS is disabled globally — routing should still use VNext (visibility-based, not CMS-based)
        Core.Configuration.CmsConfiguration.UseCmsData = false;
        try
        {
            var request = new MetricLoadRequest("Weight", metricState.SelectedSeries.ToList(), metricState.FromDate!.Value, metricState.ToDate!.Value, metricState.ResolutionTableName!);
            await coordinator.LoadMetricDataAsync(request, _ => { });

            Assert.Equal(EvidenceRuntimePath.VNextMain, chartState.LastLoadRuntime!.RuntimePath);
        }
        finally
        {
            Core.Configuration.CmsConfiguration.UseCmsData = true;
        }
    }

    private static ReasoningSessionCoordinator CreateStubSessionCoordinator()
    {
        var loader = new StubMetricSeriesLoader();
        var gateway = new LegacyMetricViewGateway(loader);
        var planner = new ChartProgramPlanner(new TimeSeriesAlignmentKernel(), new OperationKernel());
        var engine = new ReasoningEngine(gateway, planner);
        return new ReasoningSessionCoordinator(engine);
    }

    private sealed class StubMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            return Task.FromResult(0L);
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(
            string baseType,
            string? subtype,
            DateTime? from,
            DateTime? to,
            string tableName,
            int? maxRecords = null,
            SamplingMode samplingMode = SamplingMode.None,
            int? targetSamples = null)
        {
            IEnumerable<MetricData> result =
            [
                new MetricData
                {
                    NormalizedTimestamp = from ?? new DateTime(2024, 01, 01),
                    Value = 1m,
                    Unit = "u"
                }
            ];

            return Task.FromResult(result);
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName)
        {
            IEnumerable<MetricNameOption> results =
            [
                new MetricNameOption("morning", "Morning"),
                new MetricNameOption("evening", "Evening")
            ];

            return Task.FromResult(results);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }
    }

    private sealed class StubMetricSeriesLoader : IMetricSeriesLoader
    {
        public Task<LoadedMetricSeries> LoadAsync(
            MetricSeriesRequest request,
            DateTime from,
            DateTime to,
            string resolutionTableName,
            CancellationToken cancellationToken = default)
        {
            var value = string.Equals(request.QuerySubtype, "evening", StringComparison.OrdinalIgnoreCase) ? 2m : 1m;
            return Task.FromResult(new LoadedMetricSeries(
                [new MetricData { NormalizedTimestamp = from, Value = value }],
                null));
        }
    }
}
