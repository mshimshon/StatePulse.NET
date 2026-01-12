using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.Reducers;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.States;
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
        ServiceCollection.AddStatePulseServices(o =>
        {

        });
        ServiceCollection.AddStatePulseService<MainMenuLoaderStartAction>();
        ServiceCollection.AddStatePulseService<MainMenuLoaderStopAction>();
        ServiceCollection.AddStatePulseService<MainMenuLoadNavigationItemsAction>();
        ServiceCollection.AddStatePulseService<MainMenuLoadNavigationItemsResultAction>();
        ServiceCollection.AddStatePulseService<MainMenuOpenAction>();
        ServiceCollection.AddStatePulseService<ProfileCardDefineAction>();
        ServiceCollection.AddStatePulseService<ProfileCardDefineResultAction>();
        ServiceCollection.AddStatePulseService<ProfileCardLoaderStartAction>();
        ServiceCollection.AddStatePulseService<ProfileCardLoaderStopAction>();
        ServiceCollection.AddStatePulseService<UpdateCounterAction>();
        ServiceCollection.AddStatePulseService<ProfileCardDefineEffect>();
        ServiceCollection.AddStatePulseService<MainMenuLoadNavigationItemsEffect>();
        ServiceCollection.AddStatePulseService<MainMenuOpenEffect>();

        ServiceCollection.AddStatePulseService<MainMenuOpenEffectValidation>();
        ServiceCollection.AddStatePulseService<ProfileCardDefineActionValidator>();

        ServiceCollection.AddStatePulseService<MainMenuLoaderStartReducer>();
        ServiceCollection.AddStatePulseService<MainMenuLoaderStopReducer>();
        ServiceCollection.AddStatePulseService<MainMenuLoadNavigationItemsResultReducer>();
        ServiceCollection.AddStatePulseService<MainMenuOpenReducer>();
        ServiceCollection.AddStatePulseService<ProfileCardDefineResultReducer>();
        ServiceCollection.AddStatePulseService<UpdateCounterReducer>();
        ServiceCollection.AddStatePulseService<ProfileCardState>();
        ServiceCollection.AddStatePulseService<MainMenuState>();

        ServiceCollection.AddStatePulseService<CounterState>();
        // Register your services
        ServiceProvider = ServiceCollection.BuildServiceProvider();
    }
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}