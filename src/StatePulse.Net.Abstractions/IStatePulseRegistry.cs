namespace StatePulse.Net;

public interface IStatePulseRegistry
{

    IReadOnlyList<Type> KnownStates { get; }
    IReadOnlyDictionary<Type, Type> KnownEffects { get; }
    IReadOnlyDictionary<Type, Type> KnownReducers { get; }
    IReadOnlyDictionary<Type, Func<object, object?[], object?>> KnownReducersReduceMethod { get; }
    IReadOnlyList<Type> KnownActions { get; }
    IReadOnlyDictionary<Type, Type> KnownActionValidators { get; }
    IReadOnlyDictionary<Type, Func<object, object?>> KnownStateAccessorsStateGetter { get; }
    IReadOnlyDictionary<Type, Func<object, object, Type, long, Guid, bool>> KnownStateAccessorsStateUpdater { get; }

    IReadOnlyDictionary<Type, Func<object, object?>> KnownStateAccessorsVersionGetter { get; }

    IReadOnlyDictionary<Type, Func<object, object?>> KnownReducersTaskResult { get; }
    IReadOnlyDictionary<Type, Type> KnownStateToAccessors { get; }

    void RegisterState(Type stateType);
    void RegisterEffect(Type effectType, Type interfaceType);
    void RegisterReducer(Type reducerType, Type interfaceType);
    void RegisterAction(Type actionType);
    void RegisterEffectValidator(Type actionValType, Type interfaceType);

}
