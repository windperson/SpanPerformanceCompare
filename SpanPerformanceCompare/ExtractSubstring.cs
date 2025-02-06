using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace SpanPerformanceCompare;

[MemoryDiagnoser(displayGenColumns: true)]
[HideColumns("StdDev", "Median", "Job", "Ratio", "RatioSD", "Error", "Alloc Ratio")]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByJob)]
[ReturnValueValidator(failOnError: true)]
public class ExtractSubstring
{
    [Params(10, 100, 10_000, 100_000, 1000_000)]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnassignedField.Global
    public int String_Length;

    private string? _inputString;
    private int _startIndex;
    private int _length;

    [GlobalSetup]
    public void Setup()
    {
        // Use bogus to generate a random string of length String_Length
        var faker = new Bogus.Faker();
        _inputString = faker.Random.String2(String_Length);

        // Randomly select a start and end index
        _startIndex = faker.Random.Int(0, String_Length / 2 - 1);
        var endIndex = faker.Random.Int(_startIndex + 1, String_Length - 1);
        _length = endIndex - _startIndex;
    }

    [Benchmark(Description = "Substring()")]
    public string UseSubstring()
    {
        return _inputString!.Substring(_startIndex, _length);
    }

    [Benchmark(Description = "Span Slice")]
    public string UseSpanSlice()
    {
        return _inputString.AsSpan().Slice(_startIndex, _length).ToString();
    }
}