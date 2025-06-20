using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Effects;
internal class MainMenuOpenEffect : IEffect<MainMenuOpenAction>
{
    public async Task EffectAsync(MainMenuOpenAction action, IDispatcher dispatcher, Guid chainKey)
    {
        await dispatcher.Prepare<MainMenuLoadNavigationItemsAction>().DispatchFastAsync();
    }
}
