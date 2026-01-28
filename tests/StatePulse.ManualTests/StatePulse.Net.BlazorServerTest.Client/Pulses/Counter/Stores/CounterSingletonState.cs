namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Stores;
public record CounterSingletonState : IStateFeatureSingleton
{
    public int Count { get; set; }
}
