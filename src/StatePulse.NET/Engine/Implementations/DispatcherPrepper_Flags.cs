using StatePulse.Net.Configuration;
namespace StatePulse.Net.Engine.Implementations;

internal partial class DispatcherPrepper<TAction, TActionChain> : IDispatcherPrepper<TAction>
    where TAction : IAction
    where TActionChain : IAction
{

    private DispatchEffectBehavior _dispatchEffectBehavior = ServiceRegisterExt.ConfigureOptions.DispatchEffectBehavior;
    private DispatchOrdering _dispatchOrdering = ServiceRegisterExt.ConfigureOptions.DispatchOrderBehavior;
    private bool _forceSyncronous;


    public IDispatcherPrepper<TAction> Await()
    {
        _forceSyncronous = true;
        return this;
    }

    public IDispatcherPrepper<TAction> EffectsFirst()
    {
        _dispatchOrdering = DispatchOrdering.EffectsFirst;
        return this;
    }

    public IDispatcherPrepper<TAction> ReducersFirst()
    {
        _dispatchOrdering = DispatchOrdering.ReducersFirst;
        return this;
    }



    public IDispatcherPrepper<TAction> SequentialEffects()
    {
        _dispatchEffectBehavior = DispatchEffectBehavior.Sequential;
        return this;
    }
    public IDispatcherPrepper<TAction> ParallelEffects()
    {
        _dispatchEffectBehavior = DispatchEffectBehavior.Parallel;
        return this;
    }



}

