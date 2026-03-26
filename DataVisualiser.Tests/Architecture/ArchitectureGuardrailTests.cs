using System.IO;
using System.Text;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Architecture;

public sealed class ArchitectureGuardrailTests
{
    [Fact]
    public void MainChartsView_ShouldKeepExportOnDedicatedWriterSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("ReachabilityExportWriter", source);
        Assert.DoesNotContain("File.WriteAllText(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.CreateDirectory(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer.Serialize(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldKeepThemeOnDedicatedCoordinatorSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewThemeCoordinator", source);
        Assert.DoesNotContain("ThemeChanged +=", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeChanged -=", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsViewAndMainHost_ShouldNotReintroduceNamedControlFallbacks()
    {
        var mainChartsViewSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        Assert.DoesNotContain("FindName(", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VisualTreeHelper", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LogicalTreeHelper", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GetType().GetField", mainChartsViewSource, StringComparison.Ordinal);

        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "UI", "MainHost")],
            ["FindName(", "VisualTreeHelper", "LogicalTreeHelper", "GetType().GetField"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreOrchestrationAndMainHost_ShouldNotReferenceSyncfusionOrLiveChartsCore()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "Core", "Orchestration"),
                Path.Combine("DataVisualiser", "UI", "MainHost")
            ],
            ["Syncfusion", "LiveChartsCore"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreOrchestration_ShouldNotOwnThemeOrExportLogic()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "Core", "Orchestration")],
            ["AppThemeService", "ThemeChanged", "ReachabilityExportWriter", "JsonSerializer", "File.WriteAllText(", "Directory.CreateDirectory(", "reachability-", "documents\\"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void GuardedRuntimeFiles_ShouldNotUseSilentMessageBoxSingletonFallbacks()
    {
        var guardedFiles = new[]
        {
            Path.Combine("DataVisualiser", "Core", "Orchestration", "Coordinator", "ChartUpdateCoordinator.cs"),
            Path.Combine("DataVisualiser", "Core", "Orchestration", "SecondaryCharts", "SecondaryMetricChartOrchestrationPipeline.cs"),
            Path.Combine("DataVisualiser", "Core", "Orchestration", "DistributionCharts", "DistributionChartOrchestrationPipeline.cs"),
            Path.Combine("DataVisualiser", "Core", "Services", "BaseDistributionService.cs")
        };

        foreach (var relativeFile in guardedFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(relativeFile);
            Assert.DoesNotContain("MessageBoxUserNotificationService.Instance", source, StringComparison.Ordinal);
        }

        var compositionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "MainChartsViewChartPipelineFactory.cs");
        Assert.Contains("MessageBoxUserNotificationService.Instance", compositionSource);
    }

    [Fact]
    public void ExecutionPlan_ShouldKeepLateGeneralizationGuardrailsDocumented()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md");

        Assert.Contains("2-3", source, StringComparison.Ordinal);
        Assert.Contains("logical component/layer", source, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("thin adapters, pure renderers, and simple state holders", source, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoMatches(IReadOnlyList<string> offenders)
    {
        if (offenders.Count == 0)
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Forbidden architectural references were found:");
        foreach (var offender in offenders)
            builder.AppendLine(offender);

        Assert.True(offenders.Count == 0, builder.ToString());
    }
}
