//===========================================================================
// File:        Benchmarks.cs
// Project:     AlgorithmLib
// Author:      Martin Hrnecek <xhrnecm00>
// Description: BenchmarkDotNet benchmark suite for AlgorithmLib methods.
//              Used to measure runtime performance across reference and
//              obfuscated builds.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using BenchmarkDotNet.Attributes;

namespace AlgorithmLib;

/// <summary>
/// BenchmarkDotNet benchmark suite covering sorting, arithmetic, search
/// and string algorithms exposed by <see cref="AlgorithmLib"/>.
/// </summary>
/// <remarks>
/// Methods are invoked via reflection by the external BenchmarkRunner,
/// because obfuscated builds rename public symbols and direct invocation
/// by name is no longer possible.
/// </remarks>
[MemoryDiagnoser]
[SimpleJob]
public class AlgorithmBenchmarks
{
    private readonly SortingAlgorithms _sorter = new();
    private readonly MathOperations _math = new();
    private readonly SearchAlgorithms _searcher = new();
    private readonly StringOperations _strings = new();

    private int[] _data = Array.Empty<int>();
    private int[] _sorted = Array.Empty<int>();

    /// <summary>
    /// Initializes input arrays before benchmark iterations are executed.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _data = new[] { 64, 34, 25, 12, 22, 11, 90, 45, 67, 3, 78, 56, 41, 99, 7 };
        _sorted = new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29 };
    }

    /// <summary>Benchmarks the bubble sort implementation.</summary>
    [Benchmark]
    public void BubbleSort()
    {
        var copy = (int[])_data.Clone();
        _sorter.BubbleSort(copy);
    }

    /// <summary>Benchmarks the recursive quicksort implementation.</summary>
    [Benchmark]
    public void QuickSort()
    {
        var copy = (int[])_data.Clone();
        _sorter.QuickSort(copy, 0, copy.Length - 1);
    }

    /// <summary>Benchmarks the recursive Fibonacci implementation for n=20.</summary>
    [Benchmark]
    public long Fibonacci() => _math.Fibonacci(20);

    /// <summary>Benchmarks the iterative factorial implementation for n=12.</summary>
    [Benchmark]
    public long Factorial() => _math.Factorial(12);

    /// <summary>Benchmarks binary search for the value 15 in a pre-sorted array.</summary>
    [Benchmark]
    public int BinarySearch() => _searcher.BinarySearch(_sorted, 15);

    /// <summary>Benchmarks the palindrome check on the string "racecar".</summary>
    [Benchmark]
    public bool IsPalindrome() => _strings.IsPalindrome("racecar");

    /// <summary>Benchmarks the Sieve of Eratosthenes for primes up to 1000.</summary>
    [Benchmark]
    public int[] SieveOfEratosthenes() => _math.SieveOfEratosthenes(1000);
}