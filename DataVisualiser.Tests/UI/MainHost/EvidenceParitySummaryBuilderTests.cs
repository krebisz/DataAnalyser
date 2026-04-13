using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class EvidenceParitySummaryBuilderTests
{
    [Fact]
    public void BuildSummary_WhenAllParityChecksPass_ReturnsOverallPassed()
    {
        var distribution = new DistributionParitySnapshot
        {
            Status = "Completed",
            Weekly = new ParityResultSnapshot { Passed = true },
            Hourly = new ParityResultSnapshot { Passed = true }
        };
        var combined = new CombinedMetricParitySnapshot { Result = new ParityResultSnapshot { Passed = true } };
        var single = new SimpleParitySnapshot { Result = new ParityResultSnapshot { Passed = true } };
        var multi = new SimpleParitySnapshot { Result = new ParityResultSnapshot { Passed = true } };
        var normalized = new SimpleParitySnapshot { Result = new ParityResultSnapshot { Passed = true } };
        var weekday = new SimpleParitySnapshot { Result = new ParityResultSnapshot { Passed = true } };
        var transform = new TransformParitySnapshot { Result = new ParityResultSnapshot { Passed = true } };

        var summary = EvidenceParitySummaryBuilder.BuildSummary(distribution, combined, single, multi, normalized, weekday, transform);

        Assert.True(summary.OverallPassed);
        Assert.Equal(8, summary.StrategiesEvaluated.Length);
    }

    [Fact]
    public void BuildWarnings_WhenParityUnavailable_AddsUnavailableWarnings()
    {
        var warnings = EvidenceParitySummaryBuilder.BuildWarnings(
            new DistributionParitySnapshot { Status = "Unavailable", Reason = "No context" },
            new CombinedMetricParitySnapshot { Status = "CmsUnavailable", Reason = "Missing CMS" },
            new SimpleParitySnapshot { Status = "Completed" },
            new SimpleParitySnapshot { Status = "Unavailable", Reason = "Need more series" },
            new SimpleParitySnapshot { Status = "Completed" },
            new SimpleParitySnapshot { Status = "Completed" },
            new TransformParitySnapshot { Status = "Unavailable", Reason = "No operation" },
            selectedSeriesCount: 1);

        Assert.Contains(warnings, warning => warning.Contains("WeeklyDistribution parity not completed", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.Contains("CombinedMetric parity not completed", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.Contains("MultiMetric parity not completed", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.Contains("Transform parity not completed", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.Contains("Multiple series required", StringComparison.Ordinal));
    }
}
