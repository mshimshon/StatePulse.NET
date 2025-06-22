using StatePulse.Net.Tests.App.Pulsars.Weather.Actions;
using StatePulse.Net.Tests.App.Pulsars.Weather.Stores;

namespace StatePulse.Net.Tests.App.Pulsars.Weather.Reducers;

public class WeatherLoaderStatReducer : IReducer<WeatherState, WeatherLoaderStatAction>
{
    public Task<WeatherState> ReduceAsync(WeatherState state, WeatherLoaderStatAction action)
        => Task.FromResult(state with { IsLoading = true });
}
