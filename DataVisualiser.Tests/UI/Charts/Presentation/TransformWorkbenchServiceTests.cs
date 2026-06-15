using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Export;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;
using System.IO;
using System.Text.Json;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformWorkbenchServiceTests
{
    [Fact]
    public async Task LoadAsync_ShouldLoadIndependentMetricSeriesIntoInputGrids()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);
        var series = new[]
        {
            new MetricSeriesRequest("Weight", "body_fat_mass", "Weight", "Fat"),
            new MetricSeriesRequest("Steps", "count", "Steps", "Count")
        };

        var result = await service.LoadAsync(
            series,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        Assert.Equal("Mixed", result.Snapshot.Request.MetricType);
        Assert.Equal(2, result.Inputs.Count);
        Assert.Equal("Weight - Fat", result.Inputs[0].Title);
        Assert.Equal("Steps - Count", result.Inputs[1].Title);
        Assert.Equal("2026-01-01 00:00:00", result.Inputs[0].Rows[0].Timestamp);
        Assert.Equal("1.2500", result.Inputs[0].Rows[0].Value);
        Assert.Equal("2 input series loaded for Operation Chain.", result.Summary);
    }

    [Fact]
    public async Task ComputeAsync_ShouldReturnSingleResultGridForBinaryOperation()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);

        var result = await service.ComputeAsync(
            [
                new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"),
                new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics",
            "Subtract");

        Assert.Equal("Fat - Muscle", result.Title);
        Assert.Equal(5, result.Rows.Count);
        Assert.Equal("-10.0000", result.Rows[0].Raw);
        Assert.Equal("Fat - Muscle: 5 result points computed.", result.Summary);
        Assert.NotNull(result.Evidence);
    }

    [Fact]
    public async Task ComputeAsync_ShouldLoadInputsForDefaultOperation()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);

        var result = await service.ComputeAsync(
            [new MetricSeriesRequest("Weight", "fat", "Weight", "Fat")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics",
            "None");

        Assert.Equal("Input Series", result.Title);
        Assert.NotNull(result.Inputs);
        var input = Assert.Single(result.Inputs);
        Assert.Equal("Weight - Fat", input.Title);
        Assert.Equal("Weight - Fat", result.Rows[0].Series);
        Assert.Equal("1.2500", result.Rows[0].Raw);
        Assert.NotNull(result.InputSnapshot);
    }

    [Fact]
    public async Task ComputeAsync_ShouldSupportUnaryOperationWithSingleInput()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);

        var result = await service.ComputeAsync(
            [new MetricSeriesRequest("Weight", "fat", "Weight", "Fat")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics",
            "Sqrt");

        Assert.Equal("Sqrt(Fat)", result.Title);
        Assert.Equal("1.1180", result.Rows[0].Raw);
        Assert.NotNull(result.Evidence);
    }

    [Fact]
    public async Task ComputeAsync_ShouldSupportTernarySum()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);

        var result = await service.ComputeAsync(
            [
                new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"),
                new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle"),
                new MetricSeriesRequest("Weight", "bone", "Weight", "Bone")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics",
            "Sum3");

        Assert.Equal("Fat + Muscle + Bone", result.Title);
        Assert.Equal("33.7500", result.Rows[0].Raw);
    }

    [Fact]
    public async Task ComputeAsync_ShouldExecuteCompiledEquationSteps()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);
        var inputs = new[]
        {
            new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"),
            new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle"),
            new MetricSeriesRequest("Weight", "bone", "Weight", "Bone")
        };
        var compiled = TransformEquationCompiler.Compile(
            [
                new TransformEquationTerm("None", "None", 0, "Fat"),
                new TransformEquationTerm("Add", "Add", 1, "Muscle"),
                new TransformEquationTerm("Divide", "Divide", 2, "Bone")
            ],
            inputs.Length);

        var result = await service.ComputeAsync(
            inputs,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics",
            compiled.Steps,
            compiled.Title);

        Assert.Equal("Fat + Muscle / Bone", result.Title);
        Assert.Equal("0.5882", result.Rows[0].Raw);
        Assert.NotNull(result.Evidence);
    }

    [Fact]
    public async Task ComputeAsync_ShouldCorrelateTwoCanonicalInputsWithConfidenceInterval()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);

        var result = await service.ComputeAsync(
            [
                new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"),
                new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 5),
            "HealthMetrics",
            "Correlation");

        Assert.Equal("Weight - Fat ~ Weight - Muscle", result.Title);
        Assert.Equal("Correlation", result.Rows[0].Timestamp);
        Assert.Equal("1.0000", result.Rows[0].Raw);
        Assert.Equal("Sample count", result.Rows[3].Timestamp);
        Assert.Equal("5", result.Rows[3].Raw);
        Assert.Contains("95% CI", result.Summary);
        Assert.NotNull(result.Correlation);
        Assert.Equal("Weight - Fat", result.Correlation.SourceLabel);
        Assert.Equal("Weight - Muscle", result.Correlation.TargetLabel);
    }

    [Fact]
    public async Task ComputeAsync_ShouldCorrelateTernaryDerivedSetWithFourthInput()
    {
        var loader = new StubMetricSeriesLoader();
        var service = new TransformWorkbenchService(loader);

        var result = await service.ComputeAsync(
            [
                new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"),
                new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle"),
                new MetricSeriesRequest("Weight", "bone", "Weight", "Bone"),
                new MetricSeriesRequest("Weight", "target", "Weight", "Target")
            ],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 5),
            "HealthMetrics",
            "Sum3Correlation");

        Assert.Equal("Correlation: (Fat + Muscle + Bone) ~ Target", result.Title);
        Assert.Equal("1.0000", result.Rows[0].Raw);
        Assert.Equal("5", result.Rows[3].Raw);
        Assert.Contains("Fat + Muscle + Bone", result.Evidence);
    }

    [Fact]
    public void Build_ShouldProjectSnapshotWithoutExecutingOperationChain()
    {
        var selection = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "body_fat_mass", "Weight", "Fat")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");
        var snapshot = new MetricLoadSnapshot(
            selection,
            [new MetricSeriesSnapshot(selection.Series[0], CreateData([1m, 2m]), null)],
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = TransformInputGridPresenter.Build(snapshot);

        var input = Assert.Single(result.Inputs);
        Assert.Equal("Weight - Fat", input.Title);
        Assert.Equal(2, input.Rows.Count);
        Assert.Equal("2.0000", input.Rows[1].Value);
    }

    [Fact]
    public async Task ResolveAsync_ShouldAggregateDateRangeAcrossIndependentInputs()
    {
        var resolver = new TransformInputDateRangeResolver((selection, _) =>
        {
            var range = selection.MetricType switch
            {
                "Weight" => (new DateTime(2026, 1, 10), new DateTime(2026, 2, 1)),
                "Steps" => (new DateTime(2026, 1, 1), new DateTime(2026, 1, 20)),
                _ => ((DateTime, DateTime)?)null
            };

            return Task.FromResult< (DateTime MinDate, DateTime MaxDate)? >(range);
        });

        var result = await resolver.ResolveAsync(
            [
                new MetricSeriesRequest("Weight", "body_fat_mass"),
                new MetricSeriesRequest("Steps", "count")
            ],
            "HealthMetrics");

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2026, 1, 1), result.From);
        Assert.Equal(new DateTime(2026, 2, 1), result.To);
    }

    [Fact]
    public void Export_ShouldWriteOperationChainEvidencePayload()
    {
        var target = Path.Combine(Path.GetTempPath(), $"operation-chain-export-{Guid.NewGuid():N}");
        try
        {
            var service = new TransformEvidenceExportService(
                new EvidenceExportWriter(),
                new StubPathResolver(target));
            var snapshot = new TransformEvidenceExportSnapshot(
                [new MetricSeriesRequest("Weight", "fat", "Weight", "Fat")],
                "Correlation",
                "Correlation",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 5),
                "HealthMetrics",
                new TransformComputationResult(
                    "Weight - Fat ~ Weight - Muscle",
                    [new TransformResultGridRow("Correlation", "1.0000", string.Empty)],
                    "Correlation computed.")
                {
                    Evidence = "Correlation source: snapshot"
                });

            var result = service.Export(snapshot, new DateTime(2026, 5, 12, 10, 30, 0, DateTimeKind.Utc));

            Assert.EndsWith("operation-chain-20260512-103000.json", result.FilePath, StringComparison.OrdinalIgnoreCase);
            using var json = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var root = json.RootElement;
            Assert.Equal("OperationChain", root.GetProperty("ExportScope").GetString());
            var diagnostics = root.GetProperty("Diagnostics");
            Assert.Equal("Correlation", diagnostics.GetProperty("OperationTag").GetString());
            Assert.Equal("HealthMetrics", diagnostics.GetProperty("ResolutionTableName").GetString());
            Assert.Equal(1, diagnostics.GetProperty("ResultRowCount").GetInt32());
            Assert.Equal("Correlation source: snapshot", diagnostics.GetProperty("Evidence").GetString());
            Assert.True(diagnostics.GetProperty("Inputs")[0].GetProperty("IsConsumed").GetBoolean());
            Assert.Equal(0, diagnostics.GetProperty("Inputs")[0].GetProperty("Index").GetInt32());
            var coverage = diagnostics.GetProperty("EvidenceCoverage");
            Assert.Equal(1, coverage.GetProperty("InputCount").GetInt32());
            Assert.Equal(1, coverage.GetProperty("ConsumedInputCount").GetInt32());
            Assert.False(coverage.GetProperty("ComputationEvidencePresent").GetBoolean());
            Assert.True(coverage.GetProperty("ResultEvidencePresent").GetBoolean());
        }
        finally
        {
            if (Directory.Exists(target))
                Directory.Delete(target, recursive: true);
        }
    }

    [Fact]
    public void Export_ShouldCreateOperationChainEvidenceInsideLogsDirectory()
    {
        var target = Path.Combine(Path.GetTempPath(), $"operation-chain-export-{Guid.NewGuid():N}", "documents", "logs");
        try
        {
            var service = new TransformEvidenceExportService(
                new EvidenceExportWriter(),
                new StubPathResolver(target));
            var snapshot = new TransformEvidenceExportSnapshot(
                [new MetricSeriesRequest("Weight", "fat", "Weight", "Fat")],
                "None",
                "None",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 5),
                "HealthMetrics",
                new TransformComputationResult(
                    "Input Series",
                    [new TransformResultGridRow("2026-01-01 00:00:00", "1.0000", string.Empty)],
                    "Loaded."));

            var result = service.Export(snapshot, new DateTime(2026, 5, 12, 10, 30, 0, DateTimeKind.Utc));

            Assert.Equal(target, Path.GetDirectoryName(result.FilePath));
            Assert.True(Directory.Exists(target));
            Assert.True(File.Exists(result.FilePath));
        }
        finally
        {
            var root = Directory.GetParent(Directory.GetParent(target)!.FullName)!.FullName;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Export_ShouldWriteComputedOperationChainTraceEvidence()
    {
        var target = Path.Combine(Path.GetTempPath(), $"operation-chain-export-{Guid.NewGuid():N}");
        try
        {
            var loader = new StubMetricSeriesLoader();
            var computeService = new TransformWorkbenchService(loader);
            var exportService = new TransformEvidenceExportService(
                new EvidenceExportWriter(),
                new StubPathResolver(target));
            var inputs = new[]
            {
                new MetricSeriesRequest("Weight", "fat", "Weight", "Fat"),
                new MetricSeriesRequest("Weight", "muscle", "Weight", "Muscle"),
                new MetricSeriesRequest("Weight", "bone", "Weight", "Bone")
            };
            var computed = await computeService.ComputeAsync(
                inputs,
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 5),
                "HealthMetrics",
                "Add");
            var snapshot = new TransformEvidenceExportSnapshot(
                inputs,
                "Add",
                "Add",
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 5),
                "HealthMetrics",
                computed);

            var result = exportService.Export(snapshot, new DateTime(2026, 5, 12, 10, 30, 0, DateTimeKind.Utc));

            using var json = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var diagnostics = json.RootElement.GetProperty("Diagnostics");
            Assert.Equal("Sum", diagnostics.GetProperty("OperationKind").GetString());
            Assert.Equal("sum", diagnostics.GetProperty("OperationId").GetString());
            Assert.Equal([0, 1], diagnostics.GetProperty("ConsumedInputIndexes").EnumerateArray().Select(item => item.GetInt32()).ToArray());
            Assert.False(diagnostics.GetProperty("Inputs")[2].GetProperty("IsConsumed").GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(diagnostics.GetProperty("SourceSignature").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(diagnostics.GetProperty("PlanSignature").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(diagnostics.GetProperty("TraceSignature").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(diagnostics.GetProperty("ContractSignature").GetString()));
            Assert.NotEmpty(diagnostics.GetProperty("DerivedDatasetIds").EnumerateArray());
            Assert.Contains("OperationChain trace:", diagnostics.GetProperty("Evidence").GetString());
        }
        finally
        {
            if (Directory.Exists(target))
                Directory.Delete(target, recursive: true);
        }
    }

    private static IReadOnlyList<MetricData> CreateData(IReadOnlyList<decimal> values)
    {
        return values
            .Select((value, index) => new MetricData
            {
                NormalizedTimestamp = new DateTime(2026, 1, 1).AddDays(index),
                Value = value
            })
            .ToArray();
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
            var offset = request.Subtype switch
            {
                "muscle" => 10m,
                "bone" => 20m,
                "target" => 30m,
                _ when string.Equals(request.MetricType, "Steps", StringComparison.OrdinalIgnoreCase) => 10m,
                _ => 0m
            };
            return Task.FromResult(new LoadedMetricSeries(
                CreateData([1.25m + offset, 2.5m + offset, 3.75m + offset, 5m + offset, 6.25m + offset]),
                CanonicalSeries: null));
        }
    }

    private sealed class StubPathResolver(string targetDirectory) : IEvidenceExportPathResolver
    {
        public string ResolveDocumentsDirectory() => targetDirectory;
    }
}
