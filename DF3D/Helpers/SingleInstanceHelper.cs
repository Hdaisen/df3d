namespace DF3D.Helpers;

/// <summary>
/// Ensures only one instance of the application runs at a time.
/// </summary>
public sealed class SingleInstanceHelper : IDisposable
{
    private readonly Mutex _mutex;
    private bool _isFirstInstance;

    public SingleInstanceHelper(string appName = "DF3D")
    {
        _mutex = new Mutex(true, appName, out _isFirstInstance);
    }

    public bool IsFirstInstance => _isFirstInstance;

    public void Dispose()
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
    }
}
