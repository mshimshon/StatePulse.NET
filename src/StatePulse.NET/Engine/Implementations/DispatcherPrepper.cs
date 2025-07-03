
using Microsoft.Extensions.DependencyInjection;

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
    private readonly IEnumerable<IActionValidator<TAction>> _actionValidator;
    private Action<ValidationResult>? _validationErrorCallback;
    public DispatcherPrepper(TAction action, IServiceProvider serviceProvider, DispatchTrackingIdentity? chainKey)
    {
        _action = action!;
        _serviceProvider = serviceProvider;
        _statePulseRegistry = _serviceProvider.GetRequiredService<IStatePulseRegistry>();
        _actionValidator = _serviceProvider.GetServices<IActionValidator<TAction>>();
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


    protected async Task ProcessDispatch(bool entryPoint, Guid nextId)
    {
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
            var actionType = typeof(TAction);
            var effectType = typeof(IEffect<>).MakeGenericType(_action!.GetType());
            var effectServices = _serviceProvider.GetServices(effectType);
            var dispatcherService = _serviceProvider.GetRequiredService<IDispatchFactory>();
            if (nextChain != default)
                await dispatcherService.DispatchHandler.MaintainChainKey(nextChain);
            List<Task> effects = new();
            foreach (var effectService in effectServices)
            {
                if (nextChain != default && _tracker.IsCancelled(nextChain.Id))
                    break;
                var method = effectType.GetMethod(nameof(IEffect<IAction>.EffectAsync))!;
                var del = (Func<TAction, IDispatcher, Task>)Delegate.CreateDelegate(
                    typeof(Func<TAction, IDispatcher, Task>), effectService, method);
                //await del(_action, dispatcherService).ConfigureAwait(true); // to be assigned inside ExecutionContext.Run
                effects.Add(del(_action, dispatcherService.Dispatcher));
            }

            await Task.WhenAll(effects);

            foreach (var stateType in _statePulseRegistry.KnownStates)
            {
                if (nextChain != default && _tracker.IsCancelled(nextChain.Id))
                    break;
                var reducerType = typeof(IReducer<,>).MakeGenericType(stateType, actionType);
                var stateAccessorType = typeof(IStateAccessor<>).MakeGenericType(stateType);
                var stateService = _serviceProvider.GetRequiredService(stateAccessorType);
                var reducerService = _serviceProvider.GetService(reducerType);
                // Trigger Reducer
                if (reducerService != default)
                {
                    var stateProperty = stateService.GetType().GetProperty(nameof(IStateController<object>.State))!;
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
        catch
        {
            throw;
        }
        finally
        {
            // if entry point just untrack current action
            if (entryPoint && nextChain != default)
                _tracker.DeleteEntryPoint(nextChain.Id);
        }
#pragma warning restore S2737 // "catch" clauses should do more than rethrow

    }

    public IDispatcherPrepper<TAction> Sync()
    {
        _forceSyncronous = true;
        return this;
    }
    public IDispatcherPrepper<TAction> UsingSynchronousMode() => Sync();

    public IDispatcherPrepper<TAction> HandleActionValidation(Action<ValidationResult> validationResult)
    {
        _validationErrorCallback = validationResult;
        return this;
    }
    public async Task<Guid> DispatchAsync(bool asSafe = false)
    {
        // Run Validator
        if (_actionValidator.Any())
        {
            var validationResult = new ValidationResult();
            foreach (var validator in _actionValidator)
                validator.Validate(_action, ref validationResult);
            _validationErrorCallback?.Invoke(validationResult);
            if (!validationResult.IsValid)
            {
                return Guid.Empty;
            }
        }

        if ((_action is ISafeAction) || asSafe)
            return await DispatchSafeAsync();
        await DispatchFastAsync();
        return Guid.Empty;
    }
}
