using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;
using System.Windows;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class CartesianMetricControllerAdapterLifecycleTests
{
    [Fact]
    public void MainLifecycleMethods_ShouldDelegateToRenderingContract_WithMainRoute()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    IsMainVisible = true
            };
            var metricState = new MetricState();
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new MainChartController();
            var renderingContract = new FakeCartesianMetricRenderingContract
            {
                    HasRenderableContentResult = true
            };
            var adapter = new MainChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    metricService,
                    renderingContract);

            adapter.Clear(chartState);
            adapter.ResetZoom();
            var hasSeries = adapter.HasSeries(chartState);

            Assert.True(hasSeries);
            Assert.Equal(CartesianMetricChartRoute.Main, renderingContract.LastClearRoute);
            Assert.Equal(CartesianMetricChartRoute.Main, renderingContract.LastResetRoute);
            Assert.Equal(CartesianMetricChartRoute.Main, renderingContract.LastHasRenderableRoute);
            Assert.Same(controller.Chart, renderingContract.LastClearHost!.Chart);
            Assert.Same(chartState, renderingContract.LastClearHost.ChartState);
        });
    }

    [Fact]
    public void MainDisplayModeChanged_ToStacked_ShouldPopulateAndShowOverlaySubtypeCombo()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                    IsMainVisible = true,
                    MainChartDisplayMode = MainChartDisplayMode.Regular
            };
            var metricState = new MetricState();
            metricState.SetSeriesSelections(
            [
                    new MetricSeriesSelection("Weight", "A"),
                    new MetricSeriesSelection("Weight", "B")
            ]);
            var uiState = new UiState();
            var metricService = new MetricSelectionService("TestConnection");
            var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
            var controller = new MainChartController();
            var adapter = new MainChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    metricService,
                    new FakeCartesianMetricRenderingContract());

            controller.DisplayStackedRadio.IsChecked = true;
            adapter.OnDisplayModeChanged(null, EventArgs.Empty);

            Assert.Equal(MainChartDisplayMode.Stacked, chartState.MainChartDisplayMode);
            Assert.Equal(Visibility.Visible, controller.OverlaySubtypePanel.Visibility);
            Assert.True(controller.OverlaySubtypeCombo.IsEnabled);
            Assert.Equal(2, controller.OverlaySubtypeCombo.Items.Count);
            Assert.NotNull(chartState.SelectedStackedOverlaySeries);
            Assert.Contains(
                    chartState.SessionMilestones,
                    milestone => milestone.Kind == "MainChartDisplayModeChanged" &&
                                 milestone.Operation == MainChartDisplayMode.Stacked.ToString());
        });
    }

    [Fact]
    public async Task MainCreateRenderInputAsync_WithRegularMode_ShouldReturnExplicitMainInput()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var setup = CreateMainAdapter(MainChartDisplayMode.Regular);
            var context = CreateMainContext();

            var input = await setup.Adapter.CreateRenderInputAsync(context);

            Assert.NotNull(input);
            Assert.Same(context, input.Context);
            Assert.False(input.IsStacked);
            Assert.False(input.IsCumulative);
            Assert.Null(input.OverlaySeries);
            Assert.Equal(ChartProgramKind.Main, input.CapabilityContract.ProgramRequest.Kind);
        });
    }

    [Fact]
    public async Task MainRenderAsync_ShouldSendExplicitInputToCartesianRenderingContract()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var setup = CreateMainAdapter(MainChartDisplayMode.Summed);
            var context = CreateMainContext();

            await setup.Adapter.RenderAsync(context);

            Assert.NotNull(setup.RenderingContract.LastRenderRequest);
            Assert.Equal(CartesianMetricChartRoute.Main, setup.RenderingContract.LastRenderRequest.Route);
            Assert.Same(context, setup.RenderingContract.LastRenderRequest.Context);
            Assert.True(setup.RenderingContract.LastRenderRequest.IsCumulative);
            Assert.False(setup.RenderingContract.LastRenderRequest.IsStacked);
            Assert.Equal(ChartProgramKind.Main, setup.RenderingContract.LastRenderRequest.CapabilityContract!.ProgramRequest.Kind);
        });
    }

    [Fact]
    public void DiffRatioLifecycleMethods_ShouldDelegateToRenderingContract_WithDiffRatioRoute()
    {
        StaTestHelper.Run(() =>
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
            var renderingContract = new FakeCartesianMetricRenderingContract
            {
                    HasRenderableContentResult = true
            };
            var adapter = new DiffRatioChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    () => null,
                    renderingContract);

            adapter.Clear(chartState);
            adapter.ResetZoom();
            var hasSeries = adapter.HasSeries(chartState);

            Assert.True(hasSeries);
            Assert.Equal(CartesianMetricChartRoute.DiffRatio, renderingContract.LastClearRoute);
            Assert.Equal(CartesianMetricChartRoute.DiffRatio, renderingContract.LastResetRoute);
            Assert.Equal(CartesianMetricChartRoute.DiffRatio, renderingContract.LastHasRenderableRoute);
            Assert.Same(controller.Chart, renderingContract.LastClearHost!.Chart);
            Assert.Same(chartState, renderingContract.LastClearHost.ChartState);
        });
    }

    [Fact]
    public void NormalizedLifecycleMethods_ShouldDelegateToRenderingContract_WithNormalizedRoute()
    {
        StaTestHelper.Run(() =>
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
            var renderingContract = new FakeCartesianMetricRenderingContract
            {
                    HasRenderableContentResult = true
            };
            var adapter = new NormalizedChartControllerAdapter(
                    controller,
                    viewModel,
                    () => false,
                    () => NoOpScope.Instance,
                    metricService,
                    renderingContract);

            adapter.Clear(chartState);
            adapter.ResetZoom();
            var hasSeries = adapter.HasSeries(chartState);

            Assert.True(hasSeries);
            Assert.Equal(CartesianMetricChartRoute.Normalized, renderingContract.LastClearRoute);
            Assert.Equal(CartesianMetricChartRoute.Normalized, renderingContract.LastResetRoute);
            Assert.Equal(CartesianMetricChartRoute.Normalized, renderingContract.LastHasRenderableRoute);
            Assert.Same(controller.Chart, renderingContract.LastClearHost!.Chart);
            Assert.Same(chartState, renderingContract.LastClearHost.ChartState);
        });
    }

    private sealed class FakeCartesianMetricRenderingContract : ICartesianMetricChartRenderingContract
    {
        public CartesianMetricChartRoute? LastClearRoute { get; private set; }
        public CartesianMetricChartRenderHost? LastClearHost { get; private set; }
        public CartesianMetricChartRoute? LastResetRoute { get; private set; }
        public CartesianMetricChartRenderHost? LastResetHost { get; private set; }
        public CartesianMetricChartRoute? LastHasRenderableRoute { get; private set; }
        public CartesianMetricChartRenderHost? LastHasRenderableHost { get; private set; }
        public CartesianMetricChartRenderRequest? LastRenderRequest { get; private set; }
        public CartesianMetricChartRenderHost? LastRenderHost { get; private set; }
        public bool HasRenderableContentResult { get; set; }

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
            LastRenderHost = host;
            return Task.CompletedTask;
        }

        public void Clear(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
            LastClearRoute = route;
            LastClearHost = host;
        }

        public void ResetView(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
            LastResetRoute = route;
            LastResetHost = host;
        }

        public bool HasRenderableContent(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host)
        {
            LastHasRenderableRoute = route;
            LastHasRenderableHost = host;
            return HasRenderableContentResult;
        }
    }

    private sealed class NoOpScope : IDisposable
    {
        public static NoOpScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }

    private static MainChartSetup CreateMainAdapter(MainChartDisplayMode mode)
    {
        var chartState = new ChartState
        {
            IsMainVisible = true,
            MainChartDisplayMode = mode
        };
        var metricState = new MetricState
        {
            ResolutionTableName = "HealthMetrics"
        };
        metricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "A"),
            new MetricSeriesSelection("Weight", "B")
        ]);
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        var controller = new MainChartController();
        var renderingContract = new FakeCartesianMetricRenderingContract();
        var adapter = new MainChartControllerAdapter(
            controller,
            viewModel,
            () => false,
            metricService,
            renderingContract);

        return new MainChartSetup(adapter, renderingContract);
    }

    private static ChartDataContext CreateMainContext()
    {
        return new ChartDataContext
        {
            Data1 =
            [
                new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 1), Value = 1m },
                new MetricData { NormalizedTimestamp = new DateTime(2026, 1, 2), Value = 2m }
            ],
            DisplayName1 = "Weight",
            MetricType = "Weight",
            PrimaryMetricType = "Weight",
            PrimarySubtype = "A",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 2)
        };
    }

    private sealed record MainChartSetup(
        MainChartControllerAdapter Adapter,
        FakeCartesianMetricRenderingContract RenderingContract);
}
