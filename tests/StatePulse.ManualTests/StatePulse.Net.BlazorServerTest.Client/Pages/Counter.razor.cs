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
    private CancellationTokenSource? _leakTestCTS;
    protected override void OnInitialized()
    {
        PulseGlobalTracker.onAfterCleanUp += OnStateChanged;

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
                .With(p => p.NewCounter, item)
                .DispatchAsync();

        }

    }


    List<DateTime> _lastChecks = new();
    public void OnStateChanged(object? _, EventArgs __)
    {
        _lastChecks.Add(DateTime.Now);
        InvokeAsync(StateHasChanged);
    }
    public async Task CancelMassTest()
    {
        if (_leakTestCTS != default)
            _leakTestCTS.Cancel();
    }
    public async Task MassTest()
    {
        if (_leakTestCTS != default)
        {
            _ = CancelMassTest();
            return;
        }
        _leakTestCTS = new();
        do
        {
            await Task.Delay(10);
            Visible = true;
            StateHasChanged();
            await Task.Delay(10);
            Visible = false;
            StateHasChanged();
        } while (_leakTestCTS != default && !_leakTestCTS.Token.IsCancellationRequested);

        _leakTestCTS = default;
    }
    public bool Visible { get; set; }
    private Task ForceGC()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        StateHasChanged();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        PulseGlobalTracker.onAfterCleanUp -= OnStateChanged;

    }
}
