using System.Reflection;

namespace StatePulse.Net.Configuration;

public class ConfigureOptions
{
    /// <summary>
    /// Scoped on WASM = Singleton, Singleton on Blazor Server = Chaos Data Leakage
    /// </summary>
    public MiddlewareEffectBehavior MiddlewareEffectBehavior { get; set; } = MiddlewareEffectBehavior.PerGroupEffects;
    public MiddlewareTaskBehavior MiddlewareTaskBehavior { get; set; } = MiddlewareTaskBehavior.DoNotAwait;
    public DispatchEffectBehavior DispatchEffectBehavior { get; set; } = DispatchEffectBehavior.Parallel;
    public DispatchOrdering DispatchOrderBehavior { get; set; } = DispatchOrdering.ReducersFirst;
    public PulseTrackingModel PulseTrackingPerformance { get; set; } = PulseTrackingModel.ThreadSafe;
    public Assembly[] ScanAssemblies { get; set; } = new Assembly[] { };
    public Type[] AutoRegisterTypes { get; set; } = new Type[] { };
    private long _versionTicker;

    public long GetNextVersion()
    {
        var next = Interlocked.Increment(ref _versionTicker);
        return next;
    }

}
