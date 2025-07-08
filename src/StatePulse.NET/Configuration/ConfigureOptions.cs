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
    public Type[] ScanAssemblies { get; set; } = new Type[] { };
}
