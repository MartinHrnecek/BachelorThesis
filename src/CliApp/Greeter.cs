//===========================================================================
// File:        Greeter.cs
// Project:     CliApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Simple greeting class containing string literals
//              (including a placeholder secret) used to demonstrate
//              string-encryption behaviour of the tested obfuscators.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace CliApp;

/// <summary>
/// Prints a personalized greeting and exposes constant string literals
/// used to evaluate string-encryption behaviour of obfuscators.
/// </summary>
public class Greeter
{
    private readonly string _name;
    private const string SecretKey = "12345";
    private const string Version = "1.0.0";
    private const string Author = "BachelorThesis";

    /// <summary>
    /// Initializes a new <see cref="Greeter"/> with the given recipient name.
    /// </summary>
    /// <param name="name">The name used in the greeting.</param>
    public Greeter(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Prints a personalized greeting along with version and author
    /// information, and reports the length category of the recipient name.
    /// </summary>
    public void Greet()
    {
        Console.WriteLine($"Hello from {_name}!");
        Console.WriteLine($"Version: {Version}");
        Console.WriteLine($"Author: {Author}");

        if (_name.Length > 10)
            Console.WriteLine("Very long name detected.");
        else if (_name.Length > 5)
            Console.WriteLine("Long name detected.");
        else
            Console.WriteLine("Short name detected.");
    }

    /// <summary>Returns the embedded placeholder secret key.</summary>
    /// <returns>The constant secret string literal.</returns>
    public string GetSecret() => SecretKey;

    /// <summary>Returns the version constant.</summary>
    /// <returns>The version string literal.</returns>
    public string GetVersion() => Version;
}