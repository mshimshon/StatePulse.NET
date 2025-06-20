namespace StatePulse.Net;
/// <summary>
/// This is what you inject into your components to track access the state and receive updates.
/// </summary>
public interface IStateAccessor<TState>
{
    TState State { get; set; }
    event EventHandler<TState>? StateChanged;

}
