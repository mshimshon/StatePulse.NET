
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://opensource.org/licenses/MIT)
[![NuGet Version](https://img.shields.io/nuget/v/StatePulse.Net)](https://www.nuget.org/packages/StatePulse.NET)
[![](https://img.shields.io/nuget/dt/StatePulse.NET?label=Downloads)](https://www.nuget.org/packages/StatePulse.NET)
[![Build](https://github.com/mshimshon/StatePulse.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/mshimshon/StatePulse.NET/actions/workflows/ci.yml)
[![Deploy](https://github.com/mshimshon/StatePulse.NET/actions/workflows/deploy.yml/badge.svg)](https://github.com/mshimshon/StatePulse.NET/actions/workflows/deploy.yml)

# StatePulse.NET
### [Official Documentation](https://statepulse.net/)

StatePulse.NET is a precision‑engineered state and action management system designed for high‑performance fire‑and‑yield workflows. It supports optional, internally controlled execution ordering when deterministic sequencing is explicitly required.
Its multi‑layer anti‑duplication pipeline eliminates redundant dispatches, prevents race conditions, and maintains consistent outcomes even under rapid input or concurrent triggers.
A lightweight internal tracking core provides near‑zero‑overhead cancellation and dispatch control, minimizing inconsistency without sacrificing throughput.
Despite these guarantees, StatePulse.NET preserves the flexibility of traditional untracked state management, allowing developers to selectively enforce ordering and reliability without introducing global locks or compromising responsiveness.


## Features

**Fast Fire-and-Yield**  
Executes actions immediately, including tracked actions, while preserving fire-and-yield semantics.

**Multi-Layer Anti-Duplicate Dispatching**
Layer 1: Cancels previously dispatched duplicates before effects, between effects, or after effects, ensuring no redundant action progresses through the pipeline.
Layer 2: Uses a global state-change ticker enforcing a strict “latest action wins” rule so outdated or superseded actions cannot update state.

**Effect Validator System**  
Supports multiple, composable validators for modular and reusable rule enforcement.

**Synchronous Debug Mode**  
Provides an optional lockstep execution mode ideal for testing, diagnostics, and `Task.WhenAll` based pipelines.

**DispatchTracker**  
Offers high-performance cancellation, deduplication, and concurrency control through an optimized tracking mechanism.

**Short-Lived Middlewares**
Provides lightweight, disposable middleware hooks that run only during the lifetime of a single dispatch cycle.

**Dispatch Middlewares**
Runs Before, After, and WhenDispatchFails. In asynchronous dispatch modes, failures are silently discarded unless handled internally, so DEC logic should manage its own errors; a logging middleware can also capture unhandled pipeline failures.

**Effect Middlewares**
Runs Before, After, WhenValidationFails, and WhenValidationSucceed, allowing fine‑grained control and instrumentation around effect execution and validation flow.

**Reducer Middlewares**
Runs Before and After the reducer, enabling patterns such as event dispatch on state changes, logging, instrumentation, or enforcing reducer‑level invariants.


### 🚀 **State Management with Zero Boilerplate and Zero Compromises**

- **Lazy State Access Model:** Inject `IStatePulse` directly into your Blazor component and call `StateOf<TState>(()=>this, TaskMethod)` to get scoped state access.  
- **Component-Scoped Event Listening:** Automatically registers event listeners only for that component, ensuring `StateHasChanged()` is called exclusively on components subscribed to state changes.  
- **No Base Classes or Global Event Listeners:** Avoids global re-renders and boilerplate base class inheritance, giving you fine-grained control over component rendering and event subscription without forcing you into base classes.  
- **Automatic Listener Disposal:** Event listeners are automatically tracked and disposed with the component lifecycle, preventing memory leaks and dangling references.  
- **Transient `IStatePulse` Service:** Each component gets its own `IStatePulse` instance, isolating event subscriptions and making state updates scoped and efficient.


## Benchmark
| Method                                         | Mean       | Error     | StdDev    | Median     |
|----------------------------------------------- |-----------:|----------:|----------:|-----------:|
| StatePulse_Dispatch                            |   2.458 μs | 0.0344 μs | 0.0322 μs |   2.455 μs |
| StatePulse_BusrtDispatch                       | 321.243 μs | 4.6181 μs | 4.3198 μs | 322.030 μs |
| StatePulse_BusrtSafeDispatch                   | 350.282 μs | 4.4814 μs | 4.1919 μs | 351.182 μs |
| StatePulse_FireYieldDispatch                   |   3.193 μs | 0.0631 μs | 0.0675 μs |   3.193 μs |
| StatePulse_FireYield_SequentialEffectsDispatch |   3.326 μs | 0.0661 μs | 0.0969 μs |   3.303 μs |
| StatePulse_AwaitedDispatch                     |   4.420 μs | 0.6850 μs | 2.0196 μs |   3.165 μs |

StatePulse delivers strong performance given its feature set, but it’s not designed for tight, high‑frequency loops. Long‑term performance improvements are planned, as there are several areas with optimization potential. For now, the priority remains system stability, configuration robustness, and feature completeness.


## 📦 Installation & Setup


```
Install-Package StatePulse.Net

dotnet add package StatePulse.Net

```

### 3 Ways to Register Services

**Method 1**  
The most deterministic and explicit registration approach. This method avoids “magic” and one‑liners by requiring you to manually add all Reducers, Effects, Middlewares, Validators, and Actions. It provides full clarity and control over what the system loads.

```csharp
    ServiceCollection.AddStatePulseServices(o =>
    {
        o.AutoRegisterTypes = [
                typeof(MainMenuLoaderStartAction),
                typeof(MainMenuLoaderStopAction),
                typeof(MainMenuLoadNavigationItemsAction),
                typeof(MainMenuLoadNavigationItemsResultAction),
                typeof(MainMenuOpenAction),
                typeof(ProfileCardDefineAction),
                typeof(ProfileCardDefineResultAction),
                typeof(ProfileCardLoaderStartAction),
                typeof(ProfileCardLoaderStopAction),
                typeof(UpdateCounterAction),
                typeof(ProfileCardDefineEffect),
                typeof(ProfileCardDefineResultAction),
                typeof(MainMenuLoadNavigationItemsEffect),
                typeof(MainMenuOpenEffect),
                typeof(MainMenuOpenEffectValidation),
                typeof(ProfileCardDefineActionValidator),
                typeof(MainMenuLoaderStartReducer),
                typeof(MainMenuLoaderStopReducer),
                typeof(MainMenuLoadNavigationItemsResultReducer),
                typeof(MainMenuOpenReducer),
                typeof(ProfileCardDefineResultReducer),
                typeof(UpdateCounterReducer),
                typeof(ProfileCardState),
                typeof(MainMenuState),
                typeof(CounterState),
            ];
    });
```

**Method 2**  
This is also very explicit since v2+ we have a single entry `AddStatePulseService` for all statepulse types (Reducers, Effects, Middlewares, Validators, and Actions).

```csharp
    ServiceCollection.AddStatePulseServices();
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
```

**Method 3**
The assembly-scan approach. Convenient but not recommended for most scenarios. While useful for rapid setup, it can introduce problems as system grows.

```csharp
    ServiceCollection.AddStatePulseServices(o => {
        o.ScanAssemblies = [typeof(TestBase).Assembly];
    });
```

## 🧭 How It Works



### **Define Actions**:

```csharp

// IAction { }
// ISafeAction { } // Cannot be dispatched unsafely

public record ProfileCardDefineAction : IAction
{
    public string? TestData { get; set; }
}

```


### **Define Effect**:

```csharp

internal class ProfileCardDefineEffect : IEffect<ProfileCardDefineAction>
{

    public ProfileCardDefineEffect()
    {
    }
    public async Task EffectAsync(ProfileCardDefineAction action, IDispatcher dispatcher)
    {
        var random = new Random();
        int value = random.Next(100, 1001); // Upper bound is exclusive, so use 1001
        await Task.Delay(value);
        var myProfile = new UserResponse();
        await dispatcher.Prepare(() => new ProfileCardDefineResultAction(action.TestData ?? myProfile.Name, myProfile.Picture, myProfile.Id))
            .DispatchAsync();
    }

}



### **Define Effect Validator** (Optional):

```csharp
/*
* This is the best way to define clean conditional effects, it either run or not... this is not meant for triggering errors.
* This is meant for optional/condition effects to either run or not base on the action settings...
*/
internal class ProfileCardDefineActionValidator : IEffectValidator<ProfileCardDefineAction, ProfileCardDefineEffect>
{
    public Task<bool> Validate(ProfileCardDefineAction action)
    {
        if (action.TestData == "Error")
            return Task.FromResult(false);
        return Task.FromResult(true);
    }
}
```

```

### **Define Reducer**:

```csharp
internal class ProfileCardDefineResultReducer : IReducer<ProfileCardState, ProfileCardDefineResultAction>
{
    public Task<ProfileCardState> ReduceAsync(ProfileCardState state, ProfileCardDefineResultAction action)
        => Task.FromResult(state with
        {
            LastUpdate = DateTime.UtcNow,
            ProfileId = action.Id,
            ProfileName = action.Name,
            ProfilePicture = action.Picture
        });
}
```

### **Define StateFeature**:

```csharp
public record ProfileCardState : IStateFeature
{
    public string? ProfileName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? ProfileId { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
```

### **Trigger Dispatch**:

```csharp
var dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
await dispatcher.Prepare<ProfileCardDefineAction>().With(p => p.TestData, name)
    .DispatchAsync();

// You can Capture the validation in case of failure, only call if validators exist.
ValidationResult? validation = default;
await dispatcher.Prepare<ProfileCardDefineAction>().With(p => p.TestData, name)
    .HandleActionValidation(p => validation = p)
    .DispatchAsync();

// You can trigger synchronously... this will await the whole pipeline, otherwise you just await until action is send to dispatch pool.
await dispatcher.Prepare<ProfileCardDefineAction>().With(p => p.TestData, name)
    .Sync()
    .DispatchAsync();

// if the action is implementing ISafeState, the dispatch will always run asSafe=true but an action not implementing ISafeAction will
// have the option to run asSafe or not...
await dispatcher.Prepare<ProfileCardDefineAction>().With(p => p.TestData, name)
    .DispatchAsync(true);
```


### Important Notes
- Rule of thumb is always await dispatch calls, avoiding to do so can cause inconsistency for safe dispatch mode..
- ISafeAction implementations are always dispatched safely, ignoring unsafe flag.
- synchronous is an anti-pattern of statemanement use it sparingly; it is primarily for debugging or specific scenarios requiring full completion before continuation.

### **Access State**:

```csharp
var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
```

### Blazor Example Usage

```csharp
using StatePulse.Net;

public partial class CounterView : ComponentBase
{

    // METHOD 1:
    [Inject] public IStatePulse PulseState { get; set; } = default!; // Handles State Accessor

    // This is for convienience always use this method or directly PulseState.StateOf<CounterState>(this).Value
    // Never assign State Instance variable as it will not update... 
    // Never use lambda it will throw exception as WeakREference is fundamatally flawed and disposes of lambda even when its object is alive.
    private CounterState state => PulseState.StateOf<CounterState>(()=>this, OnUpdate);
    
    private async Task OnUpdate() => await InvokeAsync(StateHadChanged);

    // METHOD 2: 
    // Inject direct state but injecting the state directly requires you to handle onchanged events by sub/unsub in lifecycle
    // Or to create a basecomponent system similar to other state management systems.
    [Inject] public IStateAccessor<CounterState> State { get; set; } = default!; 

    
}
```
