//===========================================================================
// File:        StringExtensions.cs
// Project:     ClassLibrary
// Author:      Martin Hrnecek <xhrnecm00>
// Description: Extension methods on System.String providing common
//              string manipulation helpers: truncation with ellipsis,
//              title casing, multi-keyword search and repetition.
//
// Bachelor's Thesis: Analysis, Methodology and Experimental Evaluation
//                    of IL Code Obfuscation Techniques in .NET Applications
// Year:        2026
//===========================================================================

namespace ClassLibrary;

/// <summary>
/// Provides extension methods on <see cref="string"/> for common
/// manipulation tasks.
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
    /// an appended ellipsis (<c>"..."</c>).
    /// </returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    /// <summary>
    /// Converts the string to title case by capitalizing the first letter
    /// of each space-separated word and lowercasing the remainder.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>The title-cased string.</returns>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return string.Join(" ", value.Split(' ')
            .Select(w => w.Length > 0
                ? char.ToUpper(w[0]) + w[1..].ToLower()
                : w));
    }

    /// <summary>
    /// Determines whether the string contains any of the specified
    /// keywords (case-insensitive).
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="keywords">The keywords to look for.</param>
    /// <returns>
    /// <c>true</c> if at least one keyword is found in the string;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool ContainsAny(this string value, params string[] keywords)
        => keywords.Any(k => value.Contains(k, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns a new string containing the input value repeated the
    /// specified number of times.
    /// </summary>
    /// <param name="value">The string to repeat.</param>
    /// <param name="count">The number of repetitions.</param>
    /// <returns>The concatenated repeated string.</returns>
    public static string Repeat(this string value, int count)
        => string.Concat(Enumerable.Repeat(value, count));
}