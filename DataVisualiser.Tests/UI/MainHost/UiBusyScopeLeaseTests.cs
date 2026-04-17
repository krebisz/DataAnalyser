using DataVisualiser.UI.MainHost.Coordination;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class UiBusyScopeLeaseTests
{
    [Fact]
    public void Dispose_ShouldReleaseExactlyOnce()
    {
        var releaseCount = 0;
        var lease = new UiBusyScopeLease(() => releaseCount++);

        lease.Dispose();
        lease.Dispose();

        Assert.Equal(1, releaseCount);
    }
}
