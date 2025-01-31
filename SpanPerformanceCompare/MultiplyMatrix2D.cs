using BenchmarkDotNet.Attributes;
using Bogus;
using CommunityToolkit.HighPerformance;

namespace SpanPerformanceCompare;

[MemoryDiagnoser(displayGenColumns: true)]
[HideColumns("StdDev", "Median", "Job", "RatioSD", "Error", "Alloc Ratio")]
public class MultiplyMatrix2D
{
    [Params(2, 3, 5, 10, 20, 50, 100, 200, 300, 500, 750, 1000, 1500, 2000)]
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
        var result = new int[Matrix_Size, Matrix_Size];
        for (var i = 0; i < Matrix_Size; i++)
        {
            for (var j = 0; j < Matrix_Size; j++)
            {
                for (var k = 0; k < Matrix_Size; k++)
                {
                    result[i, j] += _matrix1[i, k] * _matrix2[k, j];
                }
            }
        }

        return result;
    }

    [Benchmark(Description = "Span2D<int>")]
    public int[,] MultiplyMatrixUsingSpan()
    {
        var result = new int[Matrix_Size, Matrix_Size];
        var resultSpan = result.AsSpan2D();

        var matrixASpan = _matrix1.AsSpan2D();
        var matrixBSpan = _matrix2.AsSpan2D();

        for (var i = 0; i < Matrix_Size; i++)
        {
            for (var j = 0; j < Matrix_Size; j++)
            {
                for (var k = 0; k < Matrix_Size; k++)
                {
                    resultSpan[i, j] += matrixASpan[i, k] * matrixBSpan[k, j];
                }
            }
        }

        return result;
    }
}