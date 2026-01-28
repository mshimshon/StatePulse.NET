using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.Net.Configuration;

public enum DispatchOrdering
{
    /// <summary>
    /// Run and Await Reducers then Run Effects.
    /// </summary>
    ReducersFirst,
    /// <summary>
    /// (Default) Run the effects then run and await the reducers
    /// </summary>
    EffectsFirst
}
