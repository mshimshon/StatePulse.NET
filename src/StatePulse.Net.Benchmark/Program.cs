using BenchmarkDotNet.Running;
using StatePulse.Net.Benchmark;

namespace Medihater.Benchmark;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Tests>();

        //var t = new Tests();
        //t.StatePulse_FireYield_SequentialEffectsDispatch();
        //t.Flux_Dispatch();
    }
}
