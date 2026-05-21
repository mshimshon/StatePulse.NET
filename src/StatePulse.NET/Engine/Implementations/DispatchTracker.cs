using System.Collections.Concurrent;
namespace StatePulse.Net.Engine.Implementations;

internal class DispatchTracker : IDispatchTracker
{
    private readonly ConcurrentDictionary<Guid, DispatchEntry> _cancelTracker = new();
    public EventHandler<DispatchEntry>? OnCancel { get; set; }
    public EventHandler<DispatchEntry>? OnEntry { get; set; }

    public ConcurrentDictionary<Guid, DispatchEntry> CancellationTracker => _cancelTracker;

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

    public IDispatchEntry CreateEntryPoint(Guid id, object action)
    {
        CancelAll();
        var item = new DispatchEntry(id, action.GetType());

        _cancelTracker.TryAdd(id, item);
        OnEntry?.Invoke(this, item);
        return item;
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

    public void CancelAll()
    {
        foreach (var item in _cancelTracker.Keys)
            DeleteEntryPoint(item);
    }

    public bool IsCancelled(Guid id, long version)
    {
        long current = Volatile.Read(ref _currentVersion);
        return current != version;

    }
}
