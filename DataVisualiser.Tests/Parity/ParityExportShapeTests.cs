using System.Reflection;
using DataVisualiser.UI;

namespace DataVisualiser.Tests.Parity;

public sealed class ParityExportShapeTests
{
    [Fact]
    public void ParitySummarySnapshot_ShouldExposeExpectedProperties()
    {
        var summaryType = GetNestedType("ParitySummarySnapshot");
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
        var mainType = typeof(MainChartsView);
        var buildWarnings = mainType.GetMethod("BuildParityWarnings", BindingFlags.NonPublic | BindingFlags.Static);
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

    private static Type GetNestedType(string name)
    {
        var type = typeof(MainChartsView).GetNestedType(name, BindingFlags.NonPublic);
        Assert.NotNull(type);
        return type!;
    }

    private static object CreateSnapshot(string typeName, string status, string? reason)
    {
        var type = GetNestedType(typeName);
        var instance = Activator.CreateInstance(type, true)!;

        type.GetProperty("Status")?.SetValue(instance, status);
        if (reason != null)
            type.GetProperty("Reason")?.SetValue(instance, reason);

        return instance;
    }
}