
using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Configuration;
using System.Reflection;
namespace StatePulse.Net.Engine.Implementations;

internal partial class DispatcherPrepper<TAction, TActionChain> : IDispatcherPrepper<TAction>
    where TAction : IAction
    where TActionChain : IAction
{
    private readonly TAction _action;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStatePulseRegistry _statePulseRegistry;
    private readonly DispatchTrackingIdentity? _chainKey;
    private readonly IDispatchTracker<TActionChain> _tracker;
    private long _currentVersion = -1;
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
        if (_chainKey == default)
            if (_forceSyncronous)
                await ProcessDispatch(false, Guid.Empty);
            else
                _ = ProcessDispatch(false, Guid.Empty);
        else
        {
            if (IsChainCancelled())
                return;
            if (_forceSyncronous)
                await ProcessDispatch(false, Guid.Empty);
            else
                _ = ProcessDispatch(false, Guid.Empty);
        }

    }

    public async Task<Guid> DispatchSafeAsync()
    {
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
        {
            // ignore requested dispatch if cancelled
            if (!IsChainCancelled())
                if (_forceSyncronous)
                    await ProcessDispatch(false, Guid.Empty);
                else
                    _ = ProcessDispatch(false, Guid.Empty);
        }

        return _chainKey?.Id ?? Guid.Empty;
    }

    private static MethodInfo? _cachedEffectMethod;
    private static MethodInfo? _cachedActionValidatorMethod;
    private static readonly object _lock = new();
    protected async Task ProcessDispatch(bool entryPoint, Guid nextId)
    {
        // we are tracking next chain key only when there is an entry point to avoid tracker during fast dispatch.
        // by default it will be null otherwise chain should be passed down the next calls.
        var nextChain = _chainKey;
        if (entryPoint)
        {
            _currentVersion = ServiceRegisterExt.ConfigureOptions.GetNextVersion();
            nextChain = new DispatchTrackingIdentity()
            {
                Id = nextId,
                EntryType = typeof(TAction),
                Version = _currentVersion,
                Tracker = () => _tracker
            };
            bool preCancel = _tracker.CreateExecutingAction(nextChain.Id, this, nextChain.Version);
            nextChain.TrackedEntry = _tracker.CreateEntryPoint(nextChain.Id, this);
            if (!preCancel)
                return;
        }


        var disaptchMiddlewares = _serviceProvider.GetServices<IDispatcherMiddleware>();
        var dispatchMiddlewareTasks = new List<Task>();
        foreach (var item in disaptchMiddlewares)
        {
            dispatchMiddlewareTasks.Add(item.BeforeDispatch(_action));
        }
        await Task.WhenAll(dispatchMiddlewareTasks);

        if (IsChainCancelled(nextChain))
            return;

        try
        {
            var dispatcherService = _serviceProvider.GetRequiredService<IDispatchFactory>();
            var dispatchElement = dispatcherService.CreateDispatch();
            if (nextChain != default)
                await dispatchElement.Handler.MaintainChainKey(nextChain);
            if (_dispatchOrdering == DispatchOrdering.ReducersFirst)
                await RunReducer(nextChain);

            await RunEffects(nextChain, dispatchElement);

            if (_dispatchOrdering == DispatchOrdering.EffectsFirst)
                await RunReducer(nextChain);

        }
        catch (Exception ex)
        {
            foreach (var item in disaptchMiddlewares)
                _ = item.OnDispatchFailure(ex, _action);



            if (_forceSyncronous)
                throw;
        }
        finally
        {

            dispatchMiddlewareTasks = new List<Task>();
            foreach (var item in disaptchMiddlewares)
            {
                dispatchMiddlewareTasks.Add(item.AfterDispatch(_action));
            }
            await Task.WhenAll(dispatchMiddlewareTasks);
        }

    }
    public async Task<Guid> DispatchAsync(bool asSafe = false)
    {

        if ((_action is ISafeAction) || asSafe)
            return await DispatchSafeAsync();
        await DispatchFastAsync();
        return Guid.Empty;
    }

    private async Task RunEffects(DispatchTrackingIdentity? nextChain, DispatchFactoryElement dispatcherService)
    {
        var effectType = typeof(IEffect<>).MakeGenericType(_action!.GetType());

        List<Task> effects = new();
        var effectServices = _serviceProvider.GetServices(effectType);

        var effectMiddlewares = _serviceProvider.GetServices<IEffectMiddleware>();
        var effectMiddlewareTasks = RunMiddlewareEffect(effectMiddlewares, p => p.BeforeEffect(_action), p => p == MiddlewareEffectBehavior.PerGroupEffects);

        if (ServiceRegisterExt.ConfigureOptions.MiddlewareTaskBehavior == Configuration.MiddlewareTaskBehavior.Await)
            await effectMiddlewareTasks;

        foreach (var effectService in effectServices)
        {
            if (IsChainCancelled(nextChain))
                return;

            bool validationPassed = await RunEffectValidators(effectService!.GetType(), effectMiddlewares);
            if (!validationPassed)
                continue; // skip


            lock (_lock)
                if (_cachedEffectMethod == default)
                    _cachedEffectMethod = effectType.GetMethod(nameof(IEffect<IAction>.EffectAsync))!;


            var del = (Func<TAction, IDispatcher, Task>)Delegate.CreateDelegate(typeof(Func<TAction, IDispatcher, Task>), effectService, _cachedEffectMethod);
            if (del(_action, dispatcherService.Dispatcher) is Task effectTask)
            {
                effects.Add(effectTask);
                if (!_forceSyncronous && _dispatchEffectBehavior == DispatchEffectBehavior.Parallel)
                    _ = effectTask;
                else
                    await effectTask;
            }
        }
        Task allEffects = Task.WhenAll(effects);

        if (!allEffects.IsCompletedSuccessfully)
            await allEffects;

        if (ServiceRegisterExt.ConfigureOptions.MiddlewareTaskBehavior != Configuration.MiddlewareTaskBehavior.Await)
            _ = Task.Run(async () =>
            {
                if (!effectMiddlewareTasks.IsCompletedSuccessfully)
                    await effectMiddlewareTasks;
                await RunMiddlewareEffect(effectMiddlewares, p => p.AfterEffect(_action), p => p == MiddlewareEffectBehavior.PerGroupEffects);
            });
        else
        {
            if (!effectMiddlewareTasks.IsCompletedSuccessfully)
                await effectMiddlewareTasks;
            await RunMiddlewareEffect(effectMiddlewares, p => p.AfterEffect(_action), p => p == MiddlewareEffectBehavior.PerGroupEffects);
        }


    }
    private async Task<bool> RunEffectValidators(Type effectService, IEnumerable<IEffectMiddleware> effectMiddlewares)
    {
        //TODO: Optimize This, Caching, Redundent GetServices...
        var effectValidator = typeof(IEffectValidator<,>).MakeGenericType(_action!.GetType(), effectService);

        var effectValidators = _serviceProvider.GetServices(effectValidator);
        if (effectValidators.Count() > 0)
            if (_cachedActionValidatorMethod == default)
                _cachedActionValidatorMethod = effectValidators.First()!.GetType().GetMethod(nameof(IEffectValidator<IAction, IEffect<IAction>>.Validate))!;
        Dictionary<object, Task<bool>> validationResults = new();
        foreach (var validator in effectValidators)
        {
            validationResults[validator!] = (Task<bool>)_cachedActionValidatorMethod!.Invoke(validator, new object[] { _action })!;
        }
        await Task.WhenAll(validationResults.Values);
        bool passed = true;
        foreach (var validatorNResults in validationResults)
        {
            bool pass = await validatorNResults.Value;
            if (!pass)
            {
                passed = false;
                _ = RunMiddlewareEffect(effectMiddlewares, p => p.WhenEffectValidationFailed(_action, validatorNResults.Key), p => true);
            }
            else
            {
                _ = RunMiddlewareEffect(effectMiddlewares, p => p.WhenEffectValidationSucceed(_action, validatorNResults.Key), p => true);
            }
        }
        return passed;
    }
    private async Task RunReducer(DispatchTrackingIdentity? nextChain)
    {
        var actionType = typeof(TAction);
        var middlewares = _serviceProvider.GetServices<IReducerMiddleware>();
        foreach (var stateType in _statePulseRegistry.KnownStates)
        {

            if (IsChainCancelled(nextChain))
                return;
            var reducerType = typeof(IReducer<,>).MakeGenericType(stateType, actionType);
            var stateAccessorType = _statePulseRegistry.KnownStateToAccessors[stateType];
            var stateService = () => _serviceProvider.GetRequiredService(stateAccessorType);
            var reducerService = _serviceProvider.GetService(reducerType);
            // Trigger Reducer
            if (reducerService != default)
            {

                var currentState = _statePulseRegistry.KnownStateAccessorsStateGetter[stateAccessorType](stateService())!;
                var middlewareTasks = RunMiddlewareReducer(middlewares, p => p.BeforeReducing(currentState, _action));
                if (ServiceRegisterExt.ConfigureOptions.MiddlewareTaskBehavior == Configuration.MiddlewareTaskBehavior.Await)
                    await middlewareTasks;

                var newState = _statePulseRegistry.KnownReducersReduceMethod[reducerType](reducerService,
                    new object[] {
                        currentState,
                        _action }!)!;

                if (IsChainCancelled(nextChain))
                    return;

                if (newState == null) throw new InvalidOperationException("Reducer returned null state.");

                long usedVersion = -1;
                if (nextChain != default)
                    usedVersion = nextChain.Version;
                if (usedVersion <= -1) usedVersion = _currentVersion;

                Type originType = typeof(TAction);
                if (nextChain != default)
                    originType = nextChain.EntryType;
                var isAccepted = _statePulseRegistry.KnownStateAccessorsStateUpdater[stateAccessorType](stateService(), newState, originType, usedVersion, nextChain?.Id ?? Guid.Empty);
                var stateVersioning = (StateVersioning)_statePulseRegistry.KnownStateAccessorsVersionGetter[stateAccessorType].Invoke(stateService())!;

                if (!isAccepted) return;
                // Regardless of settings ensure BeforeReducing is finished before calling after reducing.
                if (ServiceRegisterExt.ConfigureOptions.MiddlewareTaskBehavior != Configuration.MiddlewareTaskBehavior.Await)
                    _ = Task.Run(async () =>
                    {
                        if (!middlewareTasks.IsCompletedSuccessfully)
                            await middlewareTasks;
                        await RunMiddlewareReducer(middlewares, p => p.AfterReducing(newState, _action));
                    });
                else
                {
                    if (!middlewareTasks.IsCompletedSuccessfully)
                        await middlewareTasks;
                    await RunMiddlewareReducer(middlewares, p => p.AfterReducing(newState, _action));

                }

            }
        }
    }

    private Task RunMiddlewareEffect(IEnumerable<IEffectMiddleware> middlewares, Func<IEffectMiddleware, Task> func, Func<MiddlewareEffectBehavior, bool> conditionBehavior)
    {
        if (!conditionBehavior(ServiceRegisterExt.ConfigureOptions.MiddlewareEffectBehavior))
            return Task.CompletedTask;

        var effectMiddlewaresTasks = new List<Task>();
        foreach (var item in middlewares)
        {
            effectMiddlewaresTasks.Add(func(item));
        }
        return Task.WhenAll(effectMiddlewaresTasks);


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
    private bool IsChainCancelled(DispatchTrackingIdentity? nextChain = default)
        =>
        (nextChain != default && (nextChain.TrackedEntry.IsCancelled || nextChain.Tracker().IsCancelled(nextChain.Id, nextChain.Version))) ||
        (_chainKey != default && (_chainKey.TrackedEntry.IsCancelled || _chainKey.Tracker().IsCancelled(_chainKey.Id, _chainKey.Version)));
}

