using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;

internal class MainMenuLoadNavigationItemsResultReducer : IReducer<MainMenuState, MainMenuLoadNavigationItemsResultAction>
{
    public MainMenuState Reduce(MainMenuState state, MainMenuLoadNavigationItemsResultAction action)
        => state with
        {
            NavigationItems = action.Items
        };
}
