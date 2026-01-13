using StatePulse.Net.Engine;
using StatePulse.Net.Engine.Implementations;

namespace StatePulse.Net;

internal class DispatchFactoryElement
{
    public DispatchFactoryElement(Dispatcher dispatcher)
    {
        Handler = dispatcher;
        Dispatcher = dispatcher;
    }

    public IDispatchHandler Handler { get; }
    public IDispatcher Dispatcher { get; }
}
