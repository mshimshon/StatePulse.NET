using StatePulse.Net.Tests.App.Pulsars.Counter.Action;
using StatePulse.Net.Tests.App.Pulsars.Counter.Stores;

namespace StatePulse.Net.Tests.App.Pulsars.Counter.Reducers;
internal class CounterIncreaseReducer : IReducer<CounterState, CounterIncreaseAction>
{
    public Task<CounterState> ReduceAsync(CounterState state, CounterIncreaseAction action)
        => Task.FromResult(state with { Count = state.Count + 1 });
}
