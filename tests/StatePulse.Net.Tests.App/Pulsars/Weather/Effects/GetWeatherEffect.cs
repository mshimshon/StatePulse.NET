using StatePulse.Net.Tests.App.Pulsars.Weather.Actions;
using StatePulse.Net.Tests.App.Pulsars.Weather.Contracts.Responses;

namespace StatePulse.Net.Tests.App.Pulsars.Weather.Effects;

public class GetWeatherEffect : IEffect<GetWeatherAction>
{
    public async Task EffectAsync(GetWeatherAction action, IDispatcher dispatcher)
    {
        await dispatcher.Prepare<WeatherLoaderStatAction>().DispatchAsync();
        await Task.Delay(1000);
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var forecasts = Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            }).ToArray();

        await dispatcher.Prepare<GetWeatherResultAction>().With(p => p.Data, forecasts.ToList())
            .DispatchAsync();
        await dispatcher.Prepare<WeatherLoaderStopAction>().DispatchAsync();

    }
}
