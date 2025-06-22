namespace StatePulse.Net;
/// <summary>
/// This is what you inject into your components to track access the state and receive updates.
/// </summary>
public delegate void OnChangeEventHandler<in T>(object sender, T args);
public interface IStateAccessor<out TState>
{
    TState State { get; }
    event OnChangeEventHandler<TState>? StateChanged;
    event EventHandler? StateChangedNoDetails;

}
