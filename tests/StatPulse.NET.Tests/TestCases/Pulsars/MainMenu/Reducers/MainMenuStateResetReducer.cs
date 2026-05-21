using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;

internal sealed class MainMenuStateResetReducer : IReducer<MainMenuState, MainMenuStateResetAction>
{
    public MainMenuState Reduce(MainMenuState state, MainMenuStateResetAction action)
        => new();
}
