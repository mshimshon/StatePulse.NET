using StatePulse.Net.Tests.App.Pulsars.Weather.Contracts.Responses;

namespace StatePulse.Net.Tests.App.Pulsars.Weather.Actions;

public class GetWeatherResultAction : IAction
{
    public List<WeatherForecast>? Data { get; set; }
}
