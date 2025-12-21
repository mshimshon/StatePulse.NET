using StatePulse.Net;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.Actions;
using StatePulse.NET.Tests.TestCases.Pulsars.Counter.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Counter.Reducers;

internal class UpdateCounterReducer : IReducer<CounterState, UpdateCounterAction>
{
    public async Task<CounterState> ReduceAsync(CounterState state, UpdateCounterAction action)
        => await Task.FromResult(state with { Counter = action.Counter });
}
