using StatePulse.Net.Abstractions;

namespace StatePulse.Net;
/// <summary>
/// Inject to your components and effects to dispatch new actions.
/// </summary>
public interface IDispatcher
{
    IDispatcherPrepper<TAction> Prepare<TAction>(params object[] constructor) where TAction : IAction;
    IDispatcherPrepper<TAction> Prepare<TAction>(Func<TAction> createInstance) where TAction : IAction;
}
