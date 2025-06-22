using StatePulse.Net.Tests.App.Pulsars.Weather.Actions;
using StatePulse.Net.Tests.App.Pulsars.Weather.Stores;

namespace StatePulse.Net.Tests.App.Pulsars.Weather.Reducers;

public class GetWeatherActionResultReducer : IReducer<WeatherState, GetWeatherResultAction>
{
    public Task<WeatherState> ReduceAsync(WeatherState state, GetWeatherResultAction action)
        => Task.FromResult(state with { Data = action.Data });
}
