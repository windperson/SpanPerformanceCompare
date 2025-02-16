using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Bogus;
using CommunityToolkit.HighPerformance;

namespace SpanPerformanceCompare;

[MemoryDiagnoser(displayGenColumns: true)]
[HideColumns("StdDev", "Median", "RatioSD", "Error")]
[Orderer(SummaryOrderPolicy.Declared)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams, BenchmarkLogicalGroupRule.ByJob)]
[RankColumn]
public class MultiplyMatrix2D
{
    [Params(2, 3, 10, 50, 100, 200, 500, 1000, 2000)]
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once InconsistentNaming
    public int Matrix_Size;

    private int[,] _matrix1 = null!;
    private int[,] _matrix2 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _matrix1 = new int[Matrix_Size, Matrix_Size];
        _matrix2 = new int[Matrix_Size, Matrix_Size];

        var fake = new Faker();
        for (var i = 0; i < Matrix_Size; i++)
        {
            for (var j = 0; j < Matrix_Size; j++)
            {
                _matrix1[i, j] = fake.Random.Int(0, 1000);
                _matrix2[i, j] = fake.Random.Int(0, 1000);
            }
        }
    }

    [Benchmark(Description = "int[,]", Baseline = true)]
    public int[,] MultiplyMatrix()
    {
        var rows1 = _matrix1.GetUpperBound(0);
        var columns1 = _matrix1.GetUpperBound(1);
        var columns2 = _matrix2.GetUpperBound(1);
        var result = new int[rows1, columns2];
        for (var i = 0; i < rows1; i++)
        {
            for (var j = 0; j < columns2; j++)
            {
                var temp = 0;
                for (var k = 0; k < columns1; k++)
                {
                    temp += _matrix1[i, k] * _matrix2[k, j];
                }

                result[i, j] = temp;
            }
        }

        return result;
    }

    [Benchmark(Description = "int[,] & Parallel.For")]
    public int[,] MultiplyMatrixUsingParallelFor()
    {
        var rows1 = _matrix1.GetUpperBound(0);
        var columns1 = _matrix1.GetUpperBound(1);
        var columns2 = _matrix2.GetUpperBound(1);
        var result = new int[rows1, columns2];

        // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
        var exceptions = new ConcurrentQueue<Exception>();

        Parallel.For(0, rows1, i =>
        {
            try
            {
                for (var j = 0; j < columns2; j++)
                {
                    var temp = 0;
                    for (var k = 0; k < columns1; k++)
                    {
                        temp += _matrix1[i, k] * _matrix2[k, j];
                    }

                    result[i, j] = temp;
                }
            }
            catch (Exception e)
            {
                exceptions.Enqueue(e);
            }
        });

        if (!exceptions.IsEmpty)
        {
            Console.WriteLine("{0} Exception(s) occurred", exceptions.Count);
            throw new AggregateException(exceptions);
        }

        return result;
    }

    [Benchmark(Description = "Span2D<int>")]
    public int[,] MultiplyMatrixUsingSpan()
    {
        var rows1 = _matrix1.GetUpperBound(0);
        var columns1 = _matrix1.GetUpperBound(1);
        var columns2 = _matrix2.GetUpperBound(1);
        var result = new int[rows1, columns2];
        var resultSpan = result.AsSpan2D();

        var matrixASpan = _matrix1.AsSpan2D();
        var matrixBSpan = _matrix2.AsSpan2D();

        for (var i = 0; i < rows1; i++)
        {
            for (var j = 0; j < columns2; j++)
            {
                var temp = 0;
                for (var k = 0; k < columns1; k++)
                {
                    temp += matrixASpan[i, k] * matrixBSpan[k, j];
                }

                resultSpan[i, j] = temp;
            }
        }

        return result;
    }

    [Benchmark(Description = "Memory2D<int> & Parallel.For")]
    public int[,] MultiplyMatrixUsingMemory()
    {
        var rows1 = _matrix1.GetUpperBound(0);
        var columns1 = _matrix1.GetUpperBound(1);
        var columns2 = _matrix2.GetUpperBound(1);
        var result = new int[rows1, columns2];
        var resultMemory = new Memory2D<int>(result);

        var matrixAMemory = new Memory2D<int>(_matrix1);
        var matrixBMemory = new Memory2D<int>(_matrix2);

        // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
        var exceptions = new ConcurrentQueue<Exception>();

        Parallel.For(0, rows1, i =>
        {
            try
            {
                for (var j = 0; j < columns2; j++)
                {
                    var temp = 0;
                    for (var k = 0; k < columns1; k++)
                    {
                        temp += matrixAMemory.Span[i, k] * matrixBMemory.Span[k, j];
                    }

                    resultMemory.Span[i, j] = temp;
                }
            }
            catch (Exception e)
            {
                exceptions.Enqueue(e);
            }
        });

        if (!exceptions.IsEmpty)
        {
            Console.WriteLine("{0} Exception(s) occurred", exceptions.Count);
            throw new AggregateException(exceptions);
        }

        return result;
    }
}