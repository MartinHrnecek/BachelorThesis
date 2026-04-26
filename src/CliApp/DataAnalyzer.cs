//===========================================================================
// File:        DataAnalyzer.cs
// Project:     CliApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Demonstrates common imperative and LINQ-based data
//              processing patterns: aggregation, ordering, conditional
//              categorization and nested loops. Provides a mix of
//              control-flow constructs whose dekompilation behaviour
//              under obfuscation is examined in the experimental part
//              of the thesis.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace CliApp;

/// <summary>
/// Exercises common data-processing constructs (loops, LINQ queries,
/// branching) on a fixed integer dataset and prints the results.
/// </summary>
public class DataAnalyzer
{
    private readonly int[] _data = { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 7, 8, 2, 4, 6 };

    /// <summary>
    /// Runs the full analysis sequence: sum, basic statistics, value
    /// categorization and a multiplication table.
    /// </summary>
    public void RunAnalysis()
    {
        // Basic loop + sum
        int sum = 0;
        for (int i = 0; i < _data.Length; i++)
            sum += _data[i];
        Console.WriteLine($"Sum: {sum}");

        // LINQ
        var sorted = _data.OrderBy(x => x).ToList();
        Console.WriteLine($"Min: {sorted.First()}, Max: {sorted.Last()}");

        double avg = _data.Average();
        Console.WriteLine($"Average: {avg:F2}");

        // Nested branching
        foreach (var item in _data)
        {
            string category;
            if (item <= 3)
                category = "low";
            else if (item <= 6)
                category = "medium";
            else
                category = "high";
            Console.WriteLine($"{item} -> {category} ({(item % 2 == 0 ? "even" : "odd")})");
        }

        // Nested loop
        Console.WriteLine("Multiplication sample:");
        for (int i = 1; i <= 3; i++)
            for (int j = 1; j <= 3; j++)
                Console.WriteLine($"  {i} x {j} = {i * j}");
    }
}