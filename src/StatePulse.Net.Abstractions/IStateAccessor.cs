namespace StatePulse.Net;

public delegate void OnChangeEventHandler<in T>(object sender, T args);
public interface IStateAccessor<TState>
{
    TState State { get; }
    StateVersioning Version { get; }
    event OnChangeEventHandler<TState>? OnStateChanged;
    event EventHandler? OnStateChangedNoDetails;
    bool ChangeState(TState state, Type originType, long version, Guid dispatchWriter);
}
