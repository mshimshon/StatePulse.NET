using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;
internal class MainMenuLoaderStartReducer : IReducer<MainMenuState, MainMenuLoaderStartAction>
{
    public Task<MainMenuState> ReduceAsync(MainMenuState state, MainMenuLoaderStartAction action)
        => Task.FromResult(state with { IsLoading = true });
}
