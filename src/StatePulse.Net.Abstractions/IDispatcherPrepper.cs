namespace StatePulse.Net;

public interface IDispatcherPrepper<TAction> where TAction : IAction
{
    TAction ActionInstance { get; }
    IDispatcherPrepper<TAction> Await();
    Task<Guid> DispatchAsync(bool asSafe = false);
    IDispatcherPrepper<TAction> EffectsFirst();
    IDispatcherPrepper<TAction> ReducersFirst();
    IDispatcherPrepper<TAction> SequentialEffects();
    IDispatcherPrepper<TAction> ParallelEffects();

}
