using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class TransformSelectionModelsTests
{
    [Fact]
    public void ResolveRequests_ShouldProjectSelectedMetricAndSubtype()
    {
        var rows = new[]
        {
            new StubInputSelection(
                new MetricNameOption("Weight", "Weight"),
                new MetricNameOption("fat", "Fat"))
        };

        var requests = TransformInputSelectionResolver.ResolveRequests(rows);

        var request = Assert.Single(requests);
        Assert.Equal("Weight", request.MetricType);
        Assert.Equal("fat", request.Subtype);
        Assert.Equal("Weight : Fat", request.DisplayName);
    }

    [Fact]
    public void BuildInputOptions_ShouldUseSubscriptEquationLabels()
    {
        var rows = new[]
        {
            new StubInputSelection(new MetricNameOption("Weight", "Weight"), new MetricNameOption("fat", "Fat")),
            new StubInputSelection(new MetricNameOption("Weight", "Weight"), new MetricNameOption("lean", "Lean"))
        };

        var options = TransformInputSelectionResolver.BuildInputOptions(rows);

        Assert.Equal("Weight : Fat", options[0].Display);
        Assert.Equal("S\u2081", options[0].EquationLabel);
        Assert.Equal("Weight : Lean", options[1].Display);
        Assert.Equal("S\u2082", options[1].EquationLabel);
    }

    [Fact]
    public void ResultRowSelection_ShouldReturnIncludedFlagsAndCount()
    {
        var rows = new[]
        {
            new TransformSelectableResultGridRow(new TransformResultGridRow("t1", "1.0000", string.Empty)),
            new TransformSelectableResultGridRow(new TransformResultGridRow("t2", "2.0000", string.Empty))
        };
        rows[1].IsIncluded = false;

        var includedRows = TransformResultRowSelection.ResolveIncludedRows(rows);

        Assert.Equal([true, false], includedRows);
        Assert.Equal(1, TransformResultRowSelection.CountIncluded(includedRows));
    }

    private sealed record StubInputSelection(
        MetricNameOption? SelectedMetric,
        MetricNameOption? SelectedSubtype) : ITransformInputSelection;
}
