using StatePulse.Net.Engine;

namespace StatePulse.Net;
public interface IDispatchTracker<TAction> where TAction : IAction
{
    public EventHandler<DispatchEntry<TAction>>? OnCancel { get; set; }
    public EventHandler<DispatchEntry<TAction>>? OnEntry { get; set; }
    public void CreateEntryPoint(Guid id, object action);
    public void DeleteEntryPoint(Guid id);
    public void CancelDispatchFor(Guid id);
    public void CancelAll();
    public bool IsCancelled(Guid id);
}
