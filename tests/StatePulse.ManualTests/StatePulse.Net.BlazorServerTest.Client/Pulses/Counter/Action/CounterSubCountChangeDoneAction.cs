namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;

public sealed record CounterSubCountChangeDoneAction : IAction
{
    public int NewCounter { get; set; } = default!;
}
