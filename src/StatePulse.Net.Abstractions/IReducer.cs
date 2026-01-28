namespace StatePulse.Net;
/// <summary>
/// Use to define your reducer.
/// </summary>
public interface IReducer<TState, in TAction>
    where TState : IStateFeature
    where TAction : IAction
{
    TState Reduce(TState state, TAction action);
}
