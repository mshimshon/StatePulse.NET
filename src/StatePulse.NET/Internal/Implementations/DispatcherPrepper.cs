
using Microsoft.Extensions.DependencyInjection;

namespace StatePulse.Net.Internal.Implementations;
internal class DispatcherPrepper<TAction> : IDispatcherPrepper<TAction>
{
    private bool _forceSyncronous;
    private readonly TAction _action;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStatePulseRegistry _statePulseRegistry;

    public DispatcherPrepper(TAction action, IServiceProvider serviceProvider)
    {
        _action = action!;
        _serviceProvider = serviceProvider;
        _statePulseRegistry = _serviceProvider.GetRequiredService<IStatePulseRegistry>();
    }

    public TAction ActionInstance => _action;

    public async Task DispatchFastAsync()
    {
        if (_forceSyncronous)
            await ProcessDispatch(true, Guid.Empty);
        else
            _ = ProcessDispatch(true, Guid.Empty);
    }

    public async Task<Guid> DispatchSafeAsync(Guid? chainKey = default)
    {
        var id = Guid.Empty;
        var ck = chainKey ?? Guid.Empty;
        if (ck == Guid.Empty)
        {
            id = Guid.NewGuid();
            if (_forceSyncronous)
                await ProcessDispatch(true, id);
            else
                _ = ProcessDispatch(true, id);
        }
        else
            // ignore requested dispatch if cancelled
            if (!_statePulseRegistry.IsDispatchCancelled(ck)) await ProcessDispatch(false, ck);
        return chainKey == Guid.Empty ? id : ck;
    }


    protected async Task ProcessDispatch(bool entryPoint, Guid ck)
    {


        // when entry point for the chain awaited define token in asynclocal.
        if (entryPoint == true && ck != Guid.Empty)
        {
            _statePulseRegistry.CancelDispatch(typeof(TAction)); // preemptive cancel for previous runners.
            _statePulseRegistry.TrackDispatch(typeof(TAction), this, ck);
        }
#pragma warning disable S2737 // "catch" clauses should do more than rethrow
        try
        {
            var actionType = typeof(TAction);
            var effectType = typeof(IEffect<>).MakeGenericType(_action!.GetType());
            var effectServices = _serviceProvider.GetServices(effectType);
            var dispatcherService = _serviceProvider.GetRequiredService<IDispatcher>();

            List<Task> effects = new();
            foreach (var effectService in effectServices)
            {
                var method = effectType.GetMethod(nameof(IEffect<IAction>.EffectAsync))!;
                var del = (Func<TAction, IDispatcher, Guid, Task>)Delegate.CreateDelegate(
                    typeof(Func<TAction, IDispatcher, Guid, Task>), effectService, method);
                //await del(_action, dispatcherService).ConfigureAwait(true); // to be assigned inside ExecutionContext.Run
                effects.Add(del(_action, dispatcherService, ck));
            }

            await Task.WhenAll(effects);

            foreach (var stateType in _statePulseRegistry.KnownStates)
            {
                if (ck != Guid.Empty && _statePulseRegistry.IsDispatchCancelled(ck))
                    break;
                var reducerType = typeof(IReducer<,>).MakeGenericType(stateType, actionType);
                var stateAccessorType = typeof(IStateAccessor<>).MakeGenericType(stateType);
                var stateService = _serviceProvider.GetRequiredService(stateAccessorType);
                var reducerService = _serviceProvider.GetService(reducerType);
                // Trigger Reducer
                if (reducerService != default)
                {
                    var stateProperty = stateService.GetType().GetProperty(nameof(IStateAccessor<object>.State))!;
                    var method = reducerType.GetMethod(nameof(IReducer<IStateFeature, IAction>.ReduceAsync))!;
                    var task = (Task)method.Invoke(reducerService, new[] { stateProperty.GetValue(stateService)!, _action })!;
                    await task.ConfigureAwait(true);
                    // Use reflection to get the Result property (only available on Task<T>)
                    var taskType = task.GetType();
                    var resultProperty = taskType.GetProperty("Result");
                    var newState = resultProperty?.GetValue(task);
                    stateProperty.SetValue(stateService, newState);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            await Task.Delay(1000);
            // if entry point just untrack current action
            if (entryPoint == true && ck != Guid.Empty)
                _statePulseRegistry.UntrackDispatch(typeof(TAction), this);
        }
#pragma warning restore S2737 // "catch" clauses should do more than rethrow

    }

    public IDispatcherPrepper<TAction> UsingSynchronousMode()
    {
        _forceSyncronous = true;
        return this;
    }
}
