namespace StatePulse.Net.Tests.App.Pulsars.Counter.Stores;
public record CounterState : IStateFeature
{
    public int Count { get; set; }
}
