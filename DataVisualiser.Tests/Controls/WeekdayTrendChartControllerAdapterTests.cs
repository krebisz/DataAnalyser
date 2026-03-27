using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Validation;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.Controls;

public sealed class WeekdayTrendChartControllerAdapterTests
{
    [Fact]
    public void UpdateChartTypeVisibility_ShouldReflectCurrentChartMode()
    {
        StaTestHelper.Run(() =>
        {
            var setup = CreateAdapter(WeekdayTrendChartMode.Cartesian, isVisible: true);

            setup.ChartState.WeekdayTrendChartMode = WeekdayTrendChartMode.Cartesian;
            setup.Adapter.UpdateChartTypeVisibility();
            Assert.Equal(System.Windows.Visibility.Visible, setup.Controller.Chart.Visibility);
            Assert.Equal(System.Windows.Visibility.Collapsed, setup.Controller.PolarChart.Visibility);
            Assert.Equal("Polar", setup.Controller.ChartTypeToggleButton.Content);

            setup.ChartState.WeekdayTrendChartMode = WeekdayTrendChartMode.Polar;
            setup.Adapter.UpdateChartTypeVisibility();
            Assert.Equal(System.Windows.Visibility.Collapsed, setup.Controller.Chart.Visibility);
            Assert.Equal(System.Windows.Visibility.Visible, setup.Controller.PolarChart.Visibility);
            Assert.Equal("Scatter", setup.Controller.ChartTypeToggleButton.Content);

            setup.ChartState.WeekdayTrendChartMode = WeekdayTrendChartMode.Scatter;
            setup.Adapter.UpdateChartTypeVisibility();
            Assert.Equal(System.Windows.Visibility.Visible, setup.Controller.Chart.Visibility);
            Assert.Equal(System.Windows.Visibility.Collapsed, setup.Controller.PolarChart.Visibility);
            Assert.Equal("Cartesian", setup.Controller.ChartTypeToggleButton.Content);
        });
    }

    [Fact]
    public void ResetZoom_AndHasSeries_ShouldUseResolvedPolarRoute()
    {
        StaTestHelper.Run(() =>
        {
            var setup = CreateAdapter(WeekdayTrendChartMode.Polar, isVisible: true);
            setup.RenderingContract.RouteWithContent = WeekdayTrendRenderingRoute.Polar;

            setup.Adapter.ResetZoom();
            var hasSeries = setup.Adapter.HasSeries(setup.ChartState);

            Assert.True(hasSeries);
            Assert.Equal(WeekdayTrendRenderingRoute.Polar, setup.RenderingContract.LastResetRoute);
            Assert.Equal(WeekdayTrendRenderingRoute.Polar, setup.RenderingContract.LastHasRenderableRoute);
        });
    }

    [Fact]
    public void OnChartTypeToggleRequested_ShouldToggleMode_AndRenderLastContext()
    {
        StaTestHelper.Run(() =>
        {
            var setup = CreateAdapter(WeekdayTrendChartMode.Cartesian, isVisible: true, isInitializing: false);
            setup.ChartState.LastContext = CreateContext();

            setup.Adapter.OnChartTypeToggleRequested(null, EventArgs.Empty);

            Assert.Equal(WeekdayTrendChartMode.Polar, setup.ChartState.WeekdayTrendChartMode);
            Assert.NotNull(setup.RenderingContract.LastRenderRequest);
            Assert.Equal(WeekdayTrendRenderingRoute.Polar, setup.RenderingContract.LastRenderRequest!.Route);
            Assert.Equal("Scatter", setup.Controller.ChartTypeToggleButton.Content);
            Assert.Equal(1, setup.CutOverService.CreateStrategyCallCount);
            Assert.Equal(1, setup.CutOverService.Strategy.ComputeCallCount);
        });
    }

    [Fact]
    public void UpdateSubtypeOptions_ShouldPopulateCombo_AndPersistSelectionDuringInitialization()
    {
        StaTestHelper.Run(() =>
        {
            var setup = CreateAdapter(WeekdayTrendChartMode.Cartesian, isVisible: true, isInitializing: true);
            var primary = new MetricSeriesSelection("weight", "avg");
            var secondary = new MetricSeriesSelection("steps", "(All)");
            setup.MetricState.SetSeriesSelections([primary, secondary]);

            setup.Adapter.UpdateSubtypeOptions();

            Assert.Equal(2, setup.Controller.SubtypeCombo.Items.Count);
            Assert.Equal(primary, setup.ChartState.SelectedWeekdayTrendSeries);
            Assert.True(setup.Controller.SubtypeCombo.IsEnabled);
        });
    }

