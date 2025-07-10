namespace StatePulse.Net;
public interface IStatePulseRegistry
{

    IReadOnlyList<Type> KnownStates { get; }
    IReadOnlyDictionary<Type, Type> KnownEffects { get; }
    IReadOnlyDictionary<Type, Type> KnownReducers { get; }
    IReadOnlyDictionary<Type, Func<object, object?[], object?>> KnownReducersReduceMethod { get; }
    public IReadOnlyList<Type> KnownActions { get; }
    public IReadOnlyDictionary<Type, Type> KnownActionValidators { get; }
    public IReadOnlyDictionary<Type, Func<object, object?>> KnownStatesStateGetter { get; }
    public IReadOnlyDictionary<Type, Action<object, object?>> KnownStatesStateSetter { get; }
    void RegisterStateAccessor(Type stateType);
    void RegisterEffect(Type effectType, Type interfaceType);
    void RegisterReducer(Type reducerType, Type interfaceType);
    void RegisterAction(Type actionType);
    void RegisterEffectValidator(Type actionValType, Type interfaceType);

}
