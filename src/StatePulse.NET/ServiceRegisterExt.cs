using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Configuration;
using StatePulse.Net.Engine;
using StatePulse.Net.Engine.Implementations;

namespace StatePulse.Net;

public static class ServiceRegisterExt
{
    private static bool _scanned;
    public static ConfigureOptions ConfigureOptions { get; set; } = new ConfigureOptions();
    private static StatePulseRegistry Registry = new StatePulseRegistry();


    public static IServiceCollection AddStatePulseServices(this IServiceCollection services, Action<ConfigureOptions> configure)
    {
        services.AddTransient<IDispatcher, Dispatcher>();
        services.AddTransient<IDispatchFactory, DispatchFactory>();
        configure(ConfigureOptions);
        ConfigureOptions.ValidateConfiguration();
        services.AddSingleton<IPulseGlobalTracker, PulseGlobalTracker>();

        bool isSingleThreadModel = ConfigureOptions.PulseTrackingPerformance == PulseTrackingModel.SingleThreadFast || ConfigureOptions.PulseTrackingPerformance == PulseTrackingModel.BlazorWebAssemblyFast;
        if (isSingleThreadModel)
          services.AddTransient<IStatePulse, PulseLazyStateWebAssembly>();
        else
          services.AddTransient<IStatePulse, PulseLazyStateBlazorServer>();

        services.AddSingleton<IStatePulseRegistry>(Registry);
        if (ConfigureOptions.ScanAssemblies.Any())
            services.ScanStatePulseAssemblies(ConfigureOptions.ScanAssemblies);

        return services;
    }


    public static IServiceCollection AddStatePulseEffect<TImplementation>(this IServiceCollection services)
    {
        var implType = typeof(TImplementation);

        var effectInterface = implType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEffect<>));
        if (effectInterface == null)
            throw new InvalidOperationException($"{implType.Name} must implement {typeof(IEffect<>).Name}");
        return services.AddStatePulseEffect(effectInterface!, implType);
    }

    private static IServiceCollection AddStatePulseEffect(this IServiceCollection services, Type iFace, Type implementation)
    {
        if (services.IsImplementationRegistered(iFace, implementation)) return services;

        services.AddTransient(iFace, implementation);
        Registry.RegisterEffect(iFace, implementation);
        return services;
    }

    public static IServiceCollection AddStatePulseReducer<TImplementation>(this IServiceCollection services)
    {
        var implType = typeof(TImplementation);

        var reducerInterface = implType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReducer<,>));
        if (reducerInterface == null)
            throw new InvalidOperationException($"{implType.Name} must implement {typeof(IReducer<,>).Name}");

        return services.AddStatePulseReducer(reducerInterface!, implType);
    }
    private static IServiceCollection AddStatePulseReducer(this IServiceCollection services, Type iFace, Type implementation)

    {
        if (services.IsReducerRegistered(iFace)) return services;

        services.AddTransient(iFace, implementation);
        Registry.RegisterReducer(iFace, implementation);
        return services;
    }
    public static IServiceCollection AddStatePulseStateFeature<TImplementation>(this IServiceCollection services)
        where TImplementation : IStateFeature
    => services.AddStatePulseStateFeature(typeof(TImplementation));
    //private static IServiceCollection AddStatePulseStateFeature(this IServiceCollection services, Type implementation)
    //{
    //    var accessorType = typeof(IStateAccessor<>).MakeGenericType(implementation);
    //    var accessorImplementationType = typeof(StateAccessor<>).MakeGenericType(implementation);

    //    if (services.IsStateAccessorRegistered(accessorImplementationType)) return services;
    //    if (ConfigureOptions.ServiceLifetime == Lifetime.Scoped)
    //        services.AddScoped(accessorType, accessorImplementationType);
    //    else
    //        services.AddSingleton(accessorType, accessorImplementationType);

    //    Registry.RegisterState(implementation);
    //    return services;
    //}
    private static IServiceCollection AddStatePulseStateFeature(this IServiceCollection services, Type implementation)
    {
        var accessorType = typeof(IStateAccessor<>).MakeGenericType(implementation);
        var accessorImplementationType = typeof(StateAccessor<>).MakeGenericType(implementation);

        if (services.IsStateAccessorRegistered(accessorImplementationType))
            return services;

        // Check if the state implements IStateFeatureSingleton
        bool isSingletonFeature = typeof(IStateFeatureSingleton).IsAssignableFrom(implementation);

        if (isSingletonFeature)
            services.AddSingleton(accessorType, accessorImplementationType);
        else
            services.AddScoped(accessorType, accessorImplementationType);


        Registry.RegisterState(implementation);
        return services;
    }

    public static IServiceCollection AddStatePulseEffectValidator<TImplementation>(this IServiceCollection services)
    {
        var implType = typeof(TImplementation);

        var validatorInterface = implType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEffectValidator<,>));
        if (validatorInterface == null)
            throw new InvalidOperationException($"{implType.Name} must implement {typeof(IEffectValidator<,>).Name}");


        return services.AddStatePulseEffectValidator(validatorInterface!, implType);
    }
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
        if (ConfigureOptions.ServiceLifetime == Lifetime.Scoped)
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

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == reducerType)
                    {
                        services.AddStatePulseReducer(iface, type);
                        continue;
                    }

                    if (!iface.IsGenericType && iface == stateFeatureType)
                    {
                        services.AddStatePulseStateFeature(type);
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