    private static TestSetup CreateAdapter(WeekdayTrendChartMode mode, bool isVisible, bool isInitializing = false)
    {
        var chartState = new ChartState
        {
            IsWeeklyTrendVisible = isVisible,
            WeekdayTrendChartMode = mode
        };
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("TestConnection");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        var controller = new WeekdayTrendChartController();
        var renderingContract = new FakeWeekdayTrendRenderingContract();
        var cutOverService = new FakeStrategyCutOverService();
        var adapter = new WeekdayTrendChartControllerAdapter(
            controller,
            viewModel,
            () => isInitializing,
            () => NoOpScope.Instance,
            metricService,
            () => cutOverService,
            renderingContract);

        adapter.InitializeControls();
        adapter.UpdateChartTypeVisibility();

        return new TestSetup(chartState, metricState, controller, adapter, renderingContract, cutOverService);
    }

    private static ChartDataContext CreateContext()
    {
        var data = new List<MetricData>
        {
            new()
            {
                NormalizedTimestamp = new DateTime(2026, 1, 5),
                Value = 2m,
                Unit = "kg"
            }
        };

        return new ChartDataContext
        {
            Data1 = data,
            DisplayName1 = "Weight",
            MetricType = "weight",
            PrimaryMetricType = "weight",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7)
        };
    }

    private static WeekdayTrendResult CreateResult()
    {
        var result = new WeekdayTrendResult
        {
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7),
            GlobalMin = 1d,
            GlobalMax = 5d,
            Unit = "kg"
        };

        result.SeriesByDay[0] = new WeekdayTrendSeries
        {
            Day = DayOfWeek.Monday,
            Points =
            [
                new WeekdayTrendPoint
                {
                    Date = new DateTime(2026, 1, 5),
                    Value = 2d
                }
            ]
        };

        return result;
    }

    private sealed record TestSetup(
        ChartState ChartState,
        MetricState MetricState,
        WeekdayTrendChartController Controller,
        WeekdayTrendChartControllerAdapter Adapter,
        FakeWeekdayTrendRenderingContract RenderingContract,
        FakeStrategyCutOverService CutOverService);

    private sealed class FakeWeekdayTrendRenderingContract : IWeekdayTrendRenderingContract
    {
        public WeekdayTrendRenderingRoute? LastResetRoute { get; private set; }
        public WeekdayTrendRenderingRoute? LastHasRenderableRoute { get; private set; }
        public WeekdayTrendChartRenderRequest? LastRenderRequest { get; private set; }
        public WeekdayTrendRenderingRoute RouteWithContent { get; set; } = WeekdayTrendRenderingRoute.Cartesian;

        public IReadOnlyList<WeekdayTrendBackendQualification> GetBackendQualificationMatrix()
        {
            return [];
        }

        public WeekdayTrendRenderingCapabilities GetCapabilities(WeekdayTrendRenderingRoute route)
        {
            return new WeekdayTrendRenderingCapabilities("test", WeekdayTrendRenderingQualification.Qualified, true, true, true, true, true);
        }

        public void Render(WeekdayTrendChartRenderRequest request, WeekdayTrendChartRenderHost host)
        {
            LastRenderRequest = request;
        }

        public void Clear(WeekdayTrendChartRenderHost host)
        {
        }

        public void ResetView(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host)
        {
            LastResetRoute = route;
        }

        public bool HasRenderableContent(WeekdayTrendRenderingRoute route, WeekdayTrendChartRenderHost host)
        {
            LastHasRenderableRoute = route;
            return route == RouteWithContent;
        }
    }

    private sealed class FakeStrategyCutOverService : IStrategyCutOverService
    {
        public FakeWeekdayTrendStrategy Strategy { get; } = new(CreateResult());
        public int CreateStrategyCallCount { get; private set; }

        public IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
        {
            CreateStrategyCallCount++;
            return Strategy;
        }

        public IChartComputationStrategy CreateCmsStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
        {
            throw new NotSupportedException();
        }

        public IChartComputationStrategy CreateLegacyStrategy(StrategyType strategyType, StrategyCreationParameters parameters)
        {
            throw new NotSupportedException();
        }

        public bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx)
        {
            return false;
        }

        public ParityResult ValidateParity(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeWeekdayTrendStrategy : IChartComputationStrategy, IWeekdayTrendResultProvider
    {
        public FakeWeekdayTrendStrategy(WeekdayTrendResult result)
        {
            ExtendedResult = result;
        }

        public int ComputeCallCount { get; private set; }
        public string PrimaryLabel => "Weight";
        public string SecondaryLabel => string.Empty;
        public string? Unit => "kg";
        public WeekdayTrendResult? ExtendedResult { get; }

        public ChartComputationResult? Compute()
        {
            ComputeCallCount++;
            return new ChartComputationResult();
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
