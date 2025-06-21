namespace StatePulse.Net;
public interface IStatePulseRegistry
{
    IReadOnlyList<Type> KnownStates { get; }
    IReadOnlyList<Type> KnownEffects { get; }
    IReadOnlyList<Type> KnownReducers { get; }
    public IReadOnlyList<Type> KnownActions { get; }
    public IReadOnlyList<Type> KnownActionValidators { get; }
    void RegisterState(Type stateType);
    void RegisterEffect(Type effectType);
    void RegisterReducer(Type reducerType);
    void RegisterAction(Type actionType);
    void RegisterActionValidator(Type actionValType);

}
