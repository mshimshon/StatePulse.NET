using StatePulse.Net.Engine.Exceptions;

namespace StatePulse.Net.Configuration;
public class ConfigureOptions
{
    /// <summary>
    /// Scoped on WASM = Singleton, Singleton on Blazor Server = Chaos Data Leakage
    /// </summary>
    public Lifetime ServiceLifetime { get; set; } = Lifetime.Scoped;
    public MiddlewareEffectBehavior MiddlewareEffectBehavior { get; internal set; } = MiddlewareEffectBehavior.PerGroupEffects;
    public MiddlewareTaskBehavior MiddlewareTaskBehavior { get; internal set; } = MiddlewareTaskBehavior.DoNotAwait;
    public DispatchEffectBehavior DispatchEffectBehavior { get; internal set; } = DispatchEffectBehavior.Parallel;
    public DispatchEffectExecutionBehavior DispatchEffectExecutionBehavior { get; internal set; } = DispatchEffectExecutionBehavior.YieldAndFire;
    public DispatchOrdering DispatchOrderBehavior { get; internal set; } = DispatchOrdering.ReducersFirst;
    public Type[] ScanAssemblies { get; set; } = new Type[] { };

    public void ValidateConfiguration()
    {
        // FireAndForget can't be Sequential
        if (DispatchEffectExecutionBehavior == DispatchEffectExecutionBehavior.FireAndForget
            && DispatchEffectBehavior == DispatchEffectBehavior.Sequential)
        {
            throw new InvalidDispatchCombinationException(
                "DispatchEffectExecutionBehavior.FireAndForget execution cannot be combined with DispatchEffectBehavior.Sequential. " +
                "FireAndForget doesn't await effects, so ordering cannot be guaranteed. " +
                "Use DispatchEffectExecutionBehavior.YieldAndFire with DispatchEffectBehavior.Sequential/DispatchEffectBehavior.Parallel or DispatchEffectExecutionBehavior.FireAndForget and DispatchEffectBehavior.Parallel");
        }
    }
}
