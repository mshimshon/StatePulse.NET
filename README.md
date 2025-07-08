[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://opensource.org/licenses/MIT)
[![NuGet Version](https://img.shields.io/nuget/v/StatePulse.Net)](https://www.nuget.org/packages/StatePulse.NET)
[![](https://img.shields.io/nuget/dt/StatePulse.NET?label=Downloads)](https://www.nuget.org/packages/StatePulse.NET)

# StatePulse.NET
StatePulse.NET is a precision-tuned state and action management system that balances high-performance fire-and-forget operations with optional, internally controlled execution order when explicitly required. 
It enables anti-duplication chaining for critical flows, preventing race conditions and ensuring consistent outcomes even under rapid user input or concurrent triggers. 
Its internal tracking infrastructure provides near-zero overhead cancellation and dispatch control, drastically reducing inconsistency. 
At the same time, it preserves the flexibility of traditional untracked state management, letting developers selectively enforce order and reliability without compromising overall responsiveness or introducing global locks.


## ✨ Features
- ⚡ **Fast Fire-and-Forget** — Executes actions immediately even tracked action are fire-and-forget.
- 🛡 **Anti-Duplicate Dispatching** — Prevents redundant or overlapping actions that can cause race condition state inconsistency.
- 🔍 **Validator System** — Supports multiple action validators for modular and reusable rule enforcement.
- 🧪 **Synchronous Debug Mode** — Optional lockstep mode for testing, diagnostics, and `Task.WhenAll` pipelines.
- 🧵 **DispatchTracker** — High-performance cancellation and deduplication logic via optimized concurrent tracking.

### 🚀 **State Management with Zero Boilerplate and Zero Compromises**

- **Lazy State Access Model:** Inject `IStatePulse` directly into your Blazor component and call `StateOf<TState>(()=>this, TaskMethod)` to get scoped state access.  
- **Component-Scoped Event Listening:** Automatically registers event listeners only for that component, ensuring `StateHasChanged()` is called exclusively on components subscribed to state changes.  
- **No Base Classes or Global Event Listeners:** Avoids global re-renders and boilerplate base class inheritance, giving you fine-grained control over component rendering and event subscription without forcing you into base classes.  
- **Automatic Listener Disposal:** Event listeners are automatically tracked and disposed with the component lifecycle, preventing memory leaks and dangling references.  
- **Transient `IStatePulse` Service:** Each component gets its own `IStatePulse` instance, isolating event subscriptions and making state updates scoped and efficient.


## 📦 Installation & Setup


```
Install-Package StatePulse.Net

dotnet add package StatePulse.Net

```

```csharp
services.AddStatePulseServices(o =>
        {
            o.ScanAssemblies = new Type[] { typeof(Program) };
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

### **Define Actions Validator** (Optional):

```csharp
/*
You are not required to create have an action validator but it is very useful when you have business logic that conditionally only contionally fires.
When validation fails it ignores the dispatch and move on.
*/
internal class ProfileCardDefineActionValidator : IActionValidator<ProfileCardDefineAction>
{
    public void Validate(ProfileCardDefineAction action, ref ValidationResult result)
    {
        if (action.TestData == "Error")
            result.AddError("ErrorName", "Name Cannot be Error");
    }
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
- ISafeAction implementations are always dispatched safely, ignoring unsafe flags.
- synchronous is an anti-pattern of statemanement use it sparingly; it is primarily for debugging or specific scenarios requiring full completion before continuation.

### **Access State**:

```csharp
var stateAccessor = ServiceProvider.GetRequiredService<IStateAccessor<ProfileCardState>>();
```

## Blazor Example Usage

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


## 🔖 Versioning Policy

### 🚧 Pre-1.0.0 (`0.x.x`)

- The project is considered **Work In Progress**.
- **Breaking changes can occur at any time** without notice.
- No guarantees are made about stability or upgrade paths.

### ✅ Post-1.0.0 (`1.x.x` and beyond)

Follows a common-sense semantic versioning pattern:

- **Major (`X.0.0`)**  
  
  - Introduces major features or architectural changes  
  - May include well documented **breaking changes**

- **Minor (`1.X.0`)**  
  
  - Adds new features or enhancements  
  - May include significant bug fixes  
  - **No breaking changes**

- **Patch (`1.0.X`)**  
  
  - Hotfixes or urgent bug fixes  
  - Safe to upgrade  
  - **No breaking changes**
  