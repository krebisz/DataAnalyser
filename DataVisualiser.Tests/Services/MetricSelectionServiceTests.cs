using System.Configuration;
using System.Reflection;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Services;

namespace DataVisualiser.Tests.Services;

public sealed class MetricSelectionServiceTests
{
    [Fact]
    public void ResolveDataLoadStrategy_ActivatesSampling_WhenRecordCountExceedsThreshold()
    {
        // Arrange
        SetAppSetting("DataVisualiser:EnableSqlSampling", "true");
        SetAppSetting("DataVisualiser:SamplingThreshold", "1000");
        SetAppSetting("DataVisualiser:TargetSamplePoints", "200");
        SetAppSetting("DataVisualiser:EnableSqlResultLimiting", "false");

        var service = new MetricSelectionService("FakeConnectionString");

        var from = new DateTime(2024, 01, 01);
        var to = new DateTime(2024, 12, 31);
        var recordCount = 5000; // > threshold

        // Act
        var result = InvokeResolveStrategy(service, from, to, recordCount);

        // Assert
        Assert.Equal(SamplingMode.UniformOverTime, result.Mode);
        Assert.Equal(200, result.TargetSamples);
        Assert.Null(result.MaxRecords);
    }

    // ---- Test helper ----
    private static(SamplingMode Mode, int? TargetSamples, int? MaxRecords) InvokeResolveStrategy(MetricSelectionService service, DateTime from, DateTime to, long recordCount)
    {
        // ResolveDataLoadStrategy is private — invoke via reflection
        var method = typeof(MetricSelectionService).GetMethod("ResolveDataLoadStrategy", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);

        return ((SamplingMode Mode, int? TargetSamples, int? MaxRecords))method!.Invoke(service,
                new object[]
                {
                        from,
                        to,
                        recordCount
                })!;
    }

    private static void SetAppSetting(string key, string value)
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        var setting = config.AppSettings.Settings[key];
        if (setting == null)
            config.AppSettings.Settings.Add(key, value);
        else
            setting.Value = value;

        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }

    [Fact]
    public void ResolveDataLoadStrategy_DoesNotActivateSampling_WhenRecordCountIsBelowThreshold()
    {
        // Arrange
        SetAppSetting("DataVisualiser:EnableSqlSampling", "true");
        SetAppSetting("DataVisualiser:SamplingThreshold", "1000");
        SetAppSetting("DataVisualiser:TargetSamplePoints", "200");
        SetAppSetting("DataVisualiser:EnableSqlResultLimiting", "false");

        var service = new MetricSelectionService("FakeConnectionString");

        var from = new DateTime(2024, 01, 01);
        var to = new DateTime(2024, 12, 31);
        var recordCount = 500; // < threshold

        // Act
        var result = InvokeResolveStrategy(service, from, to, recordCount);

        // Assert
        Assert.Equal(SamplingMode.None, result.Mode);
        Assert.Null(result.TargetSamples);
        Assert.Null(result.MaxRecords);
    }
}
