//===========================================================================
// File:        ReflectionHelper.cs
// Project:     ClassLibrary
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Static helper methods for reflection-based type discovery
//              and dynamic invocation. Used by the test application to
//              exercise reflection paths whose behaviour may be affected
//              by symbol renaming during obfuscation.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

using System.Reflection;

namespace ClassLibrary;

/// <summary>
/// Provides reflection-based helpers for type discovery and dynamic
/// method invocation.
/// </summary>
public class ReflectionHelper
{
    private const string AssemblyPrefix = "ClassLibrary";

    /// <summary>
    /// Returns all types in the given assembly whose namespace starts
    /// with the ClassLibrary prefix.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>An enumeration of matching <see cref="Type"/> instances.</returns>
    public static IEnumerable<Type> GetAllTypes(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith(AssemblyPrefix) == true);

    /// <summary>
    /// Counts the number of non-special public methods (instance and
    /// static) declared on each type in the given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>
    /// A dictionary keyed by type name, mapping to the number of
    /// non-special public methods declared on that type.
    /// </returns>
    public static Dictionary<string, int> GetMethodCounts(Assembly assembly)
        => GetAllTypes(assembly)
            .ToDictionary(
                t => t.Name,
                t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                      .Count(m => !m.IsSpecialName));

    /// <summary>
    /// Invokes a public method by name on the given target instance.
    /// </summary>
    /// <param name="target">The object on which to invoke the method.</param>
    /// <param name="methodName">The name of the method to invoke.</param>
    /// <param name="args">The arguments to pass to the method.</param>
    /// <returns>The value returned by the invoked method, or <c>null</c> for void methods.</returns>
    /// <exception cref="MissingMethodException">
    /// Thrown when no public method with the given name is found on the target type.
    /// </exception>
    public static object? InvokeMethod(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName);
        if (method == null)
            throw new MissingMethodException($"Method '{methodName}' not found on {target.GetType().Name}");
        return method.Invoke(target, args);
    }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> using a constructor
    /// matching the given arguments.
    /// </summary>
    /// <typeparam name="T">The type to instantiate.</typeparam>
    /// <param name="args">The arguments to pass to the constructor.</param>
    /// <returns>The newly constructed instance.</returns>
    public static T CreateInstance<T>(params object[] args)
        => (T)Activator.CreateInstance(typeof(T), args)!;
}