using StatePulse.Net.Engine.Extensions;

namespace StatePulse.Net.Engine.Implementations;
internal class StatePulseRegistry : IStatePulseRegistry
{
    private readonly List<Type> _knownStates = new();
    private readonly Dictionary<Type, Type> _knownStateToAccessors = new();
    private readonly Dictionary<Type, Func<object, object?>> _knownStateAccessorsStateGetter = new();
    private readonly Dictionary<Type, Action<object, object?>> _knownStateAccessorsStateSetter = new();

    private readonly Dictionary<Type, Type> _knownEffects = new();
    private readonly Dictionary<Type, Type> _knownReducers = new();
    private readonly Dictionary<Type, Type> _knownStateToReducers = new();
    private readonly Dictionary<Type, Func<object, object?[], object?>> _knownReducersReduceMethod = new();
    private readonly Dictionary<Type, Func<object, object?>> _knownReducersTaskResult = new();
    private readonly List<Type> _knownActions = new();
    private readonly Dictionary<Type, Type> _knownActionValidators = new();

    public IReadOnlyList<Type> KnownStates => _knownStates;
    public IReadOnlyDictionary<Type, Type> KnownEffects => _knownEffects;
    public IReadOnlyDictionary<Type, Type> KnownReducers => _knownReducers;
    public IReadOnlyList<Type> KnownActions => _knownActions;
    public IReadOnlyDictionary<Type, Type> KnownActionValidators => _knownActionValidators;
    public IReadOnlyDictionary<Type, Func<object, object?>> KnownStateAccessorsStateGetter => _knownStateAccessorsStateGetter;
    public IReadOnlyDictionary<Type, Action<object, object?>> KnownStateAccessorsStateSetter => _knownStateAccessorsStateSetter;

    public IReadOnlyDictionary<Type, Func<object, object?[], object?>> KnownReducersReduceMethod => _knownReducersReduceMethod;

    public IReadOnlyDictionary<Type, Func<object, object?>> KnownReducersTaskResult => _knownReducersTaskResult;

    public IReadOnlyDictionary<Type, Type> KnownStateToAccessors => _knownStateToAccessors;

    public void RegisterEffect(Type effectType, Type interfaceType) => _knownEffects[effectType] = interfaceType;
    public void RegisterReducer(Type reducerType, Type interfaceType)
    {
        var reduceMethodName = nameof(IReducer<IStateFeature, IAction>.ReduceAsync);
        var method = reducerType.GetMethod(reduceMethodName)!;
        var returnTaskType = method.ReturnType;
        var stateType = returnTaskType.GetGenericArguments()[0]; // This is TState
        _knownReducersTaskResult[reducerType] = stateType.BuildTaskResultGetter();
        _knownReducersReduceMethod[reducerType] = method.CreateDynamicReflectionInvoker();
        _knownReducers.TryAdd(reducerType, interfaceType);
    }
    public void RegisterState(Type stateType)
    {
        var accessorType = typeof(IStateAccessor<>).MakeGenericType(stateType);
        var accessorImplementationType = typeof(StateAccessor<>).MakeGenericType(stateType);

        var property = accessorImplementationType.GetProperty(nameof(IStateAccessor<object>.State))!;
        _knownStateAccessorsStateGetter[accessorType] = property.CreateGetterDynamic();
        _knownStateAccessorsStateSetter[accessorType] = property.CreateSetterDynamic();
        _knownStateToAccessors[stateType] = accessorType;
        if (!_knownStates.Contains(stateType))
            _knownStates.Add(stateType);
    }
    public void RegisterAction(Type actionType)
    {
        if (!_knownActions.Contains(actionType))
            _knownActions.Add(actionType);
    }
    public void RegisterEffectValidator(Type actionValType, Type interfaceType) => _knownActionValidators[actionValType] = interfaceType;

}
