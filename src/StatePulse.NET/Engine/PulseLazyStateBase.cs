
using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Engine.Extensions;

namespace StatePulse.Net.Engine;

/// <summary>
/// TODO: Create a SP Binder Object which will bind TState to Callback and stash is multiple different methods can used to trigger a call.
/// </summary>
internal class PulseLazyStateBase : IStatePulse
{
    private bool _disposed;
    private readonly IServiceProvider _services;
    private readonly IPulseGlobalTracker _globalStash;
    private WeakReference<object?> _instance = new WeakReference<object?>(default);
    private readonly object _lock = new();
    public IDispatcher Dispatcher { get; private set; }
    private readonly Dictionary<Type, IStateCallbackBinder> _stash = new();

    public PulseLazyStateBase(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _globalStash = services.GetRequiredService<IPulseGlobalTracker>();
        Dispatcher = services.GetRequiredService<IDispatcher>();
    }
    /// <summary>
    /// Should be implemented by children class
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual IDictionary<Type, IStateCallbackBinder> GetState() => _stash;

    public bool IsReferenceAlive() => _instance.TryGetTarget(out var _);
    public void SelfDisposeCheck()
    {
        if (!IsReferenceAlive())
        {
            Dispose();
            return;
        }
    }

    private WeakReference<object?> CheckInstanceAlive()
    {
        if (!_instance.TryGetTarget(out var target))
        {
            Dispose();
        }
        return _instance;
    }

    public TState StateOf<TState>(Func<object> getInstance, Func<Task> onStateChanged) where TState : IStateFeature
    {
        var instance = getInstance();
        if (!_instance.TryGetTarget(out var target) || !ReferenceEquals(target, instance))
            _instance = new(instance);

        IStateCallbackBinder? binder = default;
        Type stateType = typeof(TState);
        lock (_lock)
        {
            if (GetState().ContainsKey(stateType))
                binder = GetState()[stateType];
        }
        if (binder == default)
        {
            var c = onStateChanged.GetMethodInfoOrThrow();
            var service = _services.GetRequiredService<IStateAccessor<TState>>();
            binder = new StateCallbackBinder<TState>(service)
            {
                Callback = c.CreateDynamicInvoker(),
                CheckIfInstanceAlive = CheckInstanceAlive,
            };
            lock (_lock)
            {
                GetState().TryAdd(stateType, binder);
                _globalStash.Register(this);
            }
        }

        return binder.GetStateAs<TState>();

    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _globalStash.UnRegister(this);
            foreach (var item in GetState().Values)
                item.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
