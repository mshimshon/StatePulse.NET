using StatePulse.Net.Engine.Extensions;

namespace StatePulse.Net.Engine.Implementations;

using StatePulse.Net;
using System.Linq.Expressions;
using System.Reflection;

internal class StatePulseRegistry : IStatePulseRegistry
{
    private readonly List<Type> _knownStates = new();
    private readonly Dictionary<Type, Type> _knownStateToAccessors = new();
    private readonly Dictionary<Type, Func<object, object?>> _knownStateAccessorsStateGetter = new();
    private readonly Dictionary<Type, Func<object, object, Type, long, Guid, bool>> _knownStateAccessorsStateUpdater = new();
    private readonly Dictionary<Type, Func<object, object?>> _knownStateAccessorsVersionGetter = new();

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
    public IReadOnlyDictionary<Type, Func<object, object, Type, long, Guid, bool>> KnownStateAccessorsStateUpdater => _knownStateAccessorsStateUpdater;
    public IReadOnlyDictionary<Type, Func<object, object?>> KnownStateAccessorsVersionGetter => _knownStateAccessorsVersionGetter;

    public IReadOnlyDictionary<Type, Func<object, object?[], object?>> KnownReducersReduceMethod => _knownReducersReduceMethod;

    public IReadOnlyDictionary<Type, Func<object, object?>> KnownReducersTaskResult => _knownReducersTaskResult;

    public IReadOnlyDictionary<Type, Type> KnownStateToAccessors => _knownStateToAccessors;


    public void RegisterEffect(Type effectType, Type interfaceType) => _knownEffects[effectType] = interfaceType;
    public void RegisterReducer(Type reducerType, Type interfaceType)
    {
        var reduceMethodName = nameof(IReducer<IStateFeature, IAction>.Reduce);
        var method = reducerType.GetMethod(reduceMethodName)!;
        var stateType = method.ReturnType; // This is TState
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
        var stateUpdateMethod = accessorImplementationType.GetMethod(nameof(IStateAccessor<object>.ChangeState))!;
        _knownStateAccessorsStateUpdater[accessorType] = BuildChangeStateDelegate(stateUpdateMethod);

        var propertyVersion = accessorImplementationType.GetProperty(nameof(IStateAccessor<object>.Version))!;
        _knownStateAccessorsVersionGetter[accessorType] = propertyVersion.CreateGetterDynamic();

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
    private static Func<object, object, Type, long, Guid, bool> BuildChangeStateDelegate(
        MethodInfo changeStateMethod)
    {
        // Parameters of the dynamic delegate
        var targetParam = Expression.Parameter(typeof(object), "target");
        var stateParam = Expression.Parameter(typeof(object), "state");
        var originTypeParam = Expression.Parameter(typeof(Type), "originType");
        var versionParam = Expression.Parameter(typeof(long), "version");
        var writerParam = Expression.Parameter(typeof(Guid), "writer");

        // Cast target to StateAccessor<TState>
        var typedTarget = Expression.Convert(targetParam, changeStateMethod.DeclaringType!);

        // Cast state to TState
        var typedState = Expression.Convert(
            stateParam,
            changeStateMethod.GetParameters()[0].ParameterType
        );

        // Call ChangeState(TState, Type, long, Guid)
        var call = Expression.Call(
            typedTarget,
            changeStateMethod,
            typedState,
            originTypeParam,
            versionParam,
            writerParam
        );

        // Wrap the call and return true (since ChangeState returns void)
        var body = Expression.Block(
            call,
            Expression.Constant(true)
        );

        return Expression.Lambda<Func<object, object, Type, long, Guid, bool>>(
            body,
            targetParam,
            stateParam,
            originTypeParam,
            versionParam,
            writerParam
        ).Compile();
    }
}
