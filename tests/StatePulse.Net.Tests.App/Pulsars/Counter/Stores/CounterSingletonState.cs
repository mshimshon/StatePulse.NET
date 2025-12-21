namespace StatePulse.Net.Tests.App.Pulsars.Counter.Stores;
public record CounterSingletonState : IStateFeatureSingleton
{
    public int Count { get; set; }
}
