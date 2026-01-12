using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Middlewares;

internal class MainMenuLoaderStartDispatchMiddleware : IDispatcherMiddleware, IEffectMiddleware, IReducerMiddleware
{
    public Task AfterDispatch(object action)
    {
        Console.WriteLine($"{action.GetType().Name} Executed.");
        return Task.CompletedTask;
    }

    public Task AfterEffect(object action)
    {
        Console.WriteLine($"{action.GetType().Name} Executed.");
        return Task.CompletedTask;
    }

    public Task AfterReducing(object state, object action)
    {
        Console.WriteLine($"{action.GetType().Name} Executed.");
        return Task.CompletedTask;
    }

    public Task BeforeDispatch(object action)
    {
        Console.WriteLine($"{action.GetType().Name} Starting.");
        return Task.CompletedTask;
    }

    public Task BeforeEffect(object action)
    {
        Console.WriteLine($"{action.GetType().Name} Executed.");
        return Task.CompletedTask;
    }

    public Task BeforeReducing(object state, object action)
    {
        Console.WriteLine($"{state.GetType().Name} {action.GetType().Name} Executed.");
        return Task.CompletedTask;
    }

    public Task OnDispatchFailure(Exception exception, object action)
    => Task.CompletedTask;

    public Task WhenEffectValidationFailed(object action, object effectValidator)
    {
        Console.WriteLine($"{action.GetType().Name} {effectValidator.GetType().Name}r Executed.");
        return Task.CompletedTask;
    }
    public Task WhenEffectValidationSucceed(object action, object effectValidator)
    {
        Console.WriteLine($"{action.GetType().Name} {effectValidator.GetType().Name}e Executed.");
        return Task.CompletedTask;
    }
}
