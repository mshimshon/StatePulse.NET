namespace StatePulse.Net;
public interface IDispatchFactory
{
    IDispatcher Dispatcher { get; }
    IDispatchHandler DispatchHandler { get; }
}
