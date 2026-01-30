using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Effects;

internal sealed class CounterSubCountChangeEffect : IEffect<CounterSubCountChangeAction>
{
    public async Task EffectAsync(CounterSubCountChangeAction action, IDispatcher dispatcher)
    {
        var rnd = new Random();
        await Task.Delay(rnd.Next(10, 100));
        await dispatcher.Prepare<CounterSubCountChangeDoneAction>().With(p => p.NewCounter, action.NewCounter).DispatchAsync();
    }
}
