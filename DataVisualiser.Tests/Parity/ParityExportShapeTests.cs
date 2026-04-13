using System.Reflection;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;

namespace DataVisualiser.Tests.Parity;

public sealed class ParityExportShapeTests
{
    [Fact]
    public void ParitySummarySnapshot_ShouldExposeExpectedProperties()
    {
        var summaryType = GetSnapshotType("ParitySummarySnapshot");
        var names = summaryType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expected = new[]
        {
                "Status",
                "OverallPassed",
                "WeeklyPassed",
                "HourlyPassed",
                "CombinedMetricPassed",
                "SingleMetricPassed",
                "MultiMetricPassed",
                "NormalizedPassed",
                "WeekdayTrendPassed",
                "TransformPassed",
                "StrategiesEvaluated"
        };

        foreach (var name in expected)
            Assert.Contains(name, names);
    }

    [Fact]
    public void BuildParityWarnings_ShouldWarn_WhenUnavailable()
    {
        var builderType = typeof(MainChartsEvidenceExportService).Assembly.GetType("DataVisualiser.UI.MainHost.Evidence.EvidenceParitySummaryBuilder");
        Assert.NotNull(builderType);

        var buildWarnings = builderType!.GetMethod("BuildWarnings", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(buildWarnings);

        var distribution = CreateSnapshot("DistributionParitySnapshot", "Unavailable", "No chart context available");
        var combined = CreateSnapshot("CombinedMetricParitySnapshot", "Completed", null);
        var simple = CreateSnapshot("SimpleParitySnapshot", "Completed", null);
        var transform = CreateSnapshot("TransformParitySnapshot", "Completed", null);

        var result = buildWarnings!.Invoke(null,
                new[]
                {
                        distribution,
                        combined,
                        simple,
                        simple,
                        simple,
                        simple,
                        transform,
                        1
                });

        var warnings = Assert.IsAssignableFrom<IReadOnlyList<string>>(result);
        Assert.NotEmpty(warnings);
    }

    private static Type GetSnapshotType(string name)
    {
        var type = typeof(MainChartsEvidenceExportService).Assembly.GetType($"DataVisualiser.UI.MainHost.Evidence.{name}");
        Assert.NotNull(type);
        return type!;
    }

    private static object CreateSnapshot(string typeName, string status, string? reason)
    {
        var type = GetSnapshotType(typeName);
        var instance = Activator.CreateInstance(type, true)!;

        type.GetProperty("Status")?.SetValue(instance, status);
        if (reason != null)
            type.GetProperty("Reason")?.SetValue(instance, reason);

        return instance;
    }
}
