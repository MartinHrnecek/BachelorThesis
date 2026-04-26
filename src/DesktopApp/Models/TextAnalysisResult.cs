//===========================================================================
// File:        TextAnalysisResult.cs
// Project:     DesktopApp
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Data model carrying the result of text analysis performed
//              by the WPF desktop application, plus a small string
//              extension method used by the UI for display formatting.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace DesktopApp.Models;

/// <summary>
/// Holds the outcome of a single text analysis operation, including
/// derived statistics and transformations of the original input.
/// </summary>
public class TextAnalysisResult
{
    /// <summary>Gets or sets the original input text.</summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of whitespace-separated words.</summary>
    public int WordCount { get; set; }

    /// <summary>Gets or sets the total number of characters in the input.</summary>
    public int CharacterCount { get; set; }

    /// <summary>Gets or sets the number of sentences detected in the input.</summary>
    public int SentenceCount { get; set; }

    /// <summary>Gets or sets a value indicating whether the input is a palindrome.</summary>
    public bool IsPalindrome { get; set; }

    /// <summary>Gets or sets the input converted to upper case.</summary>
    public string UppercaseText { get; set; } = string.Empty;

    /// <summary>Gets or sets the input with its character order reversed.</summary>
    public string ReversedText { get; set; } = string.Empty;

    /// <summary>Gets or sets the timestamp at which the analysis was produced.</summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Provides extension methods on <see cref="string"/> used by the UI
/// for display formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates the string to <paramref name="maxLength"/> characters,
    /// appending an ellipsis if truncation occurred.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">The maximum length before truncation.</param>
    /// <returns>
    /// The original string if its length does not exceed
    /// <paramref name="maxLength"/>; otherwise the truncated string with
    /// an appended ellipsis.
    /// </returns>
    public static string Truncate(this string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";
}