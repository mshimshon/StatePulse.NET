using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.States;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Reducers;

internal class CounterSingletonIncreaseReducer : IReducer<CounterSingletonState, CounterSingletonIncreaseAction>
{
    public CounterSingletonState Reduce(CounterSingletonState state, CounterSingletonIncreaseAction action)
        => state with
        {
            Count = state.Count + 1
        };
}
