using Microsoft.AspNetCore.Components;
using StatePulse.Net.Tests.App.Pulsars.Weather.Actions;
using StatePulse.Net.Tests.App.Pulsars.Weather.Stores;

namespace StatePulse.Net.Tests.App.Components.Pages;

public partial class Weather
{
    [Inject] public IStateAccessor<WeatherState> WeatherState { get; set; } = default!;
    protected override async Task OnParametersSetAsync()
    {
        //WeatherState.StateChanged += (a, state) =>
        //{
        //    Console.WriteLine("");

        //};
        await Dispatcher.Prepare<GetWeatherAction>().DispatchAsync();
        await Task.Delay(2000);
        Console.WriteLine("");
    }
}
