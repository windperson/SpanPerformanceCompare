using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;

namespace SpanPerformanceCompare;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[ReturnValueValidator]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class StringConcatenate
{
    [Benchmark(Description = "Use + operator")]
    public string UseStringConcatenate()
    {
        // ReSharper disable once ConvertToConstant.Local
        var firstName = "John";
        // ReSharper disable once ConvertToConstant.Local
        var middleName = "Quincy";
        // ReSharper disable once ConvertToConstant.Local
        var lastName = "Doe";

        return firstName + " " + middleName + " " + lastName;
    }

    [Benchmark(Baseline = true, Description = "Use string interpolation")]
    public string UseStringInterpolation()
    {
        // ReSharper disable once ConvertToConstant.Local
        var firstName = "John";
        // ReSharper disable once ConvertToConstant.Local
        var middleName = "Quincy";
        // ReSharper disable once ConvertToConstant.Local
        var lastName = "Doe";

        return $"{firstName} {middleName} {lastName}";
    }

    [Benchmark(Description = "string.Format()")]
    public string UseStringFormat()
    {
        // ReSharper disable once ConvertToConstant.Local
        var firstName = "John";
        // ReSharper disable once ConvertToConstant.Local
        var middleName = "Quincy";
        // ReSharper disable once ConvertToConstant.Local
        var lastName = "Doe";

        // ReSharper disable once UseStringInterpolation
        return string.Format("{0} {1} {2}", firstName, middleName, lastName);
    }

    [Benchmark(Description = "string.Join()")]
    public string UseStringJoin()
    {
        // ReSharper disable once ConvertToConstant.Local
        var firstName = "John";
        // ReSharper disable once ConvertToConstant.Local
        var middleName = "Quincy";
        // ReSharper disable once ConvertToConstant.Local
        var lastName = "Doe";

        return string.Join(" ", firstName, middleName, lastName);
    }

    [Benchmark(Description = "StringBuilder")]
    public string UseStringBuilder()
    {
        // ReSharper disable once ConvertToConstant.Local
        var firstName = "John";
        // ReSharper disable once ConvertToConstant.Local
        var middleName = "Quincy";
        // ReSharper disable once ConvertToConstant.Local
        var lastName = "Doe";

        var sb = new System.Text.StringBuilder();
        sb.Append(firstName);
        sb.Append(' ');
        sb.Append(middleName);
        sb.Append(' ');
        sb.Append(lastName);
        return sb.ToString();
    }

    [Benchmark(Description = "Use Span<char>")]
    public string UseSpanOfChar()
    {
        // ReSharper disable once ConvertToConstant.Local
        var firstName = "John";
        // ReSharper disable once ConvertToConstant.Local
        var middleName = "Quincy";
        // ReSharper disable once ConvertToConstant.Local
        var lastName = "Doe";

        var fullNameSpan = new Span<char>(new char[firstName.Length + middleName.Length + lastName.Length + 2]);
        firstName.AsSpan().CopyTo(fullNameSpan);
        fullNameSpan[firstName.Length] = ' ';
        middleName.AsSpan().CopyTo(fullNameSpan.Slice(firstName.Length + 1));
        fullNameSpan[firstName.Length + 1 + middleName.Length] = ' ';
        lastName.AsSpan().CopyTo(fullNameSpan.Slice(firstName.Length + 1 + middleName.Length + 1));
        return fullNameSpan.ToString();
    }
}