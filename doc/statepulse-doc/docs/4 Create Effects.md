---
slug: gs-create-effect
title: Create Effects
tags: [blazor, effects, state-management, async, side-effects, statepulse, csharp, .net]
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

```csharp title="IncrementCounterEffect.cs"
internal class IncrementCounterEffect : IEffect<IncrementCounterAction>
{
    readonly IStateAccessor<CounterState> _counterState;
    public IncrementCounterEffect(IStateAccessor<CounterState> counterState)
    {
        _counterState = counterState;
    }
    public async Task EffectAsync(IncrementCounterAction action, IDispatcher dispatcher)
    {
        // Recommendation: Always Await Dispatch to avoid safe execution issues, it won't block.
        await dispatcher.Prepare<IncrementCounterResultAction>()
            .With(p => p.Count, _counterState.State.Count + 1)
            .DispatchAsync();
    }
}
```
