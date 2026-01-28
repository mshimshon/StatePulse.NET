using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;

internal class MainMenuLoaderStopReducer : IReducer<MainMenuState, MainMenuLoaderStopAction>
{
    public MainMenuState Reduce(MainMenuState state, MainMenuLoaderStopAction action)
        => state with
        {
            IsLoading = false
        };
}
