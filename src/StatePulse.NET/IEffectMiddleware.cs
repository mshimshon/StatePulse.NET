namespace StatePulse.Net;
public interface IEffectMiddleware
{
    Task BeforeEffect(object action);
    Task AfterEffect(object action);

}
