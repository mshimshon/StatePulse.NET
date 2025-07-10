namespace StatePulse.Net.Engine.Implementations;
internal class StateAccessor<TState> : IStateController<TState>, IStateAccessor<TState> where TState : IStateFeature
{
    public StateAccessor()
    {
        InitializeState();
    }
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

    private void InitializeState()
    {
        // add default initialized state.
        var obj = Activator.CreateInstance<TState>();
        _state = obj;
    }
}
