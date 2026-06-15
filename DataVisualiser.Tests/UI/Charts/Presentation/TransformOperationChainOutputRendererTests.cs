using System.Windows;
using System.Windows.Media;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformOperationChainOutputRendererTests
{
    [Fact]
    public async Task RenderAsync_ShouldUseTransformContractForDerivedDataset()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var contract = new FakeTransformRenderingContract();
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), contract);
            var chart = new CartesianChart();

            await renderer.RenderAsync(chart, CreateResult());

            Assert.Equal(1, contract.RenderCalls);
            Assert.Equal(TransformRenderingRoute.ResultCartesian, contract.LastRequest!.Route);
            Assert.Equal("Total", contract.LastRequest.PrimaryLabel);
            Assert.Equal("+", contract.LastRequest.OperationType);
            Assert.True(contract.LastRequest.IsOperationChart);
        });
    }

    [Fact]
    public async Task RenderAsync_ShouldFilterDerivedDatasetRowsForChart()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var contract = new FakeTransformRenderingContract();
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), contract);
            var result = new TransformOperationChainComputationGridResult(CreateResult(), "Total", [], "computed");

            await renderer.RenderAsync(new CartesianChart(), result, [false, true]);

            var computed = contract.LastRequest!.Strategy.Compute();
            Assert.NotNull(computed);
            Assert.Single(computed!.Timestamps);
            Assert.Equal(new DateTime(2026, 1, 2), computed.Timestamps[0]);
            Assert.Equal(6d, computed.PrimaryRawValues[0]);
        });
    }

    [Fact]
    public async Task RenderAsync_ShouldClearChartWhenAllDerivedRowsAreExcluded()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var contract = new FakeTransformRenderingContract();
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), contract);
            var result = new TransformOperationChainComputationGridResult(CreateResult(), "Total", [], "computed");

            await renderer.RenderAsync(new CartesianChart(), result, [false, false]);

            Assert.Equal(1, contract.ClearCalls);
            Assert.Equal(0, contract.RenderCalls);
        });
    }

    [Fact]
    public async Task RenderAsync_ShouldUseTransformContractForInputSnapshot()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var contract = new FakeTransformRenderingContract();
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), contract);
            var selection = new MetricSelectionRequest(
                "Weight",
                [new MetricSeriesRequest("Weight", "fat", "Weight", "Fat")],
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2),
                "HealthMetrics");
            var snapshot = new MetricLoadSnapshot(
                selection,
                [new MetricSeriesSnapshot(selection.Series[0], CreateData([1m, 2m]), null)],
                DateTime.UtcNow);
            var result = new TransformOperationChainComputationGridResult(null, "Input Series", [], "loaded")
            {
                InputSnapshot = snapshot
            };

            await renderer.RenderAsync(new CartesianChart(), result);

            Assert.Equal(1, contract.RenderCalls);
            Assert.Equal("Input Series", contract.LastRequest!.PrimaryLabel);
            Assert.False(contract.LastRequest.IsOperationChart);
        });
    }

    [Fact]
    public async Task RenderAsync_ShouldClearChartWhenAllInputSnapshotRowsAreExcluded()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var contract = new FakeTransformRenderingContract();
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), contract);
            var selection = new MetricSelectionRequest(
                "Weight",
                [new MetricSeriesRequest("Weight", "fat", "Weight", "Fat")],
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 2),
                "HealthMetrics");
            var snapshot = new MetricLoadSnapshot(
                selection,
                [new MetricSeriesSnapshot(selection.Series[0], CreateData([1m, 2m]), null)],
                DateTime.UtcNow);
            var result = new TransformOperationChainComputationGridResult(null, "Input Series", [], "loaded")
            {
                InputSnapshot = snapshot
            };

            await renderer.RenderAsync(new CartesianChart(), result, [false, false]);

            Assert.Equal(1, contract.ClearCalls);
            Assert.Equal(0, contract.RenderCalls);
        });
    }

    [Fact]
    public async Task ClearAsync_ShouldDelegateToTransformContract()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var contract = new FakeTransformRenderingContract();
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), contract);

            await renderer.ClearAsync(new CartesianChart());

            Assert.Equal(1, contract.ClearCalls);
        });
    }

    [Fact]
    public void ResetZoom_ShouldDelegateToTransformContract()
    {
        StaTestHelper.Run(() =>
        {
            var contract = new FakeTransformRenderingContract();
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), contract);

            renderer.ResetZoom(new CartesianChart());

            Assert.Equal(1, contract.ResetViewCalls);
        });
    }

    [Fact]
    public async Task ClearAsync_ShouldApplySharedThemeBrushesToEmptyChartAxes()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var app = Application.Current ?? new Application();
            app.Resources["ThemeChartAxisBrush"] = Brushes.White;
            app.Resources["ThemeChartGridLineBrush"] = Brushes.DimGray;
            var renderer = new TransformOperationChainOutputRenderer(new ChartState(), new FakeTransformRenderingContract());
            var chart = new CartesianChart();

            await renderer.ClearAsync(chart);

            Assert.Same(Brushes.White, chart.AxisX[0].Foreground);
            Assert.Same(Brushes.DimGray, chart.AxisX[0].Separator.Stroke);
            Assert.Same(Brushes.White, chart.AxisY[0].Foreground);
            Assert.Same(Brushes.DimGray, chart.AxisY[0].Separator.Stroke);
        });
    }

    private static OperationChainResult CreateResult()
    {
        var selection = new MetricSelectionRequest(
            "Weight",
            [
                new MetricSeriesRequest("Weight", "morning"),
                new MetricSeriesRequest("Weight", "evening")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var step = OperationChainStep.Lossless(SeriesOperationRequest.Sum([0, 1], "Total"));
        var request = new OperationChainRequest(selection, [step], title: "Workbench");
        var program = new OperationChainProgram(
            request.Signature,
            request.Title,
            request.Selection,
            request.Steps,
            request.Delivery);
        var plan = new OperationChainExecutionPlan(
            program,
            "source-sig",
            ["Weight:morning", "Weight:evening"],
            [step.Operation]);
        var dataset = new DerivedDataset(
            "sum",
            "Total",
            [new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)],
            [3d, 6d],
            [3d, 4.5d],
            ["Weight:morning", "Weight:evening"],
            "Sum:sum:0,1:",
            new Dictionary<string, string>());
        var trace = new OperationChainTrace(
        [
            new OperationChainTraceEntry(
                0,
                SeriesOperationKind.Sum,
                [0, 1],
                dataset.Id,
                Reversible: true,
                Lossiness: "Lossless",
                Metadata: new Dictionary<string, string>())
        ]);
        var evidence = new OperationChainEvidence(
            "source-sig",
            "plan-sig",
            "trace-sig",
            "contract-sig",
            plan.SourceSeriesSignatures,
            [dataset.Id],
            new Dictionary<string, string>());

        return new OperationChainResult(
            request,
            plan,
            [dataset],
            trace,
            evidence,
            null!);
    }

    private static IReadOnlyList<MetricData> CreateData(IReadOnlyList<decimal> values) =>
        values
            .Select((value, index) => new MetricData
            {
                NormalizedTimestamp = new DateTime(2026, 1, 1).AddDays(index),
                Value = value
            })
            .ToArray();

    private sealed class FakeTransformRenderingContract : ITransformRenderingContract
    {
        public int RenderCalls { get; private set; }
        public int ClearCalls { get; private set; }
        public int ResetViewCalls { get; private set; }
        public TransformChartRenderRequest? LastRequest { get; private set; }

        public IReadOnlyList<TransformBackendQualification> GetBackendQualificationMatrix() => [];

        public TransformRenderingCapabilities GetCapabilities(TransformRenderingRoute route) =>
            new("fake", TransformRenderingQualification.Qualified, true, true, true, true, true, true);

        public Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host)
        {
            RenderCalls++;
            LastRequest = request;
            return Task.CompletedTask;
        }

        public void Clear(TransformRenderingRoute route, TransformChartRenderHost host)
        {
            ClearCalls++;
        }

        public void ResetView(TransformRenderingRoute route, TransformChartRenderHost host)
        {
            ResetViewCalls++;
        }

        public bool HasRenderableContent(TransformRenderingRoute route, TransformChartRenderHost host) => false;
    }
}
