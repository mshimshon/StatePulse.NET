namespace StatePulse.Net.Engine.Implementations;
internal class Dispatcher : IDispatcher, IDispatchHandler
{
    private readonly IServiceProvider _serviceProvider;
    private DispatchTrackingIdentity? _chainKey;
    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task MaintainChainKey(DispatchTrackingIdentity chainKey)
    {
        _chainKey = chainKey;
        return Task.CompletedTask;
    }


    public IDispatcherPrepper<TAction> Prepare<TAction>(params object[] constructor) where TAction : IAction
    {
        var instanceAction = Activator.CreateInstance(typeof(TAction), constructor)
            ?? throw new InvalidOperationException($"Cannot create instance of {typeof(TAction).Name} with given constructor parameters.");
        return CreatePrepper((TAction)instanceAction!);
    }

    public IDispatcherPrepper<TAction> Prepare<TAction>(Func<TAction> createInstance) where TAction : IAction
    {
        return CreatePrepper(createInstance.Invoke());
    }

    private IDispatcherPrepper<TAction> CreatePrepper<TAction>(TAction Instance) where TAction : IAction
    {
        var dispatcherPrepperType = typeof(DispatcherPrepper<,>).MakeGenericType(typeof(TAction), _chainKey?.EntryType ?? typeof(TAction));
        var instance = Activator.CreateInstance(dispatcherPrepperType, Instance, _serviceProvider, _chainKey)
            ?? throw new InvalidOperationException($"Cannot create instance of {typeof(TAction).Name} with given constructor parameters.");
        return (instance as IDispatcherPrepper<TAction>)!;

    }
}
