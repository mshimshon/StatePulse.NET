using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;

internal class SuccessfulReducer : IReducer<MainMenuSecondState, MainMenuOpenAction>
{
    public MainMenuSecondState Reduce(MainMenuSecondState state, MainMenuOpenAction action)
        => state with { IsSuccessful = true };
}
