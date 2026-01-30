using Microsoft.AspNetCore.Components;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;
using StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.States;

namespace StatePulse.Net.BlazorServerTest.Client.Pages;

public partial class Counter : ComponentBase
{
    [Inject] IStatePulse StatePulse { get; set; } = default!;
    [Inject] IPulseGlobalTracker PulseGlobalTracker { get; set; } = default!;
    [Inject] IStatePulseRegistry StatePulseRegistry { get; set; } = default!;
    private int Update { get; set; }

    private CounterSingletonState Shared => StatePulse.StateOf<CounterSingletonState>(() => this, OnUpdate);
    private CounterState State => StatePulse.StateOf<CounterState>(() => this, OnUpdate);
    private async Task OnUpdate() => await InvokeAsync(StateHasChanged);

    protected override void OnInitialized()
    {
    }
    private async Task SingletonIncrease()
    {
        await StatePulse.Dispatcher.Prepare<CounterSingletonIncreaseAction>().DispatchAsync();
    }

    private async Task Increase()
    {
        int[] stressvalues = [
            100,
            200,
            300,
            400,
            500
            ];
        Random d = new Random();
        foreach (var item in stressvalues)
        {

            await StatePulse.Dispatcher
                .Prepare<CounterChangeAction>()
                .With(p => p.NewCounter, 100)
                .DispatchAsync(true);

        }

    }
}
