namespace StatePulse.Net.Engine;
//public class DispatchEntry<TAction> : IDispatchEntry where TAction : IAction
public class DispatchEntry : IDispatchEntry
{
    //private readonly IDispatcherPrepper<TAction> _action;
    private readonly Guid _guid;
    private readonly Type _actionType;

    public Guid Id => _guid;
    //public IDispatcherPrepper<TAction> Action => _action;
    private readonly CancellationTokenSource _tokenSource = new();
    public DateTime Execution { get; }
    private int _disposed;
    //public DispatchEntry(Guid id, IDispatcherPrepper<TAction> action)
    public DispatchEntry(Guid id, Type actionType)
    {
        _guid = id;
        _actionType = actionType;
        //_action = action;
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
    // Equality based on Id
    public bool Equals(DispatchEntry? other)
    {
        if (other is null) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as DispatchEntry);

    public override int GetHashCode() => Id.GetHashCode();

    public bool IsCancelled =>
        Volatile.Read(ref _disposed) == 1 ||
        _tokenSource.IsCancellationRequested;
}

