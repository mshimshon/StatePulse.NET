namespace StatePulse.Net.Engine;
public interface IDispatcherPrepper<TAction> where TAction : IAction
{
    TAction ActionInstance { get; }

    /// <summary>
    /// will await dispatch events and everything awaitable... significantly slower but good for debugging and unit tests.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Deprecated use Sync instead.")]
    IDispatcherPrepper<TAction> UsingSynchronousMode();

    /// <summary>
    /// Execute as Sync (await all the pipeline until the end or until not awaited task is triggered)
    /// </summary>
    /// <returns></returns>
    IDispatcherPrepper<TAction> Sync();

    /// <summary>
    /// Use this to avoid too fast triggered action by user moving too fast on ui or long running effects... good for api request actions.
    /// </summary>
    /// <returns></returns>
    Task<Guid> DispatchAsync(bool asSafe = false);

}
