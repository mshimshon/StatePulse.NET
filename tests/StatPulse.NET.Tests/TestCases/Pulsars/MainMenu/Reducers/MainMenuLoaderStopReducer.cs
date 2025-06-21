using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Store;

namespace StatePulse.NET.Tests.TestCases.Pulsars.MainMenu.Reducers;
internal class MainMenuLoaderStopReducer : IReducer<MainMenuState, MainMenuLoaderStopAction>
{
    public async Task<MainMenuState> ReduceAsync(MainMenuState state, MainMenuLoaderStopAction action)
        => await Task.FromResult(state with { IsLoading = false });
}
