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
        _ = _pulseDispatcher.Prepare<IncreaseCounterAction>().Await().DispatchAsync();
    }

    [Benchmark]
    public async Task StatePulse_SafeDispatch()
    {
        await _pulseDispatcher.Prepare<IncreaseCounterAction>().DispatchAsync(true);
    }

    [Benchmark]
    public async Task StatePulse_BusrtDispatch()
    {
        for (int i = 0; i < 100; i++)
            await _pulseDispatcher.Prepare<IncreaseCounterAction>().DispatchAsync();
    }

    [Benchmark]
    public async Task StatePulse_BusrtSafeDispatch()
    {
        for (int i = 0; i < 100; i++)
            await _pulseDispatcher.Prepare<IncreaseCounterAction>().DispatchAsync(true);
    }


    [Benchmark]
    public async Task StatePulse_FireYieldDispatch()
    {
        await _pulseDispatcher.Prepare<IncreaseCounterAction>().ExecFireAndForget().DispatchAsync();
    }

    [Benchmark]
    public async Task StatePulse_FireYield_SequentialEffectsDispatch()
    {
        await _pulseDispatcher.Prepare<IncreaseCounterAction>().ExecYieldAndFire().SequentialEffects().DispatchAsync();
    }

    [Benchmark]
    public async Task StatePulse_AwaitedDispatch()
    {
        await _pulseDispatcher.Prepare<IncreaseCounterAction>().ExecYieldAndFire().Await().DispatchAsync();
    }

}