using StatePulse.Net.Tests.App.Pulsars.Counter.Action;
using StatePulse.Net.Tests.App.Pulsars.Counter.Stores;

namespace StatePulse.Net.Tests.App.Pulsars.Counter.Reducers;
internal class CounterIncreaseReducer : IReducer<CounterState, CounterIncreaseAction>
{
    public Task<CounterState> ReduceAsync(CounterState state, CounterIncreaseAction action)
        => ReducerExt.ReducerResult(state)
            .With(p => p.Count, state.Count + 1)
            .ToTask();
}
