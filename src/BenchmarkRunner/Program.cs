//===========================================================================
// File:        Program.cs
// Project:     BenchmarkRunner
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Reflection-based BenchmarkDotNet runner that invokes the
//              algorithms exposed by AlgorithmLib across reference and
//              obfuscated builds.
//
//              Direct invocation by method name is not possible against
//              obfuscated assemblies because the obfuscators rename public
//              symbols to short or non-printable identifiers. The runner
//              therefore loads the target DLL dynamically and locates each
//              algorithm by matching method signatures (return type and
//              parameter types). The reflection overhead introduced by
//              MethodInfo.Invoke is acknowledged as a measurement bias
//              for short methods in the methodology chapter of the thesis.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Reflection;

if (args.Length < 1) { Console.WriteLine("Usage: BenchmarkRunner <dll>"); return; }

var dllPath = Path.GetFullPath(args[0]);
if (!File.Exists(dllPath)) { Console.WriteLine($"Not found: {dllPath}"); return; }

// Pass the target DLL path to the benchmark harness via an environment
// variable; BenchmarkDotNet spawns isolated processes for each benchmark
// run, so a static field would not survive the boundary.
Environment.SetEnvironmentVariable("BENCHMARK_DLL_PATH", dllPath);

BenchmarkRunner.Run<AlgorithmLibProxy>();

/// <summary>
/// BenchmarkDotNet harness that invokes the AlgorithmLib algorithms via
/// reflection so that obfuscated builds (with renamed symbols) can be
/// measured using the same benchmark code as the reference build.
/// </summary>
/// <remarks>
/// Methods are located by matching their signatures rather than their
/// names. The matching is intentionally conservative: each predicate is
/// chosen to be unique within the AlgorithmLib surface, so that even
/// after obfuscation the correct method is identified.
/// </remarks>
public class AlgorithmLibProxy
{
    private Assembly? _asm;
    private MethodInfo? _bubbleSort, _quickSort, _fibonacci, _factorial, _binarySearch, _isPalindrome, _sieve;
    private object? _sorter, _math, _searcher, _strings;

    private int[] _data = Array.Empty<int>();
    private int[] _sorted = Array.Empty<int>();

    /// <summary>
    /// Loads the target assembly and resolves each algorithm method by
    /// matching its signature.
    /// </summary>
    /// <remarks>
    /// Invoked once by BenchmarkDotNet prior to any benchmark iteration.
    /// All reflection lookups are performed here so that the per-benchmark
    /// hot path contains only the <see cref="MethodInfo.Invoke"/> call
    /// and the algorithm itself.
    /// </remarks>
    [GlobalSetup]
    public void Setup()
    {
        var path = Environment.GetEnvironmentVariable("BENCHMARK_DLL_PATH")!;
        _asm = Assembly.LoadFrom(path);

        var types = _asm.GetTypes().Where(t => !t.IsAbstract).ToList();

        // BubbleSort: void M(int[])
        (_sorter, _bubbleSort) = FindMethod(types, m =>
            m.ReturnType == typeof(void) &&
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType == typeof(int[]));

        // QuickSort: void M(int[], int, int)
        var (_, qs) = FindMethod(types, m =>
            m.ReturnType == typeof(void) &&
            m.GetParameters().Length == 3 &&
            m.GetParameters()[0].ParameterType == typeof(int[]) &&
            m.GetParameters()[1].ParameterType == typeof(int) &&
            m.GetParameters()[2].ParameterType == typeof(int));
        _quickSort = qs;

        // Fibonacci: long M(int)
        (_math, _fibonacci) = FindMethod(types, m =>
            m.ReturnType == typeof(long) &&
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType == typeof(int));

        // Factorial shares the same signature as Fibonacci, so it is
        // resolved by enumerating long-returning unary methods on the
        // same instance and excluding the one already bound.
        _factorial = _math?.GetType().GetMethods()
            .FirstOrDefault(m => m.ReturnType == typeof(long) &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(int) &&
                m != _fibonacci);

        // BinarySearch: int M(int[], int)
        (_searcher, _binarySearch) = FindMethod(types, m =>
            m.ReturnType == typeof(int) &&
            m.GetParameters().Length == 2 &&
            m.GetParameters()[0].ParameterType == typeof(int[]) &&
            m.GetParameters()[1].ParameterType == typeof(int));

        // IsPalindrome: bool M(string)
        (_strings, _isPalindrome) = FindMethod(types, m =>
            m.ReturnType == typeof(bool) &&
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType == typeof(string));

        // SieveOfEratosthenes: int[] M(int)
        var (_, sv) = FindMethod(types, m =>
            m.ReturnType == typeof(int[]) &&
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType == typeof(int));
        _sieve = sv;

        _data = new[] { 64, 34, 25, 12, 22, 11, 90, 45, 67, 3, 78, 56, 41, 99, 7 };
        _sorted = new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29 };
    }

    /// <summary>
    /// Searches the loaded types for the first public instance method
    /// matching the given predicate and returns a fresh instance of the
    /// declaring type together with the method handle.
    /// </summary>
    /// <param name="types">The candidate types from the loaded assembly.</param>
    /// <param name="predicate">A signature-matching predicate.</param>
    /// <returns>
    /// A tuple containing the constructed instance and the method handle,
    /// or <c>(null, null)</c> if no matching method is found.
    /// </returns>
    private (object? instance, MethodInfo? method) FindMethod(List<Type> types, Func<MethodInfo, bool> predicate)
    {
        foreach (var t in types)
        {
            var m = t.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(predicate);
            if (m != null) return (Activator.CreateInstance(t), m);
        }
        return (null, null);
    }

    /// <summary>Benchmarks bubble sort via reflection invocation.</summary>
    [Benchmark]
    public void BubbleSort()
    {
        var c = (int[])_data.Clone();
        _bubbleSort?.Invoke(_sorter, new object[] { c });
    }

    /// <summary>Benchmarks quicksort via reflection invocation.</summary>
    [Benchmark]
    public void QuickSort()
    {
        var c = (int[])_data.Clone();
        _quickSort?.Invoke(_sorter, new object[] { c, 0, c.Length - 1 });
    }

    /// <summary>Benchmarks the recursive Fibonacci computation for n=20.</summary>
    [Benchmark]
    public object? Fibonacci() => _fibonacci?.Invoke(_math, new object[] { 20 });

    /// <summary>Benchmarks the factorial computation for n=12.</summary>
    [Benchmark]
    public object? Factorial() => _factorial?.Invoke(_math, new object[] { 12 });

    /// <summary>Benchmarks binary search for the value 15 in a pre-sorted array.</summary>
    [Benchmark]
    public object? BinarySearch() => _binarySearch?.Invoke(_searcher, new object[] { _sorted, 15 });

    /// <summary>Benchmarks the palindrome check on the string "racecar".</summary>
    [Benchmark]
    public object? IsPalindrome() => _isPalindrome?.Invoke(_strings, new object[] { "racecar" });

    /// <summary>Benchmarks the Sieve of Eratosthenes for primes up to 1000.</summary>
    [Benchmark]
    public object? SieveOfEratosthenes() => _sieve?.Invoke(_math, new object[] { 1000 });
}