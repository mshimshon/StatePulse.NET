namespace StatePulse.Net;
public interface IEffectValidator<in TAction, TEffect>
    where TAction : IAction
    where TEffect : IEffect<TAction>
{
    Task<bool> Validate(TAction action);
}
