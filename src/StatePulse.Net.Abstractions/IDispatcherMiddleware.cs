namespace StatePulse.Net;

public interface IDispatcherMiddleware
{
    Task BeforeDispatch(object action);
    Task OnDispatchFailure(Exception exception, object action);
    Task AfterDispatch(object action);
}
