using StatePulse.Net;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Middlewares;
internal class MainMenuLoaderStartDispatchMiddleware : IDispatcherMiddleware, IEffectMiddleware, IReducerMiddleware
{
    public Task AfterDispatch(object action)
    {
        Console.WriteLine($"{typeof(Action).Name} Executed.");
        return Task.CompletedTask;
    }

    public Task AfterEffect(object action)
    {
        Console.WriteLine($"{typeof(Action).Name} Executed.");
        return Task.CompletedTask;
    }

    public Task AfterReducing(object state, object action)
    {
        Console.WriteLine($"{typeof(Action).Name} Executed.");
        return Task.CompletedTask;
    }

    public Task BeforeDispatch(object action)
    {
        Console.WriteLine($"{typeof(Action).Name} Starting.");
        return Task.CompletedTask;
    }

    public Task BeforeEffect(object action)
    {
        Console.WriteLine($"{typeof(Action).Name} Executed.");
        return Task.CompletedTask;
    }

    public Task BeforeReducing(object state, object action)
    {
        Console.WriteLine($"{typeof(Action).Name} {typeof(Action).Name} Executed.");
        return Task.CompletedTask;
    }
}
