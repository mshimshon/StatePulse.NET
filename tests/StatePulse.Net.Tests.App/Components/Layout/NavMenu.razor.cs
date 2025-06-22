using Microsoft.AspNetCore.Components;
using StatePulse.Net.Blazor;
using StatePulse.Net.Tests.App.Pulsars.Counter.Stores;

namespace StatePulse.Net.Tests.App.Components.Layout;
public partial class NavMenu
{
    [Inject] IPulse PulseState { get; set; } = default!;
    private CounterState State => PulseState.StateOf<CounterState>(this);

}
