using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Adapters;
namespace DataVisualiser.UI.Charts.Infrastructure;

public static class ChartControllerKeys
{
    public const string Main = "Main";
    public const string Normalized = "Norm";
    public const string DiffRatio = "DiffRatio";
    public const string Distribution = "Distribution";
    public const string WeeklyTrend = "WeeklyTrend";
    public const string Transform = "Transform";
    public const string BarPie = "BarPie";

    public static readonly string[] All =
    {
            Main,
            Normalized,
            DiffRatio,
            Distribution,
            WeeklyTrend,
            Transform,
            BarPie
    };
}
