using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Configuration;
using StatePulse.Net.Engine;
using StatePulse.Net.Engine.Implementations;

namespace StatePulse.Net;
public static class ServiceRegisterExt
{
    private static bool _scanned;
    public static ConfigureOptions _configureOptions = new ConfigureOptions();
    private static StatePulseRegistry Registry = new StatePulseRegistry();
    /// <summary>
    /// Also call AddStatePulseScan otherwise you will have to manually register all Effects, Reducers, StateAccessors and also register them inside IStatePulseRegistry.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddStatePulseServices(this IServiceCollection services, Action<ConfigureOptions> configure)
    {
        // TODO: Create IDispatchFactory to bind IDispatcher and IDispatchHAndler
        services.AddTransient<IDispatcher, Dispatcher>();
        services.AddTransient<IDispatchFactory, DispatchFactory>();
        configure(_configureOptions);

        if (_configureOptions.ServiceLifetime == Lifetime.Scoped)
            services.AddScoped<IPulseGlobalTracker, PulseGlobalTracker>();
        else
            services.AddSingleton<IPulseGlobalTracker, PulseGlobalTracker>();

        services.AddTransient<IStatePulse, PulseLazyStateWebAssembly>();
        services.AddTransient<IStatePulse, PulseLazyStateBlazorServer>();
        services.AddSingleton<IStatePulseRegistry>(Registry);
        if (_configureOptions.ScanAssemblies.Count() > 0)
            services.ScanStatePulseAssemblies(_configureOptions.ScanAssemblies);

        return services;
    }


    public static IServiceCollection AddStatePulseEffect<TEffect, TImplementation>(this IServiceCollection services)
        => services.AddStatePulseEffect(typeof(TEffect), typeof(TImplementation));
    private static IServiceCollection AddStatePulseEffect(this IServiceCollection services, Type iFace, Type implementation)
    {
        if (services.IsImplementationRegistered(iFace, implementation)) return services;

        services.AddTransient(iFace, implementation);
        Registry.RegisterEffect(iFace, implementation);
        return services;
    }

    public static IServiceCollection AddStatePulseReducer<TReducer, TImplementation>(this IServiceCollection services)
        => services.AddStatePulseReducer(typeof(TReducer), typeof(TImplementation));
    private static IServiceCollection AddStatePulseReducer(this IServiceCollection services, Type iFace, Type implementation)

    {
        if (services.IsReducerRegistered(iFace)) return services;

        services.AddTransient(iFace, implementation);
        Registry.RegisterReducer(iFace, implementation);
        return services;
    }
    public static IServiceCollection AddStatePulseStateAccessor<TAccessor, TImplementation>(this IServiceCollection services)
    => services.AddStatePulseStateAccessor(typeof(TAccessor), typeof(TImplementation));
    private static IServiceCollection AddStatePulseStateAccessor(this IServiceCollection services, Type iFace, Type implementation)

    {
        var accessorType = typeof(IStateAccessor<>).MakeGenericType(implementation);
        var accessorImplementationType = typeof(StateAccessor<>).MakeGenericType(implementation);

        if (services.IsStateAccessorRegistered(accessorImplementationType)) return services;
        if (_configureOptions.ServiceLifetime == Lifetime.Scoped)
            services.AddScoped(accessorType, accessorImplementationType);
        else
            services.AddSingleton(accessorType, accessorImplementationType);

        Registry.RegisterStateAccessor(implementation);
        return services;
    }


    public static IServiceCollection AddStatePulseEffectValidator<TEffectValidator, TImplementation>(this IServiceCollection services)
    => services.AddStatePulseEffectValidator(typeof(TEffectValidator), typeof(TImplementation));
    private static IServiceCollection AddStatePulseEffectValidator(this IServiceCollection services, Type iFace, Type implementation)

    {
        if (services.IsEffectValidatorImplementationRegistered(implementation)) return services;
        services.AddTransient(iFace, implementation);
        Registry.RegisterEffectValidator(iFace, implementation);
        return services;
    }

