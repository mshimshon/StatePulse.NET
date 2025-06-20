using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Effects;
internal class MainMenuLoadNavigationItemsEffect : IEffect<MainMenuLoadNavigationItemsAction>
{
    public async Task EffectAsync(MainMenuLoadNavigationItemsAction action, IDispatcher dispatcher, Guid chainKey)
    {
        await dispatcher.Prepare(() => new MainMenuLoadNavigationItemsResultAction(new() { "sda" })).DispatchFastAsync();
    }
}
