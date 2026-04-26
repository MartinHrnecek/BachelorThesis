//===========================================================================
// File:        Interfaces.cs
// Project:     AlgorithmLib.Interfaces
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Public contracts exposed by the AlgorithmLib library.
//              These interfaces define the surface used by the benchmark
//              suite and by the BenchmarkRunner reflection-based invoker
//              to call algorithm implementations across reference and
//              obfuscated builds.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace AlgorithmLib.Interfaces;

/// <summary>
/// Contract for in-place array sorting algorithms.
/// </summary>
public interface ISortingAlgorithms
{
    /// <summary>Sorts the array in ascending order using bubble sort.</summary>
    /// <param name="arr">The array to be sorted in place.</param>
    void BubbleSort(int[] arr);

    /// <summary>Sorts the specified subarray in ascending order using quicksort.</summary>
    /// <param name="arr">The array to be sorted in place.</param>
    /// <param name="low">The inclusive lower bound of the subarray.</param>
    /// <param name="high">The inclusive upper bound of the subarray.</param>
    void QuickSort(int[] arr, int low, int high);
}

/// <summary>
/// Contract for arithmetic and number-theory algorithms.
/// </summary>
public interface IMathOperations
{
    /// <summary>Computes the n-th Fibonacci number.</summary>
    /// <param name="n">The zero-based index of the Fibonacci number.</param>
    /// <returns>The n-th Fibonacci number.</returns>
    long Fibonacci(int n);

    /// <summary>Computes the factorial of a non-negative integer.</summary>
    /// <param name="n">A non-negative integer.</param>
    /// <returns>The factorial of <paramref name="n"/>.</returns>
    long Factorial(int n);

    /// <summary>Returns all prime numbers up to the given limit.</summary>
    /// <param name="limit">The inclusive upper bound of the search range.</param>
    /// <returns>An array of all primes in the range [2, <paramref name="limit"/>].</returns>
    int[] SieveOfEratosthenes(int limit);
}

/// <summary>
/// Contract for array search algorithms.
/// </summary>
public interface ISearchAlgorithms
{
    /// <summary>Searches a sorted array for the specified value using binary search.</summary>
    /// <param name="arr">A pre-sorted array of integers.</param>
    /// <param name="target">The value to locate.</param>
    /// <returns>The zero-based index of the value, or <c>-1</c> if not found.</returns>
    int BinarySearch(int[] arr, int target);
}

/// <summary>
/// Contract for string manipulation algorithms.
/// </summary>
public interface IStringOperations
{
    /// <summary>Determines whether the specified string is a palindrome.</summary>
    /// <param name="s">The string to test.</param>
    /// <returns><c>true</c> if the string is a palindrome; otherwise <c>false</c>.</returns>
    bool IsPalindrome(string s);
}