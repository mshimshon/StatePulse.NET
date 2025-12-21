
using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Configuration;
using StatePulse.Net.Engine.Exceptions;
using System.Collections.Concurrent;
using System.Reflection;
namespace StatePulse.Net.Engine.Implementations;
internal partial class DispatcherPrepper<TAction, TActionChain> : IDispatcherPrepper<TAction>
    where TAction : IAction
    where TActionChain : IAction
{
    
    private DispatchEffectBehavior _dispatchEffectBehavior = ServiceRegisterExt.ConfigureOptions.DispatchEffectBehavior;
    private DispatchEffectExecutionBehavior _dispatchEffectExecutionBehavior = ServiceRegisterExt.ConfigureOptions.DispatchEffectExecutionBehavior;
    private DispatchOrdering _dispatchOrdering = ServiceRegisterExt.ConfigureOptions.DispatchOrderBehavior;
    private bool _forceSyncronous;


    public IDispatcherPrepper<TAction> Await()
    {
        _forceSyncronous = true;
        return this;
    }

    public IDispatcherPrepper<TAction> EffectsFirst() {
        _dispatchOrdering = DispatchOrdering.EffectsFirst;
        return this;
    }

    public IDispatcherPrepper<TAction> ReducersFirst()
    {
        _dispatchOrdering = DispatchOrdering.ReducersFirst;
        return this;
    }

    public IDispatcherPrepper<TAction> ExecFireAndForget()
    {
        _dispatchEffectExecutionBehavior = DispatchEffectExecutionBehavior.FireAndForget;
        return this;
    }

    public IDispatcherPrepper<TAction> ExecYieldAndFire()
    {
        _dispatchEffectExecutionBehavior = DispatchEffectExecutionBehavior.YieldAndFire;
        return this;
    }

    public IDispatcherPrepper<TAction> SequentialEffects()
    {
        _dispatchEffectBehavior = DispatchEffectBehavior.Sequential;
        return this;
    }
    public IDispatcherPrepper<TAction> ParallelEffects() {
        _dispatchEffectBehavior = DispatchEffectBehavior.Parallel;
        return this;
    }


    private void ValidateConfiguration()
    {
        // FireAndForget can't be Sequential
        if (_dispatchEffectExecutionBehavior == DispatchEffectExecutionBehavior.FireAndForget
            && _dispatchEffectBehavior == DispatchEffectBehavior.Sequential)
        {
            throw new InvalidDispatchCombinationException(
                "FireAndForget execution cannot be combined with Sequential effects. " +
                "FireAndForget doesn't await effects, so ordering cannot be guaranteed. " +
                "Use ExecYieldAndFire().SequentialEffects() or ExecFireAndForget().ParallelEffects()");
        }

        // Await() is locked to YieldAndFire + Sequential
        if (_forceSyncronous)
        {
            if (_dispatchEffectExecutionBehavior != DispatchEffectExecutionBehavior.YieldAndFire)
            {
                throw new InvalidDispatchCombinationException(
                    "Await() requires YieldAndFire execution. Cannot use ExecFireAndForget() with Await()");
            }
            // REMOVED IT WOULD BREAK EXISTING PROJECTS
            //if (_dispatchEffectBehavior != DispatchEffectBehavior.Sequential)
            //{
            //    throw new InvalidDispatchCombinationException(
            //        "Await() requires Sequential effects. Cannot use ParallelEffects() with Await()");
            //}
        }
    }
}

