namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.States;

public sealed record CounterState : IStateFeature
{
    public int Count { get; init; }
    public int SubCount { get; init; }
}
