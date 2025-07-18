namespace StatePulse.Net;
public delegate void OnChangeEventHandler<in T>(object sender, T args);
public interface IStateAccessor<out TState>
{
    TState State { get; }
    event OnChangeEventHandler<TState>? OnStateChanged;
    event EventHandler? OnStateChangedNoDetails;

}
