namespace StatePulse.Net.Engine.Implementations;
internal class StatePulseRegistry : IStatePulseRegistry
{
    private readonly List<Type> _knownStates = new();
    private readonly Dictionary<Type, Type> _knownEffects = new();
    private readonly Dictionary<Type, Type> _knownReducers = new();
    private readonly List<Type> _knownActions = new();
    private readonly Dictionary<Type, Type> _knownActionValidators = new();
    //TODO: SWITCH TO ConcurrentStack perform as a list avoid duplicate action as key...

    public IReadOnlyList<Type> KnownStates => _knownStates;
    public IReadOnlyDictionary<Type, Type> KnownEffects => _knownEffects;
    public IReadOnlyDictionary<Type, Type> KnownReducers => _knownReducers;
    public IReadOnlyList<Type> KnownActions => _knownActions;
    public IReadOnlyDictionary<Type, Type> KnownActionValidators => _knownActionValidators;

    public void RegisterEffect(Type effectType, Type interfaceType) => _knownEffects.Add(effectType, interfaceType);
    public void RegisterReducer(Type reducerType, Type interfaceType) => _knownReducers.Add(reducerType, interfaceType);
    public void RegisterState(Type stateType) => _knownStates.Add(stateType);
    public void RegisterAction(Type actionType) => _knownActions.Add(actionType);
    public void RegisterActionValidator(Type actionValType, Type interfaceType) => _knownActionValidators.Add(actionValType, interfaceType);
}
