namespace StatePulse.Net;
internal interface IDispatcherMiddleware
{
    Task BeforeDispatch(object action);
    Task AfterDispatch(object action);
}
