//===========================================================================
// File:        AsyncWorker.cs
// Project:     CliApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Asynchronous workload demonstration using async/await
//              and Task.WhenAll. Used to exercise the compiler-generated
//              state machine code paths whose obfuscation may differ
//              from regular synchronous methods.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace CliApp;

/// <summary>
/// Demonstrates asynchronous parallel work using <c>async/await</c>
/// and <see cref="Task.WhenAll(Task[])"/>.
/// </summary>
public static class AsyncWorker
{
    /// <summary>
    /// Runs three asynchronous payload-processing tasks in parallel and
    /// prints each result to standard output.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DoWorkAsync()
    {
        Console.WriteLine("Starting async work...");

        var tasks = new[]
        {
            ProcessDataAsync("payload-alpha"),
            ProcessDataAsync("payload-beta"),
            ProcessDataAsync("payload-gamma")
        };

        var results = await Task.WhenAll(tasks);
        foreach (var result in results)
            Console.WriteLine($"Async result: {result}");
    }

    /// <summary>
    /// Simulates asynchronous processing of the given payload by yielding
    /// briefly and returning a transformed string.
    /// </summary>
    /// <param name="input">The input payload to process.</param>
    /// <returns>A task producing the processed result string.</returns>
    private static async Task<string> ProcessDataAsync(string input)
    {
        await Task.Delay(50);
        return $"Processed: {input.ToUpper()}";
    }
}