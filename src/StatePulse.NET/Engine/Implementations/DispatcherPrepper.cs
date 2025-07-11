
using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Configuration;
using System.Collections.Concurrent;
using System.Reflection;

namespace StatePulse.Net.Engine.Implementations;
internal class DispatcherPrepper<TAction, TActionChain> : IDispatcherPrepper<TAction>
    where TAction : IAction
    where TActionChain : IAction
{
    private bool _forceSyncronous;
    private readonly TAction _action;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStatePulseRegistry _statePulseRegistry;
    private readonly DispatchTrackingIdentity? _chainKey;
    private readonly IDispatchTracker<TActionChain> _tracker;

    public DispatcherPrepper(TAction action, IServiceProvider serviceProvider, DispatchTrackingIdentity? chainKey)
    {
        _action = action!;
        _serviceProvider = serviceProvider;
        _statePulseRegistry = _serviceProvider.GetRequiredService<IStatePulseRegistry>();
        _chainKey = chainKey;
        var trackerTypeAction = typeof(IDispatchTracker<>).MakeGenericType(typeof(TActionChain));
        _tracker = (IDispatchTracker<TActionChain>)_serviceProvider.GetRequiredService(trackerTypeAction);
    }

    public TAction ActionInstance => _action;

    public async Task DispatchFastAsync()
    {
        if (_chainKey != default && _tracker.IsCancelled(_chainKey.Id))
            return;
        if (_chainKey == default)
            if (_forceSyncronous)
                await ProcessDispatch(false, Guid.Empty);
            else
                _ = ProcessDispatch(false, Guid.Empty);
        else
        {
            if (_forceSyncronous)
                await ProcessDispatch(false, Guid.Empty);
            else
                _ = ProcessDispatch(false, Guid.Empty);
        }

    }

    public async Task<Guid> DispatchSafeAsync()
    {
        if (_chainKey != default && _tracker.IsCancelled(_chainKey.Id))
            return _chainKey.Id;
        if (_chainKey == default)
        {
            Guid nextKey = Guid.NewGuid();
            if (_forceSyncronous)
                await ProcessDispatch(true, nextKey);
            else
                _ = ProcessDispatch(true, nextKey);
            return nextKey;
        }
        else
            // ignore requested dispatch if cancelled
            if (!_tracker.IsCancelled(_chainKey.Id)) await ProcessDispatch(false, Guid.Empty);
        return _chainKey?.Id ?? Guid.Empty;
    }

    private static MethodInfo? _cachedEffectMethod;
    private static MethodInfo? _cachedActionValidatorMethod;
    private static readonly ConcurrentDictionary<Type, Type> _cachedEffectInterfaceTypes = new();
    protected async Task ProcessDispatch(bool entryPoint, Guid nextId)
    {
        var disaptchMiddlewares = _serviceProvider.GetServices<IDispatcherMiddleware>();
        var dispatchMiddlewareTasks = new List<Task>();
        foreach (var item in disaptchMiddlewares)
        {
            dispatchMiddlewareTasks.Add(item.BeforeDispatch(_action));
        }
        await Task.WhenAll(dispatchMiddlewareTasks);
        // we are tracking next chain key only when there is an entry point to avoid tracker during fast dispatch.
        // by default it will be null otherwise chain should be passed down the next calls.
        var nextChain = _chainKey;
        if (entryPoint)
        {
            nextChain = new DispatchTrackingIdentity()
            {
                Id = nextId,
                EntryType = typeof(TAction)
            };
            _tracker.CreateEntryPoint(nextChain.Id, this);
        }
#pragma warning disable S2737 // "catch" clauses should do more than rethrow
        try
        {
            var dispatcherService = _serviceProvider.GetRequiredService<IDispatchFactory>();
            if (nextChain != default)
                await dispatcherService.DispatchHandler.MaintainChainKey(nextChain);

            await RunEffects(nextChain, dispatcherService);
            await RunReducer(nextChain);

        }
        catch
        {
            throw;
        }
        finally
        {
            // if entry point just untrack current action
            if (entryPoint && nextChain != default)
                _tracker.DeleteEntryPoint(nextChain.Id);

            dispatchMiddlewareTasks = new List<Task>();
            foreach (var item in disaptchMiddlewares)
            {
                dispatchMiddlewareTasks.Add(item.AfterDispatch(_action));
            }
            await Task.WhenAll(dispatchMiddlewareTasks);
        }
#pragma warning restore S2737 // "catch" clauses should do more than rethrow

    }

    public IDispatcherPrepper<TAction> Await()
    {
        _forceSyncronous = true;
        return this;
    }
    public async Task<Guid> DispatchAsync(bool asSafe = false)
    {
        if ((_action is ISafeAction) || asSafe)
            return await DispatchSafeAsync();
        await DispatchFastAsync();
        return Guid.Empty;
    }

    private async Task RunMiddlewareEffect(IEnumerable<IEffectMiddleware> middlewares, Func<IEffectMiddleware, Task> func, MiddlewareEffectBehavior typeOfEffect)
    {
        if (ServiceRegisterExt._configureOptions.MiddlewareEffectBehavior != typeOfEffect)
            return;

        var effectMiddlewaresTasks = new List<Task>();
        foreach (var item in middlewares)
        {
            effectMiddlewaresTasks.Add(func(item));
        }

        if (ServiceRegisterExt._configureOptions.MiddlewareTaskBehavior == Configuration.MiddlewareTaskBehavior.Await)
            await Task.WhenAll(effectMiddlewaresTasks);
        else
            _ = Task.WhenAll(effectMiddlewaresTasks);
    }
    private Task RunMiddlewareReducer(IEnumerable<IReducerMiddleware> middlewares, Func<IReducerMiddleware, Task> func)
    {

        var middlewaresTasks = new List<Task>();
        foreach (var item in middlewares)
        {
            middlewaresTasks.Add(func(item));
        }
        return Task.WhenAll(middlewaresTasks);
    }
    private async Task RunEffects(DispatchTrackingIdentity? nextChain, IDispatchFactory dispatcherService)
    {
        var effectType = typeof(IEffect<>).MakeGenericType(_action!.GetType());

        List<Task> effects = new();
        var effectServices = _serviceProvider.GetServices(effectType);

        var effectMiddlewares = _serviceProvider.GetServices<IEffectMiddleware>();
        await RunMiddlewareEffect(effectMiddlewares, p => p.BeforeEffect(_action), MiddlewareEffectBehavior.PerGroupEffects);

        foreach (var effectService in effectServices)
        {
            if (nextChain != default && _tracker.IsCancelled(nextChain.Id))
                break;
            await RunMiddlewareEffect(effectMiddlewares, p => p.BeforeEffect(_action), MiddlewareEffectBehavior.PerIndividualEffect);

            bool validationFailed = await RunEffectValidators(effectService!.GetType());
            if (validationFailed) continue; // skip

            if (_cachedEffectMethod == default)
                _cachedEffectMethod = effectType.GetMethod(nameof(IEffect<IAction>.EffectAsync))!;

            var del = (Func<TAction, IDispatcher, Task>)Delegate.CreateDelegate(typeof(Func<TAction, IDispatcher, Task>), effectService, _cachedEffectMethod);
            if (del(_action, dispatcherService.Dispatcher) is Task effectTask)
            {
                effects.Add(effectTask);
                if (ServiceRegisterExt._configureOptions.DispatchEffectBehavior == DispatchEffectBehavior.Sequential)
                    await effectTask;
            }

            await RunMiddlewareEffect(effectMiddlewares, p => p.AfterEffect(_action), MiddlewareEffectBehavior.PerIndividualEffect);
        }
        await Task.WhenAll(effects);

        await RunMiddlewareEffect(effectMiddlewares, p => p.AfterEffect(_action), MiddlewareEffectBehavior.PerGroupEffects);
    }
    private async Task<bool> RunEffectValidators(Type effectService)
    {
        var effectValidator = typeof(IEffectValidator<,>).MakeGenericType(_action!.GetType(), effectService);

        var effectValidators = _serviceProvider.GetServices(effectValidator);
        if (effectValidators.Count() > 0)
            if (_cachedActionValidatorMethod == default)
                _cachedActionValidatorMethod = effectValidators.First()!.GetType()
                    .GetMethod(nameof(IEffectValidator<IAction, IEffect<IAction>>.Validate))!;
        var effectValidatorRunners = effectValidators
            .Select(p => (Task<bool>)_cachedActionValidatorMethod!.Invoke(p, new object[] { _action })!);
        var validationResult = await Task.WhenAll(effectValidatorRunners);
        return validationResult.Any(p => !p);
    }
    private async Task RunReducer(DispatchTrackingIdentity? nextChain)
    {
        var actionType = typeof(TAction);
        var middlewares = _serviceProvider.GetServices<IReducerMiddleware>();

        foreach (var stateType in _statePulseRegistry.KnownStates)
        {
            if (nextChain != default && _tracker.IsCancelled(nextChain.Id))
                break;
            var reducerType = typeof(IReducer<,>).MakeGenericType(stateType, actionType);
            var stateAccessorType = _statePulseRegistry.KnownStateToAccessors[stateType];
            var stateService = _serviceProvider.GetRequiredService(stateAccessorType);
            var reducerService = _serviceProvider.GetService(reducerType);
            // Trigger Reducer
            if (reducerService != default)
            {

                var currentState = _statePulseRegistry.KnownStateAccessorsStateGetter[stateAccessorType](stateService)!;
                var middlewareTasks = RunMiddlewareReducer(middlewares, p => p.BeforeReducing(currentState, _action));
                if (ServiceRegisterExt._configureOptions.MiddlewareTaskBehavior == Configuration.MiddlewareTaskBehavior.Await)
                    await middlewareTasks;

                var reduceTask = (Task)_statePulseRegistry.KnownReducersReduceMethod[reducerType](reducerService,
                    new object[] {
                        currentState,
                        _action }!)!;

                await reduceTask;
                // make sure middlewares are done before starting to call after reduced
                await middlewareTasks;

                var newState = _statePulseRegistry.KnownReducersTaskResult[reducerType](reduceTask);
                if (newState == null) throw new InvalidOperationException("Reducer returned null state.");

                _statePulseRegistry.KnownStateAccessorsStateSetter[stateAccessorType](stateService, newState);
                middlewareTasks = RunMiddlewareReducer(middlewares, p => p.AfterReducing(newState, _action));
                if (ServiceRegisterExt._configureOptions.MiddlewareTaskBehavior == Configuration.MiddlewareTaskBehavior.Await)
                    await middlewareTasks;
            }
        }
    }

}

