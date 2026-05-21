using StatePulse.Net.Configuration;
namespace StatePulse.Net.Engine.Implementations;

internal partial class DispatcherPrepper<TAction> : IDispatcherPrepper<TAction>
    where TAction : IAction
{

    private DispatchEffectBehavior _dispatchEffectBehavior = ServiceRegisterExt.ConfigureOptions.DispatchEffectBehavior;
    private DispatchOrdering _dispatchOrdering = ServiceRegisterExt.ConfigureOptions.DispatchOrderBehavior;
    private bool _forceSyncronous;

    public IDispatcherPrepper<TAction> AsSafe()
    {
        _safe = true;
        return this;
    }
    public IDispatcherPrepper<TAction> AsUnSafe()
    {
        _safe = false;
        return this;
    }
    public IDispatcherPrepper<TAction> Await()
    {
        _forceSyncronous = true;
        return this;
    }

    /// <summary>
    /// Forces this dispatch to run in fire‑and‑yield mode when called from an effect. 
    /// Effects should await their own follow‑up dispatches, and this flag provides 
    /// a built‑in safeguard against side-effect that should never be fully awaited.
    /// </summary>

    public IDispatcherPrepper<TAction> DoNotAwait()
    {
        _forceSyncronous = false;
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

