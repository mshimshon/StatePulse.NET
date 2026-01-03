using Fluxor;
using StatePulse.Net.Benchmark.Pulses.Actions;
using StatePulse.Net.Benchmark.Pulses.States;

namespace StatePulse.Net.Benchmark.Pulses.Reducers;

public class IncreasedCounterReducer : IReducer<CounterState, IncreasedCounterAction>
{
    public Task<CounterState> ReduceAsync(CounterState state, IncreasedCounterAction action)
        => Task.FromResult(state with { Counter = state.Counter + 1 });
}


public class IncreasedCounterFluxReducer : Reducer<CounterState, IncreasedCounterAction>
{
    public override CounterState Reduce(CounterState state, IncreasedCounterAction action)
        => state with { Counter = state.Counter + 1 };
}
