using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;

namespace SpanPerformanceCompare;

[MemoryDiagnoser(displayGenColumns: true)]
[HideColumns("StdDev", "Median", "Job", "RatioSD", "Error")]
[Orderer(SummaryOrderPolicy.Declared)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams, BenchmarkLogicalGroupRule.ByJob)]
[ReturnValueValidator(failOnError: true)]
public class ExtractSubstring
{
    [Params(10, 100, 1000, 10_000, 100_000, 1_000_000)]
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

    [Benchmark(Description = "string.Substring()", Baseline = true)]
    public string UseSubstring()
    {
        return _inputString!.Substring(_startIndex, _length);
    }

    [Benchmark(Description = "Span Slice ToString")]
    public string UseSpanSliceThenToString()
    {
        return _inputString.AsSpan().Slice(_startIndex, _length).ToString();
    }

    [Benchmark(Description = "Span Slice StreamWriter")]
    public string UseSpanSliceThenStreamWriter()
    {
        var resultSpan = _inputString.AsSpan().Slice(_startIndex, _length);
        var stringBuilder = new StringBuilder(resultSpan.Length);
        using var streamWriter = new StringWriter(stringBuilder);
        streamWriter.Write(resultSpan);
        return stringBuilder.ToString();
    }
}