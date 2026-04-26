//===========================================================================
// File:        Program.cs
// Project:     AlgorithmLib
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Entry point for the AlgorithmLib test application. Either
//              executes the BenchmarkDotNet benchmark suite (when invoked
//              with the --benchmark flag) or runs a deterministic smoke
//              test that exercises the public API and prints the results
//              to standard output.
//
//              The deterministic mode is used by the pipeline to verify
//              functional correctness of obfuscated builds by comparing
//              their stdout against the reference build.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using BenchmarkDotNet.Running;
using AlgorithmLib;

// Benchmark mode: delegate execution to BenchmarkDotNet and exit.
if (args.Contains("--benchmark"))
{
    BenchmarkRunner.Run<AlgorithmBenchmarks>();
    return;
}

// Smoke-test mode: exercise each algorithm category with fixed inputs
// so that stdout is byte-for-byte reproducible across builds.
Console.WriteLine("=== Algorithm Library Test ===");

// Sorting
var sorter = new SortingAlgorithms();
int[] data = { 64, 34, 25, 12, 22, 11, 90, 45, 67, 3 };
Console.WriteLine($"Original: {string.Join(", ", data)}");
sorter.BubbleSort(data);
Console.WriteLine($"Sorted:   {string.Join(", ", data)}");

// Arithmetic
var math = new MathOperations();
Console.WriteLine($"Fibonacci(10): {math.Fibonacci(10)}");
Console.WriteLine($"Factorial(8): {math.Factorial(8)}");

// Search
var searcher = new SearchAlgorithms();
int[] sorted = { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };
Console.WriteLine($"BinarySearch(7): index {searcher.BinarySearch(sorted, 7)}");

// String operations
var strOps = new StringOperations();
Console.WriteLine($"IsPalindrome('racecar'): {strOps.IsPalindrome("racecar")}");
Console.WriteLine($"ReverseWords('hello world'): {strOps.ReverseWords("hello world")}");

Console.WriteLine("=== Done ===");