using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Blazor.Engine.Implementation;

namespace StatePulse.Net.Blazor;
public static class ServiceRegisterExt
{
    public static IServiceCollection AddStatePulseBlazor(this IServiceCollection services, bool wasm = true)
    {
        services.AddScoped<IPulseGlobalTracker, PulseGlobalTracker>();
        if (wasm)
        {
            services.AddTransient<IStatePulse, PulseLazyStateWebAssembly>();
        }
        else
        {
            services.AddTransient<IStatePulse, PulseLazyStateBlazorServer>();
        }
        return services;
    }
}
