namespace StatePulse.Net;
public interface IReducerMiddleware
{
    Task BeforeReducing(object state, object action);
    Task AfterReducing(object state, object action);
}
