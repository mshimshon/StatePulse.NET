using System.Collections.Concurrent;

namespace StatePulse.Net.Engine;

public interface IDispatchTracker
{
    public bool IsCancelled(Guid id, long version);
    public long CurrentVersion { get; }
    public bool CreateExecutingAction(Guid id, object action, long version);
}
public interface IDispatchTracker<TAction> : IDispatchTracker where TAction : IAction
{
    public ConcurrentDictionary<Guid, DispatchEntry<TAction>> CancellationTracker { get; }
    public EventHandler<DispatchEntry<TAction>>? OnCancel { get; set; }
    public EventHandler<DispatchEntry<TAction>>? OnEntry { get; set; }
    public void CreateEntryPoint(Guid id, object action);
    public void DeleteEntryPoint(Guid id);
    public void CancelDispatchFor(Guid id);
    public void CancelAll();
    public bool IsCancelled(Guid id);

}
