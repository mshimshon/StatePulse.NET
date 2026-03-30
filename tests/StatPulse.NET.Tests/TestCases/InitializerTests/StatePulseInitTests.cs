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
    public Task OnUpdate() => Task.CompletedTask;
    // Testing action dispatch
    [Fact]
    public async Task DispatchingActionShouldChangeStateUsingStateOf()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var sp = ServiceProvider.GetRequiredService<IStatePulse>();
        var state = () => sp.StateOf<ProfileCardState>(() => this, OnUpdate);
        // Dispatch action that changes state
        var action = new ProfileCardDefineAction();
        await dispatcher.Prepared(action).Await().DispatchAsync();

        Assert.Equal("Maksim Shimshon", state().ProfileName);
    }


    [Fact]
    public async Task Should_Successful_Reducer_MultipleStates()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        // Dispatch action that changes state
        var state1 = ServiceProvider.GetRequiredService<IStateAccessor<MainMenuState>>();
        var state2 = ServiceProvider.GetRequiredService<IStateAccessor<MainMenuSecondState>>();
        await dispatcher.Prepare<MainMenuOpenAction>().Await().DispatchAsync();

        Assert.NotEmpty(state1.State.NavigationItems ?? new());
        Assert.True(state2.State.IsSuccessful);
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

    [Fact]
    public async Task DispatchChangingDiffPropsOnSameStateShouldNotHaveConcurrentIssues()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<MainMenuState>>();
        bool pass = true;
        for (int i = 0; i < 100; i++)
        {
            var t1 = dispatcher.Prepare<MainMenuOpenAction>().Await().DispatchAsync();
            var t2 = dispatcher.Prepare<MainMenuLoaderStartAction>().Await().DispatchAsync();
            await Task.WhenAll([t1, t2]);
            if (stateAccessor.State.IsOpened != true || stateAccessor.State.NavigationItems == default || stateAccessor.State.NavigationItems.Count <= 0)
            {
                pass = false;
                break;
            }
            await dispatcher.Prepare<MainMenuLoaderStartAction>().Await().DispatchAsync();
        }
        Assert.True(pass);
    }

    [Fact]
    public async Task DispatchCancelTokenShouldWork()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<MainMenuState>>();
        bool pass = true;
        var ct = new CancellationTokenSource();
        var t1 = dispatcher.Prepare<MainMenuOpenAction>().Await().DispatchAsync(false, ct.Token);
        ct.Cancel();
        await t1;
        Assert.True(stateAccessor.State.NavigationItems == default);
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
        var stateAccessor = scopedServices.GetRequiredService<IStateAccessor<ProfileCardState>>();
        var tracker = scopedServices.GetRequiredService<IDispatchTracker>();

        // Dispatch action that changes state
        int changes = 0;
        stateAccessor.OnStateChanged += (s, state) =>
        {
            changes++;
        };
        List<Guid> dispatches = new();
        Random random = new Random();
        int[] timing = [
            51, // Entry
            22, // Cancels 0
            73, // 
            54,
            25,
            56,
            1007,
            28,
            59,
            20
        ];
        string winingValue = $"Profile";
        List<string> possibleRaceConditions = new();
        for (int i = 0; i < 10; i++)
        {
            winingValue = $"Profile {timing[i]}";
            var dispatcher = scopedServices.GetRequiredService<IDispatcher>();
            _ = dispatcher.Prepare<ProfileCardDefineAction>()
                .With(p => p.TestData, winingValue)
                .With(p => p.Delay, timing[i])
                .DispatchAsync(true);
            possibleRaceConditions.Add(winingValue);
        }
        await Task.Delay(timing.Sum());


        do
        {


        } while (changes <= 0);
        bool isPassing = stateAccessor.State.ProfileName.Equals(winingValue);
        if (!isPassing) Debugger.Break();
        Assert.True(isPassing);
    }

    [Fact]
    public async Task DispatchingBurstShouldTriggerInconsistentResults()
    {
        var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        var tracker = ServiceProvider.GetRequiredService<IDispatchTracker>();
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
