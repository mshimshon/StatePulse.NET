namespace StatePulse.Net.Engine;
public interface IDispatcherPrepper<TAction> where TAction : IAction
{
    TAction ActionInstance { get; }

    /// <summary>
    /// will await dispatch events and everything awaitable... significantly slower but good for debugging and unit tests.
    /// </summary>
    /// <returns></returns>
    IDispatcherPrepper<TAction> UsingSynchronousMode();

    /// <summary>
    /// Use this to avoid too fast triggered action by user moving too fast on ui or long running effects... good for api request actions.
    /// </summary>
    /// <returns></returns>
    Task<Guid> DispatchAsync(bool asSafe = false);

    /// <summary>
    /// Called when action vslidation fail
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    IDispatcherPrepper<TAction> HandleActionValidation(Action<ValidationResult> errors);
}
