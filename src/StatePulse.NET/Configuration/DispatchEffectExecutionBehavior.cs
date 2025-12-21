using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.Net.Configuration;

public enum DispatchEffectExecutionBehavior
{
    /// <summary>
    /// (Default): Like fire and forget but the statepulse engine still awaits all the effects before calling the middlewares.
    /// </summary>
    YieldAndFire,
    /// <summary>
    /// Nothing is awaited, effects runs as they want and after effect middleware have not garantuees the effect actually ran. (This will disable DispatchEffectBehavior.Sequential)
    /// </summary>
    FireAndForget
}
