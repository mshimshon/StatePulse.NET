using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.States;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Reducers;

internal sealed class CounterChangeDoneReducer : IReducer<CounterState, CounterChangeDoneAction>
{
    public CounterState Reduce(CounterState state, CounterChangeDoneAction action)
        => state with { Count = action.NewCounter };
}
