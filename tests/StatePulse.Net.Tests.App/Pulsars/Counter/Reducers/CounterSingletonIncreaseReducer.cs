using StatePulse.Net.Tests.App.Pulsars.Counter.Action;
using StatePulse.Net.Tests.App.Pulsars.Counter.Stores;

namespace StatePulse.Net.Tests.App.Pulsars.Counter.Reducers;
internal class CounterSingletonIncreaseReducer : IReducer<CounterSingletonState, CounterSingletonIncreaseAction>
{
    public Task<CounterSingletonState> ReduceAsync(CounterSingletonState state, CounterSingletonIncreaseAction action)
        => Task.FromResult(state with { Count = state.Count + 1 });
}
