namespace StatePulse.Net.Engine;
public class DispatchEntry<TAction> where TAction : IAction
{
    private readonly IDispatcherPrepper<TAction> _action;
    private readonly Guid _guid;
    public Guid Id => _guid;
    public IDispatcherPrepper<TAction> Action => _action;
    private readonly CancellationTokenSource _tokenSource = new();
    private int _disposed;
    public DispatchEntry(Guid id, IDispatcherPrepper<TAction> action)
    {
        _guid = id;
        _action = action;
    }
    public void Cancel()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            try { _tokenSource.Cancel(); }
            catch
            {
                // Ignore race condition errors; logging optional
            }
            _tokenSource.Dispose();
        }
    }

    public bool IsCancelled => _disposed == 1 || _tokenSource.IsCancellationRequested;
}

