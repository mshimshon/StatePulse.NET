namespace StatePulse.Net.Engine;

using StatePulse.Net;
public class DispatchEntry<TAction> where TAction : IAction
{
    private readonly IDispatcherPrepper<TAction> _action;
    private readonly Guid _guid;
    public Guid Id => _guid;
    public IDispatcherPrepper<TAction> Action => _action;
    private readonly CancellationTokenSource _tokenSource = new();
    public DateTime Execution { get; }
    private int _disposed;
    public DispatchEntry(Guid id, IDispatcherPrepper<TAction> action)
    {
        _guid = id;
        _action = action;
        Execution = DateTime.UtcNow;

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

