namespace StatePulse.Net.Engine.Implementations;

internal class Dispatcher : IDispatcher, IDispatchHandler
{
    private readonly IServiceProvider _serviceProvider;
    private DispatchTrackingIdentity? _chainKey;
    private CancellationToken _cancelToken = default;
    private bool forcedSync = false;
    public CancellationToken CancelToken => _cancelToken;

    public bool IsCancellationRequested => IsChainKeyCancelled() || CancelToken.IsCancellationRequested;
    private bool IsChainKeyCancelled()
    {
        if (_chainKey == default) return false;
        return _chainKey.Tracker().IsCancelled(_chainKey.Id, _chainKey.Version);
    }
    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void MaintainChainKey(DispatchTrackingIdentity chainKey)
    {
        _chainKey = chainKey;
    }

    public void AssignToken(CancellationToken ct)
    {
        _cancelToken = ct;
    }
    public void NextAwaited()
    {
        forcedSync = true;
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

    public IDispatcherPrepper<TAction> Prepared<TAction>(TAction instance) where TAction : IAction
        => CreatePrepper(instance);

    private IDispatcherPrepper<TAction> CreatePrepper<TAction>(TAction Instance) where TAction : IAction
    {
        var passKeyChain = _chainKey?.EntryType ?? typeof(TAction);
        var dispatcherPrepperType = typeof(DispatcherPrepper<,>).MakeGenericType(typeof(TAction), passKeyChain);
        var instance = Activator.CreateInstance(dispatcherPrepperType, Instance, _serviceProvider, _chainKey, _cancelToken, forcedSync)
            ?? throw new InvalidOperationException($"Cannot create instance of {typeof(TAction).Name} with given constructor parameters.");
        return (instance as IDispatcherPrepper<TAction>)!;

    }
}
