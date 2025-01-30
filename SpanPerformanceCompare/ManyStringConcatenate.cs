using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using Bogus;

namespace SpanPerformanceCompare;

[MemoryDiagnoser(displayGenColumns: true)]
[HideColumns("StdDev", "Median", "Job", "RatioSD", "Error", "Alloc Ratio")]
[RankColumn]
[ReturnValueValidator]
public class ManyStringConcatenate
{
    [Params(5, 10, 100, 1000, 10_000)]
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once InconsistentNaming
    public int Total_Strings;

    private readonly List<string> _sourceStrings = [];

    [GlobalSetup]
    public void SetupSourceStrings()
    {
        var faker = new Faker();

        //Generate random strings of length 10 using Faker (Bogus)
        for (var i = 0; i < Total_Strings; i++)
        {
            var randomString = faker.Random.String2(10);
            _sourceStrings.Add(randomString);
        }
    }

    [Benchmark(Description = "LINQ Aggregate '+' operator")]
    public string UseLinqAggregate()
    {
        return _sourceStrings.Aggregate((current, next) => current + ',' + next);
    }

    [Benchmark(Description = "Parallel LINQ  '+' operator")]
    public string UseParallelLinqAggregate()
    {
        return _sourceStrings.AsParallel().Aggregate((current, next) => current + ',' + next);
    }

    [Benchmark(Description = "StringBuilder", Baseline = true)]
    public string UseStringBuilder()
    {
        var stringBuilder = new StringBuilder();

        for (var i = 0; i < _sourceStrings.Count; i++)
        {
            stringBuilder.Append(_sourceStrings[i]);
            if (i == _sourceStrings.Count - 1)
            {
                break;
            }

            stringBuilder.Append(',');
        }

        return stringBuilder.ToString();
    }

    [Benchmark(Description = "string.Join()")]
    public string UseStringJoin()
    {
        return string.Join(',', _sourceStrings);
    }

    [Benchmark(Description = "Span<char>")]
    public string UseSpan()
    {
        var span = new Span<char>(new char[_sourceStrings.Sum(s => s.Length) + _sourceStrings.Count - 1]);

        var index = 0;
        //Note: if the source strings are changed after the Span is created, the Span will be invalid
        //see: https://blog.ndepend.com/c-array-and-list-fastest-loop/
        var sourceSpan = CollectionsMarshal.AsSpan(_sourceStrings);
        foreach (var sourceString in sourceSpan)
        {
            sourceString.CopyTo(span.Slice(index));
            index += sourceString.Length;
            if (index == span.Length)
            {
                break;
            }

            span[index] = ',';
            index++;
        }

        return span.ToString();
    }
}