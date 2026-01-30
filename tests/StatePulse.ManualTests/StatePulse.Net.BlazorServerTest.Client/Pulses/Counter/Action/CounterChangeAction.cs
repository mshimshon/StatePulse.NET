namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;

public sealed record CounterChangeAction : IAction
{
    public int NewCounter { get; set; } = default!;
}
