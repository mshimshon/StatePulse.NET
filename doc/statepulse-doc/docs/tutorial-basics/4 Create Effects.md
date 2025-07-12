---
slug: gs-create-effect
title: Create Effects
tags: [hola, docusaurus]
sidebar_position: 4
---

## Effects – Executing Logic Before State Updates

**Effects** are units of logic that run **in response to an action**, before any associated reducers are invoked.

They are commonly used for:
- Performing **side effects** (e.g. API calls, logging, validation)
- Dispatching **follow-up actions** based on results
- Handling **asynchronous workflows**

### 🧱 Common Pattern: Request + Result

A typical pattern is to define:
- `SomeAction` – the original trigger
- `SomeActionResult` – dispatched by the effect after the async work is done

Reducers usually handle the `Result` action to update the state.

### 📛 Naming Convention

Effects are usually named using the action’s name followed by the `Effect` suffix.

### Create The Effect

```csharp title="IncreamentCounterEffect.cs"
internal class IncreamentCounterEffect : IEffect<IncreamentCounterAction>
{
    readonly IStateAccessor<CounterState> _counterState;
    public IncreamentCounterEffect(IStateAccessor<CounterState> counterState)
    {
        _counterState = counterState;
    }
    public async Task EffectAsync(IncreamentCounterAction action, IDispatcher dispatcher)
    {
        // Recommendation: Always Await Dispatch to avoid safe execution issues, it won't block.
        await dispatcher.Prepare<IncreamentCounterResultAction>()
            .With(p => p.Count, _counterState.State.Count + 1)
            .DispatchAsync();
    }
}
```
