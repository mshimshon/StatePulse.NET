using StatePulse.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.NET.Tests.TestCases.Pulsars.Counter.Actions;

public record UpdateCounterAction : IAction
{
    public DateTime ExecutedOn { get; init; } = DateTime.UtcNow;
    public int Counter { get; init; }
}
