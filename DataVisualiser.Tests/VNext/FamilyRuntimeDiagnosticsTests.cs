using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class FamilyRuntimeDiagnosticsTests
{
    [Fact]
    public void SetFamilyRuntime_GetFamilyRuntime_RoundTrips()
    {
        var state = new ChartState();
        var runtime = LoadRuntimeState.FromVNextSuccess(
            EvidenceRuntimePath.VNextDistribution, "req", "snap", ChartProgramKind.Distribution, "src");

        state.SetFamilyRuntime(ChartProgramKind.Distribution, runtime);

        Assert.Same(runtime, state.GetFamilyRuntime(ChartProgramKind.Distribution));
    }

    [Fact]
    public void GetFamilyRuntime_ReturnsNull_WhenNotSet()
    {
        var state = new ChartState();

        Assert.Null(state.GetFamilyRuntime(ChartProgramKind.WeekdayTrend));
    }

    [Fact]
    public void SetFamilyRuntime_OverwritesPreviousValue()
    {
        var state = new ChartState();
        var first = LoadRuntimeState.FromVNextSuccess(
            EvidenceRuntimePath.VNextBarPie, "req1", "snap1", ChartProgramKind.BarPie, "src1");
        var second = LoadRuntimeState.LegacyFallback("req2", "failed");

        state.SetFamilyRuntime(ChartProgramKind.BarPie, first);
        state.SetFamilyRuntime(ChartProgramKind.BarPie, second);

        Assert.Same(second, state.GetFamilyRuntime(ChartProgramKind.BarPie));
    }

    [Fact]
    public void FamilyLoadRuntimes_ReflectsAllSetEntries()
    {
        var state = new ChartState();
        state.SetFamilyRuntime(ChartProgramKind.Distribution,
            LoadRuntimeState.FromVNextSuccess(EvidenceRuntimePath.VNextDistribution, "r", "s", ChartProgramKind.Distribution, "p"));
        state.SetFamilyRuntime(ChartProgramKind.Transform,
            LoadRuntimeState.FromVNextSuccess(EvidenceRuntimePath.VNextTransform, "r", "s", ChartProgramKind.Transform, "p"));

        Assert.Equal(2, state.FamilyLoadRuntimes.Count);
        Assert.True(state.FamilyLoadRuntimes.ContainsKey(ChartProgramKind.Distribution));
        Assert.True(state.FamilyLoadRuntimes.ContainsKey(ChartProgramKind.Transform));
    }

    [Fact]
    public void BuildVNextDiagnostics_ReturnsNull_ForLegacy()
    {
        var runtime = LoadRuntimeState.LegacyFallback("req", null);

        Assert.Null(EvidenceDiagnosticsBuilder.BuildVNextDiagnostics(runtime));
    }

    [Fact]
    public void BuildVNextDiagnostics_ReturnsNull_ForNull()
    {
        Assert.Null(EvidenceDiagnosticsBuilder.BuildVNextDiagnostics(null));
    }

    [Fact]
    public void BuildVNextDiagnostics_PopulatesSignatureChain_ForVNext()
    {
        var runtime = LoadRuntimeState.FromVNextSuccess(
            EvidenceRuntimePath.VNextDistribution, "sig", "sig", ChartProgramKind.Distribution, "sig");

        var snapshot = EvidenceDiagnosticsBuilder.BuildVNextDiagnostics(runtime);

        Assert.NotNull(snapshot);
        Assert.Equal("sig", snapshot!.RequestSignature);
        Assert.Equal("sig", snapshot.SnapshotSignature);
        Assert.Equal("Distribution", snapshot.ProgramKind);
        Assert.Equal("sig", snapshot.ProgramSourceSignature);
        Assert.True(snapshot.RequestMatchesSnapshot);
        Assert.True(snapshot.SnapshotMatchesProgramSource);
        Assert.Null(snapshot.FailureReason);
    }

    [Fact]
    public void BuildVNextFamilyDiagnostics_PopulatesFromDictionary()
    {
        var state = new ChartState();
        state.SetFamilyRuntime(ChartProgramKind.Distribution,
            LoadRuntimeState.FromVNextSuccess(EvidenceRuntimePath.VNextDistribution, "r", "s", ChartProgramKind.Distribution, "p"));
        state.SetFamilyRuntime(ChartProgramKind.WeekdayTrend,
            LoadRuntimeState.FromVNextSuccess(EvidenceRuntimePath.VNextWeekdayTrend, "r", "s", ChartProgramKind.WeekdayTrend, "p"));

        var result = EvidenceDiagnosticsBuilder.BuildVNextFamilyDiagnostics(state);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("Distribution"));
        Assert.True(result.ContainsKey("WeekdayTrend"));
        Assert.Equal("Distribution", result["Distribution"].ProgramKind);
        Assert.Equal("WeekdayTrend", result["WeekdayTrend"].ProgramKind);
    }

    [Fact]
    public void BuildVNextFamilyDiagnostics_ExcludesLegacyEntries()
    {
        var state = new ChartState();
        state.SetFamilyRuntime(ChartProgramKind.Distribution,
            LoadRuntimeState.FromVNextSuccess(EvidenceRuntimePath.VNextDistribution, "r", "s", ChartProgramKind.Distribution, "p"));
        state.SetFamilyRuntime(ChartProgramKind.Transform,
            LoadRuntimeState.LegacyFallback("r", "failed"));

        var result = EvidenceDiagnosticsBuilder.BuildVNextFamilyDiagnostics(state);

        Assert.Single(result);
        Assert.True(result.ContainsKey("Distribution"));
        Assert.False(result.ContainsKey("Transform"));
    }

    [Fact]
    public void BuildVNextFamilyDiagnostics_ReturnsEmpty_WhenNoFamiliesSet()
    {
        var state = new ChartState();

        var result = EvidenceDiagnosticsBuilder.BuildVNextFamilyDiagnostics(state);

        Assert.Empty(result);
    }
}
