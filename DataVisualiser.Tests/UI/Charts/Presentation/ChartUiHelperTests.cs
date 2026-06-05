using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class ChartUiHelperTests
{
    [Theory]
    [InlineData("All", DataAccessDefaults.DefaultTableName)]
    [InlineData("Hourly", DataAccessDefaults.HealthMetricsHourTable)]
    [InlineData("Daily", DataAccessDefaults.HealthMetricsDayTable)]
    [InlineData("Weekly", DataAccessDefaults.HealthMetricsWeekTable)]
    [InlineData("Monthly", DataAccessDefaults.HealthMetricsMonthTable)]
    [InlineData("Yearly", DataAccessDefaults.HealthMetricsYearTable)]
    public void GetTableNameFromResolution_ShouldMapDisplayResolutionToTableName(
        string resolution,
        string expectedTableName)
    {
        Assert.Equal(expectedTableName, ChartUiHelper.GetTableNameFromResolution(resolution));
    }

    [Theory]
    [InlineData(DataAccessDefaults.DefaultTableName, "All")]
    [InlineData(DataAccessDefaults.HealthMetricsHourTable, "Hourly")]
    [InlineData(DataAccessDefaults.HealthMetricsDayTable, "Daily")]
    [InlineData(DataAccessDefaults.HealthMetricsWeekTable, "Weekly")]
    [InlineData(DataAccessDefaults.HealthMetricsMonthTable, "Monthly")]
    [InlineData(DataAccessDefaults.HealthMetricsYearTable, "Yearly")]
    public void GetResolutionFromTableName_ShouldMapTableNameToDisplayResolution(
        string tableName,
        string expectedResolution)
    {
        Assert.Equal(expectedResolution, ChartUiHelper.GetResolutionFromTableName(tableName));
    }
}
