using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;
internal class MainMenuLoadNavigationItemsResultReducer : IReducer<MainMenuState, MainMenuLoadNavigationItemsResultAction>
{
    public async Task<MainMenuState> ReduceAsync(MainMenuState state, MainMenuLoadNavigationItemsResultAction action)
        => await Task.FromResult(state with { NavigationItems = action.Items });
}
