using StatePulse.Net.Engine.Extensions;

namespace StatePulse.Net.Engine.Implementations;
internal class StatePulseRegistry : IStatePulseRegistry
{
    private readonly List<Type> _knownStates = new();
    private readonly Dictionary<Type, Func<object, object?>> _knownStatesStateGetter = new();
    private readonly Dictionary<Type, Action<object, object?>> _knownStatesStateSetter = new();

    private readonly Dictionary<Type, Type> _knownEffects = new();
    private readonly Dictionary<Type, Type> _knownReducers = new();
    private readonly Dictionary<Type, Func<object, object?[], object?>> _knownReducersReduceMethod = new();
    private readonly List<Type> _knownActions = new();
    private readonly Dictionary<Type, Type> _knownActionValidators = new();

    public IReadOnlyList<Type> KnownStates => _knownStates;
    public IReadOnlyDictionary<Type, Type> KnownEffects => _knownEffects;
    public IReadOnlyDictionary<Type, Type> KnownReducers => _knownReducers;
    public IReadOnlyList<Type> KnownActions => _knownActions;
    public IReadOnlyDictionary<Type, Type> KnownActionValidators => _knownActionValidators;
    public IReadOnlyDictionary<Type, Func<object, object?>> KnownStatesStateGetter => _knownStatesStateGetter;
    public IReadOnlyDictionary<Type, Action<object, object?>> KnownStatesStateSetter => _knownStatesStateSetter;

    public IReadOnlyDictionary<Type, Func<object, object?[], object?>> KnownReducersReduceMethod => _knownReducersReduceMethod;

    public void RegisterEffect(Type effectType, Type interfaceType) => _knownEffects.Add(effectType, interfaceType);
    public void RegisterReducer(Type reducerType, Type interfaceType)
    {
        //if (_cachedReducerMethod == default)
        var method = reducerType.GetMethod(nameof(IReducer<IStateFeature, IAction>.ReduceAsync))!;
        //var task =
        //    (Task)_cachedReducerMethod.Invoke(reducerService, new[] { stateProperty.GetValue(stateService)!, _action })!;
        _knownReducersReduceMethod[reducerType] = method.CreateDynamicReflectionInvoker();


        _knownReducers.Add(reducerType, interfaceType);
    }
    public void RegisterStateAccessor(Type stateType)
    {
        var property = stateType.GetProperty(nameof(IStateAccessor<object>.State))!;
        _knownStatesStateGetter[stateType] = property.CreateGetterDynamic();
        _knownStatesStateSetter[stateType] = property.CreateSetterDynamic();
        _knownStates.Add(stateType);
    }
    public void RegisterAction(Type actionType) => _knownActions.Add(actionType);
    public void RegisterEffectValidator(Type actionValType, Type interfaceType) => _knownActionValidators.Add(actionValType, interfaceType);

}
