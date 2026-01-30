namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;

public sealed record CounterSubCountChangeAction : IAction
{
    public int NewCounter { get; set; } = default!;
}
