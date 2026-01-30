using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Effects.Validators;

public sealed class Counter300Effect : IEffectValidator<CounterChangeAction, CounterChangeEffect>
{
    public Task<bool> Validate(CounterChangeAction action)
        => Task.FromResult(action.NewCounter == 300);
}
