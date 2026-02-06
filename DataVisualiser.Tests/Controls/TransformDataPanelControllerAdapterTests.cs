using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

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

    private static TransformDataPanelControllerAdapter CreateAdapter(out MainWindowViewModel viewModel, out TransformDataPanelController controller, out ChartTooltipManager tooltipManager, out Window window)
    {
        var chartState = new ChartState
        {
                IsTransformPanelVisible = true
        };
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True");
        viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        controller = new TransformDataPanelController();

        var computationEngine = new ChartComputationEngine();
        var renderEngine = new ChartRenderEngine();
        window = new Window();
        tooltipManager = new ChartTooltipManager(window);
        var coordinator = new ChartUpdateCoordinator(computationEngine, renderEngine, tooltipManager, chartState.ChartTimestamps);

        return new TransformDataPanelControllerAdapter(controller, viewModel, () => false, () => new DummyDisposable(), metricService, coordinator);
    }

    private sealed class DummyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
