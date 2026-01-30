using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.States;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Reducers;

internal sealed class CounterSubCountChangeReducer : IReducer<CounterState, CounterSubCountChangeDoneAction>
{
    public CounterState Reduce(CounterState state, CounterSubCountChangeDoneAction action)
        => state with { SubCount = action.NewCounter };
}
