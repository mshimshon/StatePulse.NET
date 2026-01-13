namespace StatePulse.Net.Engine;

public interface IDispatchTracker
{
    public bool IsCancelled(Guid id, long version);
    public long CurrentVersion { get; }
    public bool CreateExecutingAction(Guid id, object action, long version);
}
public interface IDispatchTracker<TAction> : IDispatchTracker where TAction : IAction
{
    IDispatchEntry CreateEntryPoint(Guid id, object action);
    void DeleteEntryPoint(Guid id);
    void CancelDispatchFor(Guid id);
    void CancelAll();
}
