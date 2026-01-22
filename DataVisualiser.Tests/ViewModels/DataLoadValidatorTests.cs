using DataVisualiser.Core.Helpers.Validation;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.ViewModels;

public class DataLoadValidatorTests
{
    [Fact]
    public void ValidateMetricTypeSelected_ReturnsFalse_WhenMissing()
    {
        var metricState = new MetricState();
        var validator = new DataLoadValidator(metricState);

        var result = validator.ValidateMetricTypeSelected(out var message);

        Assert.False(result);
        Assert.Equal("Please select a Metric Type before loading data.", message);
    }

    [Fact]
    public void ValidateDateRange_ReturnsFalse_WhenMissing()
    {
        var metricState = new MetricState();
        var validator = new DataLoadValidator(metricState);

        var result = validator.ValidateDateRange(out var message);

        Assert.False(result);
        Assert.Equal("Please select both From and To dates before loading data.", message);
    }

    [Fact]
    public void ValidateDateRange_ReturnsFalse_WhenFromAfterTo()
    {
        var metricState = new MetricState
        {
                FromDate = new DateTime(2025, 1, 2),
                ToDate = new DateTime(2025, 1, 1)
        };
        var validator = new DataLoadValidator(metricState);

        var result = validator.ValidateDateRange(out var message);

        Assert.False(result);
        Assert.Equal("From date must be before To date.", message);
    }

    [Fact]
    public void ValidateDataLoadRequirements_ReturnsError_WhenMetricMissing()
    {
        var metricState = new MetricState
        {
                FromDate = new DateTime(2025, 1, 1),
                ToDate = new DateTime(2025, 1, 2)
        };
        var validator = new DataLoadValidator(metricState);

        var (isValid, errorMessage) = validator.ValidateDataLoadRequirements();

        Assert.False(isValid);
        Assert.Equal("Please select a Metric Type before loading data.", errorMessage);
    }
}