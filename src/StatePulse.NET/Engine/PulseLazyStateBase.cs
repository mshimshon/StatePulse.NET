
using Microsoft.Extensions.DependencyInjection;

namespace StatePulse.Net.Engine;
internal abstract class PulseLazyStateBase : IStatePulse
{
    private bool _disposed;
    private readonly IServiceProvider _services;
    private readonly IPulseGlobalTracker _globalStash;
    private WeakReference<object?> _instance = new WeakReference<object?>(default);
    private WeakReference<Func<Task>> _listener = default!;
    public PulseLazyStateBase(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _globalStash = services.GetRequiredService<IPulseGlobalTracker>();
    }
    /// <summary>
    /// Should be implemented by children class
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual IDictionary<Type, IStateAccessor<object>> GetState() => throw new NotImplementedException();
    private TState StateOf<TState>(Func<object> getInstance) where TState : IStateFeature
    {
        var instance = getInstance();
        var type = typeof(TState);
        // Something Blazor does not clear resources so to avoid duplicate
        if (!_instance.TryGetTarget(out var target) || !ReferenceEquals(target, instance))
            _instance = new(instance);
        if (GetState().TryGetValue(type, out var existing))
            return ((IStateAccessor<TState>)existing).State;
        else
        {
            var accessorType = typeof(IStateAccessor<>).MakeGenericType(type);
            var service = _services.GetRequiredService(accessorType);
            var serviceObject = (IStateAccessor<object>)service;
            GetState().TryAdd(type, serviceObject);

            var accessor = (IStateAccessor<TState>)service;
            _globalStash.Register(this);
            accessor.StateChangedNoDetails -= OnStateChanged;
            accessor.StateChangedNoDetails += OnStateChanged;
            return accessor.State;
        }
    }
    public bool IsReferenceAlive() => _instance.TryGetTarget(out var _);
    public void SelfDisposeCheck()
    {
        if (!IsReferenceAlive())
        {
            Dispose();
            return;
        }
    }
    // Placeholder method for event handler
    private void OnStateChanged(object? sender, EventArgs Args)
    {
        if (!_instance.TryGetTarget(out var target))
        {
            Dispose();
            return;
        }
        if (_listener.TryGetTarget(out var listener))
            listener?.Invoke();
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _globalStash.UnRegister(this);
        foreach (var item in GetState().Values)
            item.StateChangedNoDetails -= OnStateChanged;

    }

    public TState StateOf<TState>(Func<object> getInstance, Func<Task> onStateChanged) where TState : IStateFeature
    {
        var instance = getInstance();
        _listener = new WeakReference<Func<Task>>(onStateChanged);
        return StateOf<TState>(() => instance);
    }
}
