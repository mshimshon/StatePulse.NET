namespace StatePulse.Net.Internal.Implementations;
internal class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IDispatcherPrepper<TAction> Prepare<TAction>(params object[] constructor)
    {
        var instanceAction = Activator.CreateInstance(typeof(TAction), constructor)
            ?? throw new InvalidOperationException($"Cannot create instance of {typeof(TAction).Name} with given constructor parameters.");
        return CreatePrepper((TAction)instanceAction!);
    }

    public IDispatcherPrepper<TAction> Prepare<TAction>(Func<TAction> createInstance)
    {
        return CreatePrepper(createInstance.Invoke());
    }

    private IDispatcherPrepper<TAction> CreatePrepper<TAction>(TAction Instance)
    {
        var dispatcherPrepperType = typeof(DispatcherPrepper<>).MakeGenericType(typeof(TAction));
        var instance = Activator.CreateInstance(dispatcherPrepperType, Instance, _serviceProvider)
            ?? throw new InvalidOperationException($"Cannot create instance of {typeof(TAction).Name} with given constructor parameters.");
        return (instance as IDispatcherPrepper<TAction>)!;

    }
}
