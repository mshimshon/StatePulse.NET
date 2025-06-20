using System.Collections.Concurrent;

namespace StatePulse.NET.DependecyInjection.Internals;
internal class StatePulseRegistry : IStatePulseRegistry
{
    private readonly List<Type> _knownStates = new();
    private readonly List<Type> _knownEffects = new();
    private readonly List<Type> _knownReducers = new();
    private readonly List<Type> _knownActions = new();
    private readonly List<Type> _knownActionValidators = new();
    private ConcurrentDictionary<Type, (object Instance, Guid Id, CancellationTokenSource Cts)> _tracker = new();
    public IReadOnlyList<Type> KnownStates => _knownStates;
    public IReadOnlyList<Type> KnownEffects => _knownEffects;
    public IReadOnlyList<Type> KnownReducers => _knownReducers;
    public IReadOnlyList<Type> KnownActions => _knownActions;
    public IReadOnlyList<Type> KnownActionValidators => _knownActionValidators;

    public void RegisterEffect(Type effectType) => _knownEffects.Add(effectType);
    public void RegisterReducer(Type reducerType) => _knownReducers.Add(reducerType);
    public void RegisterState(Type stateType) => _knownStates.Add(stateType);
    public void RegisterAction(Type actionType) => _knownActions.Add(actionType);
    public void RegisterActionValidator(Type actionValType) => _knownActionValidators.Add(actionValType);

    public void CancelDispatch(Type action)
    {
        var actionsToCancel = _tracker.Where(p => p.Key == action).ToList();
        foreach (var tracked in actionsToCancel)
            if (tracked.Key == action) tracked.Value.Cts.Cancel();
    }
    public bool IsDispatchCancelled(Guid id)
    {
        var item = _tracker.FirstOrDefault(p => p.Value.Id == id);
        if (item.Value != default) return item.Value.Cts.IsCancellationRequested;
        return false;
    }
    public Guid TrackDispatch(Type action, object instance)
    {
        var guid = Guid.NewGuid();
        _tracker.TryAdd(action, new(instance, guid, new CancellationTokenSource()));
        return guid;
    }
    public void UntrackDispatch(Type action, object instance)
    {
        var item = _tracker.FirstOrDefault(p => p.Key == action && p.Value.Instance == instance);
        if (item.Value != default)
        {
            item.Value.Cts.Dispose();

            _tracker.TryRemove(item);
        }
    }

}
