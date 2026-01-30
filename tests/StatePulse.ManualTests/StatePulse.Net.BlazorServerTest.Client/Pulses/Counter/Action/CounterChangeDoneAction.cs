namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Action;

public sealed record CounterChangeDoneAction : IAction
{
    public int NewCounter { get; set; }
}
