namespace StatePulse.Net.Engine.Implementations;
internal class StateAccessor<TState> : IStateAccessor<TState> where TState : class
{
    public StateAccessor()
    {
        InitializeState();
    }
    private TState _state;
    public TState State
    {
        get => _state; set
        {

            // make sure state can never be set null always has a default state.
            if (value == null) InitializeState();
            else _state = value;
            StateChanged?.Invoke(this, _state);

        }
    }

    public event EventHandler<TState>? StateChanged;

    private void InitializeState()
    {
        // add default initialized state.
        var obj = Activator.CreateInstance<TState>();
        _state = obj;
    }
}