    public static IServiceCollection AddStatePulseAction<TImplementation>(this IServiceCollection services) where TImplementation : IAction
        => services.AddStatePulseAction(typeof(TImplementation));
    private static IServiceCollection AddStatePulseAction(this IServiceCollection services, Type implementation)

    {
        // Add Action Based Singleton Dispatch Tracker.
        var dispatchTrackerIface = typeof(IDispatchTracker<>).MakeGenericType(implementation);
        var dispatchTracker = typeof(DispatchTracker<>).MakeGenericType(implementation);
        if (services.IsDispatchTrackerRegistered(dispatchTracker)) return services;
        Registry.RegisterAction(implementation);
        if (_configureOptions.ServiceLifetime == Lifetime.Scoped)
            services.AddScoped(dispatchTrackerIface, dispatchTracker);
        else
            services.AddSingleton(dispatchTrackerIface, dispatchTracker);
        return services;
    }


    /// <summary>
    /// Automatically scan supplied assemblies for IEffect, IReducer and IStateFeature
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    private static void ScanStatePulseAssemblies(this IServiceCollection services, params Type[] assemblies)
    {
        if (_scanned) return;
        _scanned = true;

        var effectMiddlewareType = typeof(IEffectMiddleware);
        var reducerMiddlewareType = typeof(IReducerMiddleware);
        var dispatchMiddlewareType = typeof(IDispatcherMiddleware);
        var effectType = typeof(IEffect<>);
        var reducerType = typeof(IReducer<,>);
        var stateFeatureType = typeof(IStateFeature);
        var actionType = typeof(IAction);
        var actionSafeType = typeof(ISafeAction);
        var actionValidatorType = typeof(IEffectValidator<,>);

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
                    // TODO: Allow Extension MEthods to manually add each types and perform registration.

                    if (!iface.IsGenericType &&
                        (iface == effectMiddlewareType || iface == reducerMiddlewareType || iface == dispatchMiddlewareType))
                    {
                        if (services.IsImplementationRegistered(type, iface)) continue;
                        services.AddTransient(iface, type);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == effectType)
                    {
                        services.AddStatePulseEffect(iface, type);
                        continue;
                    }

                    if (iface.IsGenericType)
                    {
                        services.AddStatePulseReducer(iface, type);
                        continue;
                    }

                    if (!iface.IsGenericType && iface == stateFeatureType)
                    {
                        services.AddStatePulseStateAccessor(iface, type);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == actionValidatorType)
                    {
                        services.AddStatePulseEffectValidator(iface, type);
                        continue;
                    }

                    if (!iface.IsGenericType && (iface == actionType || iface == actionSafeType))
                    {
                        services.AddStatePulseAction(type);
                    }
                }
            }
        }




    }
    public static bool IsReducerRegistered(this IServiceCollection services, Type reducerType)
    {
        return services.Any(s => s.ServiceType == reducerType);
    }
    public static bool IsImplementationRegistered(this IServiceCollection services, Type implementationType, Type ifaceType)
    {
        return services.Any(s =>
            s.ImplementationType == implementationType &&
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == ifaceType ||
            !s.ServiceType.IsGenericType && s.ServiceType == ifaceType
        );
    }
    public static bool IsEffectValidatorImplementationRegistered(this IServiceCollection services, Type implementationType)
    {
        return services.Any(s =>
            s.ImplementationType == implementationType &&
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IEffectValidator<,>)
        );
    }

    public static bool IsDispatchTrackerRegistered(this IServiceCollection services, Type implementationType)
    {
        if (!implementationType.IsGenericType) return false;

        var interfaces = implementationType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDispatchTracker<>));

        foreach (var iface in interfaces)
        {
            if (services.Any(s =>
                s.ServiceType == iface &&
                s.ImplementationType == implementationType))
            {
                return true; // Found match
            }
        }

        return false;
    }

    public static bool IsStateAccessorRegistered(this IServiceCollection services, Type implementationType)
    {
        if (!implementationType.IsGenericType) return false;

        var interfaces = implementationType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStateAccessor<>));

        foreach (var iface in interfaces)
        {
            if (services.Any(s =>
                s.ServiceType == iface &&
                s.ImplementationType == implementationType))
            {
                return true; // Found match
            }
        }

        return false;
    }
}
