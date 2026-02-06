using System.Globalization;
using DataVisualiser.UI.Charts.Converters;

namespace DataVisualiser.Tests.Converters;

public sealed class SunburstTooltipConvertersTests
{
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
}
