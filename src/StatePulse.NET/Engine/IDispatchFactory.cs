namespace StatePulse.Net.Engine;
public interface IDispatchFactory
{
    IDispatcher Dispatcher { get; }
    IDispatchHandler DispatchHandler { get; }
}
