using StatePulse.Net.Tests.App.Pulsars.Weather.Contracts.Responses;

namespace StatePulse.Net.Tests.App.Pulsars.Weather.Stores;

public record WeatherState : IStateFeature
{
    public bool IsLoading { get; set; }
    public List<WeatherForecast>? Data { get; set; }
    public string? Error { get; set; }
}
