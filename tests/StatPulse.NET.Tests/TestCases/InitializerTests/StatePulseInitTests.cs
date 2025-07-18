using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net;
using StatePulse.Net.Engine;
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
        stateAccessor.OnStateChanged += (sender, state) =>
        {
            Assert.Equal("Maksim Shimshon", stateAccessor.State.ProfileName);
        };
        // Dispatch action that changes state
        var action = new ProfileCardDefineAction();
        await dispatcher.Prepare(() => action).Await().DispatchAsync();

        Assert.Equal("Maksim Shimshon", stateAccessor.State.ProfileName);
    }

    [Fact]
    public async Task DispatchingEffectShouldCorrectlyTriggerActions()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        // Dispatch action that changes state
        await dispatcher.Prepare<MainMenuOpenAction>().Await().DispatchAsync();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<MainMenuState>>();

        Assert.NotEmpty(stateAccessor.State.NavigationItems ?? new());
    }

    [Theory]
    [InlineData("Test")]
    [InlineData("Error")]
    public async Task DispatchingEffectShouldCorrectlyFailActionValidator(string name)
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
        // Dispatch action that changes state
        await dispatcher.Prepare<ProfileCardDefineAction>().With(p => p.TestData, name)
            .Await().DispatchAsync();
        if (name == "Error") Assert.True(stateAccessor.State.UnitTestStringer == default);
        else Assert.True(stateAccessor.State.UnitTestStringer == name);
    }

    [Fact]
    public async Task DispatchingBurstShouldTriggerSafetyCancel()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var tracker = ServiceProvider.GetRequiredService<IDispatchTracker<ProfileCardDefineAction>>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
        // Dispatch action that changes state
        int changes = 0;
        stateAccessor.OnStateChanged += (s, state) =>
        {
            changes++;
        };
        Guid a = Guid.Empty, b = Guid.Empty;
        bool cont = false, aDone = false, bDone = false;
        tracker.OnCancel += (o, action) =>
        {
            if (action.Id == a) aDone = true;
            if (action.Id == b) bDone = true;
            if (aDone && bDone) cont = true;
        };
        List<string> result = new();
        for (int i = 0; i < 100; i++)
        {
            a = await dispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, "Profile 1")
                .DispatchAsync(true);

            b = await dispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, "Profile 2")
                .DispatchAsync(true);

            while (!cont) { await Task.Delay(100); }
            cont = false;
            result.Add(stateAccessor.State.ProfileName!);
        }
        foreach (var item in result)
            Assert.Equal("Profile 2", item);
        Assert.True(result.Count > 0);
    }

    [Fact]
    public async Task DispatchingBurstShouldTriggerInconsistentResults()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var tracker = ServiceProvider.GetRequiredService<IDispatchTracker<ProfileCardDefineAction>>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
        // Dispatch action that changes state
        int changes = 0;
        stateAccessor.OnStateChanged += (s, state) =>
        {
            changes++;
        };

        List<string> result = new();
        for (int i = 0; i < 25; i++)
        {
            var a = dispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, "Profile 1")
                .Await()
                .DispatchAsync();


            var b = dispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, "Profile 2")
                .Await()
                .DispatchAsync();
            await Task.WhenAll(a, b);
            result.Add(stateAccessor.State.ProfileName!);
        }
        int inConsistenceCount = 0;
        string lastEntry = string.Empty;
        foreach (var item in result)
        {
            if (lastEntry != item && lastEntry != string.Empty)
            {
                inConsistenceCount++;
            }
            lastEntry = item;
        }
        Assert.True(inConsistenceCount > 0);

    }
}
