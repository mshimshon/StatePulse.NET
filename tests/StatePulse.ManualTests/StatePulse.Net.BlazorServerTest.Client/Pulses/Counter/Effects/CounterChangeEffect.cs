using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Effects;

public sealed class CounterChangeEffect : IEffect<CounterChangeAction>
{
    public async Task EffectAsync(CounterChangeAction action, IDispatcher dispatcher)
    {
        var rnd = new Random();
        await Task.Delay(rnd.Next(10, 100));
        try
        {

        }
        catch (Exception)
        {

            throw;
        }
        await dispatcher.Prepare<CounterChangeDoneAction>()
            .With(p => p.NewCounter, action.NewCounter)
            .DispatchAsync();
        await dispatcher.Prepare<CounterSubCountChangeAction>().With(p => p.NewCounter, action.NewCounter).DispatchAsync();
    }
}
