namespace StatePulse.Net.Engine.Implementations;
internal class StatePulseRegistry : IStatePulseRegistry
{
    private readonly List<Type> _knownStates = new();
    private readonly List<Type> _knownEffects = new();
    private readonly List<Type> _knownReducers = new();
    private readonly List<Type> _knownActions = new();
    private readonly List<Type> _knownActionValidators = new();
    //TODO: SWITCH TO ConcurrentStack perform as a list avoid duplicate action as key...

    public IReadOnlyList<Type> KnownStates => _knownStates;
    public IReadOnlyList<Type> KnownEffects => _knownEffects;
    public IReadOnlyList<Type> KnownReducers => _knownReducers;
    public IReadOnlyList<Type> KnownActions => _knownActions;
    public IReadOnlyList<Type> KnownActionValidators => _knownActionValidators;

    public void RegisterEffect(Type effectType) => _knownEffects.Add(effectType);
    public void RegisterReducer(Type reducerType) => _knownReducers.Add(reducerType);
    public void RegisterState(Type stateType) => _knownStates.Add(stateType);
    public void RegisterAction(Type actionType) => _knownActions.Add(actionType);
    public void RegisterActionValidator(Type actionValType) => _knownActionValidators.Add(actionValType);
}
