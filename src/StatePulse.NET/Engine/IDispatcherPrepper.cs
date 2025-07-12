namespace StatePulse.Net.Engine;
public interface IDispatcherPrepper<TAction> where TAction : IAction
{
    TAction ActionInstance { get; }
    IDispatcherPrepper<TAction> Await();
    Task<Guid> DispatchAsync(bool asSafe = false);

}
