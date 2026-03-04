using Microsoft.AspNetCore.Components;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.States;

namespace StatePulse.Net.BlazorServerTest.Client.Pages;

public partial class ComponentTest : ComponentBase
{
    [Inject] IStatePulse StatePulse { get; set; } = default!;
    private CounterState State => StatePulse.StateOf<CounterState>(() => this, OnUpdate);
    private async Task OnUpdate() => await InvokeAsync(StateHasChanged);

}
