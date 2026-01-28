
namespace StatePulse.Net;
/// <summary>
/// Inject to your components and effects to dispatch new actions.
/// </summary>
public interface IDispatcher
{
    IDispatcherPrepper<TAction> Prepare<TAction>(params object[] constructor) where TAction : IAction;

    [Obsolete("Use Prepared instead.")]
    IDispatcherPrepper<TAction> Prepare<TAction>(Func<TAction> createInstance) where TAction : IAction;
    IDispatcherPrepper<TAction> Prepared<TAction>(TAction instance) where TAction : IAction;

}
