using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Internal.Implementations;

namespace StatePulse.Net;
public static class ServiceRegisterExt
{
    /// <summary>
    /// Also call AddStatePulseScan otherwise you will have to manually register all Effects, Reducers, StateAccessors and also register them inside IStatePulseRegistry.
    /// </summary>
    /// <param name="services"></param>
    public static void AddStatePulseServices(this IServiceCollection services)
    {
        services.AddSingleton<IDispatcher, Dispatcher>();
    }

    /// <summary>
    /// Automatically scan supplied assemblies for IEffect, IReducer and IStateFeature
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    public static void AddStatePulseScan(this IServiceCollection services, params Type[] assemblies)
    {
        var effectType = typeof(IEffect<>);
        var reducerType = typeof(IReducer<,>);
        var stateFeatureType = typeof(IStateFeature);
        var actionType = typeof(IAction);
        var actionValidatorType = typeof(IActionValidator<>);
        var registry = new StatePulseRegistry();
        foreach (var assembly in assemblies)
        {
            var types = assembly.Assembly.GetTypes();

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                var interfaces = type.GetInterfaces();
                foreach (var iface in interfaces)
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == effectType)
                    {
                        services.AddTransient(iface, type);
                        registry.RegisterEffect(type);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == reducerType)
                    {
                        services.AddTransient(iface, type);
                        registry.RegisterReducer(type);
                        continue;
                    }

                    if (!iface.IsGenericType && iface == stateFeatureType)
                    {
                        var accessorType = typeof(IStateAccessor<>).MakeGenericType(type);
                        var accessorImplementationType = typeof(StateAccessor<>).MakeGenericType(type);
                        services.AddSingleton(accessorType, accessorImplementationType);  // Registering the correct interface
                        registry.RegisterState(type);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == actionValidatorType)
                    {
                        services.AddTransient(iface, type);
                        registry.RegisterActionValidator(type);
                        continue;
                    }

                    if (!iface.IsGenericType && iface == actionType)
                        registry.RegisterAction(actionType);
                }
            }
        }

        services.AddSingleton<IStatePulseRegistry>(registry);


    }
}
