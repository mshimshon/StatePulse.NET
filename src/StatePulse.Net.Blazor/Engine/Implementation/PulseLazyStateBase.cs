using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace StatePulse.Net.Blazor.Engine.Implementation;
internal abstract class PulseLazyStateBase : IPulse
{
    private bool _disposed;
    private readonly IServiceProvider _services;
    private readonly IPulseGlobalTracker _globalStash;
    private WeakReference<object?> _instance = new WeakReference<object?>(default);
    public PulseLazyStateBase(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _globalStash = services.GetRequiredService<IPulseGlobalTracker>();
    }
    protected virtual IDictionary<Type, IStateAccessor<object>> GetState() => throw new NotImplementedException();
    public TState StateOf<TState>(object instance) where TState : IStateFeature
    {
        var type = typeof(TState);
        // Something Blazor does not clear resources so to avoid duplicate
        if (!_instance.TryGetTarget(out var target) || !ReferenceEquals(target, instance))
        {
            _instance = new(instance);


        }
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
    public bool IsReferenceAlive() => _instance.TryGetTarget(out object? _);
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
        Console.WriteLine(sender!.GetType()!);
        var invokeAsyncMethod = typeof(ComponentBase).GetMethod(
            "InvokeAsync",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null,
            new[] { typeof(Action) },
            null
        );

        invokeAsyncMethod?.Invoke(target, [new Action(() =>
        {
            var method = typeof(ComponentBase).GetMethod(
                "StateHasChanged",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            );
            method?.Invoke(target, null);
        })]);
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _globalStash.UnRegister(this);
        foreach (var item in GetState().Values)
            item.StateChangedNoDetails -= OnStateChanged;

    }

}
