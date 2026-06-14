using System.Windows.Media;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformCorrelationChartModelBuilder
{
    public static UiChartRenderModel Build(TransformCorrelationSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        return new UiChartRenderModel
        {
            Series =
            [
                new ChartSeriesModel
                {
                    Name = summary.Label,
                    SeriesType = ChartSeriesType.Column,
                    Values =
                    [
                        NormalizeValue(summary.Correlation),
                        NormalizeValue(summary.ConfidenceLower),
                        NormalizeValue(summary.ConfidenceUpper)
                    ],
                    Color = Colors.SteelBlue
                }
            ],
            AxesX =
            [
                CartesianChartPresentationBuilder.CreateCategoryAxis(
                    "Measure",
                    ["Correlation", "CI lower", "CI upper"])
            ],
            AxesY =
            [
                CartesianChartPresentationBuilder.CreateFixedValueAxis("Value", -1d, 1d, 0.5d)
            ]
        };
    }

    private static double NormalizeValue(double value) =>
        double.IsFinite(value) ? value : 0d;
}
