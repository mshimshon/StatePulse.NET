using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;
internal class MainMenuOpenReducer : IReducer<MainMenuState, MainMenuOpenAction>
{
    public Task<MainMenuState> ReduceAsync(MainMenuState state, MainMenuOpenAction action) =>
        Task.FromResult(state with { IsOpened = true });
}
