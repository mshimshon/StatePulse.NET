namespace StatePulse.Net;
public interface IStatePulseRegistry
{

    IReadOnlyList<Type> KnownStates { get; }
    IReadOnlyDictionary<Type, Type> KnownEffects { get; }
    IReadOnlyDictionary<Type, Type> KnownReducers { get; }
    public IReadOnlyList<Type> KnownActions { get; }
    public IReadOnlyDictionary<Type, Type> KnownActionValidators { get; }
    void RegisterState(Type stateType);
    void RegisterEffect(Type effectType, Type interfaceType);
    void RegisterReducer(Type reducerType, Type interfaceType);
    void RegisterAction(Type actionType);
    void RegisterEffectValidator(Type actionValType, Type interfaceType);

}
