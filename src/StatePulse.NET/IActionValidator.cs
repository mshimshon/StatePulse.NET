namespace StatePulse.Net;
public interface IActionValidator<in TAction, TEffect>
    where TAction : IAction
    where TEffect : IEffect<TAction>
{
    Task<bool> Validate(TAction action);
}
