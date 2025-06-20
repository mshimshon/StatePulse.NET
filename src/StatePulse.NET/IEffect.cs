namespace StatePulse.Net;

/// <summary>
/// Used to define Effects
/// </summary>
/// <typeparam name="TAction"></typeparam>
public interface IEffect<TAction>
    where TAction : IAction
{
    Task EffectAsync(TAction action, IDispatcher dispatcher, Guid chainKey);
}
