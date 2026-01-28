using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatePulse.Net.Configuration;

public enum PulseTrackingModel
{
    ThreadSafe,
    SingleThreadFast,
    BlazorServerSafe,
    BlazorWebAssemblyFast,
}
