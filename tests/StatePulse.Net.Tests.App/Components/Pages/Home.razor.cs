using Microsoft.AspNetCore.Components;
using StatePulse.Net.Engine;
using StatePulse.Net.Tests.App.Pulsars.Counter.Action;

namespace StatePulse.Net.Tests.App.Components.Pages;
public partial class Home : ComponentBase, IDisposable
{

    [Inject] IPulseGlobalTracker State { get; set; } = default!;
    public async Task Click() => await Dispatcher.Prepare<CounterIncreaseAction>().DispatchAsync();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        State.onAfterCleanUp += OnStateChanged;
    }
    List<DateTime> _lastChecks = new();
    public void OnStateChanged(object? _, EventArgs __)
    {
        _lastChecks.Add(DateTime.Now);
        InvokeAsync(StateHasChanged);
    }
    public async Task MassTest()
    {
        for (int i = 0; i < 300; i++)
        {

            await Task.Delay(10);
            Visible = true;
            StateHasChanged();
            await Task.Delay(10);
            Visible = false;
            StateHasChanged();
        }
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
        State.onAfterCleanUp -= OnStateChanged;

    }
}
