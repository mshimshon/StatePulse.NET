using StatePulse.Net.Tests.App.Pulsars.Weather.Actions;
using StatePulse.Net.Tests.App.Pulsars.Weather.Stores;

namespace StatePulse.Net.Tests.App.Pulsars.Weather.Reducers;

public class WeatherLoaderStopReducer : IReducer<WeatherState, WeatherLoaderStopAction>
{
    public Task<WeatherState> ReduceAsync(WeatherState state, WeatherLoaderStopAction action)
        => Task.FromResult(state with { IsLoading = false });
}
