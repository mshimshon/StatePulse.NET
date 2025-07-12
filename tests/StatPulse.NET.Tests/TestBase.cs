using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Effects;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Effects.Validators;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Effects;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Effects.Validators;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Reducers;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Store;

namespace StatePulse.NET.Tests;
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase()
    {
        IServiceCollection services = new ServiceCollection();
        // TOOD: Remove Scan Add Manual for Tests which is best policy would most lekily avoid inconsistent
        // service exceptions du to thread safe on bulk testing.
        services.AddStatePulseServices(o => { });
        services.AddStatePulseAction<MainMenuLoaderStartAction>();
        services.AddStatePulseAction<MainMenuLoaderStopAction>();
        services.AddStatePulseAction<MainMenuLoadNavigationItemsAction>();
        services.AddStatePulseAction<MainMenuLoadNavigationItemsResultAction>();
        services.AddStatePulseAction<MainMenuOpenAction>();
        services.AddStatePulseAction<ProfileCardDefineAction>();
        services.AddStatePulseAction<ProfileCardDefineResultAction>();
        services.AddStatePulseAction<ProfileCardLoaderStartAction>();
        services.AddStatePulseAction<ProfileCardLoaderStopAction>();
        services.AddStatePulseEffect<ProfileCardDefineEffect>();
        services.AddStatePulseEffect<MainMenuLoadNavigationItemsEffect>();
        services.AddStatePulseEffect<MainMenuOpenEffect>();
        services.AddStatePulseEffectValidator<MainMenuOpenEffectValidation>();
        services.AddStatePulseEffectValidator<ProfileCardDefineActionValidator>();
        services.AddStatePulseReducer<MainMenuLoaderStartReducer>();
        services.AddStatePulseReducer<MainMenuLoaderStopReducer>();
        services.AddStatePulseReducer<MainMenuLoadNavigationItemsResultReducer>();
        services.AddStatePulseReducer<MainMenuOpenReducer>();
        services.AddStatePulseReducer<ProfileCardDefineResultReducer>();
        services.AddStatePulseStateFeature<ProfileCardState>();
        services.AddStatePulseStateFeature<MainMenuState>();
        // Register your services
        ServiceProvider = services.BuildServiceProvider();
    }
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}