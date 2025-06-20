using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Store;

namespace StatePulse.NET.Tests.TestCases.InitializerTests;

public class StatePulseInitTests : TestBase
{
    // Testing state initialization
    [Fact]
    public void StateShouldInitializeCorrectly()
    {
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<MainMenuState>>();
        var state = stateAccessor.State;

        Assert.NotNull(state);
        Assert.True(!state.IsOpened);  // Default value should be 0
    }

    // Testing action dispatch
    [Fact]
    public async Task DispatchingActionShouldChangeState()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
        stateAccessor.StateChanged += (sender, state) =>
        {
            Assert.Equal("Maksim Shimshon", stateAccessor.State.ProfileName);
        };
        // Dispatch action that changes state
        var action = new ProfileCardDefineAction();
        await dispatcher.Prepare(() => action).UsingSynchronousMode().DispatchFastAsync();

        Assert.Equal("Maksim Shimshon", stateAccessor.State.ProfileName);
    }

    [Fact]
    public async Task DispatchingEffectShouldCorrectlyTriggerActions()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<MainMenuState>>();
        // Dispatch action that changes state
        var action = new ProfileCardDefineAction();
        await dispatcher.Prepare<MainMenuOpenAction>().UsingSynchronousMode().DispatchFastAsync();

        Assert.NotEmpty(stateAccessor.State.NavigationItems ?? new());
    }

    [Fact]
    public async Task DispatchingBurstShouldTriggerSafetyCancel()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
        // Dispatch action that changes state
        var action = new ProfileCardDefineAction();

        for (int i = 0; i < 10; i++)
        {
            await dispatcher.Prepare<ProfileCardDefineAction>().With(p => p.TestData, "Trigger 1").DispatchSafeAsync();
            await dispatcher.Prepare<ProfileCardDefineAction>().With(p => p.TestData, "Trigger 2").UsingSynchronousMode().DispatchSafeAsync();
            Assert.Equal("Trigger 2", stateAccessor.State.ProfileName);

        }
    }
}
