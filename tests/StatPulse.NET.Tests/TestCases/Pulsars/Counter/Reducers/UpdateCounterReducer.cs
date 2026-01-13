using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.States;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Counter.Reducers;

internal class UpdateCounterReducer : IReducer<CounterState, UpdateCounterAction>
{
    public CounterState Reduce(CounterState state, UpdateCounterAction action)
        => state with
        {
            Counter = action.Counter
        };
}
