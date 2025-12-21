using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Stores;

namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Reducers;
internal class CounterIncreaseReducer : IReducer<CounterState, CounterIncreaseAction>
{
    public Task<CounterState> ReduceAsync(CounterState state, CounterIncreaseAction action)
        => Task.FromResult(state with { Count = state.Count + 1 });
}
