//===========================================================================
// File:        ReflectionInspector.cs
// Project:     CliApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Reflection-based type inspector that prints public methods
//              and private fields of a given type. Used to demonstrate
//              the impact of symbol renaming on reflection output, which
//              is recorded as a REFLECTION_IMPACT side effect by the
//              experimental pipeline.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using System.Reflection;

namespace CliApp;

/// <summary>
/// Prints reflection metadata (public methods, private fields) for a
/// given <see cref="Type"/>, and provides a thin wrapper around
/// <see cref="Activator.CreateInstance(Type, object[])"/>.
/// </summary>
public class ReflectionInspector
{
    /// <summary>
    /// Prints the full name of the type along with its public methods
    /// (with parameter types) and its private instance fields.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <remarks>
    /// The output of this method changes after obfuscation because
    /// renamed symbols are reported with their obfuscated identifiers.
    /// This behaviour is captured as a REFLECTION_IMPACT side effect
    /// in the experimental results.
    /// </remarks>
    public void InspectType(Type type)
    {
        Console.WriteLine($"Inspecting type: {type.FullName}");

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName)
            .ToList();

        Console.WriteLine($"Public methods ({methods.Count}):");
        foreach (var method in methods)
            Console.WriteLine($"  - {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");

        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        Console.WriteLine($"Private fields ({fields.Length}):");
        foreach (var field in fields)
            Console.WriteLine($"  - {field.FieldType.Name} {field.Name}");
    }

    /// <summary>
    /// Creates an instance of the given type using a constructor matching
    /// the supplied arguments.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">The arguments to pass to the constructor.</param>
    /// <returns>The newly constructed instance, or <c>null</c> if creation fails.</returns>
    public object? CreateInstance(Type type, params object[] args)
    {
        return Activator.CreateInstance(type, args);
    }
}