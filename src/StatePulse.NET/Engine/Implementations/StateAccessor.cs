namespace StatePulse.Net.Engine.Implementations;

using StatePulse.Net;
using StatePulse.Net.Engine;

internal class StateAccessor<TState> : IStateController<TState>, IStateAccessor<TState> where TState : IStateFeature
{
    public StateAccessor()
    {
        InitializeState();
    }
    public StateVersioning Version { get; set; } = new(typeof(TState), -1, Guid.Empty);
    private TState _state = default!;

    public TState State
    {
        get => _state; set
        {

            // make sure state can never be set null always has a default state.
            if (value == null) InitializeState();
            else _state = value;
            OnStateChanged?.Invoke(this, _state);
            OnStateChangedNoDetails?.Invoke(this, new EventArgs());
        }
    }

    public event OnChangeEventHandler<TState>? OnStateChanged;
    public event EventHandler? OnStateChangedNoDetails;

    public readonly Object _lock = new object();
    public bool ChangeState(TState state, Type originType, long version, Guid dispatchWriter)
    {
        lock (_lock)
        {
            if (version >= 0 && Version.Version > version)
                return false;
            Version = new(originType, version, dispatchWriter);
            State = state;
            return true;
        }

    }
    private void InitializeState()
    {
        // add default initialized state.
        var obj = Activator.CreateInstance<TState>();
        _state = obj;
    }

    public T GetAs<T>() where T : class, IStateFeature
        => (State as T)!;
}
