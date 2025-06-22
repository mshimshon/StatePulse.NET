using StatePulse.Net.Tests.App.Pulsars.Counter.Action;

namespace StatePulse.Net.Tests.App.Components.Pages;
public partial class Counter
{



    private async Task Click() => await Dispatcher.Prepare<CounterIncreaseAction>().DispatchAsync();
}
