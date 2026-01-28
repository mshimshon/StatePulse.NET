namespace StatePulse.Net;

public delegate void OnChangeEventHandler<in T>(object sender, T args);

public interface IStateAccessor
{
    event EventHandler? OnStateChangedNoDetails;
    StateVersioning Version { get; }
    T GetAs<T>() where T : class, IStateFeature;
}
public interface IStateAccessor<TState> : IStateAccessor
{
    TState State { get; }
    event OnChangeEventHandler<TState>? OnStateChanged;
    bool ChangeState(TState state, Type originType, long version, Guid dispatchWriter);
}
