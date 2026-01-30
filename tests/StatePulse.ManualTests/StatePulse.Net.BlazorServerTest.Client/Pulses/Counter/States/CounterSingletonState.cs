namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.States;

public record CounterSingletonState : IStateFeatureSingleton
{
    public int Count { get; set; }
}
