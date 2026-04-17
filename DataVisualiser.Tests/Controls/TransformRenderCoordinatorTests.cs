using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class TransformRenderCoordinatorTests
{
    [Fact]
    public void Clear_ShouldClearGridsAndDelegateToRenderingContract()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new FakeTransformController();
            controller.TransformGrid1.ItemsSource = new[]
            {
                new { Value = "1.0000" }
            };
            controller.TransformGrid2.ItemsSource = new[]
            {
                new { Value = "2.0000" }
            };
            controller.TransformGrid3.ItemsSource = new[]
            {
                new { Value = "3.0000" }
            };

            var contract = new FakeTransformRenderingContract();
            var coordinator = new TransformRenderCoordinator(controller, new ChartState(), contract);

            coordinator.Clear();

            Assert.Equal(1, contract.ClearCalls);
            Assert.Null(controller.TransformGrid1.ItemsSource);
            Assert.Null(controller.TransformGrid2.ItemsSource);
            Assert.Null(controller.TransformGrid3.ItemsSource);
        });
    }

    [Fact]
    public async Task RenderResultsAsync_ShouldPopulateResultGridAndInvokeRenderContract()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var controller = new FakeTransformController();
            var contract = new FakeTransformRenderingContract();
            var coordinator = new TransformRenderCoordinator(controller, new ChartState(), contract);
            var timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dataList = new List<MetricData>
            {
                new() { NormalizedTimestamp = timestamp, Value = 1m },
                new() { NormalizedTimestamp = timestamp.AddHours(1), Value = 2m }
            };

            var execution = new TransformExecutionResult(
                dataList,
                [1d, 2d],
                "Identity",
                1,
                [dataList],
                null);

            var resolution = new TransformResolutionResult(
                new TransformSelectionResolution(new MetricSeriesSelection("Weight", "body_mass"), null, false),
                dataList,
                null,
                new ChartDataContext
                {
                    Data1 = dataList,
                    DisplayName1 = "Weight:body_mass",
                    From = timestamp,
                    To = timestamp.AddHours(1)
                });

            await coordinator.RenderResultsAsync(execution, resolution);

            Assert.Equal(1, contract.RenderCalls);
            var items = Assert.IsAssignableFrom<IEnumerable>(controller.TransformGrid3.ItemsSource);
            Assert.Equal(2, items.Cast<object>().Count());
            Assert.Equal(Visibility.Visible, controller.TransformGrid3Panel.Visibility);
            Assert.Equal(Visibility.Visible, controller.TransformChartContentPanel.Visibility);
        });
    }

    [Fact]
    public void HasRenderableContent_ShouldDelegateToRenderingContract()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new FakeTransformController();
            var contract = new FakeTransformRenderingContract
            {
                HasRenderableContentResult = true
            };
            var coordinator = new TransformRenderCoordinator(controller, new ChartState(), contract);

            var hasContent = coordinator.HasRenderableContent();

            Assert.True(hasContent);
            Assert.Equal(1, contract.HasRenderableContentCalls);
        });
    }

    [Fact]
    public async Task RenderResultsAsync_ShouldUseTransformLayoutCapabilitiesWhenAvailable()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var controller = new CapableTransformController();
            controller.TransformGrid3.Columns.Add(new DataGridTextColumn());
            controller.TransformGrid3.Columns.Add(new DataGridTextColumn());
            var contract = new FakeTransformRenderingContract();
            var coordinator = new TransformRenderCoordinator(controller, new ChartState(), contract);
            var timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dataList = new List<MetricData>
            {
                new() { NormalizedTimestamp = timestamp, Value = 1m }
            };

            var execution = new TransformExecutionResult(dataList, [1d], "Identity", 1, [dataList], null);
            var resolution = new TransformResolutionResult(
                new TransformSelectionResolution(new MetricSeriesSelection("Weight", "body_mass"), null, false),
                dataList,
                null,
                new ChartDataContext
                {
                    Data1 = dataList,
                    DisplayName1 = "Weight:body_mass",
                    From = timestamp,
                    To = timestamp
                });

            await coordinator.RenderResultsAsync(execution, resolution);

            Assert.True(controller.UpdateAuxiliaryVisualsCalled);
            Assert.Equal(DataGridLengthUnitType.SizeToCells, controller.TransformGrid3.Columns[0].Width.UnitType);
            Assert.Equal(DataGridLengthUnitType.SizeToCells, controller.TransformGrid3.Columns[1].Width.UnitType);
        });
    }

    [Fact]
    public void Clear_ShouldResetTransformLayoutCapabilitiesWhenAvailable()
    {
        StaTestHelper.Run(() =>
        {
            var controller = new CapableTransformController();
            var contract = new FakeTransformRenderingContract();
            var coordinator = new TransformRenderCoordinator(controller, new ChartState(), contract);

            coordinator.Clear();

            Assert.True(controller.ResetAuxiliaryVisualsCalled);
        });
    }

    private sealed class FakeTransformRenderingContract : ITransformRenderingContract
    {
        public int ClearCalls { get; private set; }
        public int RenderCalls { get; private set; }
        public int HasRenderableContentCalls { get; private set; }
        public bool HasRenderableContentResult { get; set; }

        public IReadOnlyList<TransformBackendQualification> GetBackendQualificationMatrix()
        {
            return [];
        }

        public TransformRenderingCapabilities GetCapabilities(TransformRenderingRoute route)
        {
            return new TransformRenderingCapabilities(
                "fake",
                TransformRenderingQualification.Qualified,
                true,
                true,
                true,
                true,
                true,
                true);
        }

        public Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host)
        {
            RenderCalls++;
            return Task.CompletedTask;
        }

        public void Clear(TransformRenderingRoute route, TransformChartRenderHost host)
        {
            ClearCalls++;
            host.ResetAuxiliaryVisuals?.Invoke();
        }

        public void ResetView(TransformRenderingRoute route, TransformChartRenderHost host)
        {
        }

        public bool HasRenderableContent(TransformRenderingRoute route, TransformChartRenderHost host)
        {
            HasRenderableContentCalls++;
            return HasRenderableContentResult;
        }
    }

    private class FakeTransformController : ITransformDataPanelController
    {
        public ChartPanelController Panel { get; } = new();
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
        public CartesianChart ChartTransformResult { get; } = new();
        public CartesianChart Chart => ChartTransformResult;
        public Dispatcher Dispatcher => Dispatcher.CurrentDispatcher;
        public event EventHandler? ToggleRequested;
        public event EventHandler? OperationChanged;
        public event EventHandler? PrimarySubtypeChanged;
        public event EventHandler? SecondarySubtypeChanged;
        public event EventHandler? ComputeRequested;
        public Button ToggleButton => Panel.ToggleButtonControl;

        public void SetToggleEnabled(bool enabled)
        {
            ToggleButton.IsEnabled = enabled;
        }

        public void SetTitle(string title)
        {
        }
    }

    private sealed class CapableTransformController : FakeTransformController, ITransformLayoutCapabilities
    {
        public bool UpdateAuxiliaryVisualsCalled { get; private set; }
        public bool ResetAuxiliaryVisualsCalled { get; private set; }
        public DataGridLength ResultGridColumnWidth => new(1, DataGridLengthUnitType.SizeToCells);
        public bool UsesAutomaticChartWidth => true;

        public void UpdateAuxiliaryVisuals()
        {
            UpdateAuxiliaryVisualsCalled = true;
        }

        public void ResetAuxiliaryVisuals()
        {
            ResetAuxiliaryVisualsCalled = true;
        }
    }
}
