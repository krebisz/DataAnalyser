namespace DataVisualiser.VNext.Contracts;

public static class ChartProgramDeliveryTargetResolver
{
    public static string ResolveDefaultTarget(ChartProgramKind kind)
    {
        return kind switch
        {
            ChartProgramKind.Main => "MainChart",
            ChartProgramKind.Normalized => "NormalizedChart",
            ChartProgramKind.Difference or ChartProgramKind.Ratio => "DiffRatioChart",
            ChartProgramKind.Transform => "TransformChart",
            ChartProgramKind.Distribution => "DistributionChart",
            ChartProgramKind.WeekdayTrend => "WeekdayTrendChart",
            ChartProgramKind.BarPie => "BarPieChart",
            ChartProgramKind.SyncfusionSunburst => "SyncfusionSunburst",
            _ => "ChartSurface"
        };
    }

    public static ConsumerDeliveryContract CreateDelivery(ChartProgramKind kind, string? deliveryTarget = null)
    {
        var target = string.IsNullOrWhiteSpace(deliveryTarget)
            ? ResolveDefaultTarget(kind)
            : deliveryTarget;

        return kind == ChartProgramKind.SyncfusionSunburst
            ? ConsumerDeliveryContract.HierarchyChart(kind, target)
            : ConsumerDeliveryContract.Chart(kind, target);
    }
}
