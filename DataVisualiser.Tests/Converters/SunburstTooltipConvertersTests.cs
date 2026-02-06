using System.Globalization;
using DataVisualiser.UI.Charts.Converters;

namespace DataVisualiser.Tests.Converters;

public sealed class SunburstTooltipConvertersTests
{
    [Fact]
    public void PercentOfTotalConverter_ShouldFormatPercent_WhenValuesProvided()
    {
        var converter = new PercentOfTotalConverter();
        var result = converter.Convert(new object[] { 25.0, 100.0 }, typeof(string), null, CultureInfo.InvariantCulture);

        var expected = string.Format(CultureInfo.InvariantCulture, "Percent: {0:P1}", 0.25);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PercentOfTotalConverter_ShouldUseFirstAvailableTotal()
    {
        var converter = new PercentOfTotalConverter();
        var result = converter.Convert(new object[] { 50.0, "invalid", 200.0 }, typeof(string), null, CultureInfo.InvariantCulture);

        var expected = string.Format(CultureInfo.InvariantCulture, "Percent: {0:P1}", 0.25);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PercentOfTotalConverter_ShouldReturnNa_WhenMissingData()
    {
        var converter = new PercentOfTotalConverter();
        var result = converter.Convert(new object[] { "bad", 10.0 }, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("Percent: n/a", result);
    }

    [Fact]
    public void FirstNonEmptyTextConverter_ShouldReturnFirstNonEmptyValue()
    {
        var converter = new FirstNonEmptyTextConverter();
        var result = converter.Convert(new object[] { null!, "", "  ", "Bucket A" }, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("Bucket A", result);
    }

    [Fact]
    public void FirstNonEmptyTextConverter_ShouldReturnEmptyString_WhenNoValues()
    {
        var converter = new FirstNonEmptyTextConverter();
        var result = converter.Convert(new object[] { null!, "", "   " }, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SunburstValueTextConverter_ShouldFormatValue()
    {
        var converter = new SunburstValueTextConverter();
        var result = converter.Convert(12.3456, typeof(string), null, CultureInfo.InvariantCulture);

        var expected = string.Format(CultureInfo.InvariantCulture, "Value: {0:N2}", 12.3456);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SunburstValueTextConverter_ShouldFormatValueFromProperty()
    {
        var converter = new SunburstValueTextConverter();
        var source = new { Value = 42.0 };
        var result = converter.Convert(source, typeof(string), null, CultureInfo.InvariantCulture);

        var expected = string.Format(CultureInfo.InvariantCulture, "Value: {0:N2}", 42.0);
        Assert.Equal(expected, result);
    }
}
