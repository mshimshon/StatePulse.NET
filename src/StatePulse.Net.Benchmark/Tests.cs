using BenchmarkDotNet.Attributes;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net.Benchmark.Pulses.Actions;
using StatePulse.Net.Benchmark.Pulses.States;

namespace StatePulse.Net.Benchmark;

public class Tests
{
    private readonly IDispatcher _pulseDispatcher;
    private CounterState PulseCounter { get; set; }
    private CounterFluxState FluxCounter { get; set; }
    private readonly Fluxor.IDispatcher _fluxDispatcher;

    public Tests()
    {
        ServiceCollection services = new();
        services.AddStatePulseServices(c =>
        {
            c.DispatchOrderBehavior = Configuration.DispatchOrdering.ReducersFirst;
            c.DispatchEffectBehavior = Configuration.DispatchEffectBehavior.Parallel;
            c.DispatchEffectExecutionBehavior = Configuration.DispatchEffectExecutionBehavior.FireAndForget;
            c.MiddlewareEffectBehavior = Configuration.MiddlewareEffectBehavior.PerGroupEffects;
            c.MiddlewareTaskBehavior = Configuration.MiddlewareTaskBehavior.DoNotAwait;
            c.ScanAssemblies = [typeof(Tests).Assembly];
        });
        services.AddFluxor(c =>
        {
            c.ScanAssemblies(typeof(Tests).Assembly);
        });
        var provider = services.BuildServiceProvider();
        _pulseDispatcher = provider.GetRequiredService<IDispatcher>();
        _fluxDispatcher = provider.GetRequiredService<Fluxor.IDispatcher>();
        var access = provider.GetRequiredService<IStateAccessor<CounterState>>();
        PulseCounter = access.State;
        access.OnStateChanged += (_, e) => { PulseCounter = e; };
        var fluxcces = provider.GetRequiredService<IState<CounterFluxState>>();
        FluxCounter = fluxcces.Value;
        fluxcces.StateChanged += (_, e) => { FluxCounter = fluxcces.Value; };
    }

    [Benchmark]
    public void StatePulse_Dispatch()
    {
        int counter = PulseCounter.Counter;
        _ = _pulseDispatcher.Prepare<IncreaseCounterAction>().DispatchAsync();
        // lock until state changes
        do { } while (counter == PulseCounter.Counter);
    }

    [Benchmark]
    public void StatePulse_SafeDispatch()
    {
        int counter = PulseCounter.Counter;
        _ = _pulseDispatcher.Prepare<IncreaseCounterAction>().DispatchAsync(true);
        // lock until state changes
        do { } while (counter == PulseCounter.Counter);
    }

    [Benchmark]
    public void StatePulse_FireYieldDispatch()
    {
        int counter = PulseCounter.Counter;
        _ = _pulseDispatcher.Prepare<IncreaseCounterAction>().ExecFireAndForget().DispatchAsync();
        // lock until state changes
        do { } while (counter == PulseCounter.Counter);
    }

    [Benchmark]
    public void StatePulse_FireYield_SequentialEffectsDispatch()
    {
        int counter = PulseCounter.Counter;
        _ = _pulseDispatcher.Prepare<IncreaseCounterAction>().ExecFireAndForget().SequentialEffects().DispatchAsync();
        // lock until state changes
        do { } while (counter == PulseCounter.Counter);
    }

    [Benchmark]
    public async Task StatePulse_AwaitedDispatch()
    {
        int counter = PulseCounter.Counter;
        await _pulseDispatcher.Prepare<IncreaseCounterAction>().ExecFireAndForget().Await().DispatchAsync();
        // lock until state changes
        do { } while (counter == PulseCounter.Counter);
    }

    //[Benchmark]
    //public void Flux_Dispatch()
    //{
    //    int counter = FluxCounter.Counter;
    //    _fluxDispatcher.Dispatch(new IncreaseCounterAction());
    //    // lock until state changes
    //    do
    //    {
    //    } while (counter == FluxCounter.Counter);
    //}

}