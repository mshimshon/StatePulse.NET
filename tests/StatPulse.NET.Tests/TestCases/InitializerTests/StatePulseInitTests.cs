using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;
using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net;
using StatePulse.Net.Engine;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.States;
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
    public async Task DispatchingEffectShouldCorrectlyFailEffectValidator(string name)
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


    [Fact]
    public async Task DispatchSingletonState()
    {
        using var circuitA = ServiceProvider.CreateScope();
        var circuitAAccessor = circuitA.ServiceProvider.GetRequiredService<IStateAccessor<CounterState>>();
        var circuitADispatcher = circuitA.ServiceProvider.GetRequiredService<IDispatcher>();

        using var circuitB = ServiceProvider.CreateScope();
        var circuitBAccessor = circuitB.ServiceProvider.GetRequiredService<IStateAccessor<CounterState>>();
        var circuitBDispatcher = circuitB.ServiceProvider.GetRequiredService<IDispatcher>();


        // Dispatch action that changes state
        int changesOnA = 0;
        circuitAAccessor.OnStateChanged += (s, state) =>
        {
            changesOnA++;
        };

        int changesOnB = 0;
        circuitBAccessor.OnStateChanged += (s, state) =>
        {
            changesOnB++;
        };
        List<Task> tasks = new();
        for (int i = 0; i < 2; i++)
        {
            var tsk = circuitADispatcher.Prepare<UpdateCounterAction>().Await().DispatchAsync();
            tasks.Add(tsk);
        }
        for (int i = 0; i < 4; i++)
        {
            var tsk = circuitBDispatcher.Prepare<UpdateCounterAction>().Await().DispatchAsync();
            tasks.Add(tsk);
        }
        await Task.WhenAll(tasks);
        Assert.True(changesOnB == changesOnA);

    }

    [Fact]
    public async Task DispatchSingletonState_Failure()
    {
        using var circuitA = ServiceProvider.CreateScope();
        var circuitAAccessor = circuitA.ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
        var circuitADispatcher = circuitA.ServiceProvider.GetRequiredService<IDispatcher>();

        using var circuitB = ServiceProvider.CreateScope();
        var circuitBAccessor = circuitB.ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
        var circuitBDispatcher = circuitB.ServiceProvider.GetRequiredService<IDispatcher>();


        // Dispatch action that changes state
        int changesOnA = 0;
        circuitAAccessor.OnStateChanged += (s, state) =>
        {
            changesOnA++;
            Assert.Equal("Profile 1", state.ProfileName);
        };

        int changesOnB = 0;
        circuitBAccessor.OnStateChanged += (s, state) =>
        {
            Assert.Equal("Profile 2", state.ProfileName);
            changesOnB++;
        };
        List<Task> tasks = new();
        for (int i = 0; i < 2; i++)
        {
            var tsk = circuitADispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, "Profile 1")
                .Await()
                .DispatchAsync();
            tasks.Add(tsk);
        }
        for (int i = 0; i < 4; i++)
        {
            var tsk = circuitBDispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, "Profile 2")
                .Await()
                .DispatchAsync();
            tasks.Add(tsk);
        }
        await Task.WhenAll(tasks);
        Assert.True(changesOnB != changesOnA);

    }
}
