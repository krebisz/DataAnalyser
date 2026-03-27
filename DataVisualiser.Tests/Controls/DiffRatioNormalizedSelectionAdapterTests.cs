using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.Controls;

public sealed class DiffRatioNormalizedSelectionAdapterTests
{
    [Fact]
    public void DiffRatioUpdateSubtypeOptions_ShouldPopulatePrimaryAndSecondaryCombos()
    {
        StaTestHelper.Run(() =>
        {
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            var setup = CreateDiffRatioAdapter();
            setup.MetricState.SetSeriesSelections([primary, secondary]);

            setup.Adapter.UpdateSubtypeOptions();

            Assert.Equal(2, setup.Controller.PrimarySubtypeCombo.Items.Count);
            Assert.Equal(2, setup.Controller.SecondarySubtypeCombo.Items.Count);
            Assert.True(setup.Controller.PrimarySubtypeCombo.IsEnabled);
            Assert.True(setup.Controller.SecondarySubtypeCombo.IsEnabled);
            Assert.Equal(System.Windows.Visibility.Visible, setup.Controller.SecondarySubtypePanel.Visibility);
            Assert.Equal(primary, setup.ChartState.SelectedDiffRatioPrimarySeries);
            Assert.Equal(secondary, setup.ChartState.SelectedDiffRatioSecondarySeries);
        });
    }

    [Fact]
    public async Task DiffRatioRenderAsync_ShouldRebuildContextFromSelectedSeries()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var setup = CreateDiffRatioAdapter();
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            setup.ChartState.SelectedDiffRatioPrimarySeries = secondary;
            setup.ChartState.SelectedDiffRatioSecondarySeries = primary;
            var context = CreateComparisonContext();

            await setup.Adapter.RenderAsync(context);

