//===========================================================================
// File:        SafeCalculator.cs
// Project:     CliApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Demonstrates structured exception handling with try-catch
//              and finally blocks. The IL representation of exception
//              handling produces .try and .handler regions whose
//              preservation under obfuscation is verified by the
//              experimental pipeline.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace CliApp;

/// <summary>
/// Demonstrates structured exception handling by performing integer
/// division across a sequence of denominators, including zeros that
/// trigger a <see cref="DivideByZeroException"/>.
/// </summary>
public class SafeCalculator
{
    /// <summary>
    /// Iterates over a fixed set of denominators, divides 100 by each,
    /// and prints the result or the captured exception message.
    /// </summary>
    public void RunDemo()
    {
        Console.WriteLine("Calculator demo:");

        int[] denominators = { 5, 0, 2, 0, 8 };
        foreach (var d in denominators)
        {
            try
            {
                int result = Divide(100, d);
                Console.WriteLine($"100 / {d} = {result}");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine($"Error dividing by {d}: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"  Attempt with denominator {d} finished.");
            }
        }
    }

    /// <summary>
    /// Divides <paramref name="a"/> by <paramref name="b"/>, throwing
    /// <see cref="DivideByZeroException"/> on a zero divisor.
    /// </summary>
    /// <param name="a">The dividend.</param>
    /// <param name="b">The divisor.</param>
    /// <returns>The integer quotient <paramref name="a"/> / <paramref name="b"/>.</returns>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="b"/> equals zero.</exception>
    private int Divide(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException($"Cannot divide {a} by zero.");
        return a / b;
    }
}