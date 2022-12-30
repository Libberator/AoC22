using BenchmarkDotNet.Running;

namespace Benchmarks;

public class Program
{
    public static void Main()
    {
        var summary = BenchmarkRunner.Run<PuzzleBenchmarks>();
    }
}
