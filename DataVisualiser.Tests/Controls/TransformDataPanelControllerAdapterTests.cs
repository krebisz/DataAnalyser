using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading.Tasks;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Data.Abstractions;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class TransformDataPanelControllerAdapterTests
{
    [Fact]
    public void UpdateTransformComputeButtonState_Enables_WhenNoOperationSelected_AndPrimaryDataAvailable()
    {
        StaTestHelper.Run(() =>
        {
            var adapter = CreateAdapter(out var viewModel, out var controller, out var tooltipManager, out var window);

            var data = new List<MetricData>
            {
                    new()
                    {
                            NormalizedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            Value = 1m
                    }
            };

            viewModel.ChartState.LastContext = new ChartDataContext
            {
                    Data1 = data,
                    MetricType = "MetricA",
                    PrimaryMetricType = "MetricA",
                    PrimarySubtype = "SubA",
                    DisplayPrimaryMetricType = "MetricA",
                    DisplayPrimarySubtype = "SubA",
                    DisplayName1 = "MetricA:SubA",
                    From = data[0].NormalizedTimestamp,
                    To = data[0].NormalizedTimestamp
            };

            adapter.UpdateTransformComputeButtonState();

            Assert.True(controller.TransformComputeButton.IsEnabled);

            tooltipManager.Dispose();
            window.Close();
        });
    }

    [Fact]
    public async Task RenderPrimarySelection_WhenNoOperationSelected_UsesPrimarySeriesValues()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var adapter = CreateAdapter(out var viewModel, out var controller, out var tooltipManager, out var window);

            viewModel.MetricState.SetSeriesSelections(new List<MetricSeriesSelection>
            {
                    new("MetricA", "SubA", "MetricA", "SubA")
            });

            adapter.UpdateTransformSubtypeOptions();

            var data = new List<MetricData>
            {
                    new()
                    {
                            NormalizedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            Value = 1m
                    },
                    new()
                    {
                            NormalizedTimestamp = new DateTime(2024, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                            Value = 2m
                    }
            };

            var ctx = new ChartDataContext
            {
                    Data1 = data,
                    MetricType = "MetricA",
                    PrimaryMetricType = "MetricA",
                    PrimarySubtype = "SubA",
                    DisplayPrimaryMetricType = "MetricA",
                    DisplayPrimarySubtype = "SubA",
                    DisplayName1 = "MetricA:SubA",
                    From = data[0].NormalizedTimestamp,
                    To = data[1].NormalizedTimestamp
            };

            viewModel.ChartState.LastContext = ctx;

            var method = typeof(TransformDataPanelControllerAdapter).GetMethod("RenderPrimarySelectionAsResult", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            var task = (Task)method!.Invoke(adapter, new object?[]
            {
                    ctx
            })!;

            await task;

            var items = controller.TransformGrid3.ItemsSource as IEnumerable;
            Assert.NotNull(items);

            var list = items.Cast<object>().ToList();
            Assert.Equal(2, list.Count);

            var first = list[0];
            var valueProp = first.GetType().GetProperty("Value");
            Assert.Equal("1.0000", valueProp?.GetValue(first));

            tooltipManager.Dispose();
            window.Close();
        });
    }

    [Fact]
    public void HandleVisibilityOnlyToggle_WhenVisible_PopulatesGridAndSubtypeOptions()
    {
        StaTestHelper.Run(() =>
        {
            var adapter = CreateAdapter(out var viewModel, out var controller, out var tooltipManager, out var window);

            viewModel.ChartState.IsTransformPanelVisible = true;
            viewModel.MetricState.SetSeriesSelections(new List<MetricSeriesSelection>
            {
                new("MetricA", "SubA", "MetricA", "SubA"),
                new("MetricB", "SubB", "MetricB", "SubB")
            });

            var data = new List<MetricData>
            {
                new()
                {
                    NormalizedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Value = 1m
                }
            };

            var ctx = new ChartDataContext
            {
                Data1 = data,
                MetricType = "MetricA",
                PrimaryMetricType = "MetricA",
                PrimarySubtype = "SubA",
                DisplayPrimaryMetricType = "MetricA",
                DisplayPrimarySubtype = "SubA",
                DisplayName1 = "MetricA:SubA",
                From = data[0].NormalizedTimestamp,
                To = data[0].NormalizedTimestamp
            };

            viewModel.ChartState.LastContext = ctx;

            adapter.HandleVisibilityOnlyToggle(ctx);

            Assert.NotNull(controller.TransformGrid1.ItemsSource);
            Assert.True(controller.TransformPrimarySubtypeCombo.Items.Count > 0);
            Assert.True(controller.TransformComputeButton.IsEnabled);

            tooltipManager.Dispose();
            window.Close();
        });
    }

    [Fact]
    public async Task RefreshTransformGridsFromSelectionAsync_EnablesBinaryCompute_WhenSecondarySelectionExistsOutsideLastContext()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var adapter = CreateAdapter(
                out var viewModel,
                out var controller,
                out var tooltipManager,
                out var window,
                new ReturningMetricSelectionDataQueries());

            viewModel.MetricState.SetSeriesSelections(new List<MetricSeriesSelection>
            {
                new("MetricA", "SubA", "MetricA", "SubA"),
                new("MetricB", "SubB", "MetricB", "SubB")
            });

            var data = new List<MetricData>
            {
                new()
                {
                    NormalizedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Value = 1m
                }
            };

            viewModel.ChartState.LastContext = new ChartDataContext
            {
                Data1 = data,
                MetricType = "MetricA",
                PrimaryMetricType = "MetricA",
                PrimarySubtype = "SubA",
                DisplayPrimaryMetricType = "MetricA",
                DisplayPrimarySubtype = "SubA",
                DisplayName1 = "MetricA:SubA",
                From = data[0].NormalizedTimestamp,
                To = data[0].NormalizedTimestamp
            };

            controller.TransformOperationCombo.Items.Add(new ComboBoxItem
            {
                Tag = "Add",
                Content = "Add"
            });

            adapter.UpdateTransformSubtypeOptions();
            controller.TransformOperationCombo.SelectedIndex = 0;

            var method = typeof(TransformDataPanelControllerAdapter).GetMethod("RefreshTransformGridsFromSelectionAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            var task = (Task)method!.Invoke(adapter, Array.Empty<object>())!;
            await task;

            Assert.True(controller.TransformComputeButton.IsEnabled);

            tooltipManager.Dispose();
            window.Close();
        });
    }

    private static TransformDataPanelControllerAdapter CreateAdapter(out MainWindowViewModel viewModel, out ITransformDataPanelController controller, out ChartTooltipManager tooltipManager, out Window window, IMetricSelectionDataQueries? queries = null)
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
        var coordinator = new ChartUpdateCoordinator(computationEngine, renderEngine, tooltipManager, chartState.ChartTimestamps, new CapturingNotificationService());
        var renderingContract = new TransformRenderingContract(new TransformChartRenderInvoker(coordinator));

        return new TransformDataPanelControllerAdapter(controller, viewModel, () => false, () => new DummyDisposable(), metricService, renderingContract);
    }

    private sealed class DummyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
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
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            return Task.FromResult(0L);
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
        {
            return Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
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

    private sealed class ReturningMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            return Task.FromResult(1L);
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
        {
            IEnumerable<MetricData> data =
            [
                new MetricData
                {
                    NormalizedTimestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Value = 2m
                }
            ];

            return Task.FromResult(data);
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
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
}
