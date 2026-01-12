using System.Collections.Concurrent;
namespace StatePulse.Net.Engine.Implementations;

internal class DispatchTracker<TAction> : IDispatchTracker<TAction> where TAction : IAction
{
    private readonly ConcurrentDictionary<Guid, DispatchEntry<TAction>> _cancelTracker = new();
    public EventHandler<DispatchEntry<TAction>>? OnCancel { get; set; }
    public EventHandler<DispatchEntry<TAction>>? OnEntry { get; set; }

    public ConcurrentDictionary<Guid, DispatchEntry<TAction>> CancellationTracker => _cancelTracker;

    public long CurrentVersion => _currentVersion;

    private long _currentVersion = 0;
    public bool CreateExecutingAction(Guid id, object action, long version)
    {
        long current = Volatile.Read(ref _currentVersion);

        // If my version is not greater, I lose
        if (version <= current)
            return false;

        // Try to atomically promote the version
        long original = Interlocked.CompareExchange(
            ref _currentVersion,
            version,
            current);

        // If original == current, I successfully promoted
        return original == current;

    }

    public void CreateEntryPoint(Guid id, object action)
    {
        CancelAll();
        var item = new DispatchEntry<TAction>(id, (IDispatcherPrepper<TAction>)action);

        _cancelTracker.TryAdd(id, item);
        OnEntry?.Invoke(this, item);
    }

    public void DeleteEntryPoint(Guid id)
    {
        if (_cancelTracker.TryRemove(id, out var entry))
        {
            entry.Cancel();
            OnCancel?.Invoke(this, entry);

        }
    }

    public void CancelDispatchFor(Guid id)
    {
        if (_cancelTracker.TryGetValue(id, out var entry) && !entry.IsCancelled)
        {
            entry.Cancel();
            OnCancel?.Invoke(this, entry);
        }
    }

    public bool IsCancelled(Guid id)
    {
        if (!_cancelTracker.TryGetValue(id, out var entry))
            return true;
        return entry.IsCancelled;
    }

    public void CancelAll()
    {
        foreach (var item in _cancelTracker.Keys)
            DeleteEntryPoint(item);
    }

    public bool CreateEntryPoint(Guid id, object action, long version) => throw new NotImplementedException();
    public bool IsCancelled(Guid id, long version)
    {
        long current = Volatile.Read(ref _currentVersion);
        return current != version;

    }
}
