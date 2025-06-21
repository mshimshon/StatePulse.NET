namespace StatePulse.Net.Engine.Implementations;

internal class DispatchFactory : IDispatchFactory
{
    private readonly Dispatcher _dispatcher;
    public DispatchFactory(IServiceProvider serviceProvider)
    {
        _dispatcher = new Dispatcher(serviceProvider);
    }
    public IDispatcher Dispatcher => _dispatcher;

    public IDispatchHandler DispatchHandler => _dispatcher;
}
