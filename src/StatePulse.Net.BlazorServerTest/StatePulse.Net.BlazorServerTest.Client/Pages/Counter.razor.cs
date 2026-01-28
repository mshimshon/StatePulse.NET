using Microsoft.AspNetCore.Components;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Stores;

namespace StatePulse.Net.BlazorServerTest.Client.Pages;

public partial class Counter : ComponentBase
{
    [Inject] IStatePulse _statePulse { get; set; } = default!;
    [Inject] IDispatcher _dispatcher { get; set; } = default!;
    [Inject] IPulseGlobalTracker _pulseGlobalTracker { get; set; } = default!;
    [Inject] IStatePulseRegistry _statePulseRegistry { get; set; } = default!;
    private int Update { get; set; }

    public CounterSingletonState Shared => _statePulse.StateOf<CounterSingletonState>(() => this, OnUpdate);
    public CounterState State => _statePulse.StateOf<CounterState>(() => this, () => OnUpdate());
    private async Task OnUpdate() => await InvokeAsync(StateHasChanged);

    protected override void OnInitialized()
    {
    }
    private async Task SingletonIncrease()
    {
        await _dispatcher.Prepare<CounterSingletonIncreaseAction>().DispatchAsync();
    }

    private async Task Increase()
    {
        await _dispatcher.Prepare<CounterIncreaseAction>().DispatchAsync();
    }
}
