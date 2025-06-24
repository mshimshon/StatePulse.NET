using Microsoft.AspNetCore.Components;
using StatePulse.Net.Tests.App.Pulsars.Counter.Stores;

namespace StatePulse.Net.Tests.App.Components;
public partial class CounterView : ComponentBase
{
    [Inject] IStatePulse PulseState { get; set; } = default!;
    private CounterState State => PulseState.StateOf<CounterState>(this, () => InvokeAsync(StateHasChanged));
}
