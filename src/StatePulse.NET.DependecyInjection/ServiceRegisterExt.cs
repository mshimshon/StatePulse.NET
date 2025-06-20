using Microsoft.Extensions.DependencyInjection;
using StatePulse.NET.DependecyInjection.Internals;

namespace StatePulse.NET.DependecyInjection;
internal static class ServiceRegisterExt
{

    public static void AddStatePulse(this IServiceCollection services)
    {
        services.AddSingleton<IStatePulseRegistry, StatePulseRegistry>();
    }
    /// <summary>
    /// Automatically scan supplied assemblies for IEffect, IReducer and IStateFeature
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    public static void AddStatePulseWithScan(this IServiceCollection services, params Type[] assemblies)
    {
        var effectType = typeof(IEffect<>);
        var reducerType = typeof(IReducer<,>);
        var stateFeatureType = typeof(IStateFeature);
        var actionType = typeof(IAction);
        var actionValidatorType = typeof(IActionValidator<>);
        var registry = new StatePulseRegistry();
        foreach (var assembly in assemblies)
            foreach (var type in assembly.Assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                var interfaces = type.GetInterfaces();
                foreach (var iface in interfaces)
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == effectType)
                    {
                        services.AddTransient(iface, type);
                        registry.RegisterEffect(iface);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == reducerType)
                    {
                        services.AddTransient(iface, type);
                        registry.RegisterReducer(iface);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == stateFeatureType)
                    {
                        services.AddSingleton(iface, type);
                        registry.RegisterState(iface);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == actionValidatorType)
                    {
                        services.AddTransient(iface, type);
                        registry.RegisterActionValidator(iface);
                        continue;
                    }

                    if (iface == actionType)
                        registry.RegisterAction(actionType);
                }
            }
    }
}
