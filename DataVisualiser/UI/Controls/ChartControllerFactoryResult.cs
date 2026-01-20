namespace DataVisualiser.UI.Controls;

public sealed record ChartControllerFactoryResult(
    MainChartControllerAdapter Main,
    NormalizedChartControllerAdapter Normalized,
    DiffRatioChartControllerAdapter DiffRatio,
    DistributionChartControllerAdapter Distribution,
    WeekdayTrendChartControllerAdapter WeekdayTrend,
    TransformDataPanelControllerAdapter Transform,
    BarPieChartControllerAdapter BarPie,
    IChartControllerRegistry Registry);
