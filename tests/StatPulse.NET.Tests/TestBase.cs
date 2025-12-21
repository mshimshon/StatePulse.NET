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
    protected IServiceProvider ServiceProvider { get; set; }
    protected IServiceCollection ServiceCollection { get; set; }
    protected TestBase()
    {
        ServiceCollection = new ServiceCollection();
        // TOOD: Remove Scan Add Manual for Tests which is best policy would most lekily avoid inconsistent
        // service exceptions du to thread safe on bulk testing.
        ServiceCollection.AddStatePulseServices(o => {
            
        });
        ServiceCollection.AddStatePulseAction<MainMenuLoaderStartAction>();
        ServiceCollection.AddStatePulseAction<MainMenuLoaderStopAction>();
        ServiceCollection.AddStatePulseAction<MainMenuLoadNavigationItemsAction>();
        ServiceCollection.AddStatePulseAction<MainMenuLoadNavigationItemsResultAction>();
        ServiceCollection.AddStatePulseAction<MainMenuOpenAction>();
        ServiceCollection.AddStatePulseAction<ProfileCardDefineAction>();
        ServiceCollection.AddStatePulseAction<ProfileCardDefineResultAction>();
        ServiceCollection.AddStatePulseAction<ProfileCardLoaderStartAction>();
        ServiceCollection.AddStatePulseAction<ProfileCardLoaderStopAction>();
        ServiceCollection.AddStatePulseEffect<ProfileCardDefineEffect>();
        ServiceCollection.AddStatePulseEffect<MainMenuLoadNavigationItemsEffect>();
        ServiceCollection.AddStatePulseEffect<MainMenuOpenEffect>();
        ServiceCollection.AddStatePulseEffectValidator<MainMenuOpenEffectValidation>();
        ServiceCollection.AddStatePulseEffectValidator<ProfileCardDefineActionValidator>();
        ServiceCollection.AddStatePulseReducer<MainMenuLoaderStartReducer>();
        ServiceCollection.AddStatePulseReducer<MainMenuLoaderStopReducer>();
        ServiceCollection.AddStatePulseReducer<MainMenuLoadNavigationItemsResultReducer>();
        ServiceCollection.AddStatePulseReducer<MainMenuOpenReducer>();
        ServiceCollection.AddStatePulseReducer<ProfileCardDefineResultReducer>();
        ServiceCollection.AddStatePulseStateFeature<ProfileCardState>();
        ServiceCollection.AddStatePulseStateFeature<MainMenuState>();
        // Register your services
        ServiceProvider = ServiceCollection.BuildServiceProvider();
    }
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}