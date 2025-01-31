using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Bogus;
using CommunityToolkit.HighPerformance;

namespace SpanPerformanceCompare;

[MemoryDiagnoser(displayGenColumns: true)]
[HideColumns("StdDev", "Median", "Job", "RatioSD", "Error", "Alloc Ratio")]
[RankColumn]
[ReturnValueValidator]
public class MultiplyMatrix2D
{
    [Params(2, 3, 5, 10, 50, 100, 200, 500, 1000, 2000)]
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
    public int[] MultiplyMatrix()
    {
        var rows1 = _matrix1.GetUpperBound(0);
        var columns1 = _matrix1.GetUpperBound(1);
        var columns2 = _matrix2.GetUpperBound(1);
        var result = new int[rows1, columns2];
        try
        {
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Convert2DArrayTo1D(result);
    }

    [Benchmark(Description = "Span2D<int>")]
    public int[] MultiplyMatrixUsingSpan()
    {
        var rows1 = _matrix1.GetUpperBound(0);
        var columns1 = _matrix1.GetUpperBound(1);
        var columns2 = _matrix2.GetUpperBound(1);
        var result = new int[rows1, columns2];
        var resultSpan = result.AsSpan2D();

        var matrixASpan = _matrix1.AsSpan2D();
        var matrixBSpan = _matrix2.AsSpan2D();

        try
        {
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Convert2DArrayTo1D(result);
    }

    [Benchmark(Description = "Memory2D<int> & Parallel.For")]
    public int[] MultiplyMatrixUsingMemory()
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

        return Convert2DArrayTo1D(result);
    }

    // We need to convert two-dimensional array into one-dimensional for BDN's ReturnValueValidator compare the results
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T[] Convert2DArrayTo1D<T>(T[,] array2D)
    {
        var result = new T[array2D.Length];
        var index = 0;

        try
        {
            for (var i = 0; i < array2D.GetUpperBound(0); i++)
            {
                for (var j = 0; j < array2D.GetUpperBound(1); j++)
                {
                    result[index] = array2D[i, j];
                    index++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return result;
    }
}