namespace StatePulse.Net.Internal;
public interface IDispatcherPrepper<TAction>
{
    TAction ActionInstance { get; }

    /// <summary>
    /// will await dispatch events and everything awaitable... significantly slower but good for debugging and unit tests.
    /// </summary>
    /// <returns></returns>
    IDispatcherPrepper<TAction> UsingSynchronousMode();
    /// <summary>
    /// Run very fast without duplicate safety... good for most actions.
    /// </summary>
    /// <returns></returns>
    Task DispatchFastAsync();

    /// <summary>
    /// Use this to avoid too fast triggered action by user moving too fast on ui or long running effects... good for api request actions.
    /// </summary>
    /// <returns></returns>
    Task<Guid> DispatchSafeAsync(Guid? chainKey = default);
}
