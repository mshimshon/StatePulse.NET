using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Configuration;
using StatePulse.Net.Engine;
using StatePulse.Net.Engine.Exceptions;
using StatePulse.Net.Engine.Implementations;
using System.Reflection;

namespace StatePulse.Net;

public static class ServiceRegisterExt
{

    private static bool _scanned;
    public static ConfigureOptions ConfigureOptions { get; set; } = new ConfigureOptions();
    private static StatePulseRegistry Registry = new StatePulseRegistry();


    public static IServiceCollection AddStatePulseServices(this IServiceCollection services, Action<ConfigureOptions>? configure = default)
    {
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IDispatchFactory, DispatchFactory>();
        if (configure != default)
            configure(ConfigureOptions);

        bool isSingleThreadModel = ConfigureOptions.PulseTrackingPerformance == PulseTrackingModel.SingleThreadFast || ConfigureOptions.PulseTrackingPerformance == PulseTrackingModel.BlazorWebAssemblyFast;
        if (isSingleThreadModel)
            services.AddTransient<IStatePulse, PulseLazyStateWebAssembly>();
        else
            services.AddTransient<IStatePulse, PulseLazyStateBlazorServer>();

        services.AddScoped<IPulseGlobalTracker, PulseGlobalTracker>();
        services.AddSingleton<IStatePulseRegistry>(Registry);
        services.AutoRegisterTypes(ConfigureOptions.AutoRegisterTypes);
        services.ScanStatePulseAssemblies(ConfigureOptions.ScanAssemblies);
        return services;
    }

    public static IServiceCollection AddStatePulseService<TImplementation>(this IServiceCollection services)
    {
        services.AddStatePulseService(typeof(TImplementation));
        return services;
    }
    public static IServiceCollection AddStatePulseService(this IServiceCollection services, Type implementation)
    {
        services.RegisterTypeService(implementation);
        return services;
    }


    private static void AutoRegisterTypes(this IServiceCollection services, Type[] types)
    {
        foreach (var type in types)
            services.AddStatePulseService(type);
    }
    private static IServiceCollection AddStatePulseEffect(this IServiceCollection services, Type iFace, Type implementation)
    {
        if (services.IsImplementationRegistered(iFace, implementation)) return services;
        services.AddTransient(iFace, implementation);

        Registry.RegisterEffect(iFace, implementation);
        return services;
    }

    private static IServiceCollection AddStatePulseReducer(this IServiceCollection services, Type iFace, Type implementation)

    {
        if (services.IsReducerRegistered(iFace)) return services;
        services.AddTransient(iFace, implementation);
        Registry.RegisterReducer(iFace, implementation);
        return services;
    }
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

    private static IServiceCollection AddStatePulseEffectValidator(this IServiceCollection services, Type iFace, Type implementation)

    {
        if (services.IsEffectValidatorImplementationRegistered(implementation)) return services;
        services.AddTransient(iFace, implementation);
        Registry.RegisterEffectValidator(iFace, implementation);
        return services;
    }
    private static IServiceCollection AddStatePulseAction(this IServiceCollection services, Type implementation)

    {
        // Add Action Based Singleton Dispatch Tracker.
        var dispatchTrackerIface = typeof(IDispatchTracker<>).MakeGenericType(implementation);
        var dispatchTracker = typeof(DispatchTracker<>).MakeGenericType(implementation);
        if (services.IsDispatchTrackerRegistered(dispatchTracker)) return services;
        Registry.RegisterAction(implementation);
        services.AddScoped(dispatchTrackerIface, dispatchTracker);

        return services;
    }


    /// <summary>
    /// Automatically scan supplied assemblies for IEffect, IReducer and IStateFeature
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    private static void ScanStatePulseAssemblies(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (_scanned) return;
        _scanned = true;

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(p => (!p.IsAbstract && !p.IsInterface)).ToArray();
            services.AutoRegisterTypes(types);
        }
    }

    private static void RegisterTypeService(this IServiceCollection services, Type type)
    {

        if (type.IsAbstract || type.IsInterface)
            throw new UnableToRegisterExplicitServiceException($"{type.Name} is Abstract or Interface!");
        foreach (var iface in type.GetInterfaces())
            services.RegisterSelfDetectedInterface(iface, type);

    }

    private static void RegisterSelfDetectedInterface(this IServiceCollection services, Type iface, Type type)
    {
        var effectMiddlewareType = typeof(IEffectMiddleware);
        var reducerMiddlewareType = typeof(IReducerMiddleware);
        var dispatchMiddlewareType = typeof(IDispatcherMiddleware);
        var effectType = typeof(IEffect<>);
        var reducerType = typeof(IReducer<,>);
        var stateFeatureType = typeof(IStateFeature);
        var stateFeatureSingletonType = typeof(IStateFeatureSingleton);
        var actionType = typeof(IAction);
        var actionSafeType = typeof(ISafeAction);
        var actionValidatorType = typeof(IEffectValidator<,>);

        bool isMiddlewareType = !iface.IsGenericType &&
            (iface == effectMiddlewareType || iface == reducerMiddlewareType || iface == dispatchMiddlewareType) &&
            !services.IsImplementationRegistered(type, iface);

        if (isMiddlewareType)
            services.AddTransient(iface, type);
        else if (!iface.IsGenericType && (iface == actionType || iface == actionSafeType))
            services.AddStatePulseAction(type);
        else if (!iface.IsGenericType && (iface == stateFeatureType || iface == stateFeatureSingletonType))
            services.AddStatePulseStateFeature(type);

        if (!iface.IsGenericType) return;

        var genericDef = iface.GetGenericTypeDefinition();
        if (iface.IsGenericType && genericDef == effectType)
            services.AddStatePulseEffect(iface, type);
        else if (iface.IsGenericType && genericDef == reducerType)
            services.AddStatePulseReducer(iface, type);
        else if (genericDef == actionValidatorType)
            services.AddStatePulseEffectValidator(iface, type);


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
