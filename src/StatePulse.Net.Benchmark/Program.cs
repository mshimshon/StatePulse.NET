using BenchmarkDotNet.Running;
using StatePulse.Net.Benchmark;

namespace Medihater.Benchmark;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Tests>();

        //var t = new Tests();
        //t.StatePulse_Dispatch();
        //t.Flux_Dispatch();
    }
}
