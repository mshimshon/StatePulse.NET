namespace StatePulse.Net.Configuration;
public class ConfigureOptions
{
    /// <summary>
    /// Scoped on WASM = Singleton, Singleton on Blazor Server = Chaos Data Leakage
    /// </summary>
    public LifetimeEnum ServiceLifetime { get; set; } = LifetimeEnum.Scoped;
    public Type[] ScanAssemblies { get; set; } = new Type[] { };
}
