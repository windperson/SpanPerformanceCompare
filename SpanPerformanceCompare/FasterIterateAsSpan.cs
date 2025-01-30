using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Bogus;

namespace SpanPerformanceCompare;

[HideColumns("StdDev", "Median", "Job", "RatioSD", "Error", "Alloc Ratio")]
[RankColumn]
[ReturnValueValidator]
public class FasterIterateAsSpan
{
    [Params(10, 100, 1000, 10_000, 100_000, 1000_000)]
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once InconsistentNaming
    public int Total_Numbers;

    private readonly List<int> _sourceNumbers = [];

    [GlobalSetup]
    public void SetupSourceNumbers()
    {
        var faker = new Faker();
        //Generate random number from 0 to 1000
        for (var i = 0; i < Total_Numbers; i++)
        {
            var randomNumber = faker.Random.Int(0, 1000);
            _sourceNumbers.Add(randomNumber);
        }
    }

    [Benchmark(Description = "use normal for loop", Baseline = true)]
    public int UseNormalForLoop()
    {
        var sum = 0;
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var number in _sourceNumbers)
        {
            sum += number;
        }

        return sum;
    }

    [Benchmark(Description = "Use Span<int>")]
    public int UseSpan()
    {
        var sum = 0;
        var span = CollectionsMarshal.AsSpan(_sourceNumbers);
        foreach (var number in span)
        {
            sum += number;
        }

        return sum;
    }

    [Benchmark(Description = "Use LINQ Sum()")]
    public int UseLinqSum()
    {
        return _sourceNumbers.Sum();
    }
}