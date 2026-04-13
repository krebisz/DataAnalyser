namespace DataVisualiser.UI.MainHost;

internal static class EvidenceParitySummaryBuilder
{
    internal static ParitySummarySnapshot BuildSummary(
        DistributionParitySnapshot distributionSnapshot,
        CombinedMetricParitySnapshot combinedSnapshot,
        SimpleParitySnapshot singleSnapshot,
        SimpleParitySnapshot multiSnapshot,
        SimpleParitySnapshot normalizedSnapshot,
        SimpleParitySnapshot weekdayTrendSnapshot,
        TransformParitySnapshot transformSnapshot)
    {
        var weeklyPassed = distributionSnapshot.Weekly?.Passed;
        var hourlyPassed = distributionSnapshot.Hourly?.Passed;
        var combinedPassed = combinedSnapshot.Result?.Passed;
        var singlePassed = singleSnapshot.Result?.Passed;
        var multiPassed = multiSnapshot.Result?.Passed;
        var normalizedPassed = normalizedSnapshot.Result?.Passed;
        var weekdayTrendPassed = weekdayTrendSnapshot.Result?.Passed;
        var transformPassed = transformSnapshot.Result?.Passed;
        var completed = string.Equals(distributionSnapshot.Status, "Completed", StringComparison.OrdinalIgnoreCase);

        return new ParitySummarySnapshot
        {
            Status = distributionSnapshot.Status,
            WeeklyPassed = weeklyPassed,
            HourlyPassed = hourlyPassed,
            CombinedMetricPassed = combinedPassed,
            SingleMetricPassed = singlePassed,
            MultiMetricPassed = multiPassed,
            NormalizedPassed = normalizedPassed,
            WeekdayTrendPassed = weekdayTrendPassed,
            TransformPassed = transformPassed,
            OverallPassed = completed
                            && weeklyPassed == true
                            && hourlyPassed == true
                            && combinedPassed != false
                            && singlePassed != false
                            && multiPassed != false
                            && normalizedPassed != false
                            && weekdayTrendPassed != false
                            && transformPassed != false,
            StrategiesEvaluated =
            [
                "WeeklyDistribution",
                "HourlyDistribution",
                "CombinedMetric",
                "SingleMetric",
                "MultiMetric",
                "Normalized",
                "WeekdayTrend",
                "Transform"
            ]
        };
    }

    internal static IReadOnlyList<string> BuildWarnings(
        DistributionParitySnapshot distributionSnapshot,
        CombinedMetricParitySnapshot combinedSnapshot,
        SimpleParitySnapshot singleSnapshot,
        SimpleParitySnapshot multiSnapshot,
        SimpleParitySnapshot normalizedSnapshot,
        SimpleParitySnapshot weekdayTrendSnapshot,
        TransformParitySnapshot transformSnapshot,
        int selectedSeriesCount)
    {
        var warnings = new List<string>();
        AddWarningIfUnavailable(warnings, "WeeklyDistribution", distributionSnapshot.Status, distributionSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "CombinedMetric", combinedSnapshot.Status, combinedSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "SingleMetric", singleSnapshot.Status, singleSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "MultiMetric", multiSnapshot.Status, multiSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "Normalized", normalizedSnapshot.Status, normalizedSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "WeekdayTrend", weekdayTrendSnapshot.Status, weekdayTrendSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "Transform", transformSnapshot.Status, transformSnapshot.Reason);

        if (selectedSeriesCount < 2)
            warnings.Add("Multiple series required for CombinedMetric/Normalized/Transform parity; select at least two series.");

        return warnings;
    }

    private static void AddWarningIfUnavailable(List<string> warnings, string label, string status, string? reason)
    {
        if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
            return;

        var detail = string.IsNullOrWhiteSpace(reason) ? "Unavailable" : reason;
        warnings.Add($"{label} parity not completed: {detail}");
    }
}
