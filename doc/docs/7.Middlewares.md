---
slug: gs-the-middlewares
title: The Middlewares
tags: [blazor, reducer, state-management, pure-functions, async, statepulse, csharp, .net]
sidebar_position: 7
---

## ⚙️ What are Middlewares?

StatePulse uses **middleware interfaces** to tap into the lifecycle of **effects**, **reducers**, and **dispatches**.  
These middleware hooks are useful for **logging**, **metrics**, **analytics**, or **debugging** — but should **never alter behavior** or mutate state.

> ❗ Middleware is observational only — do not use it to change logic or outcomes.

## 🧩 Effect Middleware

`IEffectMiddleware` allows you to hook into the execution of any effect.

### Available Hooks

- `BeforeEffect(object action)` – called **before** the effect runs
- `AfterEffect(object action)` – called **after** the effect completes
- `WhenEffectValidationFailed(object action, object effectValidator)` – called when a validator blocks execution
- `WhenEffectValidationSucceed(object action, object effectValidator)` – called when a validator passes

### Example: Effect Middleware

```csharp title="LoggingMiddleware.cs"
internal class LoggingMiddleware : IEffectMiddleware
{
    private readonly ILogger _logger;

    public LoggingMiddleware(ILogger logger)
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

## 📘 Other Middleware Types

Additional interfaces are available to observe other parts of the StatePulse pipeline:

- `IReducerMiddleware` – Observe **reducer** executions (before/after)
- `IDispatcherMiddleware` – Observe all **dispatched actions** (before/after)

These follow a similar structure to `IEffectMiddleware`, offering lifecycle hooks such as:

- `Before...`
- `After...`

> ⚠️ Just like with effects, these middleware interfaces are **observational only** — they should not alter state or behavior.


## 🧼 Use Cases

Here are common use cases for StatePulse middleware:

- ✅ Logging effects, reducers, or dispatches for **debugging**
- 📊 Tracking **user behavior**
- ⏱️ Measuring **performance metrics**
- 📈 Collecting **analytics** without mutating state or logic

