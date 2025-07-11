namespace StatePulse.Net;
public interface IEffectMiddleware
{
    Task BeforeEffect(object action);
    Task AfterEffect(object action);
    Task WhenEffectValidationFailed(object action, object effectValidator);
    Task WhenEffectValidationSucceed(object action, object effectValidator);

}
