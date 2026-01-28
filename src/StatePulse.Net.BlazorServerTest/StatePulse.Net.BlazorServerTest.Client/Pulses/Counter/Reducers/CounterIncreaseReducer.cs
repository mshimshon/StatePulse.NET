using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Stores;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Reducers;

internal class CounterIncreaseReducer : IReducer<CounterState, CounterIncreaseAction>
{
    public CounterState Reduce(CounterState state, CounterIncreaseAction action)
        => state with { Count = state.Count + 1 };
}
