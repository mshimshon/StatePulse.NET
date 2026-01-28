using Fluxor;
using StatePulse.Net.Benchmark.Pulses.Actions;

namespace StatePulse.Net.Benchmark.Pulses.Effects;

public class IncreaseCounterEffect : IEffect<IncreaseCounterAction>
{
    public Task EffectAsync(IncreaseCounterAction action, IDispatcher dispatcher)
    {
        _ = dispatcher.Prepare<IncreasedCounterAction>().DispatchAsync();
        return Task.CompletedTask;
    }
}

public class IncreaseCounterFluxEffect : Effect<IncreaseCounterAction>
{
    public override Task HandleAsync(IncreaseCounterAction action, Fluxor.IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new IncreasedCounterAction());
        return Task.CompletedTask;
    }
}
