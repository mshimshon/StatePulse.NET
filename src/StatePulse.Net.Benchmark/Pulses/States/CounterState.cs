using Fluxor;

namespace StatePulse.Net.Benchmark.Pulses.States;

public record CounterState : IStateFeature
{
    public int Counter { get; init; }
}

[FeatureState]
public record CounterFluxState
{
    public int Counter { get; init; }
}
