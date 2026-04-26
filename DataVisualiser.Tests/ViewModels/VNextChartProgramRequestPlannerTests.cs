using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.ViewModels;

public sealed class VNextChartProgramRequestPlannerTests
{
    [Fact]
    public void BuildMainFamilyRequests_ShouldReturnMainRequestForMainOnly()
    {
        var state = new ChartState { IsMainVisible = true };

        var requests = VNextChartProgramRequestPlanner.BuildMainFamilyRequests(state);

        Assert.Single(requests);
        Assert.Equal(ChartProgramKind.Main, requests[0].Kind);
    }

    [Fact]
    public void BuildMainFamilyRequests_ShouldPreserveMainDisplayMode()
    {
        var state = new ChartState
        {
            IsMainVisible = true,
            MainChartDisplayMode = MainChartDisplayMode.Stacked
        };

        var requests = VNextChartProgramRequestPlanner.BuildMainFamilyRequests(state);

        Assert.Equal(ChartDisplayMode.Stacked, requests[0].DisplayMode);
    }

    [Fact]
    public void BuildMainFamilyRequests_ShouldIncludeNormalizedWhenVisible()
    {
        var state = new ChartState
        {
            IsMainVisible = true,
            IsNormalizedVisible = true
        };

        var requests = VNextChartProgramRequestPlanner.BuildMainFamilyRequests(state);

        Assert.Equal([ChartProgramKind.Main, ChartProgramKind.Normalized], requests.Select(request => request.Kind));
    }

    [Theory]
    [InlineData(true, ChartProgramKind.Difference)]
    [InlineData(false, ChartProgramKind.Ratio)]
    public void BuildMainFamilyRequests_ShouldIncludeSelectedDiffRatioOperation(bool differenceMode, ChartProgramKind expectedKind)
    {
        var state = new ChartState
        {
            IsMainVisible = true,
            IsDiffRatioVisible = true,
            IsDiffRatioDifferenceMode = differenceMode
        };

        var requests = VNextChartProgramRequestPlanner.BuildMainFamilyRequests(state);

        Assert.Equal([ChartProgramKind.Main, expectedKind], requests.Select(request => request.Kind));
    }

    [Fact]
    public void BuildVisibleChartFamilyRequests_ShouldIncludeAllVisibleFamilyRequests()
    {
        var state = new ChartState
        {
            IsMainVisible = true,
            IsNormalizedVisible = true,
            IsDiffRatioVisible = true,
            IsDiffRatioDifferenceMode = false,
            IsDistributionVisible = true,
            IsWeeklyTrendVisible = true,
            IsTransformPanelVisible = true,
            IsBarPieVisible = true
        };
        var transformOperation = SeriesOperationRequest.Normalize(0, "normalized", "Normalized");

        var requests = VNextChartProgramRequestPlanner.BuildVisibleChartFamilyRequests(
            state,
            [transformOperation],
            "Custom Transform");

        Assert.Equal(
            [
                ChartProgramKind.Main,
                ChartProgramKind.Normalized,
                ChartProgramKind.Ratio,
                ChartProgramKind.Distribution,
                ChartProgramKind.WeekdayTrend,
                ChartProgramKind.Transform,
                ChartProgramKind.BarPie
            ],
            requests.Select(request => request.Kind));

        var transform = requests.Single(request => request.Kind == ChartProgramKind.Transform);
        Assert.Equal("Custom Transform", transform.TitleOverride);
        Assert.Equal([transformOperation], transform.SeriesOperations);
    }

    [Fact]
    public void BuildVisibleChartFamilyRequests_ShouldAllowTransformWithoutOperationsAsIdentityPreparation()
    {
        var state = new ChartState
        {
            IsMainVisible = true,
            IsTransformPanelVisible = true
        };

        var requests = VNextChartProgramRequestPlanner.BuildVisibleChartFamilyRequests(state);

        var transform = requests.Single(request => request.Kind == ChartProgramKind.Transform);
        Assert.Empty(transform.SeriesOperations);
        Assert.Equal("Transform", transform.TitleOverride);
    }

    [Theory]
    [InlineData(ChartProgramKind.Main, EvidenceRuntimePath.VNextMain)]
    [InlineData(ChartProgramKind.Normalized, EvidenceRuntimePath.VNextNormalized)]
    [InlineData(ChartProgramKind.Difference, EvidenceRuntimePath.VNextDiffRatio)]
    [InlineData(ChartProgramKind.Ratio, EvidenceRuntimePath.VNextDiffRatio)]
    [InlineData(ChartProgramKind.Distribution, EvidenceRuntimePath.VNextDistribution)]
    [InlineData(ChartProgramKind.WeekdayTrend, EvidenceRuntimePath.VNextWeekdayTrend)]
    [InlineData(ChartProgramKind.Transform, EvidenceRuntimePath.VNextTransform)]
    [InlineData(ChartProgramKind.BarPie, EvidenceRuntimePath.VNextBarPie)]
    public void ResolveRuntimePath_ShouldMapKnownProgramKinds(ChartProgramKind programKind, EvidenceRuntimePath expectedPath)
    {
        Assert.Equal(expectedPath, VNextChartProgramRequestPlanner.ResolveRuntimePath(programKind));
    }
}
