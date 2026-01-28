using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;

internal class MainMenuLoaderStartReducer : IReducer<MainMenuState, MainMenuLoaderStartAction>
{
    public MainMenuState Reduce(MainMenuState state, MainMenuLoaderStartAction action)
        => state with
        {
            IsLoading = true
        };
}
