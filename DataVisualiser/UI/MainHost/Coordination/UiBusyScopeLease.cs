namespace DataVisualiser.UI.MainHost.Coordination;

internal sealed class UiBusyScopeLease : IDisposable
{
    private readonly Action _release;
    private bool _disposed;

    public UiBusyScopeLease(Action release)
    {
        _release = release ?? throw new ArgumentNullException(nameof(release));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _release();
    }
}
