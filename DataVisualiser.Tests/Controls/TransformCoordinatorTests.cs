using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class TransformCoordinatorTests
{
    [Fact]
    public void OperationStateCoordinator_UpdateComputeButtonState_EnablesWithoutExplicitOperation_WhenPrimaryDataExists()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new FakeTransformDataPanelController();
            var coordinator = new TransformOperationStateCoordinator();
            var executionCoordinator = new TransformOperationExecutionCoordinator(new DataVisualiser.Core.Transforms.TransformComputationService());
            var context = CreatePrimaryOnlyContext();

            coordinator.UpdateComputeButtonState(
                controller,
                context,
                isSelectionPendingLoad: false,
                _ => new TransformSelectionResolution(new MetricSeriesSelection("MetricA", "SubA"), null, false),
                executionCoordinator);

            Assert.True(controller.TransformComputeButton.IsEnabled);
        });
    }

    [Fact]
    public void OperationStateCoordinator_GetSelectedOperationTag_ReturnsCurrentComboSelection()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new FakeTransformDataPanelController();
            controller.TransformOperationCombo.Items.Add(new ComboBoxItem { Tag = "Add", Content = "Add" });
            controller.TransformOperationCombo.SelectedIndex = 0;

            var coordinator = new TransformOperationStateCoordinator();

            var operationTag = coordinator.GetSelectedOperationTag(controller);

            Assert.Equal("Add", operationTag);
        });
    }

    [Fact]
    public void OperationExecutionCoordinator_CanExecuteBinary_WhenSecondarySelectionExistsOutsideLoadedContext()
    {
        var coordinator = new TransformOperationExecutionCoordinator(new DataVisualiser.Core.Transforms.TransformComputationService());
        var context = CreatePrimaryOnlyContext();
        var selection = new TransformSelectionResolution(
            new MetricSeriesSelection("MetricA", "SubA"),
            new MetricSeriesSelection("MetricB", "SubB"),
            true);

        var canExecute = coordinator.CanExecute(context, selection, "Add");

        Assert.True(canExecute);
    }

    [Fact]
    public void OperationExecutionCoordinator_ExecuteWithoutOperation_ProjectsPrimaryData()
    {
        var coordinator = new TransformOperationExecutionCoordinator(new DataVisualiser.Core.Transforms.TransformComputationService());
        var primary = CreatePrimaryData();
        var context = CreatePrimaryOnlyContext();
        var resolution = new TransformResolutionResult(
            new TransformSelectionResolution(new MetricSeriesSelection("MetricA", "SubA"), null, false),
            primary,
            null,
            context);

        var result = coordinator.Execute(resolution, null);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result!.OperationTag);
        Assert.Equal("MetricA:SubA", result.OverrideLabel);
        Assert.Equal(2, result.DataList.Count);
        Assert.Equal([1d, 2d], result.Results);
    }

    [Fact]
    public void DataResolutionCoordinator_ResolveSelections_UsesExplicitSecondarySelectionOutsideLoadedContext()
    {
        StaTestHelper.Run(() =>
        {
            var coordinator = CreateResolutionCoordinator(
                out var viewModel,
                out var controller,
                out var tooltipManager,
                out var window);

            viewModel.ChartState.SelectedTransformSecondarySeries = new MetricSeriesSelection("MetricB", "SubB", "MetricB", "SubB");

            var resolution = coordinator.ResolveSelections(CreatePrimaryOnlyContext(), isSelectionPendingLoad: false);

            Assert.NotNull(resolution.SecondarySelection);
            Assert.True(resolution.HasAvailableSecondaryInput);

            tooltipManager.Dispose();
            window.Close();
        });
    }

    [Fact]
    public async Task DataResolutionCoordinator_ResolveAsync_LoadsSecondaryDataFromSelectionService()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var coordinator = CreateResolutionCoordinator(
                out var viewModel,
                out var controller,
                out var tooltipManager,
                out var window,
                new ReturningMetricSelectionDataQueries());

            viewModel.ChartState.SelectedTransformSecondarySeries = new MetricSeriesSelection("MetricB", "SubB", "MetricB", "SubB");

            var result = await coordinator.ResolveAsync(CreatePrimaryOnlyContext(), isSelectionPendingLoad: false);

            Assert.NotNull(result.SecondaryData);
            Assert.Single(result.SecondaryData!);
            Assert.Equal("MetricB", result.Context.SecondaryMetricType);
            Assert.Equal("MetricB - SubB", result.Context.DisplayName2);

            tooltipManager.Dispose();
            window.Close();
        });
    }

    private static TransformDataResolutionCoordinator CreateResolutionCoordinator(
        out MainWindowViewModel viewModel,
        out ITransformDataPanelController controller,
        out ChartTooltipManager tooltipManager,
        out Window window,
        IMetricSelectionDataQueries? queries = null)
    {
        var chartState = new ChartState
        {
            IsTransformPanelVisible = true
        };
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService(queries ?? new StubMetricSelectionDataQueries(), "TestConnection");
        viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        controller = new FakeTransformDataPanelController();

        var computationEngine = new ChartComputationEngine();
        var renderEngine = new ChartRenderEngine();
        window = new Window();
        tooltipManager = new ChartTooltipManager(window);
        _ = new ChartUpdateCoordinator(computationEngine, renderEngine, tooltipManager, chartState.ChartTimestamps, new CapturingNotificationService());

        return new TransformDataResolutionCoordinator(controller, viewModel, metricService, new MetricSeriesSelectionCache());
    }

    private static ChartDataContext CreatePrimaryOnlyContext()
    {
        var primary = CreatePrimaryData();
        return new ChartDataContext
        {
            Data1 = primary,
            MetricType = "MetricA",
            PrimaryMetricType = "MetricA",
            PrimarySubtype = "SubA",
            DisplayPrimaryMetricType = "MetricA",
            DisplayPrimarySubtype = "SubA",
            DisplayName1 = "MetricA:SubA",
            From = primary[0].NormalizedTimestamp,
            To = primary[^1].NormalizedTimestamp
        };
    }

    private static List<MetricData> CreatePrimaryData()
    {
        return
        [
            new MetricData
            {
                NormalizedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Value = 1m
            },
            new MetricData
            {
                NormalizedTimestamp = new DateTime(2024, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                Value = 2m
            }
        ];
    }

    private sealed class CapturingNotificationService : IUserNotificationService
    {
        public void ShowError(string title, string message)
        {
        }
    }

    private sealed class FakeTransformDataPanelController : ITransformDataPanelController
    {
        public ChartPanelController Panel { get; } = new();
        public Button ToggleButton => Panel.ToggleButtonControl;
        public CartesianChart Chart { get; } = new();
        public ComboBox TransformPrimarySubtypeCombo { get; } = new();
        public ComboBox TransformSecondarySubtypeCombo { get; } = new();
        public ComboBox TransformOperationCombo { get; } = new();
        public Button TransformComputeButton { get; } = new();
        public StackPanel TransformSecondarySubtypePanel { get; } = new();
        public StackPanel TransformGrid2Panel { get; } = new();
        public StackPanel TransformGrid3Panel { get; } = new();
        public StackPanel TransformChartContentPanel { get; } = new();
        public Grid TransformChartContainer { get; } = new();
        public DataGrid TransformGrid1 { get; } = new();
        public DataGrid TransformGrid2 { get; } = new();
        public DataGrid TransformGrid3 { get; } = new();
        public TextBlock TransformGrid1Title { get; } = new();
        public TextBlock TransformGrid2Title { get; } = new();
        public TextBlock TransformGrid3Title { get; } = new();
        public CartesianChart ChartTransformResult => Chart;
        public System.Windows.Threading.Dispatcher Dispatcher => System.Windows.Threading.Dispatcher.CurrentDispatcher;

        public event EventHandler? ToggleRequested;
        public event EventHandler? OperationChanged;
        public event EventHandler? PrimarySubtypeChanged;
        public event EventHandler? SecondarySubtypeChanged;
        public event EventHandler? ComputeRequested;
    }

    private sealed class StubMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => Task.FromResult(0L);
        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null) => Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());
        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }

    private sealed class ReturningMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => Task.FromResult(1L);

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
        {
            IEnumerable<MetricData> data =
            [
                new MetricData
                {
                    NormalizedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Value = 3m
                }
            ];

            return Task.FromResult(data);
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
