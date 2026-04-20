using System.Configuration;
using System.Diagnostics;
using DataVisualiser.Core.Data;
using DataVisualiser.Shared.Helpers;

namespace DataVisualiser.Core.Services;

internal static class MetricDataLoadStrategyResolver
{
    public static MetricDataLoadStrategy Resolve(DateTime from, DateTime to, long recordCount)
    {
        var enableSampling = bool.TryParse(ConfigurationManager.AppSettings["DataVisualiser:EnableSqlSampling"], out var samplingEnabled) && samplingEnabled;

        var samplingThreshold = int.TryParse(ConfigurationManager.AppSettings["DataVisualiser:SamplingThreshold"], out var threshold) ? threshold : 5000;

        var targetSamples = int.TryParse(ConfigurationManager.AppSettings["DataVisualiser:TargetSamplePoints"], out var samples) ? samples : 2000;

        var enableLimiting = bool.TryParse(ConfigurationManager.AppSettings["DataVisualiser:EnableSqlResultLimiting"], out var limitingEnabled) && limitingEnabled;

        if (enableSampling && recordCount > samplingThreshold)
        {
            Debug.WriteLine($"[Sampling] Activated UniformOverTime | Records={recordCount}, Target={targetSamples}, Range={from:yyyy-MM-dd}->{to:yyyy-MM-dd}");

            return new MetricDataLoadStrategy(SamplingMode.UniformOverTime, targetSamples, null);
        }

        if (enableLimiting)
            return new MetricDataLoadStrategy(SamplingMode.None, null, MathHelper.CalculateOptimalMaxRecords(from, to));

        return new MetricDataLoadStrategy(SamplingMode.None, null, null);
    }
}

internal readonly record struct MetricDataLoadStrategy(SamplingMode Mode, int? TargetSamples, int? MaxRecords);
