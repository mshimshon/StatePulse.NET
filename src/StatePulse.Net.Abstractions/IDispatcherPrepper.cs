namespace StatePulse.Net;

public interface IDispatcherPrepper<TAction> where TAction : IAction
{
    TAction ActionInstance { get; }
    IDispatcherPrepper<TAction> Await();
    /// <summary>
    /// Runs pipeline as same as ISafeAction with anti duplication mechanism <br/>
    /// Note: This is not the recommended way, youo should use ISafeAction instead.
    /// </summary>
    IDispatcherPrepper<TAction> AsSafe();
    /// <summary>
    /// This execute the pipeline as a regular normal untrack manner where no anti-duplication/race condition features activate.<br/>
    /// Note: this is useful to stop the tracking of a usually tracked action and make sure that action executes completely.
    /// </summary>
    IDispatcherPrepper<TAction> AsUnSafe();
    IDispatcherPrepper<TAction> DoNotAwait();
    Task DispatchAsync(CancellationToken ct = default);
    IDispatcherPrepper<TAction> EffectsFirst();
    IDispatcherPrepper<TAction> ReducersFirst();
    IDispatcherPrepper<TAction> SequentialEffects();
    IDispatcherPrepper<TAction> ParallelEffects();

}
