using StatePulse.Net.Engine.Exceptions;

namespace StatePulse.Net.Configuration;
public class ConfigureOptions
{
    /// <summary>
    /// Scoped on WASM = Singleton, Singleton on Blazor Server = Chaos Data Leakage
    /// </summary>
    public Lifetime ServiceLifetime { get; set; } = Lifetime.Scoped;
    public MiddlewareEffectBehavior MiddlewareEffectBehavior { get; set; } = MiddlewareEffectBehavior.PerGroupEffects;
    public MiddlewareTaskBehavior MiddlewareTaskBehavior { get; set; } = MiddlewareTaskBehavior.DoNotAwait;
    public DispatchEffectBehavior DispatchEffectBehavior { get; set; } = DispatchEffectBehavior.Parallel;
    public DispatchEffectExecutionBehavior DispatchEffectExecutionBehavior { get; set; } = DispatchEffectExecutionBehavior.YieldAndFire;
    public DispatchOrdering DispatchOrderBehavior { get; set; } = DispatchOrdering.ReducersFirst;
    public PulseTrackingModel PulseTrackingPerformance { get; set; } = PulseTrackingModel.ThreadSafe;
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