            var request = Assert.IsType<CartesianMetricChartRenderRequest>(setup.RenderingContract.LastRenderRequest);
            Assert.Equal(CartesianMetricChartRoute.DiffRatio, request.Route);
            Assert.Same(context.Data2, request.Context.Data1);
            Assert.Same(context.Data1, request.Context.Data2);
            Assert.Equal("Steps", request.Context.DisplayName1);
            Assert.Equal("Weight", request.Context.DisplayName2);
            Assert.Equal(secondary.MetricType, request.Context.PrimaryMetricType);
            Assert.Equal(primary.MetricType, request.Context.SecondaryMetricType);
        });
    }

    [Fact]
    public void DiffRatioOnPrimarySubtypeChanged_ShouldUpdateState_AndRerenderLastContext()
    {
        StaTestHelper.Run(() =>
        {
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            var setup = CreateDiffRatioAdapter();
            setup.MetricState.SetSeriesSelections([primary, secondary]);
            setup.Adapter.UpdateSubtypeOptions();
            setup.ChartState.LastContext = CreateComparisonContext();
            setup.Controller.PrimarySubtypeCombo.SelectedIndex = 1;

            setup.Adapter.OnPrimarySubtypeChanged(null, EventArgs.Empty);

            Assert.Equal(secondary, setup.ChartState.SelectedDiffRatioPrimarySeries);
            var request = Assert.IsType<CartesianMetricChartRenderRequest>(setup.RenderingContract.LastRenderRequest);
            Assert.Equal("Steps", request.Context.DisplayName1);
            Assert.Equal(1, setup.RenderingContract.RenderCallCount);
        });
    }

    [Fact]
    public void DiffRatioOnPrimarySubtypeChanged_ShouldUpdateState_WithoutRerenderWhenHidden()
    {
        StaTestHelper.Run(() =>
        {
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            var setup = CreateDiffRatioAdapter();
            setup.ChartState.IsDiffRatioVisible = false;
            setup.MetricState.SetSeriesSelections([primary, secondary]);
            setup.Adapter.UpdateSubtypeOptions();
            setup.ChartState.LastContext = CreateComparisonContext();
            setup.Controller.PrimarySubtypeCombo.SelectedIndex = 1;

            setup.Adapter.OnPrimarySubtypeChanged(null, EventArgs.Empty);

            Assert.Equal(secondary, setup.ChartState.SelectedDiffRatioPrimarySeries);
            Assert.Null(setup.RenderingContract.LastRenderRequest);
            Assert.Equal(0, setup.RenderingContract.RenderCallCount);
        });
    }

    [Fact]
    public void NormalizedUpdateSubtypeOptions_ShouldPopulatePrimaryAndSecondaryCombos()
    {
        StaTestHelper.Run(() =>
        {
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            var setup = CreateNormalizedAdapter();
            setup.MetricState.SetSeriesSelections([primary, secondary]);

            setup.Adapter.UpdateSubtypeOptions();

            Assert.Equal(2, setup.Controller.NormalizedPrimarySubtypeCombo.Items.Count);
            Assert.Equal(2, setup.Controller.NormalizedSecondarySubtypeCombo.Items.Count);
            Assert.True(setup.Controller.NormalizedPrimarySubtypeCombo.IsEnabled);
            Assert.True(setup.Controller.NormalizedSecondarySubtypeCombo.IsEnabled);
            Assert.Equal(System.Windows.Visibility.Visible, setup.Controller.NormalizedSecondarySubtypePanel.Visibility);
            Assert.Equal(primary, setup.ChartState.SelectedNormalizedPrimarySeries);
            Assert.Equal(secondary, setup.ChartState.SelectedNormalizedSecondarySeries);
        });
    }

    [Fact]
    public async Task NormalizedRenderAsync_ShouldRebuildContextFromSelectedSeries()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var setup = CreateNormalizedAdapter();
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            setup.ChartState.SelectedNormalizedPrimarySeries = secondary;
            setup.ChartState.SelectedNormalizedSecondarySeries = primary;
            var context = CreateComparisonContext();

            await setup.Adapter.RenderAsync(context);

            var request = Assert.IsType<CartesianMetricChartRenderRequest>(setup.RenderingContract.LastRenderRequest);
            Assert.Equal(CartesianMetricChartRoute.Normalized, request.Route);
            Assert.Same(context.Data2, request.Context.Data1);
            Assert.Same(context.Data1, request.Context.Data2);
            Assert.Equal("Steps", request.Context.DisplayName1);
            Assert.Equal("Weight", request.Context.DisplayName2);
            Assert.Equal(secondary.MetricType, request.Context.PrimaryMetricType);
            Assert.Equal(primary.MetricType, request.Context.SecondaryMetricType);
        });
    }

    [Fact]
    public void NormalizedOnSecondarySubtypeChanged_ShouldUpdateState_AndRerenderLastContext()
    {
        StaTestHelper.Run(() =>
        {
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            var setup = CreateNormalizedAdapter();
            setup.MetricState.SetSeriesSelections([primary, secondary]);
            setup.Adapter.UpdateSubtypeOptions();
            setup.ChartState.LastContext = CreateComparisonContext();
            setup.Controller.NormalizedSecondarySubtypeCombo.SelectedIndex = 0;

            setup.Adapter.OnSecondarySubtypeChanged(null, EventArgs.Empty);

            Assert.Equal(primary, setup.ChartState.SelectedNormalizedSecondarySeries);
            var request = Assert.IsType<CartesianMetricChartRenderRequest>(setup.RenderingContract.LastRenderRequest);
            Assert.Equal("Weight", request.Context.DisplayName2);
            Assert.Equal(1, setup.RenderingContract.RenderCallCount);
        });
    }

    [Fact]
    public void NormalizedOnSecondarySubtypeChanged_ShouldUpdateState_WithoutRerenderWhenNoLastContext()
    {
        StaTestHelper.Run(() =>
        {
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            var setup = CreateNormalizedAdapter();
            setup.MetricState.SetSeriesSelections([primary, secondary]);
            setup.Adapter.UpdateSubtypeOptions();
            setup.Controller.NormalizedSecondarySubtypeCombo.SelectedIndex = 0;

            setup.Adapter.OnSecondarySubtypeChanged(null, EventArgs.Empty);

            Assert.Equal(primary, setup.ChartState.SelectedNormalizedSecondarySeries);
            Assert.Null(setup.RenderingContract.LastRenderRequest);
            Assert.Equal(0, setup.RenderingContract.RenderCallCount);
        });
    }

    private static DiffRatioSetup CreateDiffRatioAdapter()
    {
        var chartState = new ChartState
        {
                IsDiffRatioVisible = true
        };
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        var controller = new DiffRatioChartController();
        var renderingContract = new FakeCartesianMetricRenderingContract();
        var adapter = new DiffRatioChartControllerAdapter(
                controller,
                viewModel,
                () => false,
                () => NoOpScope.Instance,
                metricService,
                () => null,
                renderingContract);

        return new DiffRatioSetup(chartState, metricState, controller, adapter, renderingContract);
    }

    private static NormalizedSetup CreateNormalizedAdapter()
    {
        var chartState = new ChartState
        {
                IsNormalizedVisible = true
        };
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        var controller = new NormalizedChartController();
        var renderingContract = new FakeCartesianMetricRenderingContract();
        var adapter = new NormalizedChartControllerAdapter(
                controller,
                viewModel,
                () => false,
                () => NoOpScope.Instance,
                metricService,
                renderingContract);

        return new NormalizedSetup(chartState, metricState, controller, adapter, renderingContract);
    }

    private static ChartDataContext CreateComparisonContext()
    {
        var primaryData = new List<MetricData>
        {
            new()
            {
                NormalizedTimestamp = new DateTime(2026, 1, 1),
                Value = 10m,
                Unit = "kg"
            }
        };

        var secondaryData = new List<MetricData>
        {
            new()
            {
                NormalizedTimestamp = new DateTime(2026, 1, 1),
                Value = 20m,
                Unit = "steps"
            }
        };

        return new ChartDataContext
        {
                Data1 = primaryData,
                Data2 = secondaryData,
                DisplayName1 = "Weight",
                DisplayName2 = "Steps",
                MetricType = "weight",
                PrimaryMetricType = "weight",
                PrimarySubtype = "avg",
                SecondaryMetricType = "steps",
                SecondarySubtype = null,
                DisplayPrimaryMetricType = "Weight",
                DisplayPrimarySubtype = "Avg",
                DisplaySecondaryMetricType = "Steps",
                DisplaySecondarySubtype = "(All)",
                From = new DateTime(2026, 1, 1),
                To = new DateTime(2026, 1, 7)
        };
    }

    private sealed record DiffRatioSetup(
        ChartState ChartState,
        MetricState MetricState,
        DiffRatioChartController Controller,
        DiffRatioChartControllerAdapter Adapter,
        FakeCartesianMetricRenderingContract RenderingContract);

    private sealed record NormalizedSetup(
        ChartState ChartState,
        MetricState MetricState,
        NormalizedChartController Controller,
        NormalizedChartControllerAdapter Adapter,
        FakeCartesianMetricRenderingContract RenderingContract);

    private sealed class FakeCartesianMetricRenderingContract : ICartesianMetricChartRenderingContract
    {
        public object? LastRenderRequest { get; private set; }
        public int RenderCallCount { get; private set; }

        public IReadOnlyList<CartesianMetricBackendQualification> GetBackendQualificationMatrix()
        {
            return [];
        }

        public CartesianMetricRenderingCapabilities GetCapabilities(CartesianMetricChartRoute route)
        {
            return new CartesianMetricRenderingCapabilities("test", CartesianMetricRenderingQualification.Qualified, true, true, true, true, true, true);
        }

        public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
        {
            LastRenderRequest = request;
            RenderCallCount++;
            return Task.CompletedTask;
        }

        public void Clear(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
        }

        public void ResetView(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
        }

        public bool HasRenderableContent(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
            return false;
        }
    }

    private sealed class NoOpScope : IDisposable
    {
        public static NoOpScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
