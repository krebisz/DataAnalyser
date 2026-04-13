using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Helpers;

public sealed class ChartLabelFormatterTests
{
    [Fact]
    public void FormatDateTimeLabel_ShouldUseMonthFormat_ForMonthInterval()
    {
        var result = ChartSeriesLabelFormatter.FormatDateTimeLabel(new DateTime(2024, 03, 15, 14, 30, 00), TickInterval.Month);

        Assert.Equal("Mar 2024", result);
    }

    [Fact]
    public void FormatDateTimeLabel_ShouldUseHourFormat_ForHourInterval()
    {
        var result = ChartSeriesLabelFormatter.FormatDateTimeLabel(new DateTime(2024, 03, 15, 14, 30, 00), TickInterval.Hour);

        Assert.EndsWith("14:30", result);
        Assert.Contains("03", result);
        Assert.Contains("15", result);
    }
}
