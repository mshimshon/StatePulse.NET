---
slug: gs-the-effect
title: The Effects
tags: [blazor, effects, state-management, async, side-effects, statepulse, csharp, .net]
sidebar_position: 5
---

## What are Effects

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

## Create The Effect

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

## Effects Validators

Effect validators allow you to **run checks before an effect executes**, deciding whether the effect should run or be skipped.

They receive both the **action** and the **effect instance**, since a single action can trigger **multiple effects**.  
By binding one or more validators to a specific effect, you can control its execution based on any logic you want.

### Real-World Scenario

Imagine you have an action, say `RequestContentAction`, that is triggered by **all users**:

- For **non-subscribed users**, you want an effect that returns a limited preview or redirects to a subscription page.
- For **subscribed members**, you want a different effect that fetches full content.

Using effect validators, you can bind:

- A validator on the **non-member effect** that checks if the user is *not* subscribed and only runs the effect in that case.
- A validator on the **member effect** that checks if the user *is* subscribed.

This way, **both effects respond to the same action but run conditionally based on user status.**

### Benefits

- Allows **clean separation** of logic for different user scenarios
- Keeps action handling unified while supporting **multiple conditional outcomes**
- Avoids complex branching inside effects themselves

Effect validators are a powerful way to **condition effect execution** by checking custom conditions involving the action and the current state,  
enabling flexible and maintainable multi-effect workflows based on business rules.

if any validator fails the effect is skipped.

```csharp title="IncrementEffectValidator.cs"
internal class IncrementEffectValidator : IEffectValidator<IncrementCounterAction, IncrementCounterEffect>
{
    public Task<bool> Validate(IncrementCounterAction action)
    {
        if (action.Delay > 1000) return Task.FromResult(false);
        return Task.FromResult(true);
    }
}

```


## ⚙️ What are Effect Middlewares?

StatePulse uses **middleware interfaces** to tap into the lifecycle of **effects**, **reducers**, and **dispatches**.  
These middleware hooks are useful for **logging**, **metrics**, **analytics**, or **debugging** — but should **never alter behavior** or mutate state.

> ❗ Middleware is observational only — do not use it to change logic or outcomes.

`IEffectMiddleware` allows you to hook into the execution of any effect.

### Available Hooks

- `BeforeEffect(object action)` – called **before** the effects run
- `AfterEffect(object action)` – called **after** the effects completed
- `WhenEffectValidationFailed(object action, object effectValidator)` – called when a validator fails.
- `WhenEffectValidationSucceed(object action, object effectValidator)` – called when a validator passes

### Example: Effect Middleware

```csharp title="LoggingMiddleware.cs"
internal class LoggingMiddleware : IEffectMiddleware
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }
    public Task AfterEffect(object action)
    {
        string message = $"{action.GetType()} finished execution.";
        _logger.LogDebug(message);
        return Task.CompletedTask;
    }
    public Task BeforeEffect(object action) => Task.CompletedTask;
    public Task WhenEffectValidationFailed(object action, object effectValidator) => Task.CompletedTask;
    public Task WhenEffectValidationSucceed(object action, object effectValidator) => Task.CompletedTask;
}

```