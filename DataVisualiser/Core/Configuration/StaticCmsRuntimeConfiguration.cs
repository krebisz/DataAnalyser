namespace DataVisualiser.Core.Configuration;

public sealed class StaticCmsRuntimeConfiguration : ICmsRuntimeConfiguration
{
    public static StaticCmsRuntimeConfiguration Instance { get; } = new();

    private StaticCmsRuntimeConfiguration()
    {
    }

    public bool UseCmsData
    {
        get => CmsConfiguration.UseCmsData;
        set => CmsConfiguration.UseCmsData = value;
    }

    public bool UseCmsForSingleMetric
    {
        get => CmsConfiguration.UseCmsForSingleMetric;
        set => CmsConfiguration.UseCmsForSingleMetric = value;
    }

    public bool UseCmsForMultiMetric
    {
        get => CmsConfiguration.UseCmsForMultiMetric;
        set => CmsConfiguration.UseCmsForMultiMetric = value;
    }

    public bool UseCmsForCombinedMetric
    {
        get => CmsConfiguration.UseCmsForCombinedMetric;
        set => CmsConfiguration.UseCmsForCombinedMetric = value;
    }

    public bool UseCmsForDifference
    {
        get => CmsConfiguration.UseCmsForDifference;
        set => CmsConfiguration.UseCmsForDifference = value;
    }

    public bool UseCmsForRatio
    {
        get => CmsConfiguration.UseCmsForRatio;
        set => CmsConfiguration.UseCmsForRatio = value;
    }

    public bool UseCmsForNormalized
    {
        get => CmsConfiguration.UseCmsForNormalized;
        set => CmsConfiguration.UseCmsForNormalized = value;
    }

    public bool UseCmsForWeeklyDistribution
    {
        get => CmsConfiguration.UseCmsForWeeklyDistribution;
        set => CmsConfiguration.UseCmsForWeeklyDistribution = value;
    }

    public bool UseCmsForWeekdayTrend
    {
        get => CmsConfiguration.UseCmsForWeekdayTrend;
        set => CmsConfiguration.UseCmsForWeekdayTrend = value;
    }

    public bool UseCmsForHourlyDistribution
    {
        get => CmsConfiguration.UseCmsForHourlyDistribution;
        set => CmsConfiguration.UseCmsForHourlyDistribution = value;
    }

    public bool UseCmsForBarPie
    {
        get => CmsConfiguration.UseCmsForBarPie;
        set => CmsConfiguration.UseCmsForBarPie = value;
    }

    public bool ShouldUseCms(string strategyType)
    {
        return CmsConfiguration.ShouldUseCms(strategyType);
    }
}
