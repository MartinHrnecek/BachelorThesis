//===========================================================================
// File:        MathOperations.cs
// Project:     AlgorithmLib
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Implementations of arithmetic and number-theory algorithms
//              used by the test application: Fibonacci, factorial,
//              primality testing and the Sieve of Eratosthenes.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using AlgorithmLib.Interfaces;

namespace AlgorithmLib;

/// <summary>
/// Provides implementations of arithmetic and number-theory algorithms.
/// </summary>
public class MathOperations : IMathOperations
{
    /// <summary>
    /// Computes the n-th Fibonacci number using a naive recursive definition.
    /// </summary>
    /// <param name="n">The zero-based index of the Fibonacci number to compute.</param>
    /// <returns>The n-th Fibonacci number.</returns>
    /// <remarks>
    /// The recursive variant is intentionally chosen to expose deeply nested
    /// call stacks to control-flow obfuscation transformations.
    /// </remarks>
    public long Fibonacci(int n)
    {
        if (n <= 1) return n;
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }

    /// <summary>
    /// Computes the factorial of <paramref name="n"/> recursively.
    /// </summary>
    /// <param name="n">A non-negative integer.</param>
    /// <returns>The factorial of <paramref name="n"/>.</returns>
    public long Factorial(int n)
    {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }

    /// <summary>
    /// Determines whether the given integer is a prime number.
    /// </summary>
    /// <param name="n">The integer to test.</param>
    /// <returns><c>true</c> if <paramref name="n"/> is prime; otherwise <c>false</c>.</returns>
    public bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
            if (n % i == 0) return false;
        return true;
    }

    /// <summary>
    /// Returns all prime numbers up to <paramref name="limit"/> using the
    /// Sieve of Eratosthenes.
    /// </summary>
    /// <param name="limit">The inclusive upper bound of the search range.</param>
    /// <returns>An array of all primes in the range [2, <paramref name="limit"/>].</returns>
    public int[] SieveOfEratosthenes(int limit)
    {
        bool[] isPrime = new bool[limit + 1];
        Array.Fill(isPrime, true);
        isPrime[0] = isPrime[1] = false;

        for (int i = 2; i * i <= limit; i++)
            if (isPrime[i])
                for (int j = i * i; j <= limit; j += i)
                    isPrime[j] = false;

        return Enumerable.Range(0, limit + 1)
            .Where(i => isPrime[i])
            .ToArray();
    }
}