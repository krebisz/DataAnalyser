using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;
using Moq;
using PolarChart = LiveChartsCore.SkiaSharpView.WPF.PolarChart;

namespace DataVisualiser.Tests.Controls;

public sealed class ChartControllerFactoryTests
{
    [Fact]
    public void Create_RegistersAllStandardChartControllers()
    {
        StaTestHelper.Run(() =>
        {
            using var harness = new FactoryHarness();
            var factory = new ChartControllerFactory();

            var result = factory.Create(harness.CreateContext());

            Assert.Same(result.Main, result.Registry.Get(ChartControllerKeys.Main));
            Assert.Same(result.Normalized, result.Registry.Get(ChartControllerKeys.Normalized));
            Assert.Same(result.DiffRatio, result.Registry.Get(ChartControllerKeys.DiffRatio));
            Assert.Same(result.Distribution, result.Registry.Get(ChartControllerKeys.Distribution));
            Assert.Same(result.WeekdayTrend, result.Registry.Get(ChartControllerKeys.WeeklyTrend));
            Assert.Same(result.Transform, result.Registry.Get(ChartControllerKeys.Transform));
            Assert.Same(result.BarPie, result.Registry.Get(ChartControllerKeys.BarPie));

            var keys = result.Registry.All().Select(controller => controller.Key).ToList();
            Assert.DoesNotContain(ChartControllerKeys.SyncfusionSunburst, keys, StringComparer.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Create_WithSyncfusionController_RegistersSyncfusionController()
    {
        StaTestHelper.Run(() =>
        {
            using var harness = new FactoryHarness();
            var factory = new ChartControllerFactory();

            var result = factory.Create(harness.CreateContext(new FakeSyncfusionSunburstChartController()));

            var syncfusion = result.Registry.Get(ChartControllerKeys.SyncfusionSunburst);
            Assert.Equal(ChartControllerKeys.SyncfusionSunburst, syncfusion.Key);
        });
    }

    private sealed class FactoryHarness : IDisposable
    {
        private readonly Window _window;
        private readonly ChartTooltipManager _tooltipManager;
        private readonly CapturingNotificationService _notifications = new();

        public FactoryHarness()
        {
            var chart = new CartesianChart();
            _window = new Window { Content = chart };
            _tooltipManager = new ChartTooltipManager(_window);

            var chartState = new ChartState();
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");

            ViewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            MetricSelectionService = metricService;
            ChartUpdateCoordinator = new ChartUpdateCoordinator(
                new ChartComputationEngine(),
                new ChartRenderEngine(),
                _tooltipManager,
                chartState.ChartTimestamps,
                _notifications);
            WeekdayTrendChartUpdateCoordinator = new WeekdayTrendChartUpdateCoordinator(new WeekdayTrendRenderingService(), chartState.ChartTimestamps);
            ChartRendererResolver = new ChartRendererResolver();
            ChartSurfaceFactory = new ChartSurfaceFactory(ChartRendererResolver);
        }

        public MainWindowViewModel ViewModel { get; }
        public MetricSelectionService MetricSelectionService { get; }
        public ChartUpdateCoordinator ChartUpdateCoordinator { get; }
        public WeekdayTrendChartUpdateCoordinator WeekdayTrendChartUpdateCoordinator { get; }
        public IChartRendererResolver ChartRendererResolver { get; }
        public IChartSurfaceFactory ChartSurfaceFactory { get; }

        public ChartControllerFactoryContext CreateContext(ISyncfusionSunburstChartController? syncfusion = null)
        {
            return new ChartControllerFactoryContext(
                new FakeMainChartController(),
                new FakeNormalizedChartController(),
                new FakeDiffRatioChartController(),
                new FakeDistributionChartController(),
                new FakeWeekdayTrendChartController(),
                new FakeTransformDataPanelController(),
                new FakeBarPieChartController(),
                ViewModel,
                () => false,
                () => new NoOpDisposable(),
                MetricSelectionService,
                () => null,
                ChartUpdateCoordinator,
                () => null,
                WeekdayTrendChartUpdateCoordinator,
                Mock.Of<IDistributionService>(),
                Mock.Of<IDistributionService>(),
                new DistributionPolarRenderingService(),
                () => null,
                () => _tooltipManager,
                ChartRendererResolver,
                ChartSurfaceFactory,
                syncfusion);
        }

        public void Dispose()
        {
            _tooltipManager.Dispose();
            _window.Close();
        }
    }

    private sealed class CapturingNotificationService : IUserNotificationService
    {
        public void ShowError(string title, string message)
        {
        }
    }

    private abstract class FakeChartPanelHost
    {
        protected FakeChartPanelHost()
        {
            Panel = new ChartPanelController();
        }

        public ChartPanelController Panel { get; }
        public Button ToggleButton => Panel.ToggleButtonControl;
    }

    private sealed class FakeMainChartController : FakeChartPanelHost, IMainChartController
    {
        public CartesianChart Chart { get; } = new();
        public RadioButton DisplayRegularRadio { get; } = new();
        public RadioButton DisplaySummedRadio { get; } = new();
        public RadioButton DisplayStackedRadio { get; } = new();
        public ComboBox OverlaySubtypeCombo { get; } = new();
        public StackPanel OverlaySubtypePanel { get; } = new();
        public event EventHandler? ToggleRequested;
        public event EventHandler? DisplayModeChanged;
        public event EventHandler? OverlaySubtypeChanged;
    }

    private sealed class FakeNormalizedChartController : FakeChartPanelHost, INormalizedChartController
    {
        public CartesianChart Chart { get; } = new();
        public RadioButton NormZeroToOneRadio { get; } = new();
        public RadioButton NormPercentOfMaxRadio { get; } = new();
        public RadioButton NormRelativeToMaxRadio { get; } = new();
        public ComboBox NormalizedPrimarySubtypeCombo { get; } = new();
        public ComboBox NormalizedSecondarySubtypeCombo { get; } = new();
        public StackPanel NormalizedSecondarySubtypePanel { get; } = new();
        public event EventHandler? ToggleRequested;
        public event EventHandler? NormalizationModeChanged;
        public event EventHandler? PrimarySubtypeChanged;
        public event EventHandler? SecondarySubtypeChanged;
    }

    private sealed class FakeDiffRatioChartController : FakeChartPanelHost, IDiffRatioChartController
    {
        public CartesianChart Chart { get; } = new();
        public Button OperationToggleButton { get; } = new();
        public ComboBox PrimarySubtypeCombo { get; } = new();
        public ComboBox SecondarySubtypeCombo { get; } = new();
        public StackPanel SecondarySubtypePanel { get; } = new();
        public event EventHandler? ToggleRequested;
        public event EventHandler? OperationToggleRequested;
        public event EventHandler? PrimarySubtypeChanged;
        public event EventHandler? SecondarySubtypeChanged;
    }

    private sealed class FakeDistributionChartController : FakeChartPanelHost, IDistributionChartController
    {
        public CartesianChart Chart { get; } = new();
        public ComboBox ModeCombo { get; } = new();
        public ComboBox SubtypeCombo { get; } = new();
        public RadioButton FrequencyShadingRadio { get; } = new();
        public RadioButton SimpleRangeRadio { get; } = new();
        public ComboBox IntervalCountCombo { get; } = new();
        public Button ChartTypeToggleButton { get; } = new();
        public PolarChart PolarChart { get; } = new();
        public event EventHandler? ToggleRequested;
        public event EventHandler? ChartTypeToggleRequested;
        public event EventHandler? ModeChanged;
        public event EventHandler? SubtypeChanged;
        public event EventHandler? DisplayModeChanged;
        public event EventHandler? IntervalCountChanged;
    }

    private sealed class FakeWeekdayTrendChartController : FakeChartPanelHost, IWeekdayTrendChartController
    {
        public CartesianChart Chart { get; } = new();
        public CartesianChart PolarChart { get; } = new();
        public Button ChartTypeToggleButton { get; } = new();
        public ComboBox SubtypeCombo { get; } = new();
        public ComboBox AverageWindowCombo { get; } = new();
        public event EventHandler? ToggleRequested;
        public event EventHandler? ChartTypeToggleRequested;
        public event EventHandler? SubtypeChanged;
        public event EventHandler<DataVisualiser.UI.Events.WeekdayTrendDayToggleEventArgs>? DayToggled;
        public event EventHandler<DataVisualiser.UI.Events.WeekdayTrendAverageToggleEventArgs>? AverageToggled;
        public event EventHandler? AverageWindowChanged;
    }

    private sealed class FakeTransformDataPanelController : FakeChartPanelHost, ITransformDataPanelController
    {
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

    private sealed class FakeBarPieChartController : FakeChartPanelHost, IBarPieChartController
    {
        public RadioButton BarModeRadio { get; } = new();
        public RadioButton PieModeRadio { get; } = new();
        public ComboBox BucketCountCombo { get; } = new();
        public event EventHandler? ToggleRequested;
        public event EventHandler? DisplayModeChanged;
        public event EventHandler? BucketCountChanged;
    }

    private sealed class FakeSyncfusionSunburstChartController : FakeChartPanelHost, ISyncfusionSunburstChartController
    {
        public object? ItemsSource { get; set; }
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    private sealed class StubMetricSelectionDataQueries : Core.Data.Abstractions.IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => Task.FromResult(0L);
        public Task<IEnumerable<DataVisualiser.Shared.Models.MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, DataVisualiser.Core.Data.SamplingMode samplingMode = DataVisualiser.Core.Data.SamplingMode.None, int? targetSamples = null)
            => Task.FromResult<IEnumerable<DataVisualiser.Shared.Models.MetricData>>(Array.Empty<DataVisualiser.Shared.Models.MetricData>());
        public Task<IEnumerable<DataVisualiser.Shared.Models.MetricNameOption>> GetBaseMetricTypeOptions(string tableName)
            => Task.FromResult<IEnumerable<DataVisualiser.Shared.Models.MetricNameOption>>(Array.Empty<DataVisualiser.Shared.Models.MetricNameOption>());
        public Task<IEnumerable<DataVisualiser.Shared.Models.MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName)
            => Task.FromResult<IEnumerable<DataVisualiser.Shared.Models.MetricNameOption>>(Array.Empty<DataVisualiser.Shared.Models.MetricNameOption>());
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName)
            => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null)
            => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName)
            => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
