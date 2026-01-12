using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net;
using StatePulse.Net.Engine;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.States;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Profile.Store;
using System.Diagnostics;

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
        var scopedServices = ServiceProvider.CreateScope().ServiceProvider;
        var dispatcher = scopedServices.GetRequiredService<IDispatcher>();
        var stateAccessor = scopedServices.GetRequiredService<IStateAccessor<ProfileCardState>>();
        var tracker = scopedServices.GetRequiredService<IDispatchTracker<ProfileCardDefineAction>>();
        var tracker2 = scopedServices.GetRequiredService<IDispatchTracker<ProfileCardDefineAction>>();
        int cancelled = 0;
        tracker.OnCancel += (o, action) =>
        {

            cancelled++;
        };
        // Dispatch action that changes state
        int changes = 0;
        stateAccessor.OnStateChanged += (s, state) =>
        {
            changes++;
        };
        List<Guid> dispatches = new();
        Random random = new Random();
        int[] timing = [
            50, // Entry
            25, // Cancels 0
            75, // 
            50,
            25,
            50,
            1000,
            25,
            50,
            25
        ];
        string winingValue = $"Profile";
        List<string> possibleRaceConditions = new();
        for (int i = 0; i < 10; i++)
        {
            winingValue = $"Profile {random.Next()}";
            var id = await dispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, winingValue)
                .With(p => p.Delay, timing[i])
                .DispatchAsync(true);
            dispatches.Add(id);
            possibleRaceConditions.Add(winingValue);
        }
        await Task.Delay(timing.Sum());


        do
        {


        } while (changes <= 0);
        bool isPassing = stateAccessor.State.ProfileName == possibleRaceConditions.Last();
        if (!isPassing) Debugger.Break();
        Assert.True(isPassing);
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
