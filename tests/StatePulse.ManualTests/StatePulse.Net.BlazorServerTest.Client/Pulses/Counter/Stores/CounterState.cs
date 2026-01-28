namespace StatePulse.Net.BlazorServerTest.Client.Pulses.Counter.Stores;
public record CounterState : IStateFeature
{
    public int Count { get; set; }
}
