﻿using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Configuration;
using StatePulse.Net.Engine;
using StatePulse.Net.Engine.Implementations;

namespace StatePulse.Net;
public static class ServiceRegisterExt
{
    private static bool _scanned;
    private static ConfigureOptions _configureOptions = new ConfigureOptions();
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

        if (_configureOptions.ServiceLifetime == LifetimeEnum.Scoped)
            services.AddScoped<IPulseGlobalTracker, PulseGlobalTracker>();
        else
            services.AddSingleton<IPulseGlobalTracker, PulseGlobalTracker>();

        services.AddTransient<IStatePulse, PulseLazyStateWebAssembly>();
        services.AddTransient<IStatePulse, PulseLazyStateBlazorServer>();
        services.ScanStatePulseAssemblies(_configureOptions.ScanAssemblies);
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

        var effectType = typeof(IEffect<>);
        var reducerType = typeof(IReducer<,>);
        var stateFeatureType = typeof(IStateFeature);
        var actionType = typeof(IAction);
        var actionSafeType = typeof(ISafeAction);
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
                        if (services.IsEffectImplementationRegistered(type)) continue;

                        services.AddTransient(iface, type);
                        registry.RegisterEffect(type);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == reducerType)
                    {
                        if (services.IsReducerRegistered(iface)) continue;
                        services.AddTransient(iface, type);
                        registry.RegisterReducer(type);
                        continue;
                    }

                    if (!iface.IsGenericType && iface == stateFeatureType)
                    {
                        var accessorType = typeof(IStateAccessor<>).MakeGenericType(type);
                        var accessorImplementationType = typeof(StateAccessor<>).MakeGenericType(type);

                        if (services.IsStateAccessorRegistered(accessorImplementationType)) continue;
                        if (_configureOptions.ServiceLifetime == LifetimeEnum.Scoped)
                            services.AddScoped(accessorType, accessorImplementationType);  // Registering the correct interface
                        else
                            services.AddSingleton(accessorType, accessorImplementationType);  // Registering the correct interface
                        registry.RegisterState(type);
                        continue;
                    }

                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == actionValidatorType)
                    {
                        if (services.IsActionValidatorImplementationRegistered(type)) continue;
                        services.AddTransient(iface, type);
                        registry.RegisterActionValidator(type);
                        continue;
                    }

                    if (!iface.IsGenericType && (iface == actionType || iface == actionSafeType))
                    {
                        // Add Action Based Singleton Dispatch Tracker.
                        var dispatchTrackerIface = typeof(IDispatchTracker<>).MakeGenericType(type);
                        var dispatchTracker = typeof(DispatchTracker<>).MakeGenericType(type);
                        if (services.IsDispatchTrackerRegistered(dispatchTracker)) continue;
                        registry.RegisterAction(type);
                        if (_configureOptions.ServiceLifetime == LifetimeEnum.Scoped)
                            services.AddScoped(dispatchTrackerIface, dispatchTracker);
                        else
                            services.AddSingleton(dispatchTrackerIface, dispatchTracker);

                    }
                }
            }
        }

        services.AddSingleton<IStatePulseRegistry>(registry);


    }
    public static bool IsReducerRegistered(this IServiceCollection services, Type reducerType)
    {
        return services.Any(s => s.ServiceType == reducerType);
    }
    public static bool IsEffectImplementationRegistered(this IServiceCollection services, Type implementationType)
    {
        return services.Any(s =>
            s.ImplementationType == implementationType &&
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IEffect<>)
        );
    }
    public static bool IsActionValidatorImplementationRegistered(this IServiceCollection services, Type implementationType)
    {
        return services.Any(s =>
            s.ImplementationType == implementationType &&
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IActionValidator<>)
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
