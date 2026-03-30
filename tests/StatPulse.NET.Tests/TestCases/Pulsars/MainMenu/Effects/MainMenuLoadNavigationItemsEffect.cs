using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Effects;

internal class MainMenuLoadNavigationItemsEffect : IEffect<MainMenuLoadNavigationItemsAction>
{
    public async Task EffectAsync(MainMenuLoadNavigationItemsAction action, IDispatcher dispatcher)
    {
        if (dispatcher.IsCancellationRequested) return;
        await Task.Delay(10);
        if (dispatcher.IsCancellationRequested) return;
        await dispatcher.Prepared(new MainMenuLoadNavigationItemsResultAction(new() { "sda" })).DispatchAsync();
    }
}
