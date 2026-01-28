namespace StatePulse.Net.Engine.Implementations;

internal class DispatchFactory : IDispatchFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DispatchFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public DispatchFactoryElement CreateDispatch()
    {
        var dispatch = new Dispatcher(_serviceProvider);
        return new DispatchFactoryElement(dispatch);
    }
}
