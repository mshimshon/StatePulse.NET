using StatePulse.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Counter.States;

public record CounterState : IStateFeatureSingleton
{
    public int Counter { get; init; }
}
